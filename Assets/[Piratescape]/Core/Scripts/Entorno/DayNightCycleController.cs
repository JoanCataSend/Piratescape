using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycleController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private GhostSpawn ghostSpawn;

    [Header("Actores nocturnos")]
    [SerializeField] private NightMonkeySpawn[] monosNocturnos;
    [SerializeField] private NightTreeClimberMonkeySequence secuenciaMonosEscaladores;

    [Header("Skyboxes 6 Sided")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material sunsetSkybox;
    [SerializeField] private Material nightSkybox;

    [Header("Material de mezcla")]
    [SerializeField] private Material blendSkyboxMaterial;
    [SerializeField] private float duracionTransicionSkybox = 40f;
    [SerializeField] private float skyboxExposure = 1f;

    [Header("Horas")]
    [SerializeField] private float dayHour = 6f;
    [SerializeField] private float sunsetHour = 18f;
    [SerializeField] private float nightHour = 22f;

    [Header("Reflejos del skybox")]
    [SerializeField] private float dayReflectionIntensity = 1f;
    [SerializeField] private float sunsetReflectionIntensity = 0.35f;
    [SerializeField] private float nightReflectionIntensity = 0.03f;

    [Header("Colores ambiente")]
    [SerializeField] private Color dayAmbientColor = new Color(0.50f, 0.53f, 0.55f);
    [SerializeField] private Color sunsetAmbientColor = new Color(0.30f, 0.20f, 0.18f);
    [SerializeField] private Color nightAmbientColor = new Color(0.003f, 0.005f, 0.015f);

    [Header("Color del sol")]
    [SerializeField] private Color daySunColor = new Color(1.00f, 0.86f, 0.62f);
    [SerializeField] private Color sunsetSunColor = new Color(1.00f, 0.42f, 0.22f);
    [SerializeField] private Color duskSunColor = new Color(0.45f, 0.22f, 0.25f);

    [Header("Color de la luna")]
    [SerializeField] private Color moonColor = new Color(0.35f, 0.45f, 0.95f);

    [Header("Intensidades del sol")]
    [SerializeField] private float daySunIntensity = 0.55f;
    [SerializeField] private float sunsetSunIntensity = 0.35f;
    [SerializeField] private float nightSunIntensity = 0f;

    [Header("Intensidades de la luna")]
    [SerializeField] private float dayMoonIntensity = 0f;
    [SerializeField] private float sunsetMoonIntensity = 0.03f;
    [SerializeField] private float nightMoonIntensity = 0.025f;

    [Header("Niebla")]
    [SerializeField] private bool usarNiebla = true;
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;

    [Header("Colores de niebla")]
    [SerializeField] private Color dayFogColor = new Color(0.78f, 0.86f, 0.90f);
    [SerializeField] private Color sunsetFogColor = new Color(0.95f, 0.55f, 0.42f);
    [SerializeField] private Color nightFogColor = new Color(0.04f, 0.06f, 0.13f);

    [Header("Densidad de niebla")]
    [SerializeField] private float dayFogDensity = 0.004f;
    [SerializeField] private float sunsetFogDensity = 0.009f;
    [SerializeField] private float nightFogDensity = 0.015f;

    [Header("Rotación")]
    [SerializeField] private float sunYaw = 170f;
    [SerializeField] private float moonYaw = 170f;

    [Header("Debug")]
    [SerializeField] private string skyboxActualNombre;
    [SerializeField] private string skyboxDestinoNombre;
    [SerializeField] private float blendActual;

    private Material runtimeBlendSkybox;

    private Material skyboxActual;
    private Material skyboxDestino;

    private float tiempoTransicionSkybox;
    private bool transicionSkyboxActiva;
    private bool eraDeNoche;

    private void Awake()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.fog = usarNiebla;
        RenderSettings.fogMode = fogMode;
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

        skyboxActual = ObtenerSkyboxObjetivo(hour);
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
        }

        if (!esDeNoche && eraDeNoche)
        {
            EmpezarDiaActores();
        }

        eraDeNoche = esDeNoche;

        ActualizarSkybox(hour);
        ActualizarAmbiente(hour);
        ActualizarSol(hour);
        ActualizarLuna(hour);
        ActualizarNiebla(hour);
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

        Material nuevoDestino = ObtenerSkyboxObjetivo(hour);

        if (!transicionSkyboxActiva && nuevoDestino != skyboxActual)
        {
            IniciarTransicionSkybox(nuevoDestino);
        }

        if (transicionSkyboxActiva)
        {
            ActualizarTransicionSkybox();
        }
    }

    private void IniciarTransicionSkybox(Material nuevoDestino)
    {
        if (nuevoDestino == null || runtimeBlendSkybox == null)
        {
            return;
        }

        skyboxDestino = nuevoDestino;

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

    private Material ObtenerSkyboxObjetivo(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return nightSkybox;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            return sunsetSkybox;
        }

        return daySkybox;
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
    }

    private void ActualizarAmbiente(float hour)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ObtenerColorAmbiente(hour);
        RenderSettings.reflectionIntensity = ObtenerIntensidadReflejos(hour);
    }

    private void ActualizarSol(float hour)
    {
        if (sunLight == null)
        {
            return;
        }

        sunLight.intensity = ObtenerIntensidadSol(hour);
        sunLight.color = ObtenerColorSol(hour);
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

        moonLight.intensity = ObtenerIntensidadLuna(hour);
        moonLight.color = moonColor;
        moonLight.enabled = moonLight.intensity > 0.001f;

        float anguloX = ObtenerAnguloLuna(hour);
        moonLight.transform.rotation = Quaternion.Euler(anguloX, moonYaw, 0f);
    }

    private void ActualizarNiebla(float hour)
    {
        RenderSettings.fog = usarNiebla;

        if (!usarNiebla)
        {
            return;
        }

        RenderSettings.fogMode = fogMode;
        RenderSettings.fogColor = ObtenerColorNiebla(hour);
        RenderSettings.fogDensity = ObtenerDensidadNiebla(hour);
    }

    private float ObtenerIntensidadReflejos(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return nightReflectionIntensity;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Suavizar(sunsetHour, nightHour, hour);
            return Mathf.Lerp(sunsetReflectionIntensity, nightReflectionIntensity, t);
        }

        float tDia = Suavizar(dayHour, sunsetHour, hour);
        return Mathf.Lerp(dayReflectionIntensity, sunsetReflectionIntensity, tDia);
    }

    private Color ObtenerColorAmbiente(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return nightAmbientColor;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Suavizar(sunsetHour, nightHour, hour);
            return Color.Lerp(sunsetAmbientColor, nightAmbientColor, t);
        }

        float tDia = Suavizar(dayHour, sunsetHour, hour);
        return Color.Lerp(dayAmbientColor, sunsetAmbientColor, tDia);
    }

    private float ObtenerIntensidadSol(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return nightSunIntensity;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Suavizar(sunsetHour, nightHour, hour);
            return Mathf.Lerp(sunsetSunIntensity, nightSunIntensity, t);
        }

        float tDia = Suavizar(dayHour, sunsetHour, hour);
        return Mathf.Lerp(daySunIntensity, sunsetSunIntensity, tDia);
    }

    private Color ObtenerColorSol(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return duskSunColor;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Suavizar(sunsetHour, nightHour, hour);
            return Color.Lerp(sunsetSunColor, duskSunColor, t);
        }

        float tDia = Suavizar(dayHour, sunsetHour, hour);
        return Color.Lerp(daySunColor, sunsetSunColor, tDia);
    }

    private float ObtenerIntensidadLuna(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return nightMoonIntensity;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Suavizar(sunsetHour, nightHour, hour);
            return Mathf.Lerp(sunsetMoonIntensity, nightMoonIntensity, t);
        }

        return dayMoonIntensity;
    }

    private Color ObtenerColorNiebla(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return nightFogColor;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Suavizar(sunsetHour, nightHour, hour);
            return Color.Lerp(sunsetFogColor, nightFogColor, t);
        }

        float tDia = Suavizar(dayHour, sunsetHour, hour);
        return Color.Lerp(dayFogColor, sunsetFogColor, tDia);
    }

    private float ObtenerDensidadNiebla(float hour)
    {
        if (EsHoraDeNoche(hour))
        {
            return nightFogDensity;
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Suavizar(sunsetHour, nightHour, hour);
            return Mathf.Lerp(sunsetFogDensity, nightFogDensity, t);
        }

        float tDia = Suavizar(dayHour, sunsetHour, hour);
        return Mathf.Lerp(dayFogDensity, sunsetFogDensity, tDia);
    }

    private float ObtenerAnguloSol(float hour)
    {
        if (hour >= dayHour && hour < sunsetHour)
        {
            float t = Mathf.InverseLerp(dayHour, sunsetHour, hour);
            return Mathf.Lerp(-5f, 120f, t);
        }

        if (hour >= sunsetHour && hour < nightHour)
        {
            float t = Mathf.InverseLerp(sunsetHour, nightHour, hour);
            return Mathf.Lerp(120f, 200f, t);
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
            return Mathf.Lerp(-5f, 200f, t);
        }

        return 200f;
    }

    private float Suavizar(float inicio, float fin, float valor)
    {
        float t = Mathf.InverseLerp(inicio, fin, valor);
        return Mathf.SmoothStep(0f, 1f, t);
    }
}