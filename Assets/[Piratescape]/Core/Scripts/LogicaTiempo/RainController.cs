using System;
using System.Collections;
using UnityEngine;

public class RainController : MonoBehaviour
{
    public enum EstadoClima
    {
        Soleado,
        Nublado,
        LluviaSuave,
        Tormenta,
        Niebla
    }

    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private ParticleSystem rainFX;
    [SerializeField] private ParticleSystem fogFX;
    [SerializeField] private Transform jugador;

    [Header("Audio")]
    [SerializeField] private AudioSource rainAudioSource;
    [SerializeField] private AudioSource stormAudioSource;
    [SerializeField] private AudioSource thunderAudioSource;
    [SerializeField] private AudioClip[] thunderClips;
    [SerializeField] private bool crearThunderAudioSourceAutomaticoSiFalta = true;
    [SerializeField] private bool truenoEn2D = true;
    [Range(0f, 1f)]
    [SerializeField] private float volumenTrueno = 0.95f;

    [Header("Rayos / tormenta")]
    [SerializeField] private Light lightningLight;
    [SerializeField] private Color lightningColor = new Color(0.75f, 0.86f, 1f);
    [SerializeField] private float lightningIntensity = 2.7f;
    [SerializeField] private float lightningDuration = 0.08f;
    [SerializeField] private float minSecondsBetweenLightning = 7f;
    [SerializeField] private float maxSecondsBetweenLightning = 18f;
    [SerializeField] private float thunderDelay = 0.35f;

    [Header("Clima diario")]
    [SerializeField] private bool elegirClimaCadaDia = true;
    [SerializeField] private EstadoClima climaInicial = EstadoClima.Soleado;
    [Range(0f, 1f)] [SerializeField] private float probabilidadNublado = 0.22f;
    [Range(0f, 1f)] [SerializeField] private float probabilidadLluviaSuave = 0.12f;
    [Range(0f, 1f)] [SerializeField] private float probabilidadTormenta = 0.045f;
    [Range(0f, 1f)] [SerializeField] private float probabilidadNiebla = 0.08f;

    [Header("Compatibilidad lluvia antigua")]
    [Range(0f, 1f)]
    [SerializeField] private float rainChancePerDay = 0.08f;
    [SerializeField] private bool usarProbabilidadLluviaAntigua = false;

    [Header("Transiciones suaves")]
    [SerializeField] private float duracionTransicionClima = 16f;
    [SerializeField] private float fadeAudioVelocidad = 2.5f;
    [SerializeField] private bool detenerFXCuandoNoSeVen = true;

    [Header("Lluvia siguiendo al jugador")]
    [SerializeField] private bool rainFXSigueAlJugador = true;
    [SerializeField] private bool buscarJugadorPorTag = true;
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private Vector3 offsetRainFXJugador = new Vector3(0f, 14f, 0f);
    [SerializeField] private Vector3 offsetFogFXJugador = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private bool mantenerAlturaRainFX = true;

    [Header("Interiores / cuevas")]
    [SerializeField] private bool permitirBloqueoLluviaEnInteriores = true;
    [Tooltip("Velocidad del fade al entrar/salir de cuevas. 12 = casi instantaneo, 3 = suave.")]
    [SerializeField] private float velocidadFadeInterior = 12f;
    [Tooltip("Si esta activo, la lluvia deja de emitir dentro de cuevas cuando la oclusion llega casi a 1.")]
    [SerializeField] private bool apagarRainFXSiInteriorCompleto = true;
    [SerializeField] private bool reducirAudioLluviaEnInterior = true;
    [Range(0f, 1f)] [SerializeField] private float volumenLluviaInterior = 0f;
    [Range(0f, 1f)] [SerializeField] private float volumenTormentaInterior = 0.18f;
    [SerializeField] private bool reducirTruenoEnInterior = true;
    [Range(0f, 1f)] [SerializeField] private float volumenTruenoInterior = 0.28f;
    [SerializeField] private bool bloquearRayosEnInteriorProfundo = false;
    [Range(0f, 1f)] [SerializeField] private float oclusionInteriorParaBloquearRayos = 0.8f;

    [Header("Configuracion automatica FX")]
    [SerializeField] private bool crearRainFXAutomaticoSiFalta = false;
    [SerializeField] private bool configurarRainFXPorCodigo = true;
    [SerializeField] private bool instanciarRainFXSiEsPrefabAsset = true;
    [SerializeField] private bool usarLluviaSimpleVisible = true;
    [SerializeField] private bool crearMaterialLluviaSimple = true;
    [SerializeField] private bool crearFogFXAutomaticoSiFalta = false;
    [SerializeField] private bool configurarFogFXPorCodigo = true;

    [Header("Visual lluvia")]
    [SerializeField] private float rainRateSuave = 520f;
    [SerializeField] private float rainRateTormenta = 1100f;
    [SerializeField] private float rainVelocidadSuave = 14f;
    [SerializeField] private float rainVelocidadTormenta = 22f;
    [SerializeField] private float rainRadio = 18f;
    [SerializeField] private float alturaCajaLluvia = 2.5f;
    [SerializeField] private float tamanoGotaMin = 0.010f;
    [SerializeField] private float tamanoGotaMax = 0.022f;
    [SerializeField] private float largoGota = 1.15f;
    [SerializeField] private Color rainColor = new Color(0.78f, 0.90f, 1f, 0.72f);
    [SerializeField] private bool usarTexturaGotaFina = true;

    [Header("Visual niebla")]
    [SerializeField] private float fogRate = 20f;
    [SerializeField] private float fogRadio = 7f;
    [SerializeField] private Color fogParticleColor = new Color(0.75f, 0.85f, 1f, 0.16f);

    [Header("Modificadores para luz / ambiente")]
    [SerializeField] private Color colorTinteNublado = new Color(0.68f, 0.72f, 0.80f);
    [SerializeField] private Color colorTinteLluvia = new Color(0.50f, 0.60f, 0.76f);
    [SerializeField] private Color colorTinteTormenta = new Color(0.35f, 0.42f, 0.60f);
    [SerializeField] private Color colorTinteNiebla = new Color(0.72f, 0.80f, 0.90f);
    [SerializeField] private Color colorNieblaClima = new Color(0.58f, 0.66f, 0.80f);
    [SerializeField] private Color colorNieblaTormenta = new Color(0.28f, 0.34f, 0.50f);

    [Header("Pesca / bonus opcional")]
    [Tooltip("Valor informativo para otros scripts: 1 = normal, 1.25 = algo mas de rareza, 1.8 = mucha mas rareza.")]
    [SerializeField] private float bonusRarezaPescaLluvia = 1.25f;
    [SerializeField] private float bonusRarezaPescaTormenta = 1.8f;
    [SerializeField] private float bonusRarezaPescaNiebla = 1.15f;

    [Header("Debug / Pruebas")]
    [SerializeField] private bool alwaysRain = false;
    [SerializeField] private EstadoClima climaAlwaysRain = EstadoClima.LluviaSuave;
    [SerializeField] private bool forzarClima = false;
    [SerializeField] private EstadoClima climaForzado = EstadoClima.Soleado;
    [SerializeField] private bool mostrarLogsClima = true;
    [SerializeField] private EstadoClima climaActualDebug;
    [SerializeField] private float intensidadClimaDebug;
    [SerializeField] private bool estaLloviendoDebug;

    private int lastCheckedDay = -1;
    private EstadoClima climaObjetivo;
    private EstadoClima climaActual;
    private float intensidadClima;
    private float targetRainAmount;
    private float currentRainAmount;
    private float targetFogAmount;
    private float currentFogAmount;
    private float baseRainVolume = 1f;
    private float baseStormVolume = 1f;
    private bool shouldRainToday;
    private bool rainActive;
    private bool lastAlwaysRain;
    private bool lastForzarClima;
    private EstadoClima lastClimaForzado;
    private float nextLightningTime;
    private Coroutine lightningRoutine;
    private Material materialLluviaRuntime;
    private Texture2D texturaLluviaRuntime;
    private float targetInteriorOcclusion;
    private float currentInteriorOcclusion;

    public event Action<bool> OnRainChanged;
    public event Action<EstadoClima> OnWeatherChanged;

    public bool EstaLloviendo => rainActive;
    public bool EstaEnTormenta => climaActual == EstadoClima.Tormenta && intensidadClima > 0.15f;
    public bool EstaNublado => climaActual == EstadoClima.Nublado && intensidadClima > 0.15f;
    public bool HayNiebla => climaActual == EstadoClima.Niebla && intensidadClima > 0.15f;
    public EstadoClima ClimaActual => climaActual;
    public float IntensidadVisualClima => intensidadClima;
    public float OclusionLluviaInterior => currentInteriorOcclusion;
    public bool LluviaVisibleExterior => rainActive && currentRainAmount * (1f - currentInteriorOcclusion) > 0.02f;
    public float BonusRarezaPesca => ObtenerBonusRarezaPesca();
    public float MultiplicadorSolClima => Mathf.Lerp(1f, ObtenerMultiplicadorSolObjetivo(climaActual), intensidadClima);
    public float MultiplicadorLunaClima => Mathf.Lerp(1f, ObtenerMultiplicadorLunaObjetivo(climaActual), intensidadClima);
    public float MultiplicadorReflejosClima => Mathf.Lerp(1f, ObtenerMultiplicadorReflejosObjetivo(climaActual), intensidadClima);
    public float MultiplicadorDensidadNieblaClima => Mathf.Lerp(1f, ObtenerMultiplicadorNieblaObjetivo(climaActual), intensidadClima);
    public float DensidadNieblaAdicionalClima => Mathf.Lerp(0f, ObtenerDensidadNieblaAdicionalObjetivo(climaActual), intensidadClima);
    public Color ColorTinteAmbienteClima => Color.Lerp(Color.white, ObtenerTinteAmbienteObjetivo(climaActual), intensidadClima);
    public Color ColorTinteNieblaClima => Color.Lerp(Color.white, ObtenerColorNieblaObjetivo(climaActual), intensidadClima);

    private void Awake()
    {
        if (timeSystem == null)
        {
            timeSystem = FindFirstObjectByType<GameTimeSystem>();
        }

        if (jugador == null && buscarJugadorPorTag)
        {
            BuscarJugador();
        }

        if (rainAudioSource != null)
        {
            baseRainVolume = Mathf.Max(0.01f, rainAudioSource.volume);
            rainAudioSource.volume = 0f;
        }

        if (stormAudioSource != null)
        {
            baseStormVolume = Mathf.Max(0.01f, stormAudioSource.volume);
            stormAudioSource.volume = 0f;
        }

        PrepararAudioTruenoAutomatico();
        PrepararFXAutomaticos();

        if (lightningLight != null)
        {
            lightningLight.enabled = false;
            lightningLight.color = lightningColor;
        }

        lastAlwaysRain = alwaysRain;
        lastForzarClima = forzarClima;
        lastClimaForzado = climaForzado;

        climaActual = climaInicial;
        climaObjetivo = climaInicial;

        UpdateRainDecision(true);
        AplicarClimaObjetivo(ObtenerClimaDeseado(), true);
        ProgramarSiguienteRayo();
    }

    private void OnEnable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnDayChanged += HandleDayChanged;
            timeSystem.OnTimeChanged += HandleTimeChanged;
        }

        UpdateRainDecision(false);
        AplicarClimaObjetivo(ObtenerClimaDeseado(), true);
    }

    private void OnDisable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnDayChanged -= HandleDayChanged;
            timeSystem.OnTimeChanged -= HandleTimeChanged;
        }
    }

    private void Update()
    {
        if (lastAlwaysRain != alwaysRain || lastForzarClima != forzarClima || lastClimaForzado != climaForzado)
        {
            lastAlwaysRain = alwaysRain;
            lastForzarClima = forzarClima;
            lastClimaForzado = climaForzado;
            AplicarClimaObjetivo(ObtenerClimaDeseado(), false);
        }

        if (jugador == null && buscarJugadorPorTag)
        {
            BuscarJugador();
        }

        SeguirJugadorConFX();
        ActualizarTransicionClima();
        ActualizarOcclusionInterior();
        ActualizarFXLluvia();
        ActualizarFXNiebla();
        ActualizarAudio();
        ActualizarRayos();
        ActualizarDebug();
    }

    private void HandleDayChanged(int newDay)
    {
        UpdateRainDecision(true);
        AplicarClimaObjetivo(ObtenerClimaDeseado(), false);
    }

    private void HandleTimeChanged(int day, int hour, int minute)
    {
        if (elegirClimaCadaDia && day != lastCheckedDay)
        {
            UpdateRainDecision(true);
            AplicarClimaObjetivo(ObtenerClimaDeseado(), false);
        }
    }

    private void BuscarJugador()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(tagJugador);
        if (playerObject != null)
        {
            jugador = playerObject.transform;
        }
    }

    private void PrepararAudioTruenoAutomatico()
    {
        if (thunderAudioSource != null)
        {
            ConfigurarAudioSourceTrueno(thunderAudioSource);
            return;
        }

        if (!crearThunderAudioSourceAutomaticoSiFalta)
        {
            return;
        }

        GameObject go = new GameObject("ThunderAudio_Automatico");
        go.transform.SetParent(transform, false);
        thunderAudioSource = go.AddComponent<AudioSource>();
        ConfigurarAudioSourceTrueno(thunderAudioSource);
    }

    private void ConfigurarAudioSourceTrueno(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = false;
        source.volume = Mathf.Clamp01(volumenTrueno);
        source.spatialBlend = truenoEn2D ? 0f : 1f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = 8f;
        source.maxDistance = 80f;
    }

    private void PrepararFXAutomaticos()
    {
        if (rainFX != null && instanciarRainFXSiEsPrefabAsset && !rainFX.gameObject.scene.IsValid())
        {
            ParticleSystem instancia = Instantiate(rainFX, transform);
            instancia.name = rainFX.name + "_Runtime";
            rainFX = instancia;
        }

        if (fogFX != null && instanciarRainFXSiEsPrefabAsset && !fogFX.gameObject.scene.IsValid())
        {
            ParticleSystem instancia = Instantiate(fogFX, transform);
            instancia.name = fogFX.name + "_Runtime";
            fogFX = instancia;
        }

        if (rainFX == null && crearRainFXAutomaticoSiFalta)
        {
            GameObject go = new GameObject("RainFX_Automatico_Visible");
            go.transform.SetParent(transform, false);
            rainFX = go.AddComponent<ParticleSystem>();
        }

        if (fogFX == null && crearFogFXAutomaticoSiFalta)
        {
            GameObject go = new GameObject("FogFX_Automatico");
            go.transform.SetParent(transform, false);
            fogFX = go.AddComponent<ParticleSystem>();
        }

        if (rainFX != null)
        {
            rainFX.gameObject.SetActive(true);
            rainFX.transform.rotation = Quaternion.identity;

            if (configurarRainFXPorCodigo)
            {
                ConfigurarRainFX(rainFX);
            }
        }

        if (fogFX != null && configurarFogFXPorCodigo)
        {
            ConfigurarFogFX(fogFX);
        }
    }

    private void ConfigurarRainFX(ParticleSystem ps)
    {
        ps.transform.localScale = Vector3.one;

        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.85f, 1.35f);
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoGotaMin, tamanoGotaMax);
        main.startColor = rainColor;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = Mathf.Max(2500, Mathf.CeilToInt(rainRateTormenta * 3f));
        main.scalingMode = ParticleSystemScalingMode.Shape;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;

        if (usarLluviaSimpleVisible)
        {
            // Mejor que Circle para lluvia: emite en una caja horizontal X/Z encima del jugador.
            // El Circle a veces crea una cortina vertical y parece que no llueve delante de la cámara.
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(rainRadio * 2f, Mathf.Max(0.5f, alturaCajaLluvia), rainRadio * 2f);
            shape.position = Vector3.zero;
            shape.rotation = Vector3.zero;
        }
        else
        {
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = rainRadio;
            shape.arc = 360f;
        }

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-1.8f, 0.6f);
        velocity.y = new ParticleSystem.MinMaxCurve(-rainVelocidadTormenta, -rainVelocidadSuave);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(rainColor.r, rainColor.g, rainColor.b), 0f),
                new GradientColorKey(new Color(rainColor.r, rainColor.g, rainColor.b), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(rainColor.a, 0.08f),
                new GradientAlphaKey(rainColor.a * 0.85f, 0.82f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = gradient;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Stretch;
        renderer.lengthScale = largoGota;
        renderer.velocityScale = 0.035f;
        renderer.cameraVelocityScale = 0f;
        renderer.sortingFudge = 2f;
        renderer.alignment = ParticleSystemRenderSpace.View;

        if (crearMaterialLluviaSimple)
        {
            Material mat = CrearMaterialLluviaSimple();
            if (mat != null)
            {
                renderer.material = mat;
            }
        }
    }

    private Material CrearMaterialLluviaSimple()
    {
        if (materialLluviaRuntime != null)
        {
            return materialLluviaRuntime;
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
            return null;
        }

        materialLluviaRuntime = new Material(shader);
        materialLluviaRuntime.name = "M_Rain_Runtime_Gota_Fina";
        materialLluviaRuntime.color = rainColor;

        Texture2D texturaGota = usarTexturaGotaFina ? CrearTexturaGotaFina() : null;

        if (texturaGota != null)
        {
            if (materialLluviaRuntime.HasProperty("_BaseMap"))
            {
                materialLluviaRuntime.SetTexture("_BaseMap", texturaGota);
            }

            if (materialLluviaRuntime.HasProperty("_MainTex"))
            {
                materialLluviaRuntime.SetTexture("_MainTex", texturaGota);
            }
        }

        if (materialLluviaRuntime.HasProperty("_BaseColor"))
        {
            materialLluviaRuntime.SetColor("_BaseColor", rainColor);
        }

        if (materialLluviaRuntime.HasProperty("_Color"))
        {
            materialLluviaRuntime.SetColor("_Color", rainColor);
        }

        return materialLluviaRuntime;
    }

    private Texture2D CrearTexturaGotaFina()
    {
        if (texturaLluviaRuntime != null)
        {
            return texturaLluviaRuntime;
        }

        const int ancho = 16;
        const int alto = 64;
        texturaLluviaRuntime = new Texture2D(ancho, alto, TextureFormat.RGBA32, false);
        texturaLluviaRuntime.name = "T_RainDrop_Runtime_Fina";
        texturaLluviaRuntime.wrapMode = TextureWrapMode.Clamp;
        texturaLluviaRuntime.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < alto; y++)
        {
            float ty = (y + 0.5f) / alto;
            float fadeVertical = Mathf.Sin(ty * Mathf.PI);

            for (int x = 0; x < ancho; x++)
            {
                float tx = (x + 0.5f) / ancho;
                float distanciaCentro = Mathf.Abs(tx - 0.5f) * 2f;
                float lineaFina = Mathf.Clamp01(1f - distanciaCentro * 3.8f);
                float alpha = lineaFina * fadeVertical;
                texturaLluviaRuntime.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texturaLluviaRuntime.Apply(false, true);
        return texturaLluviaRuntime;
    }

    private void ConfigurarFogFX(ParticleSystem ps)
    {
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
        main.startSize = new ParticleSystem.MinMaxCurve(1.4f, 3.2f);
        main.startColor = fogParticleColor;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 180;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = fogRadio;
        shape.arc = 360f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);
        velocity.y = new ParticleSystem.MinMaxCurve(0.02f, 0.10f);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.18f, 0.18f);

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.75f, 0.85f, 1f), 0f),
                new GradientColorKey(new Color(0.75f, 0.85f, 1f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(0.16f, 0.25f),
                new GradientAlphaKey(0f, 1f)
            });
        color.color = gradient;

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = -1f;
    }

    private void SeguirJugadorConFX()
    {
        if (!rainFXSigueAlJugador || jugador == null)
        {
            return;
        }

        if (rainFX != null)
        {
            Vector3 posicion = jugador.position + offsetRainFXJugador;
            if (mantenerAlturaRainFX)
            {
                posicion.y = jugador.position.y + offsetRainFXJugador.y;
            }
            rainFX.transform.position = posicion;
            rainFX.transform.rotation = Quaternion.identity;
            rainFX.transform.localScale = Vector3.one;
        }

        if (fogFX != null)
        {
            fogFX.transform.position = jugador.position + offsetFogFXJugador;
        }
    }

    private void UpdateRainDecision(bool force)
    {
        if (timeSystem == null)
        {
            return;
        }

        if (!force && timeSystem.CurrentDay == lastCheckedDay)
        {
            return;
        }

        lastCheckedDay = timeSystem.CurrentDay;

        if (!elegirClimaCadaDia)
        {
            climaObjetivo = climaInicial;
            shouldRainToday = climaObjetivo == EstadoClima.LluviaSuave || climaObjetivo == EstadoClima.Tormenta;
            return;
        }

        if (usarProbabilidadLluviaAntigua)
        {
            shouldRainToday = UnityEngine.Random.value <= rainChancePerDay;
            climaObjetivo = shouldRainToday ? EstadoClima.LluviaSuave : EstadoClima.Soleado;
        }
        else
        {
            climaObjetivo = SortearClimaDelDia();
            shouldRainToday = climaObjetivo == EstadoClima.LluviaSuave || climaObjetivo == EstadoClima.Tormenta;
        }

        if (mostrarLogsClima)
        {
            Debug.Log($"Dia {timeSystem.CurrentDay}: clima = {climaObjetivo}", this);
        }
    }

    private EstadoClima SortearClimaDelDia()
    {
        float nublado = Mathf.Max(0f, probabilidadNublado);
        float lluvia = Mathf.Max(0f, probabilidadLluviaSuave);
        float tormenta = Mathf.Max(0f, probabilidadTormenta);
        float niebla = Mathf.Max(0f, probabilidadNiebla);
        float soleado = Mathf.Max(0.01f, 1f - nublado - lluvia - tormenta - niebla);

        float total = soleado + nublado + lluvia + tormenta + niebla;
        float valor = UnityEngine.Random.Range(0f, total);

        if (valor < soleado) return EstadoClima.Soleado;
        valor -= soleado;
        if (valor < nublado) return EstadoClima.Nublado;
        valor -= nublado;
        if (valor < lluvia) return EstadoClima.LluviaSuave;
        valor -= lluvia;
        if (valor < tormenta) return EstadoClima.Tormenta;
        return EstadoClima.Niebla;
    }

    private EstadoClima ObtenerClimaDeseado()
    {
        if (alwaysRain)
        {
            return climaAlwaysRain;
        }

        if (forzarClima)
        {
            return climaForzado;
        }

        return climaObjetivo;
    }

    private void AplicarClimaObjetivo(EstadoClima nuevoClima, bool inmediato)
    {
        if (climaActual == nuevoClima && !inmediato)
        {
            return;
        }

        EstadoClima climaAnterior = climaActual;
        climaActual = nuevoClima;

        if (inmediato)
        {
            intensidadClima = EsClimaLimpio(climaActual) ? 0f : 1f;
            currentRainAmount = targetRainAmount = ObtenerRainTarget(climaActual);
            currentFogAmount = targetFogAmount = ObtenerFogTarget(climaActual);
        }
        else
        {
            targetRainAmount = ObtenerRainTarget(climaActual);
            targetFogAmount = ObtenerFogTarget(climaActual);
        }

        bool nuevaLluvia = climaActual == EstadoClima.LluviaSuave || climaActual == EstadoClima.Tormenta;
        SetRainFlag(nuevaLluvia);

        if (climaAnterior != climaActual || inmediato)
        {
            OnWeatherChanged?.Invoke(climaActual);
        }
    }

    private void ActualizarTransicionClima()
    {
        float targetIntensity = EsClimaLimpio(climaActual) ? 0f : 1f;
        float duracion = Mathf.Max(0.01f, duracionTransicionClima);
        intensidadClima = Mathf.MoveTowards(intensidadClima, targetIntensity, Time.deltaTime / duracion);

        targetRainAmount = ObtenerRainTarget(climaActual);
        targetFogAmount = ObtenerFogTarget(climaActual);
        currentRainAmount = Mathf.MoveTowards(currentRainAmount, targetRainAmount, Time.deltaTime / duracion);
        currentFogAmount = Mathf.MoveTowards(currentFogAmount, targetFogAmount, Time.deltaTime / duracion);
    }

    private bool EsClimaLimpio(EstadoClima clima)
    {
        return clima == EstadoClima.Soleado;
    }

    private float ObtenerRainTarget(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Tormenta:
                return 1f;
            case EstadoClima.LluviaSuave:
                return 0.55f;
            default:
                return 0f;
        }
    }

    private float ObtenerFogTarget(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Niebla:
                return 1f;
            case EstadoClima.Tormenta:
                return 0.45f;
            case EstadoClima.LluviaSuave:
                return 0.25f;
            case EstadoClima.Nublado:
                return 0.15f;
            default:
                return 0f;
        }
    }

    private void ActualizarOcclusionInterior()
    {
        if (!permitirBloqueoLluviaEnInteriores)
        {
            currentInteriorOcclusion = 0f;
            targetInteriorOcclusion = 0f;
            return;
        }

        float velocidad = Mathf.Max(0.1f, velocidadFadeInterior);
        currentInteriorOcclusion = Mathf.MoveTowards(currentInteriorOcclusion, targetInteriorOcclusion, Time.deltaTime * velocidad);
    }

    private float ObtenerFactorLluviaVisible()
    {
        if (!permitirBloqueoLluviaEnInteriores)
        {
            return 1f;
        }

        return 1f - Mathf.Clamp01(currentInteriorOcclusion);
    }

    private void ActualizarFXLluvia()
    {
        if (rainFX == null)
        {
            return;
        }

        float factorExterior = ObtenerFactorLluviaVisible();
        float rainAmountVisible = currentRainAmount * factorExterior;
        bool visible = rainAmountVisible > 0.015f;

        if (visible && !rainFX.gameObject.activeSelf)
        {
            rainFX.gameObject.SetActive(true);
        }

        if (visible && !rainFX.isPlaying)
        {
            rainFX.Play(true);
        }

        var emission = rainFX.emission;
        float rateTarget = Mathf.Lerp(0f, climaActual == EstadoClima.Tormenta ? rainRateTormenta : rainRateSuave, rainAmountVisible);
        emission.rateOverTime = rateTarget;

        var main = rainFX.main;
        main.startColor = rainColor;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoGotaMin, tamanoGotaMax);
        main.scalingMode = ParticleSystemScalingMode.Shape;

        ParticleSystemRenderer renderer = rainFX.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.lengthScale = largoGota;
            renderer.velocityScale = 0.035f;
            renderer.cameraVelocityScale = 0f;
        }

        var velocity = rainFX.velocityOverLifetime;
        if (velocity.enabled)
        {
            float velocidadMax = climaActual == EstadoClima.Tormenta ? rainVelocidadTormenta : rainVelocidadSuave;
            velocity.y = new ParticleSystem.MinMaxCurve(-velocidadMax, -Mathf.Max(6f, rainVelocidadSuave));
        }

        if (!visible && detenerFXCuandoNoSeVen)
        {
            ParticleSystemStopBehavior stopBehavior = apagarRainFXSiInteriorCompleto && currentInteriorOcclusion > 0.95f
                ? ParticleSystemStopBehavior.StopEmittingAndClear
                : ParticleSystemStopBehavior.StopEmitting;

            rainFX.Stop(true, stopBehavior);
            rainFX.gameObject.SetActive(false);
        }
    }

    private void ActualizarFXNiebla()
    {
        if (fogFX == null)
        {
            return;
        }

        bool visible = currentFogAmount > 0.015f;

        if (visible && !fogFX.gameObject.activeSelf)
        {
            fogFX.gameObject.SetActive(true);
        }

        if (visible && !fogFX.isPlaying)
        {
            fogFX.Play(true);
        }

        var emission = fogFX.emission;
        emission.rateOverTime = Mathf.Lerp(0f, fogRate, currentFogAmount);

        if (!visible && detenerFXCuandoNoSeVen)
        {
            fogFX.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            fogFX.gameObject.SetActive(false);
        }
    }

    private void ActualizarAudio()
    {
        float factorAudioInterior = reducirAudioLluviaEnInterior
            ? Mathf.Lerp(1f, Mathf.Clamp01(volumenLluviaInterior), currentInteriorOcclusion)
            : 1f;

        float factorTormentaInterior = reducirAudioLluviaEnInterior
            ? Mathf.Lerp(1f, Mathf.Clamp01(volumenTormentaInterior), currentInteriorOcclusion)
            : 1f;

        float rainTargetVolume = currentRainAmount > 0.02f ? baseRainVolume * Mathf.Clamp01(currentRainAmount) * factorAudioInterior : 0f;
        float stormTargetVolume = climaActual == EstadoClima.Tormenta ? baseStormVolume * intensidadClima * factorTormentaInterior : 0f;

        if (rainAudioSource != null)
        {
            if (rainTargetVolume > 0.02f && !rainAudioSource.isPlaying)
            {
                rainAudioSource.Play();
            }

            rainAudioSource.volume = Mathf.MoveTowards(rainAudioSource.volume, rainTargetVolume, Time.deltaTime * fadeAudioVelocidad);

            if (rainAudioSource.volume <= 0.01f && rainAudioSource.isPlaying)
            {
                rainAudioSource.Stop();
            }
        }

        if (stormAudioSource != null)
        {
            if (stormTargetVolume > 0.02f && !stormAudioSource.isPlaying)
            {
                stormAudioSource.Play();
            }

            stormAudioSource.volume = Mathf.MoveTowards(stormAudioSource.volume, stormTargetVolume, Time.deltaTime * fadeAudioVelocidad);

            if (stormAudioSource.volume <= 0.01f && stormAudioSource.isPlaying)
            {
                stormAudioSource.Stop();
            }
        }
    }

    private void ActualizarRayos()
    {
        if (climaActual != EstadoClima.Tormenta || intensidadClima < 0.85f)
        {
            return;
        }

        if (bloquearRayosEnInteriorProfundo && currentInteriorOcclusion >= oclusionInteriorParaBloquearRayos)
        {
            return;
        }

        if (Time.time < nextLightningTime)
        {
            return;
        }

        if (lightningRoutine == null)
        {
            lightningRoutine = StartCoroutine(RutinaRayo());
        }
    }

    private IEnumerator RutinaRayo()
    {
        if (lightningLight != null)
        {
            lightningLight.enabled = true;
            lightningLight.color = lightningColor;
            lightningLight.intensity = lightningIntensity;
        }

        yield return new WaitForSeconds(lightningDuration);

        if (lightningLight != null)
        {
            lightningLight.enabled = false;
        }

        if (thunderDelay > 0f)
        {
            yield return new WaitForSeconds(thunderDelay);
        }

        ReproducirTrueno();
        ProgramarSiguienteRayo();
        lightningRoutine = null;
    }

    private void ProgramarSiguienteRayo()
    {
        float min = Mathf.Max(0.5f, minSecondsBetweenLightning);
        float max = Mathf.Max(min, maxSecondsBetweenLightning);
        nextLightningTime = Time.time + UnityEngine.Random.Range(min, max);
    }

    private void ReproducirTrueno()
    {
        if (thunderAudioSource == null)
        {
            PrepararAudioTruenoAutomatico();
        }

        if (thunderAudioSource == null)
        {
            if (mostrarLogsClima)
            {
                Debug.LogWarning("[RainController] No hay Thunder Audio Source. Asigna uno o activa 'Crear Thunder Audio Source Automatico Si Falta'.", this);
            }
            return;
        }

        ConfigurarAudioSourceTrueno(thunderAudioSource);

        if (thunderClips != null && thunderClips.Length > 0)
        {
            AudioClip clip = thunderClips[UnityEngine.Random.Range(0, thunderClips.Length)];
            if (clip != null)
            {
                float factorTruenoInterior = reducirTruenoEnInterior
                    ? Mathf.Lerp(1f, Mathf.Clamp01(volumenTruenoInterior), currentInteriorOcclusion)
                    : 1f;
                thunderAudioSource.PlayOneShot(clip, Mathf.Clamp01(volumenTrueno * factorTruenoInterior));

                if (mostrarLogsClima)
                {
                    Debug.Log($"[RainController] Trueno reproducido: {clip.name}", this);
                }
                return;
            }
        }

        if (thunderAudioSource.clip != null)
        {
            float factorTruenoInterior = reducirTruenoEnInterior
                ? Mathf.Lerp(1f, Mathf.Clamp01(volumenTruenoInterior), currentInteriorOcclusion)
                : 1f;
            thunderAudioSource.volume = Mathf.Clamp01(volumenTrueno * factorTruenoInterior);
            thunderAudioSource.Play();

            if (mostrarLogsClima)
            {
                Debug.Log($"[RainController] Trueno reproducido desde clip del AudioSource: {thunderAudioSource.clip.name}", this);
            }
            return;
        }

        if (mostrarLogsClima)
        {
            Debug.LogWarning("[RainController] Hay Thunder Audio Source, pero no hay ningun clip de trueno asignado.", this);
        }
    }

    private void SetRainFlag(bool active)
    {
        if (rainActive == active)
        {
            return;
        }

        rainActive = active;
        OnRainChanged?.Invoke(rainActive);
    }

    private float ObtenerMultiplicadorSolObjetivo(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Nublado:
                return 0.72f;
            case EstadoClima.LluviaSuave:
                return 0.55f;
            case EstadoClima.Tormenta:
                return 0.34f;
            case EstadoClima.Niebla:
                return 0.63f;
            default:
                return 1f;
        }
    }

    private float ObtenerMultiplicadorLunaObjetivo(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Nublado:
                return 0.75f;
            case EstadoClima.LluviaSuave:
                return 0.55f;
            case EstadoClima.Tormenta:
                return 0.32f;
            case EstadoClima.Niebla:
                return 0.48f;
            default:
                return 1f;
        }
    }

    private float ObtenerMultiplicadorReflejosObjetivo(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Nublado:
                return 0.78f;
            case EstadoClima.LluviaSuave:
                return 0.68f;
            case EstadoClima.Tormenta:
                return 0.48f;
            case EstadoClima.Niebla:
                return 0.58f;
            default:
                return 1f;
        }
    }

    private float ObtenerMultiplicadorNieblaObjetivo(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Nublado:
                return 1.35f;
            case EstadoClima.LluviaSuave:
                return 1.7f;
            case EstadoClima.Tormenta:
                return 2.15f;
            case EstadoClima.Niebla:
                return 3.4f;
            default:
                return 1f;
        }
    }

    private float ObtenerDensidadNieblaAdicionalObjetivo(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Nublado:
                return 0.0008f;
            case EstadoClima.LluviaSuave:
                return 0.0015f;
            case EstadoClima.Tormenta:
                return 0.0030f;
            case EstadoClima.Niebla:
                return 0.0065f;
            default:
                return 0f;
        }
    }

    private Color ObtenerTinteAmbienteObjetivo(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Nublado:
                return colorTinteNublado;
            case EstadoClima.LluviaSuave:
                return colorTinteLluvia;
            case EstadoClima.Tormenta:
                return colorTinteTormenta;
            case EstadoClima.Niebla:
                return colorTinteNiebla;
            default:
                return Color.white;
        }
    }

    private Color ObtenerColorNieblaObjetivo(EstadoClima clima)
    {
        switch (clima)
        {
            case EstadoClima.Tormenta:
                return colorNieblaTormenta;
            case EstadoClima.Nublado:
            case EstadoClima.LluviaSuave:
            case EstadoClima.Niebla:
                return colorNieblaClima;
            default:
                return Color.white;
        }
    }

    private float ObtenerBonusRarezaPesca()
    {
        switch (climaActual)
        {
            case EstadoClima.Tormenta:
                return Mathf.Lerp(1f, bonusRarezaPescaTormenta, intensidadClima);
            case EstadoClima.LluviaSuave:
                return Mathf.Lerp(1f, bonusRarezaPescaLluvia, intensidadClima);
            case EstadoClima.Niebla:
                return Mathf.Lerp(1f, bonusRarezaPescaNiebla, intensidadClima);
            default:
                return 1f;
        }
    }

    private void ActualizarDebug()
    {
        climaActualDebug = climaActual;
        intensidadClimaDebug = intensidadClima;
        estaLloviendoDebug = rainActive;
    }

    public void SetInteriorRainOcclusion(float cantidad)
    {
        if (!permitirBloqueoLluviaEnInteriores)
        {
            return;
        }

        targetInteriorOcclusion = Mathf.Clamp01(cantidad);
    }

    public void SetInteriorRainOcclusionImmediate(float cantidad)
    {
        if (!permitirBloqueoLluviaEnInteriores)
        {
            return;
        }

        targetInteriorOcclusion = Mathf.Clamp01(cantidad);
        currentInteriorOcclusion = targetInteriorOcclusion;
    }

    public void ClearInteriorRainOcclusion()
    {
        targetInteriorOcclusion = 0f;
    }

    public void ConfigurarModoLluviaExteriorFija()
    {
        rainFXSigueAlJugador = false;
        mantenerAlturaRainFX = false;
        rainRadio = Mathf.Max(rainRadio, 35f);
        alturaCajaLluvia = Mathf.Max(alturaCajaLluvia, 4f);

        if (rainFX != null && configurarRainFXPorCodigo)
        {
            ConfigurarRainFX(rainFX);
        }
    }

    public void ForzarClima(EstadoClima nuevoClima)
    {
        forzarClima = true;
        climaForzado = nuevoClima;
        AplicarClimaObjetivo(nuevoClima, false);
    }

    public void QuitarClimaForzado()
    {
        forzarClima = false;
        AplicarClimaObjetivo(ObtenerClimaDeseado(), false);
    }

    [ContextMenu("Debug/Cueva bloquear lluvia")]
    private void DebugCuevaBloquearLluvia()
    {
        SetInteriorRainOcclusionImmediate(1f);
    }

    [ContextMenu("Debug/Cueva permitir lluvia")]
    private void DebugCuevaPermitirLluvia()
    {
        SetInteriorRainOcclusionImmediate(0f);
    }

    [ContextMenu("Debug/Configurar modo lluvia exterior fija")]
    private void DebugConfigurarModoLluviaExteriorFija()
    {
        ConfigurarModoLluviaExteriorFija();
    }

    [ContextMenu("Debug/Reconfigurar Rain FX Visible")]
    private void DebugReconfigurarRainFXVisible()
    {
        if (rainFX != null)
        {
            ConfigurarRainFX(rainFX);
            rainFX.gameObject.SetActive(true);
            rainFX.Play(true);
        }
    }

    [ContextMenu("Debug/Lluvia pequeña y alta")]
    private void DebugLluviaPequenaYAlta()
    {
        offsetRainFXJugador = new Vector3(0f, 14f, 0f);
        tamanoGotaMin = 0.010f;
        tamanoGotaMax = 0.022f;
        largoGota = 1.15f;
        alturaCajaLluvia = 2.5f;
        rainRadio = 18f;

        if (rainFX != null)
        {
            ConfigurarRainFX(rainFX);
            rainFX.gameObject.SetActive(true);
            rainFX.Play(true);
        }
    }

    [ContextMenu("Debug/Probar trueno ahora")]
    private void DebugProbarTruenoAhora()
    {
        ReproducirTrueno();
    }

    [ContextMenu("Debug/Probar rayo + trueno")]
    private void DebugProbarRayoYTrueno()
    {
        if (lightningRoutine == null)
        {
            lightningRoutine = StartCoroutine(RutinaRayo());
        }
    }

    [ContextMenu("Debug/Clima Soleado")]
    private void DebugClimaSoleado() => ForzarClima(EstadoClima.Soleado);

    [ContextMenu("Debug/Clima Nublado")]
    private void DebugClimaNublado() => ForzarClima(EstadoClima.Nublado);

    [ContextMenu("Debug/Clima Lluvia Suave")]
    private void DebugClimaLluviaSuave() => ForzarClima(EstadoClima.LluviaSuave);

    [ContextMenu("Debug/Clima Tormenta")]
    private void DebugClimaTormenta() => ForzarClima(EstadoClima.Tormenta);

    [ContextMenu("Debug/Clima Niebla")]
    private void DebugClimaNiebla() => ForzarClima(EstadoClima.Niebla);

    [ContextMenu("Debug/Quitar Clima Forzado")]
    private void DebugQuitarClimaForzado() => QuitarClimaForzado();

    [ContextMenu("Debug/Probar lluvia")]
    private void DebugStartRain()
    {
        ForzarClima(EstadoClima.LluviaSuave);
    }

    [ContextMenu("Debug/Parar lluvia")]
    private void DebugStopRain()
    {
        ForzarClima(EstadoClima.Soleado);
    }

    [ContextMenu("Debug/Sortear clima hoy")]
    private void DebugRollWeatherToday()
    {
        climaObjetivo = SortearClimaDelDia();
        shouldRainToday = climaObjetivo == EstadoClima.LluviaSuave || climaObjetivo == EstadoClima.Tormenta;
        AplicarClimaObjetivo(climaObjetivo, false);
        Debug.Log($"Sorteo manual clima hoy: {climaObjetivo}", this);
    }
}
