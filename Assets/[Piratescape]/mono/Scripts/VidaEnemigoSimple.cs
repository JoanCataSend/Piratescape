using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class VidaEnemigoSimple : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 30;
    [SerializeField] private int vidaActual = 30;
    [SerializeField] private bool reiniciarVidaAlActivarse = true;

    [Header("Muerte")]
    [SerializeField] private bool destruirAlMorir = false;
    [SerializeField] private bool desactivarAlMorir = true;
    [SerializeField] private float retardoMuerte = 0.1f;

    [Header("Feedback visual de danio")]
    [SerializeField] private bool activarFlashRojoAlRecibirDanio = true;
    [SerializeField] private Color colorFlashDanio = new Color(1f, 0.05f, 0.02f, 1f);
    [SerializeField] private float duracionFlashDanio = 0.12f;
    [SerializeField] private int repeticionesFlashDanio = 1;
    [SerializeField] private bool incluirRenderersInactivos = true;

    [Header("FX opcional")]
    [SerializeField] private ParticleSystem particulasMuerte;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoMuerte;
    [Range(0f, 1f)]
    [SerializeField] private float volumenMuerte = 0.8f;

    [Header("Particulas muerte por codigo")]
    [SerializeField] private bool crearParticulasMuerteAutomaticas = true;
    [SerializeField] private bool usarAutomaticasSoloSiNoHayAsignadas = true;
    [SerializeField] private bool usarSimulationSpaceWorld = true;
    [SerializeField] private Vector3 offsetParticulasMuerte = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private float tiempoDestruirFXMuerte = 3f;

    [Header("FX muerte - polvo cartoon")]
    [SerializeField] private bool crearPolvoMuerte = true;
    [SerializeField] private Color colorPolvoMuerte = new Color(0.72f, 0.62f, 0.46f, 0.85f);
    [SerializeField] private int polvoMuerteMin = 28;
    [SerializeField] private int polvoMuerteMax = 42;
    [SerializeField] private float tamanoPolvoMin = 0.08f;
    [SerializeField] private float tamanoPolvoMax = 0.24f;
    [SerializeField] private float expansionHorizontalPolvo = 0.9f;
    [SerializeField] private float velocidadVerticalPolvoMin = 0.15f;
    [SerializeField] private float velocidadVerticalPolvoMax = 0.75f;
    [SerializeField] private float radioSpawnPolvo = 0.18f;

    [Header("FX muerte - brillitos")]
    [SerializeField] private bool crearBrillosMuerte = true;
    [SerializeField] private Color colorBrilloMuerte = new Color(1f, 0.9f, 0.25f, 1f);
    [SerializeField] private int brillosMuerteMin = 8;
    [SerializeField] private int brillosMuerteMax = 14;
    [SerializeField] private float tamanoBrilloMin = 0.045f;
    [SerializeField] private float tamanoBrilloMax = 0.09f;
    [SerializeField] private float expansionHorizontalBrillo = 0.55f;
    [SerializeField] private float velocidadVerticalBrilloMin = 0.75f;
    [SerializeField] private float velocidadVerticalBrilloMax = 1.55f;
    [SerializeField] private float radioSpawnBrillos = 0.1f;

    [Header("Animator opcional")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerDanio = "Hit";
    [SerializeField] private string triggerMuerte = "Die";

    private bool muerto;
    private Renderer[] renderersFlash;
    private MaterialPropertyBlock[] bloquesOriginales;
    private Coroutine rutinaFlashDanio;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        CachearRenderersFlash();
        vidaActual = Mathf.Clamp(vidaActual <= 0 ? vidaMaxima : vidaActual, 1, vidaMaxima);
    }

    private void OnEnable()
    {
        if (reiniciarVidaAlActivarse)
        {
            muerto = false;
            vidaActual = Mathf.Max(1, vidaMaxima);
        }

        RestaurarColorOriginal();
    }

    private void OnDisable()
    {
        RestaurarColorOriginal();
    }

    public void RecibirDanio(int cantidad)
    {
        if (muerto || cantidad <= 0)
        {
            return;
        }

        vidaActual -= cantidad;
        ReproducirFlashRojoDanio();
        ReproducirTrigger(triggerDanio);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    public void TakeDamage(int cantidad)
    {
        RecibirDanio(cantidad);
    }

    public void AplicarDanio(int cantidad)
    {
        RecibirDanio(cantidad);
    }

    public void ReproducirFlashRojoDanio()
    {
        if (!activarFlashRojoAlRecibirDanio || !isActiveAndEnabled)
        {
            return;
        }

        if (renderersFlash == null || renderersFlash.Length == 0)
        {
            CachearRenderersFlash();
        }

        if (renderersFlash == null || renderersFlash.Length == 0)
        {
            return;
        }

        if (rutinaFlashDanio != null)
        {
            StopCoroutine(rutinaFlashDanio);
            RestaurarColorOriginal();
        }

        rutinaFlashDanio = StartCoroutine(RutinaFlashRojoDanio());
    }

    private IEnumerator RutinaFlashRojoDanio()
    {
        int repeticiones = Mathf.Max(1, repeticionesFlashDanio);
        float duracion = Mathf.Max(0.02f, duracionFlashDanio);

        for (int i = 0; i < repeticiones; i++)
        {
            AplicarColorFlash(colorFlashDanio);
            yield return new WaitForSeconds(duracion);

            RestaurarColorOriginal();

            if (i < repeticiones - 1)
            {
                yield return new WaitForSeconds(0.04f);
            }
        }

        rutinaFlashDanio = null;
    }

    private void CachearRenderersFlash()
    {
        renderersFlash = GetComponentsInChildren<Renderer>(incluirRenderersInactivos);
        bloquesOriginales = new MaterialPropertyBlock[renderersFlash.Length];

        for (int i = 0; i < renderersFlash.Length; i++)
        {
            bloquesOriginales[i] = new MaterialPropertyBlock();

            if (renderersFlash[i] != null)
            {
                renderersFlash[i].GetPropertyBlock(bloquesOriginales[i]);
            }
        }
    }

    private void AplicarColorFlash(Color color)
    {
        if (renderersFlash == null)
        {
            return;
        }

        for (int i = 0; i < renderersFlash.Length; i++)
        {
            Renderer rendererFlash = renderersFlash[i];

            if (rendererFlash == null)
            {
                continue;
            }

            MaterialPropertyBlock bloque = new MaterialPropertyBlock();
            rendererFlash.GetPropertyBlock(bloque);
            bloque.SetColor("_BaseColor", color);
            bloque.SetColor("_Color", color);
            rendererFlash.SetPropertyBlock(bloque);
        }
    }

    private void RestaurarColorOriginal()
    {
        if (renderersFlash == null || bloquesOriginales == null)
        {
            return;
        }

        for (int i = 0; i < renderersFlash.Length; i++)
        {
            Renderer rendererFlash = renderersFlash[i];

            if (rendererFlash == null)
            {
                continue;
            }

            if (i < bloquesOriginales.Length && bloquesOriginales[i] != null)
            {
                rendererFlash.SetPropertyBlock(bloquesOriginales[i]);
            }
            else
            {
                rendererFlash.SetPropertyBlock(null);
            }
        }
    }

    private void Morir()
    {
        if (muerto)
        {
            return;
        }

        muerto = true;
        ReproducirTrigger(triggerMuerte);
        ReproducirParticulasMuerte();

        if (audioSource != null && sonidoMuerte != null)
        {
            audioSource.PlayOneShot(sonidoMuerte, volumenMuerte);
        }

        if (desactivarAlMorir)
        {
            Invoke(nameof(DesactivarObjeto), retardoMuerte);
            return;
        }

        if (destruirAlMorir)
        {
            Destroy(gameObject, retardoMuerte);
        }
    }

    private void ReproducirParticulasMuerte()
    {
        bool seHaReproducidoAsignada = false;

        if (particulasMuerte != null)
        {
            particulasMuerte.transform.SetParent(null, true);
            particulasMuerte.transform.position = transform.position + offsetParticulasMuerte;
            particulasMuerte.Play(true);
            Destroy(particulasMuerte.gameObject, tiempoDestruirFXMuerte);
            seHaReproducidoAsignada = true;
        }

        if (!crearParticulasMuerteAutomaticas)
        {
            return;
        }

        if (usarAutomaticasSoloSiNoHayAsignadas && seHaReproducidoAsignada)
        {
            return;
        }

        CrearFXMuertePorCodigo(transform.position + offsetParticulasMuerte);
    }

    private void CrearFXMuertePorCodigo(Vector3 posicion)
    {
        GameObject raizFX = new GameObject("FX_Muerte_Enemigo_Codigo");
        raizFX.transform.position = posicion;
        raizFX.transform.rotation = Quaternion.identity;

        if (crearPolvoMuerte)
        {
            ParticleSystem polvo = CrearSistemaParticulas(raizFX.transform, "FX_Polvo_Muerte_Codigo");
            ConfigurarParticulasPolvoMuerte(polvo);
            polvo.Play(true);
        }

        if (crearBrillosMuerte)
        {
            ParticleSystem brillos = CrearSistemaParticulas(raizFX.transform, "FX_Brillos_Muerte_Codigo");
            ConfigurarParticulasBrillosMuerte(brillos);
            brillos.Play(true);
        }

        Destroy(raizFX, Mathf.Max(0.5f, tiempoDestruirFXMuerte));
    }

    private ParticleSystem CrearSistemaParticulas(Transform padre, string nombre)
    {
        GameObject objeto = new GameObject(nombre);
        objeto.transform.SetParent(padre, false);
        objeto.transform.localPosition = Vector3.zero;
        objeto.transform.localRotation = Quaternion.identity;

        return objeto.AddComponent<ParticleSystem>();
    }

    private void ConfigurarParticulasPolvoMuerte(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 0.75f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.85f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoPolvoMin, tamanoPolvoMax);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = 0.03f;
        main.simulationSpace = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.maxParticles = Mathf.Max(20, polvoMuerteMax + 10);

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(polvoMuerteMin, polvoMuerteMax))
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radioSpawnPolvo;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-expansionHorizontalPolvo, expansionHorizontalPolvo);
        velocity.y = new ParticleSystem.MinMaxCurve(velocidadVerticalPolvoMin, velocidadVerticalPolvoMax);
        velocity.z = new ParticleSystem.MinMaxCurve(-expansionHorizontalPolvo, expansionHorizontalPolvo);

        ConfigurarTamanoYColor(ps, colorPolvoMuerte, 0.08f, 1f, 0f);
        ConfigurarRendererParticulas(ps, colorPolvoMuerte);
    }

    private void ConfigurarParticulasBrillosMuerte(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 0.65f;
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.75f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoBrilloMin, tamanoBrilloMax);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        main.gravityModifier = 0f;
        main.simulationSpace = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.maxParticles = Mathf.Max(12, brillosMuerteMax + 6);

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(brillosMuerteMin, brillosMuerteMax))
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = radioSpawnBrillos;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = usarSimulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-expansionHorizontalBrillo, expansionHorizontalBrillo);
        velocity.y = new ParticleSystem.MinMaxCurve(velocidadVerticalBrilloMin, velocidadVerticalBrilloMax);
        velocity.z = new ParticleSystem.MinMaxCurve(-expansionHorizontalBrillo, expansionHorizontalBrillo);

        ConfigurarTamanoYColor(ps, colorBrilloMuerte, 0f, 0.85f, 0f);
        ConfigurarRendererParticulas(ps, colorBrilloMuerte);
    }

    private void ConfigurarTamanoYColor(ParticleSystem ps, Color color, float escalaInicial, float escalaMedia, float escalaFinal)
    {
        var size = ps.sizeOverLifetime;
        size.enabled = true;

        AnimationCurve curvaTamano = new AnimationCurve();
        curvaTamano.AddKey(0f, escalaInicial);
        curvaTamano.AddKey(0.2f, escalaMedia);
        curvaTamano.AddKey(1f, escalaFinal);
        size.size = new ParticleSystem.MinMaxCurve(1f, curvaTamano);

        var colorLifetime = ps.colorOverLifetime;
        colorLifetime.enabled = true;

        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(color.r, color.g, color.b), 0f),
                new GradientColorKey(new Color(color.r, color.g, color.b), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(color.a, 0f),
                new GradientAlphaKey(color.a * 0.75f, 0.35f),
                new GradientAlphaKey(0f, 1f)
            }
        );

        colorLifetime.color = gradiente;
    }

    private void ConfigurarRendererParticulas(ParticleSystem ps, Color color)
    {
        ParticleSystemRenderer particleRenderer = ps.GetComponent<ParticleSystemRenderer>();

        if (particleRenderer == null)
        {
            return;
        }

        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        particleRenderer.sortingFudge = 1f;
        particleRenderer.material = CrearMaterialParticulas(color);
    }

    private Material CrearMaterialParticulas(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        Material material = new Material(shader);
        material.name = "M_FX_Muerte_Enemigo_Codigo";

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }

        return material;
    }

    private void DesactivarObjeto()
    {
        gameObject.SetActive(false);
    }

    private void ReproducirTrigger(string nombreTrigger)
    {
        if (animator == null || string.IsNullOrWhiteSpace(nombreTrigger))
        {
            return;
        }

        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.type == AnimatorControllerParameterType.Trigger && parametro.name == nombreTrigger)
            {
                animator.SetTrigger(nombreTrigger);
                return;
            }
        }
    }
}
