using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class AguaCorrienteParticulas : MonoBehaviour
{
    [Header("Puntos de particulas")]
    [SerializeField] private Transform[] puntosEspumaSuave;
    [SerializeField] private Transform[] puntosSalpicaduraPiedras;
    [SerializeField] private Transform[] puntosChoqueFinal;
    [SerializeField] private bool autoBuscarPuntosPorNombre = true;
    [SerializeField] private bool usarEsteObjetoSiNoHayPuntos = true;

    [Header("Material")]
    [SerializeField] private Material materialParticulasAgua;
    [SerializeField] private Color colorAguaClara = new Color(0.82f, 0.95f, 1f, 0.75f);
    [SerializeField] private Color colorEspuma = new Color(1f, 1f, 1f, 0.72f);

    [Header("Espuma suave continua")]
    [SerializeField] private bool crearEspumaSuave = true;
    [SerializeField] private float espumaParticulasPorSegundo = 8f;
    [SerializeField] private float espumaRadio = 0.28f;
    [SerializeField] private Vector2 espumaTamano = new Vector2(0.06f, 0.16f);
    [SerializeField] private Vector2 espumaLifetime = new Vector2(0.7f, 1.4f);

    [Header("Salpicadura entre piedras")]
    [SerializeField] private bool crearSalpicaduraPiedras = true;
    [SerializeField] private float salpicaduraParticulasPorSegundo = 10f;
    [SerializeField] private float salpicaduraRadio = 0.16f;
    [SerializeField] private Vector2 salpicaduraTamano = new Vector2(0.035f, 0.11f);
    [SerializeField] private Vector2 salpicaduraVelocidadVertical = new Vector2(0.45f, 1.15f);
    [SerializeField] private float salpicaduraExpansionHorizontal = 0.38f;

    [Header("Choque final / cascada")]
    [SerializeField] private bool crearChoqueFinal = true;
    [SerializeField] private float choqueParticulasPorSegundo = 24f;
    [SerializeField] private float choqueRadio = 0.35f;
    [SerializeField] private Vector2 choqueTamano = new Vector2(0.07f, 0.2f);
    [SerializeField] private Vector2 choqueVelocidadVertical = new Vector2(0.5f, 1.45f);
    [SerializeField] private float choqueExpansionHorizontal = 0.75f;

    [Header("Ajustes")]
    [SerializeField] private bool reproducirAlActivarse = true;
    [SerializeField] private bool simulationSpaceWorld = true;
    [SerializeField] private bool crearSoloUnaVez = true;

    private readonly List<ParticleSystem> particulasCreadas = new List<ParticleSystem>();
    private Material materialRuntime;
    private bool creado;

    private void Awake()
    {
        PrepararMaterial();
        CrearParticulasSiHaceFalta();
    }

    private void OnEnable()
    {
        CrearParticulasSiHaceFalta();

        if (reproducirAlActivarse)
        {
            ReproducirTodas();
        }
    }

    private void CrearParticulasSiHaceFalta()
    {
        if (crearSoloUnaVez && creado)
        {
            return;
        }

        PrepararMaterial();
        AutoBuscarPuntos();

        if (crearEspumaSuave)
        {
            CrearGrupoParticulas(puntosEspumaSuave, "FX_Espuma_Suave_Codigo", ConfigurarEspumaSuave);
        }

        if (crearSalpicaduraPiedras)
        {
            CrearGrupoParticulas(puntosSalpicaduraPiedras, "FX_Salpicadura_Piedras_Codigo", ConfigurarSalpicaduraPiedras);
        }

        if (crearChoqueFinal)
        {
            CrearGrupoParticulas(puntosChoqueFinal, "FX_Choque_Final_Codigo", ConfigurarChoqueFinal);
        }

        creado = true;
    }

    public void ReproducirTodas()
    {
        foreach (ParticleSystem ps in particulasCreadas)
        {
            if (ps != null)
            {
                ps.Play(true);
            }
        }
    }

    public void PararTodas()
    {
        foreach (ParticleSystem ps in particulasCreadas)
        {
            if (ps != null)
            {
                ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    private void CrearGrupoParticulas(Transform[] puntos, string nombreBase, System.Action<ParticleSystem> configurar)
    {
        if (puntos == null || puntos.Length == 0)
        {
            if (!usarEsteObjetoSiNoHayPuntos)
            {
                return;
            }

            puntos = new Transform[] { transform };
        }

        for (int i = 0; i < puntos.Length; i++)
        {
            Transform punto = puntos[i];

            if (punto == null)
            {
                continue;
            }

            GameObject fx = new GameObject(nombreBase + "_" + i);
            fx.transform.SetParent(punto, false);
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localRotation = Quaternion.identity;

            ParticleSystem ps = fx.AddComponent<ParticleSystem>();
            configurar.Invoke(ps);
            particulasCreadas.Add(ps);
        }
    }

    private void ConfigurarEspumaSuave(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 2f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(espumaLifetime.x, espumaLifetime.y);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
        main.startSize = new ParticleSystem.MinMaxCurve(espumaTamano.x, espumaTamano.y);
        main.gravityModifier = 0f;
        main.simulationSpace = simulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.maxParticles = 120;
        main.startColor = new ParticleSystem.MinMaxGradient(colorEspuma);

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = espumaParticulasPorSegundo;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = espumaRadio;
        shape.arc = 360f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.02f, 0.12f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.08f, 0.08f);

        AplicarCurvaTamano(ps);
        AplicarGradienteColor(ps, colorEspuma, 0f, colorEspuma.a, 0f);
        ConfigurarRenderer(ps);
    }

    private void ConfigurarSalpicaduraPiedras(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1.2f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.8f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(salpicaduraTamano.x, salpicaduraTamano.y);
        main.gravityModifier = 0.03f;
        main.simulationSpace = simulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.maxParticles = 140;
        main.startColor = new ParticleSystem.MinMaxGradient(colorAguaClara);

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = salpicaduraParticulasPorSegundo;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 22f;
        shape.radius = salpicaduraRadio;
        shape.arc = 360f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-salpicaduraExpansionHorizontal, salpicaduraExpansionHorizontal);
        velocity.y = new ParticleSystem.MinMaxCurve(salpicaduraVelocidadVertical.x, salpicaduraVelocidadVertical.y);
        velocity.z = new ParticleSystem.MinMaxCurve(-salpicaduraExpansionHorizontal, salpicaduraExpansionHorizontal);

        AplicarCurvaTamano(ps);
        AplicarGradienteColor(ps, colorAguaClara, colorAguaClara.a, 0.35f, 0f);
        ConfigurarRenderer(ps);
    }

    private void ConfigurarChoqueFinal(ParticleSystem ps)
    {
        var main = ps.main;
        main.duration = 1.5f;
        main.loop = true;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 1.1f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(choqueTamano.x, choqueTamano.y);
        main.gravityModifier = 0.02f;
        main.simulationSpace = simulationSpaceWorld ? ParticleSystemSimulationSpace.World : ParticleSystemSimulationSpace.Local;
        main.playOnAwake = false;
        main.maxParticles = 220;
        main.startColor = new ParticleSystem.MinMaxGradient(colorEspuma);

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = choqueParticulasPorSegundo;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 35f;
        shape.radius = choqueRadio;
        shape.arc = 360f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-choqueExpansionHorizontal, choqueExpansionHorizontal);
        velocity.y = new ParticleSystem.MinMaxCurve(choqueVelocidadVertical.x, choqueVelocidadVertical.y);
        velocity.z = new ParticleSystem.MinMaxCurve(-choqueExpansionHorizontal, choqueExpansionHorizontal);

        AplicarCurvaTamano(ps);
        AplicarGradienteColor(ps, colorEspuma, colorEspuma.a, 0.45f, 0f);
        ConfigurarRenderer(ps);
    }

    private void AplicarCurvaTamano(ParticleSystem ps)
    {
        var size = ps.sizeOverLifetime;
        size.enabled = true;

        AnimationCurve curva = new AnimationCurve();
        curva.AddKey(0f, 0.15f);
        curva.AddKey(0.25f, 1f);
        curva.AddKey(1f, 0f);

        size.size = new ParticleSystem.MinMaxCurve(1f, curva);
    }

    private void AplicarGradienteColor(ParticleSystem ps, Color color, float alphaInicio, float alphaMedio, float alphaFinal)
    {
        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(color.r, color.g, color.b), 0f),
                new GradientColorKey(new Color(color.r, color.g, color.b), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(alphaInicio, 0f),
                new GradientAlphaKey(alphaMedio, 0.35f),
                new GradientAlphaKey(alphaFinal, 1f)
            }
        );

        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradiente);
    }

    private void ConfigurarRenderer(ParticleSystem ps)
    {
        ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = 1f;

        if (materialRuntime != null)
        {
            renderer.material = materialRuntime;
        }
    }

    private void PrepararMaterial()
    {
        if (materialParticulasAgua != null)
        {
            materialRuntime = materialParticulasAgua;
            return;
        }

        if (materialRuntime != null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            return;
        }

        materialRuntime = new Material(shader);
        materialRuntime.name = "M_Particulas_Agua_Codigo";

        if (materialRuntime.HasProperty("_BaseColor"))
        {
            materialRuntime.SetColor("_BaseColor", colorAguaClara);
        }

        if (materialRuntime.HasProperty("_Color"))
        {
            materialRuntime.SetColor("_Color", colorAguaClara);
        }
    }

    private void AutoBuscarPuntos()
    {
        if (!autoBuscarPuntosPorNombre)
        {
            return;
        }

        if (puntosEspumaSuave == null || puntosEspumaSuave.Length == 0)
        {
            puntosEspumaSuave = BuscarHijosPorPalabras("espuma", "foam", "borde", "edge");
        }

        if (puntosSalpicaduraPiedras == null || puntosSalpicaduraPiedras.Length == 0)
        {
            puntosSalpicaduraPiedras = BuscarHijosPorPalabras("piedra", "rock", "splash", "salpicadura", "rebote");
        }

        if (puntosChoqueFinal == null || puntosChoqueFinal.Length == 0)
        {
            puntosChoqueFinal = BuscarHijosPorPalabras("caida", "caída", "final", "choque", "impacto", "cascada");
        }
    }

    private Transform[] BuscarHijosPorPalabras(params string[] palabras)
    {
        List<Transform> encontrados = new List<Transform>();
        Transform[] hijos = GetComponentsInChildren<Transform>(true);

        foreach (Transform hijo in hijos)
        {
            if (hijo == null || hijo == transform)
            {
                continue;
            }

            string nombre = hijo.name.ToLowerInvariant();

            foreach (string palabra in palabras)
            {
                if (!string.IsNullOrWhiteSpace(palabra) && nombre.Contains(palabra.ToLowerInvariant()))
                {
                    encontrados.Add(hijo);
                    break;
                }
            }
        }

        return encontrados.ToArray();
    }
}
