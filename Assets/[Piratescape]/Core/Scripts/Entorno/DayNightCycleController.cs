using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycleController : MonoBehaviour
{
    private enum MomentoDia
    {
        Dia,
        Atardecer,
        Noche
    }

    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private GhostSpawn ghostSpawn;
    [SerializeField] private RainController rainController;

    [SerializeField] private GameObject audioDia;
    [SerializeField] private GameObject audioNoche;

    [Header("Actores nocturnos")]
    [SerializeField] private NightMonkeySpawn[] monosNocturnos;
    [SerializeField] private NightTreeClimberMonkeySequence secuenciaMonosEscaladores;
    [Header("Mensaje al caer la noche")]
    [SerializeField]
    private string mensajeInicioNoche =
    "La noche está cayendo... será peligroso permanecer fuera.";

    [Header("Skyboxes 6 Sided")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material sunsetSkybox;
    [SerializeField] private Material nightSkybox;

    [Header("Material de mezcla")]
    [SerializeField] private Material blendSkyboxMaterial;
    [SerializeField] private float duracionTransicionSkybox = 70f;
    [SerializeField] private float skyboxExposure = 1f;

    [Header("Horas")]
    [SerializeField] private float dayHour = 8f;
    [SerializeField] private float sunsetHour = 17.5f;
    [SerializeField] private float nightHour = 18f;

    [Header("Reflejos del skybox")]
    [SerializeField] private float dayReflectionIntensity = 0.75f;
    [SerializeField] private float sunsetReflectionIntensity = 0.42f;
    [SerializeField] private float nightReflectionIntensity = 0.42f;

    [Header("Colores ambiente")]
    [SerializeField] private Color dayAmbientColor = new Color(0.64f, 0.68f, 0.72f);
    [SerializeField] private Color sunsetAmbientColor = new Color(0.48f, 0.38f, 0.40f);
    [SerializeField] private Color nightAmbientColor = new Color(0.18f, 0.24f, 0.36f);

    [Header("Color del sol")]
    [SerializeField] private Color daySunColor = new Color(0.94f, 0.97f, 1.00f);
    [SerializeField] private Color sunsetSunColor = new Color(0.95f, 0.55f, 0.45f);
    [SerializeField] private Color duskSunColor = new Color(0.56f, 0.34f, 0.48f);

    [Header("Color de la luna")]
    [SerializeField] private Color moonColor = new Color(0.64f, 0.78f, 1.00f);

    [Header("Intensidades del sol")]
    [SerializeField] private float daySunIntensity = 0.76f;
    [SerializeField] private float sunsetSunIntensity = 0.52f;
    [SerializeField] private float nightSunIntensity = 0f;

    [Header("Intensidades de la luna")]
    [SerializeField] private float dayMoonIntensity = 0f;
    [SerializeField] private float sunsetMoonIntensity = 0.05f;
    [SerializeField] private float nightMoonIntensity = 0.78f;

    [Header("Niebla")]
   [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;

    [Header("Colores de niebla")]
    [SerializeField] private Color dayFogColor = new Color(0.96f, 0.84f, 0.80f);
    [SerializeField] private Color sunsetFogColor = new Color(0.78f, 0.50f, 0.48f);
    [SerializeField] private Color nightFogColor = new Color(0.10f, 0.16f, 0.30f);

    [Header("Densidad de niebla")]
    [SerializeField] private float dayFogDensity = 0.008f;
    [SerializeField] private float sunsetFogDensity = 0.0030f;
    [SerializeField] private float nightFogDensity = 0.0048f;

    [Header("Rotación")]
    [SerializeField] private float sunYaw = 170f;
    [SerializeField] private float moonYaw = 170f;

    [Header("Debug")]
    [SerializeField] private string skyboxActualNombre;
    [SerializeField] private string skyboxDestinoNombre;
    [SerializeField] private float blendActual;
    [SerializeField] private string momentoActualNombre;
    [SerializeField] private string momentoDestinoNombre;

    private Material runtimeBlendSkybox;

    private Material skyboxActual;
    private Material skyboxDestino;

    private MomentoDia momentoActual;
    private MomentoDia momentoDestino;

    private float tiempoTransicionSkybox;
    private bool transicionSkyboxActiva;
    private bool eraDeNoche;

    private void Reset()
    {
        fogMode = FogMode.ExponentialSquared;
    }

    private void OnValidate()
    {
        fogMode = FogMode.ExponentialSquared;

        dayFogDensity = Mathf.Max(0f, dayFogDensity);
        sunsetFogDensity = Mathf.Max(0f, sunsetFogDensity);
        nightFogDensity = Mathf.Max(0f, nightFogDensity);
    }

    private void Awake()
    {
        ForzarNieblaActiva();
        RenderSettings.ambientMode = AmbientMode.Flat;

        if (rainController == null)
        {
            rainController = FindFirstObjectByType<RainController>();
        }
    }

    private void OnEnable()
    {
        ForzarNieblaActiva();

        if (rainController != null)
        {
            rainController.OnRainChanged += AlCambiarLluvia;
        }
    }
    private void OnDisable()
    {
        if (rainController != null)
        {
            rainController.OnRainChanged -= AlCambiarLluvia;
        }
    }

    private void Start()
    {
        CrearSkyboxInicial();

        if (timeSystem != null)
        {
            float hour = timeSystem.CurrentHour + (timeSystem.CurrentMinute / 60f);
            eraDeNoche = EsHoraDeNoche(hour);

            if (eraDeNoche)
            {
                EmpezarNocheActores();
            }
            else
            {
                EmpezarDiaActores();
            }
        }

        ActualizarCiclo();
    }

    private void Update()
    {
        ActualizarCiclo();
    }

    private void ForzarNieblaActiva()
    {
        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;
    }

    private void CrearSkyboxInicial()
    {
        if (daySkybox == null || sunsetSkybox == null || nightSkybox == null || blendSkyboxMaterial == null)
        {
            Debug.LogWarning("DayNightCycleController: falta asignar algún skybox o el material de mezcla.");
            RenderSettings.skybox = null;
            DynamicGI.UpdateEnvironment();
            return;
        }

        float hour = 8f;

        if (timeSystem != null)
        {
            hour = timeSystem.CurrentHour + (timeSystem.CurrentMinute / 60f);
        }

        momentoActual = ObtenerMomentoObjetivo(hour);
        momentoDestino = momentoActual;

        skyboxActual = ObtenerSkyboxPorMomento(momentoActual);
        skyboxDestino = skyboxActual;

        runtimeBlendSkybox = new Material(blendSkyboxMaterial);

        CopiarSkyboxA(skyboxActual);
        CopiarSkyboxB(skyboxActual);

        runtimeBlendSkybox.SetFloat("_Blend", 0f);
        runtimeBlendSkybox.SetFloat("_Exposure", skyboxExposure);
        runtimeBlendSkybox.SetColor("_Tint", Color.white);

        RenderSettings.skybox = runtimeBlendSkybox;
        DynamicGI.UpdateEnvironment();

        ActualizarDebug(0f);
    }

    private void ActualizarCiclo()
    {
        if (timeSystem == null)
        {
            return;
        }

        float hour = timeSystem.CurrentHour + (timeSystem.CurrentMinute / 60f);

        bool esDeNoche = EsHoraDeNoche(hour);

        if (esDeNoche && !eraDeNoche)
        {
            EmpezarNocheActores();
            MostrarMensajeInicioNoche();

        }

        if (!esDeNoche && eraDeNoche)
        {
            EmpezarDiaActores();
        }

        eraDeNoche = esDeNoche;

        ActualizarSkybox(hour);
        ActualizarAmbiente();
        ActualizarSol(hour);
        ActualizarLuna(hour);
        ActualizarNiebla();
    }

    private void EmpezarNocheActores()
    {
        if (ghostSpawn != null)
        {
            ghostSpawn.EmpezarNoche();
        }

        if (monosNocturnos != null)
        {
            for (int i = 0; i < monosNocturnos.Length; i++)
            {
                if (monosNocturnos[i] != null)
                {
                    monosNocturnos[i].EmpezarNoche();
                }
            }
        }

        if (secuenciaMonosEscaladores != null)
        {
            secuenciaMonosEscaladores.EmpezarNoche();
        }
        if (audioNoche != null)
        {
            audioNoche.SetActive(true);
        }

        if (audioDia != null)
        {
            audioDia.SetActive(false);
        }
    }

    private void EmpezarDiaActores()
    {
        if (ghostSpawn != null)
        {
            ghostSpawn.EmpezarDia();
        }

        if (monosNocturnos != null)
        {
            for (int i = 0; i < monosNocturnos.Length; i++)
            {
                if (monosNocturnos[i] != null)
                {
                    monosNocturnos[i].EmpezarDia();
                }
            }
        }

        if (secuenciaMonosEscaladores != null)
        {
            secuenciaMonosEscaladores.EmpezarDia();
        }

        if (audioNoche != null)
        {
            audioNoche.SetActive(false);
        }

        ActualizarAudioDia();
    }

    private void ActualizarSkybox(float hour)
    {
        if (daySkybox == null || sunsetSkybox == null || nightSkybox == null || blendSkyboxMaterial == null)
        {
            RenderSettings.skybox = null;
            return;
        }

        if (runtimeBlendSkybox == null)
        {
            CrearSkyboxInicial();
            return;
        }

        MomentoDia nuevoMomentoDestino = ObtenerMomentoObjetivo(hour);
        Material nuevoSkyboxDestino = ObtenerSkyboxPorMomento(nuevoMomentoDestino);

        if (!transicionSkyboxActiva && nuevoMomentoDestino != momentoActual)
        {
            IniciarTransicionSkybox(nuevoMomentoDestino, nuevoSkyboxDestino);
        }

        if (transicionSkyboxActiva)
        {
            ActualizarTransicionSkybox();
        }
    }

    private void IniciarTransicionSkybox(MomentoDia nuevoMomentoDestino, Material nuevoSkyboxDestino)
    {
        if (nuevoSkyboxDestino == null || runtimeBlendSkybox == null)
        {
            return;
        }

        momentoDestino = nuevoMomentoDestino;
        skyboxDestino = nuevoSkyboxDestino;

        Debug.Log("Cambiando skybox: " + skyboxActual.name + " -> " + skyboxDestino.name);

        CopiarSkyboxA(skyboxActual);
        CopiarSkyboxB(skyboxDestino);

        runtimeBlendSkybox.SetFloat("_Blend", 0f);
        runtimeBlendSkybox.SetFloat("_Exposure", skyboxExposure);
        runtimeBlendSkybox.SetColor("_Tint", Color.white);

        tiempoTransicionSkybox = 0f;
        transicionSkyboxActiva = true;

        RenderSettings.skybox = runtimeBlendSkybox;
        DynamicGI.UpdateEnvironment();

        ActualizarDebug(0f);
    }

    private void ActualizarTransicionSkybox()
    {
        tiempoTransicionSkybox += Time.deltaTime;

        float t = 1f;

        if (duracionTransicionSkybox > 0f)
        {
            t = Mathf.Clamp01(tiempoTransicionSkybox / duracionTransicionSkybox);
        }

        float tSuave = Mathf.SmoothStep(0f, 1f, t);

        runtimeBlendSkybox.SetFloat("_Blend", tSuave);
        runtimeBlendSkybox.SetFloat("_Exposure", skyboxExposure);

        RenderSettings.skybox = runtimeBlendSkybox;

        ActualizarDebug(tSuave);

        if (t >= 1f)
        {
            FinalizarTransicionSkybox();
        }
    }

    private void FinalizarTransicionSkybox()
    {
        skyboxActual = skyboxDestino;
        momentoActual = momentoDestino;

        CopiarSkyboxA(skyboxActual);
        CopiarSkyboxB(skyboxActual);

        runtimeBlendSkybox.SetFloat("_Blend", 0f);
        runtimeBlendSkybox.SetFloat("_Exposure", skyboxExposure);
        runtimeBlendSkybox.SetColor("_Tint", Color.white);

        transicionSkyboxActiva = false;

        RenderSettings.skybox = runtimeBlendSkybox;
        DynamicGI.UpdateEnvironment();

        Debug.Log("Skybox final: " + skyboxActual.name);

        ActualizarDebug(0f);
    }

    private MomentoDia ObtenerMomentoObjetivo(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return MomentoDia.Noche;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            return MomentoDia.Atardecer;
        }

        return MomentoDia.Dia;
    }

    private Material ObtenerSkyboxPorMomento(MomentoDia momento)
    {
        switch (momento)
        {
            case MomentoDia.Noche:
                return nightSkybox;

            case MomentoDia.Atardecer:
                return sunsetSkybox;

            default:
                return daySkybox;
        }
    }

    private bool EsHoraDeNoche(float hour)
    {
        return hour >= nightHour || hour < dayHour;
    }

    private void CopiarSkyboxA(Material source)
    {
        CopiarTextura(source, "_FrontTex", "_FrontTexA");
        CopiarTextura(source, "_BackTex", "_BackTexA");
        CopiarTextura(source, "_LeftTex", "_LeftTexA");
        CopiarTextura(source, "_RightTex", "_RightTexA");
        CopiarTextura(source, "_UpTex", "_UpTexA");
        CopiarTextura(source, "_DownTex", "_DownTexA");
    }

    private void CopiarSkyboxB(Material source)
    {
        CopiarTextura(source, "_FrontTex", "_FrontTexB");
        CopiarTextura(source, "_BackTex", "_BackTexB");
        CopiarTextura(source, "_LeftTex", "_LeftTexB");
        CopiarTextura(source, "_RightTex", "_RightTexB");
        CopiarTextura(source, "_UpTex", "_UpTexB");
        CopiarTextura(source, "_DownTex", "_DownTexB");
    }

    private void CopiarTextura(Material source, string sourceProperty, string targetProperty)
    {
        if (source == null || runtimeBlendSkybox == null)
        {
            return;
        }

        if (!source.HasProperty(sourceProperty))
        {
            Debug.LogWarning("El material " + source.name + " no tiene la propiedad " + sourceProperty + ". Debe usar Skybox/6 Sided.", source);
            return;
        }

        if (!runtimeBlendSkybox.HasProperty(targetProperty))
        {
            Debug.LogWarning("El material de mezcla no tiene la propiedad " + targetProperty + ".", runtimeBlendSkybox);
            return;
        }

        Texture texture = source.GetTexture(sourceProperty);

        if (texture == null)
        {
            Debug.LogWarning("La textura " + sourceProperty + " de " + source.name + " está vacía.", source);
        }

        runtimeBlendSkybox.SetTexture(targetProperty, texture);
    }

    private void ActualizarDebug(float blend)
    {
        skyboxActualNombre = skyboxActual != null ? skyboxActual.name : "NULL";
        skyboxDestinoNombre = skyboxDestino != null ? skyboxDestino.name : "NULL";
        blendActual = blend;

        momentoActualNombre = momentoActual.ToString();
        momentoDestinoNombre = momentoDestino.ToString();
    }

    private float ObtenerBlendVisual()
    {
        if (!transicionSkyboxActiva)
        {
            return 0f;
        }

        return blendActual;
    }

    private Color ObtenerColorAmbienteVisual()
    {
        Color colorActual = ObtenerColorAmbientePorMomento(momentoActual);

        if (!transicionSkyboxActiva)
        {
            return colorActual;
        }

        Color colorDestino = ObtenerColorAmbientePorMomento(momentoDestino);
        return Color.Lerp(colorActual, colorDestino, ObtenerBlendVisual());
    }

    private float ObtenerReflejosVisual()
    {
        float valorActual = ObtenerReflejosPorMomento(momentoActual);

        if (!transicionSkyboxActiva)
        {
            return valorActual;
        }

        float valorDestino = ObtenerReflejosPorMomento(momentoDestino);
        return Mathf.Lerp(valorActual, valorDestino, ObtenerBlendVisual());
    }

    private Color ObtenerColorSolVisual()
    {
        Color colorActual = ObtenerColorSolPorMomento(momentoActual);

        if (!transicionSkyboxActiva)
        {
            return colorActual;
        }

        Color colorDestino = ObtenerColorSolPorMomento(momentoDestino);
        return Color.Lerp(colorActual, colorDestino, ObtenerBlendVisual());
    }

    private float ObtenerIntensidadSolVisual()
    {
        float intensidadActual = ObtenerIntensidadSolPorMomento(momentoActual);

        if (!transicionSkyboxActiva)
        {
            return intensidadActual;
        }

        float intensidadDestino = ObtenerIntensidadSolPorMomento(momentoDestino);
        return Mathf.Lerp(intensidadActual, intensidadDestino, ObtenerBlendVisual());
    }

    private float ObtenerIntensidadLunaVisual()
    {
        float intensidadActual = ObtenerIntensidadLunaPorMomento(momentoActual);

        if (!transicionSkyboxActiva)
        {
            return intensidadActual;
        }

        float intensidadDestino = ObtenerIntensidadLunaPorMomento(momentoDestino);
        return Mathf.Lerp(intensidadActual, intensidadDestino, ObtenerBlendVisual());
    }

    private Color ObtenerColorNieblaVisual()
    {
        Color colorActual = ObtenerColorNieblaPorMomento(momentoActual);

        if (!transicionSkyboxActiva)
        {
            return colorActual;
        }

        Color colorDestino = ObtenerColorNieblaPorMomento(momentoDestino);
        return Color.Lerp(colorActual, colorDestino, ObtenerBlendVisual());
    }

    private float ObtenerDensidadNieblaVisual()
    {
        float densidadActual = ObtenerDensidadNieblaPorMomento(momentoActual);

        if (!transicionSkyboxActiva)
        {
            return densidadActual;
        }

        float densidadDestino = ObtenerDensidadNieblaPorMomento(momentoDestino);
        return Mathf.Lerp(densidadActual, densidadDestino, ObtenerBlendVisual());
    }

    private void ActualizarAmbiente()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ObtenerColorAmbienteVisual();
        RenderSettings.reflectionIntensity = ObtenerReflejosVisual();
    }

    private void ActualizarSol(float hour)
    {
        if (sunLight == null)
        {
            return;
        }

        sunLight.intensity = ObtenerIntensidadSolVisual();
        sunLight.color = ObtenerColorSolVisual();
        sunLight.enabled = sunLight.intensity > 0.001f;

        float anguloX = ObtenerAnguloSol(hour);
        sunLight.transform.rotation = Quaternion.Euler(anguloX, sunYaw, 0f);
    }

    private void ActualizarLuna(float hour)
    {
        if (moonLight == null)
        {
            return;
        }

        moonLight.intensity = ObtenerIntensidadLunaVisual();
        moonLight.color = moonColor;
        moonLight.enabled = moonLight.intensity > 0.001f;

        float anguloX = ObtenerAnguloLuna(hour);
        moonLight.transform.rotation = Quaternion.Euler(anguloX, moonYaw, 0f);
    }

    private void ActualizarNiebla()
    {

        RenderSettings.fog = true;
        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = ObtenerColorNieblaVisual();
        RenderSettings.fogDensity = ObtenerDensidadNieblaVisual();
    }

    private Color ObtenerColorAmbientePorMomento(MomentoDia momento)
    {
        switch (momento)
        {
            case MomentoDia.Noche:
                return nightAmbientColor;

            case MomentoDia.Atardecer:
                return sunsetAmbientColor;

            default:
                return dayAmbientColor;
        }
    }

    private float ObtenerReflejosPorMomento(MomentoDia momento)
    {
        switch (momento)
        {
            case MomentoDia.Noche:
                return nightReflectionIntensity;

            case MomentoDia.Atardecer:
                return sunsetReflectionIntensity;

            default:
                return dayReflectionIntensity;
        }
    }

    private Color ObtenerColorSolPorMomento(MomentoDia momento)
    {
        switch (momento)
        {
            case MomentoDia.Noche:
                return duskSunColor;

            case MomentoDia.Atardecer:
                return sunsetSunColor;

            default:
                return daySunColor;
        }
    }

    private float ObtenerIntensidadSolPorMomento(MomentoDia momento)
    {
        switch (momento)
        {
            case MomentoDia.Noche:
                return nightSunIntensity;

            case MomentoDia.Atardecer:
                return sunsetSunIntensity;

            default:
                return daySunIntensity;
        }
    }

    private float ObtenerIntensidadLunaPorMomento(MomentoDia momento)
    {
        switch (momento)
        {
            case MomentoDia.Noche:
                return nightMoonIntensity;

            case MomentoDia.Atardecer:
                return sunsetMoonIntensity;

            default:
                return dayMoonIntensity;
        }
    }

    private Color ObtenerColorNieblaPorMomento(MomentoDia momento)
    {
        switch (momento)
        {
            case MomentoDia.Noche:
                return nightFogColor;

            case MomentoDia.Atardecer:
                return sunsetFogColor;

            default:
                return dayFogColor;
        }
    }

    private float ObtenerDensidadNieblaPorMomento(MomentoDia momento)
    {
        switch (momento)
        {
            case MomentoDia.Noche:
                return nightFogDensity;

            case MomentoDia.Atardecer:
                return sunsetFogDensity;

            default:
                return dayFogDensity;
        }
    }

    private float ObtenerAnguloSol(float hour)
    {
        if (hour >= dayHour && hour < sunsetHour)
        {
            float t = Mathf.InverseLerp(dayHour, sunsetHour, hour);
            return Mathf.Lerp(28f, 72f, t);
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Mathf.InverseLerp(sunsetHour, nightHour, hour);
            return Mathf.Lerp(72f, 175f, t);
        }

        return 200f;
    }

    private float ObtenerAnguloLuna(float hour)
    {
        float adjustedHour = hour;

        if (adjustedHour < dayHour)
        {
            adjustedHour += 24f;
        }

        float moonStart = nightHour;
        float moonEnd = dayHour + 24f;

        if (adjustedHour >= moonStart && adjustedHour <= moonEnd)
        {
            float t = Mathf.InverseLerp(moonStart, moonEnd, adjustedHour);
            return Mathf.Lerp(55f, 135f, t);
        }

        return 200f;
    }
    private void MostrarMensajeInicioNoche()
    {
        if (NightMessageUI.Instance != null)
        {
            NightMessageUI.Instance.ShowMessage(mensajeInicioNoche);
        }
        else
        {
            Debug.Log("Mensaje inicio noche: " + mensajeInicioNoche);
        }
    }
    private void AlCambiarLluvia(bool estaLloviendo)
    {
        ActualizarAudioDia();
    }

    private void ActualizarAudioDia()
    {
        if (audioDia == null)
        {
            return;
        }

        bool activar = !eraDeNoche &&
                        (rainController == null || !rainController.EstaLloviendo);

        audioDia.SetActive(activar);
    }
}
