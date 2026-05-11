using UnityEngine;
using UnityEngine.Rendering;

public class DayNightCycleController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;

    [Header("Skybox")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;

    [Header("Colores ambiente")]
    [SerializeField] private Color sunriseAmbientColor = new Color(0.30f, 0.26f, 0.32f);
    [SerializeField] private Color morningAmbientColor = new Color(0.42f, 0.45f, 0.46f);
    [SerializeField] private Color dayAmbientColor = new Color(0.50f, 0.53f, 0.55f);
    [SerializeField] private Color sunsetAmbientColor = new Color(0.30f, 0.20f, 0.18f);
    [SerializeField] private Color duskAmbientColor = new Color(0.08f, 0.10f, 0.16f);
    [SerializeField] private Color nightAmbientColor = new Color(0.01f, 0.015f, 0.04f);

    [Header("Color del sol")]
    [SerializeField] private Color sunriseSunColor = new Color(1.00f, 0.65f, 0.40f);
    [SerializeField] private Color morningSunColor = new Color(1.00f, 0.82f, 0.60f);
    [SerializeField] private Color daySunColor = new Color(1.00f, 0.86f, 0.62f);
    [SerializeField] private Color sunsetSunColor = new Color(1.00f, 0.42f, 0.22f);
    [SerializeField] private Color duskSunColor = new Color(0.45f, 0.22f, 0.25f);

    [Header("Color de la luna")]
    [SerializeField] private Color moonColor = new Color(0.35f, 0.45f, 0.95f);

    [Header("Intensidades del sol")]
    [SerializeField] private float sunriseSunIntensity = 0.18f;
    [SerializeField] private float morningSunIntensity = 0.35f;
    [SerializeField] private float daySunIntensity = 0.55f;
    [SerializeField] private float sunsetSunIntensity = 0.35f;
    [SerializeField] private float duskSunIntensity = 0.05f;
    [SerializeField] private float nightSunIntensity = 0f;

    [Header("Intensidades de la luna")]
    [SerializeField] private float dawnMoonIntensity = 0.06f;
    [SerializeField] private float dayMoonIntensity = 0f;
    [SerializeField] private float nightMoonIntensity = 0.07f;

    [Header("Niebla")]
    [SerializeField] private bool usarNiebla = true;
    [SerializeField] private FogMode fogMode = FogMode.ExponentialSquared;

    [Header("Colores de niebla")]
    [SerializeField] private Color sunriseFogColor = new Color(0.95f, 0.72f, 0.60f);
    [SerializeField] private Color morningFogColor = new Color(0.85f, 0.80f, 0.72f);
    [SerializeField] private Color dayFogColor = new Color(0.78f, 0.86f, 0.90f);
    [SerializeField] private Color sunsetFogColor = new Color(0.95f, 0.55f, 0.42f);
    [SerializeField] private Color duskFogColor = new Color(0.22f, 0.22f, 0.34f);
    [SerializeField] private Color nightFogColor = new Color(0.04f, 0.06f, 0.13f);

    [Header("Densidad de niebla")]
    [SerializeField] private float sunriseFogDensity = 0.008f;
    [SerializeField] private float morningFogDensity = 0.006f;
    [SerializeField] private float dayFogDensity = 0.004f;
    [SerializeField] private float sunsetFogDensity = 0.009f;
    [SerializeField] private float duskFogDensity = 0.012f;
    [SerializeField] private float nightFogDensity = 0.015f;

    [Header("Rotación")]
    [SerializeField] private float sunYaw = 170f;
    [SerializeField] private float moonYaw = 170f;

    [Header("Horas")]
    [SerializeField] private float sunriseHour = 6f;
    [SerializeField] private float dayHour = 8f;
    [SerializeField] private float afternoonHour = 14f;
    [SerializeField] private float sunsetHour = 18f;
    [SerializeField] private float duskHour = 20f;
    [SerializeField] private float nightHour = 22f;

    private Material runtimeSkybox;

    private void Awake()
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.fog = usarNiebla;
        RenderSettings.fogMode = fogMode;
    }

    private void Start()
    {
        CrearSkyboxRuntime();
        ActualizarCiclo();
    }

    private void Update()
    {
        ActualizarCiclo();
    }

    private void CrearSkyboxRuntime()
    {
        if (daySkybox == null || nightSkybox == null)
        {
            RenderSettings.skybox = null;
            DynamicGI.UpdateEnvironment();
            return;
        }

        runtimeSkybox = new Material(daySkybox);
        RenderSettings.skybox = runtimeSkybox;
        DynamicGI.UpdateEnvironment();
    }

    private void ActualizarCiclo()
    {
        if (timeSystem == null)
        {
            return;
        }

        float hour = timeSystem.CurrentHour + (timeSystem.CurrentMinute / 60f);

        ActualizarSkybox(hour);
        ActualizarAmbiente(hour);
        ActualizarSol(hour);
        ActualizarLuna(hour);
        ActualizarNiebla(hour);
    }

    private void ActualizarSkybox(float hour)
    {
        if (daySkybox == null || nightSkybox == null)
        {
            RenderSettings.skybox = null;
            return;
        }

        if (runtimeSkybox == null)
        {
            runtimeSkybox = new Material(daySkybox);
            RenderSettings.skybox = runtimeSkybox;
        }

        float dayFactor = ObtenerFactorDia(hour);
        runtimeSkybox.Lerp(nightSkybox, daySkybox, dayFactor);
        DynamicGI.UpdateEnvironment();
    }

    private void ActualizarAmbiente(float hour)
    {
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = ObtenerColorAmbiente(hour);
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

    private float ObtenerFactorDia(float hour)
    {
        if (hour >= nightHour || hour < sunriseHour)
        {
            return 0f;
        }

        if (hour >= sunriseHour && hour < dayHour)
        {
            float t = Suavizar(sunriseHour, dayHour, hour);
            return Mathf.Lerp(0.08f, 0.60f, t);
        }

        if (hour >= dayHour && hour < afternoonHour)
        {
            float t = Suavizar(dayHour, afternoonHour, hour);
            return Mathf.Lerp(0.60f, 1f, t);
        }

        if (hour >= afternoonHour && hour < sunsetHour)
        {
            return 1f;
        }

        if (hour >= sunsetHour && hour < duskHour)
        {
            float t = Suavizar(sunsetHour, duskHour, hour);
            return Mathf.Lerp(1f, 0.35f, t);
        }

        if (hour >= duskHour && hour < nightHour)
        {
            float t = Suavizar(duskHour, nightHour, hour);
            return Mathf.Lerp(0.35f, 0f, t);
        }

        return 0f;
    }

    private Color ObtenerColorAmbiente(float hour)
    {
        if (hour >= nightHour || hour < sunriseHour)
        {
            return nightAmbientColor;
        }

        if (hour >= sunriseHour && hour < dayHour)
        {
            float t = Suavizar(sunriseHour, dayHour, hour);
            return Color.Lerp(nightAmbientColor, sunriseAmbientColor, t);
        }

        if (hour >= dayHour && hour < afternoonHour)
        {
            float t = Suavizar(dayHour, afternoonHour, hour);
            return Color.Lerp(morningAmbientColor, dayAmbientColor, t);
        }

        if (hour >= afternoonHour && hour < sunsetHour)
        {
            return dayAmbientColor;
        }

        if (hour >= sunsetHour && hour < duskHour)
        {
            float t = Suavizar(sunsetHour, duskHour, hour);
            return Color.Lerp(dayAmbientColor, sunsetAmbientColor, t);
        }

        if (hour >= duskHour && hour < nightHour)
        {
            float t = Suavizar(duskHour, nightHour, hour);
            return Color.Lerp(sunsetAmbientColor, nightAmbientColor, t);
        }

        return nightAmbientColor;
    }

    private float ObtenerIntensidadSol(float hour)
    {
        if (hour >= nightHour || hour < sunriseHour)
        {
            return nightSunIntensity;
        }

        if (hour >= sunriseHour && hour < dayHour)
        {
            float t = Suavizar(sunriseHour, dayHour, hour);
            return Mathf.Lerp(0f, morningSunIntensity, t);
        }

        if (hour >= dayHour && hour < afternoonHour)
        {
            float t = Suavizar(dayHour, afternoonHour, hour);
            return Mathf.Lerp(morningSunIntensity, daySunIntensity, t);
        }

        if (hour >= afternoonHour && hour < sunsetHour)
        {
            return daySunIntensity;
        }

        if (hour >= sunsetHour && hour < duskHour)
        {
            float t = Suavizar(sunsetHour, duskHour, hour);
            return Mathf.Lerp(daySunIntensity, sunsetSunIntensity, t);
        }

        if (hour >= duskHour && hour < nightHour)
        {
            float t = Suavizar(duskHour, nightHour, hour);
            return Mathf.Lerp(sunsetSunIntensity, 0f, t);
        }

        return 0f;
    }

    private Color ObtenerColorSol(float hour)
    {
        if (hour >= nightHour || hour < sunriseHour)
        {
            return duskSunColor;
        }

        if (hour >= sunriseHour && hour < dayHour)
        {
            float t = Suavizar(sunriseHour, dayHour, hour);
            return Color.Lerp(sunriseSunColor, morningSunColor, t);
        }

        if (hour >= dayHour && hour < afternoonHour)
        {
            float t = Suavizar(dayHour, afternoonHour, hour);
            return Color.Lerp(morningSunColor, daySunColor, t);
        }

        if (hour >= afternoonHour && hour < sunsetHour)
        {
            return daySunColor;
        }

        if (hour >= sunsetHour && hour < duskHour)
        {
            float t = Suavizar(sunsetHour, duskHour, hour);
            return Color.Lerp(daySunColor, sunsetSunColor, t);
        }

        if (hour >= duskHour && hour < nightHour)
        {
            float t = Suavizar(duskHour, nightHour, hour);
            return Color.Lerp(sunsetSunColor, duskSunColor, t);
        }

        return duskSunColor;
    }

    private float ObtenerIntensidadLuna(float hour)
    {
        if (hour >= nightHour || hour < sunriseHour)
        {
            return nightMoonIntensity;
        }

        if (hour >= sunriseHour && hour < dayHour)
        {
            float t = Suavizar(sunriseHour, dayHour, hour);
            return Mathf.Lerp(nightMoonIntensity, dawnMoonIntensity, t);
        }

        if (hour >= dayHour && hour < sunsetHour)
        {
            return dayMoonIntensity;
        }

        if (hour >= sunsetHour && hour < duskHour)
        {
            float t = Suavizar(sunsetHour, duskHour, hour);
            return Mathf.Lerp(0f, 0.03f, t);
        }

        if (hour >= duskHour && hour < nightHour)
        {
            float t = Suavizar(duskHour, nightHour, hour);
            return Mathf.Lerp(0.03f, nightMoonIntensity, t);
        }

        return nightMoonIntensity;
    }

    private Color ObtenerColorNiebla(float hour)
    {
        if (hour >= nightHour || hour < sunriseHour)
        {
            return nightFogColor;
        }

        if (hour >= sunriseHour && hour < dayHour)
        {
            float t = Suavizar(sunriseHour, dayHour, hour);
            return Color.Lerp(nightFogColor, sunriseFogColor, t);
        }

        if (hour >= dayHour && hour < afternoonHour)
        {
            float t = Suavizar(dayHour, afternoonHour, hour);
            return Color.Lerp(morningFogColor, dayFogColor, t);
        }

        if (hour >= afternoonHour && hour < sunsetHour)
        {
            return dayFogColor;
        }

        if (hour >= sunsetHour && hour < duskHour)
        {
            float t = Suavizar(sunsetHour, duskHour, hour);
            return Color.Lerp(dayFogColor, sunsetFogColor, t);
        }

        if (hour >= duskHour && hour < nightHour)
        {
            float t = Suavizar(duskHour, nightHour, hour);
            return Color.Lerp(sunsetFogColor, nightFogColor, t);
        }

        return nightFogColor;
    }

    private float ObtenerDensidadNiebla(float hour)
    {
        if (hour >= nightHour || hour < sunriseHour)
        {
            return nightFogDensity;
        }

        if (hour >= sunriseHour && hour < dayHour)
        {
            float t = Suavizar(sunriseHour, dayHour, hour);
            return Mathf.Lerp(nightFogDensity, sunriseFogDensity, t);
        }

        if (hour >= dayHour && hour < afternoonHour)
        {
            float t = Suavizar(dayHour, afternoonHour, hour);
            return Mathf.Lerp(morningFogDensity, dayFogDensity, t);
        }

        if (hour >= afternoonHour && hour < sunsetHour)
        {
            return dayFogDensity;
        }

        if (hour >= sunsetHour && hour < duskHour)
        {
            float t = Suavizar(sunsetHour, duskHour, hour);
            return Mathf.Lerp(dayFogDensity, sunsetFogDensity, t);
        }

        if (hour >= duskHour && hour < nightHour)
        {
            float t = Suavizar(duskHour, nightHour, hour);
            return Mathf.Lerp(sunsetFogDensity, nightFogDensity, t);
        }

        return nightFogDensity;
    }

    private float ObtenerAnguloSol(float hour)
    {
        if (hour >= sunriseHour && hour < afternoonHour)
        {
            float t = Mathf.InverseLerp(sunriseHour, afternoonHour, hour);
            return Mathf.Lerp(-5f, 90f, t);
        }

        if (hour >= afternoonHour && hour < nightHour)
        {
            float t = Mathf.InverseLerp(afternoonHour, nightHour, hour);
            return Mathf.Lerp(90f, 200f, t);
        }

        return 200f;
    }

    private float ObtenerAnguloLuna(float hour)
    {
        float adjustedHour = hour;

        if (adjustedHour < sunriseHour)
        {
            adjustedHour += 24f;
        }

        float moonStart = sunsetHour;
        float moonEnd = sunriseHour + 24f;

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