using UnityEngine;

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
    [SerializeField] private Color sunriseAmbientColor = new Color(0.30f, 0.28f, 0.35f);
    [SerializeField] private Color morningAmbientColor = new Color(0.55f, 0.52f, 0.45f);
    [SerializeField] private Color dayAmbientColor = new Color(0.95f, 0.95f, 0.90f);
    [SerializeField] private Color sunsetAmbientColor = new Color(0.35f, 0.25f, 0.25f);
    [SerializeField] private Color duskAmbientColor = new Color(0.18f, 0.20f, 0.28f);
    [SerializeField] private Color nightAmbientColor = new Color(0.08f, 0.10f, 0.16f);

    [Header("Color del sol")]
    [SerializeField] private Color sunriseSunColor = new Color(1.00f, 0.72f, 0.42f);
    [SerializeField] private Color morningSunColor = new Color(1.00f, 0.88f, 0.70f);
    [SerializeField] private Color daySunColor = new Color(1.00f, 0.96f, 0.84f);
    [SerializeField] private Color sunsetSunColor = new Color(1.00f, 0.55f, 0.32f);
    [SerializeField] private Color duskSunColor = new Color(0.75f, 0.45f, 0.35f);

    [Header("Color de la luna")]
    [SerializeField] private Color moonColor = new Color(0.45f, 0.55f, 0.90f);

    [Header("Intensidades")]
    [SerializeField] private float sunriseSunIntensity = 0.18f;
    [SerializeField] private float morningSunIntensity = 0.45f;
    [SerializeField] private float daySunIntensity = 1.10f;
    [SerializeField] private float sunsetSunIntensity = 0.55f;
    [SerializeField] private float duskSunIntensity = 0.18f;
    [SerializeField] private float nightSunIntensity = 0f;

    [SerializeField] private float dawnMoonIntensity = 0.20f;
    [SerializeField] private float dayMoonIntensity = 0f;
    [SerializeField] private float nightMoonIntensity = 0.32f;

    [Header("Rotacion")]
    [SerializeField] private float sunYaw = 170f;
    [SerializeField] private float moonYaw = 170f;

    private Material runtimeSkybox;

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
        if (daySkybox == null)
        {
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
    }

    private void ActualizarSkybox(float hour)
    {
        if (runtimeSkybox == null || daySkybox == null || nightSkybox == null)
        {
            return;
        }

        float dayFactor = ObtenerFactorDia(hour);
        runtimeSkybox.Lerp(nightSkybox, daySkybox, dayFactor);
        DynamicGI.UpdateEnvironment();
    }

    private void ActualizarAmbiente(float hour)
    {
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

        float anguloX = ObtenerAnguloLuna(hour);
        moonLight.transform.rotation = Quaternion.Euler(anguloX, moonYaw, 0f);
    }

    private float ObtenerFactorDia(float hour)
    {
        // 23:00 - 06:00 -> noche cerrada
        if (hour >= 23f || hour < 6f)
        {
            return 0f;
        }

        // 06:00 - 07:00 -> amanecer inicial
        if (hour >= 6f && hour < 7f)
        {
            float t = Mathf.InverseLerp(6f, 7f, hour);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(0.08f, 0.22f, t);
        }

        // 07:00 - 08:00 -> salida de sol visible
        if (hour >= 7f && hour < 8f)
        {
            float t = Mathf.InverseLerp(7f, 8f, hour);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(0.22f, 0.45f, t);
        }

        // 08:00 - 14:00 -> dia progresivo
        if (hour >= 8f && hour < 14f)
        {
            float t = Mathf.InverseLerp(8f, 14f, hour);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(0.45f, 1f, t);
        }

        // 14:00 - 18:00 -> tarde luminosa
        if (hour >= 14f && hour < 18f)
        {
            return 1f;
        }

        // 18:00 - 20:00 -> atardecer
        if (hour >= 18f && hour < 20f)
        {
            float t = Mathf.InverseLerp(18f, 20f, hour);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(1f, 0.55f, t);
        }

        // 20:00 - 21:00 -> oscurece bastante
        if (hour >= 20f && hour < 21f)
        {
            float t = Mathf.InverseLerp(20f, 21f, hour);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(0.55f, 0.28f, t);
        }

        // 21:00 - 22:00 -> casi noche
        if (hour >= 21f && hour < 22f)
        {
            float t = Mathf.InverseLerp(21f, 22f, hour);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(0.28f, 0.10f, t);
        }

        // 22:00 - 23:00 -> entrada a noche cerrada
        {
            float t = Mathf.InverseLerp(22f, 23f, hour);
            t = Mathf.SmoothStep(0f, 1f, t);
            return Mathf.Lerp(0.10f, 0f, t);
        }
    }

    private Color ObtenerColorAmbiente(float hour)
    {
        if (hour >= 23f || hour < 6f)
        {
            return nightAmbientColor;
        }

        if (hour >= 6f && hour < 7f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(6f, 7f, hour));
            return Color.Lerp(nightAmbientColor, sunriseAmbientColor, t);
        }

        if (hour >= 7f && hour < 8f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(7f, 8f, hour));
            return Color.Lerp(sunriseAmbientColor, morningAmbientColor, t);
        }

        if (hour >= 8f && hour < 14f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(8f, 14f, hour));
            return Color.Lerp(morningAmbientColor, dayAmbientColor, t);
        }

        if (hour >= 14f && hour < 18f)
        {
            return dayAmbientColor;
        }

        if (hour >= 18f && hour < 20f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(18f, 20f, hour));
            return Color.Lerp(dayAmbientColor, sunsetAmbientColor, t);
        }

        if (hour >= 20f && hour < 21f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(20f, 21f, hour));
            return Color.Lerp(sunsetAmbientColor, duskAmbientColor, t);
        }

        if (hour >= 21f && hour < 22f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(21f, 22f, hour));
            return Color.Lerp(duskAmbientColor, new Color(0.12f, 0.13f, 0.20f), t);
        }

        float tFinal = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(22f, 23f, hour));
        return Color.Lerp(new Color(0.12f, 0.13f, 0.20f), nightAmbientColor, tFinal);
    }

    private float ObtenerIntensidadSol(float hour)
    {
        if (hour >= 23f || hour < 6f)
        {
            return nightSunIntensity;
        }

        if (hour >= 6f && hour < 7f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(6f, 7f, hour));
            return Mathf.Lerp(0f, sunriseSunIntensity, t);
        }

        if (hour >= 7f && hour < 8f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(7f, 8f, hour));
            return Mathf.Lerp(sunriseSunIntensity, morningSunIntensity, t);
        }

        if (hour >= 8f && hour < 14f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(8f, 14f, hour));
            return Mathf.Lerp(morningSunIntensity, daySunIntensity, t);
        }

        if (hour >= 14f && hour < 18f)
        {
            return daySunIntensity;
        }

        if (hour >= 18f && hour < 20f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(18f, 20f, hour));
            return Mathf.Lerp(daySunIntensity, sunsetSunIntensity, t);
        }

        if (hour >= 20f && hour < 21f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(20f, 21f, hour));
            return Mathf.Lerp(sunsetSunIntensity, duskSunIntensity, t);
        }

        if (hour >= 21f && hour < 22f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(21f, 22f, hour));
            return Mathf.Lerp(duskSunIntensity, 0.05f, t);
        }

        float tFinal = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(22f, 23f, hour));
        return Mathf.Lerp(0.05f, 0f, tFinal);
    }

    private Color ObtenerColorSol(float hour)
    {
        if (hour >= 23f || hour < 6f)
        {
            return duskSunColor;
        }

        if (hour >= 6f && hour < 7f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(6f, 7f, hour));
            return Color.Lerp(duskSunColor, sunriseSunColor, t);
        }

        if (hour >= 7f && hour < 8f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(7f, 8f, hour));
            return Color.Lerp(sunriseSunColor, morningSunColor, t);
        }

        if (hour >= 8f && hour < 14f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(8f, 14f, hour));
            return Color.Lerp(morningSunColor, daySunColor, t);
        }

        if (hour >= 14f && hour < 18f)
        {
            return daySunColor;
        }

        if (hour >= 18f && hour < 20f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(18f, 20f, hour));
            return Color.Lerp(daySunColor, sunsetSunColor, t);
        }

        if (hour >= 20f && hour < 21f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(20f, 21f, hour));
            return Color.Lerp(sunsetSunColor, duskSunColor, t);
        }

        if (hour >= 21f && hour < 23f)
        {
            return duskSunColor;
        }

        return daySunColor;
    }

    private float ObtenerIntensidadLuna(float hour)
    {
        if (hour >= 23f || hour < 6f)
        {
            return nightMoonIntensity;
        }

        if (hour >= 6f && hour < 7f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(6f, 7f, hour));
            return Mathf.Lerp(nightMoonIntensity, dawnMoonIntensity, t);
        }

        if (hour >= 7f && hour < 8f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(7f, 8f, hour));
            return Mathf.Lerp(dawnMoonIntensity, 0.08f, t);
        }

        if (hour >= 8f && hour < 14f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(8f, 14f, hour));
            return Mathf.Lerp(0.08f, dayMoonIntensity, t);
        }

        if (hour >= 14f && hour < 18f)
        {
            return dayMoonIntensity;
        }

        if (hour >= 18f && hour < 20f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(18f, 20f, hour));
            return Mathf.Lerp(0f, 0.10f, t);
        }

        if (hour >= 20f && hour < 21f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(20f, 21f, hour));
            return Mathf.Lerp(0.10f, 0.18f, t);
        }

        if (hour >= 21f && hour < 22f)
        {
            float t = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(21f, 22f, hour));
            return Mathf.Lerp(0.18f, 0.25f, t);
        }

        float tFinal = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(22f, 23f, hour));
        return Mathf.Lerp(0.25f, nightMoonIntensity, tFinal);
    }

    private float ObtenerAnguloSol(float hour)
    {
        // 06:00 -> 15:00   sube hasta el cenit
        if (hour >= 6f && hour < 15f)
        {
            float t = Mathf.InverseLerp(6f, 15f, hour);
            return Mathf.Lerp(-5f, 90f, t);
        }

        // 15:00 -> 23:00   baja hasta desaparecer
        if (hour >= 15f && hour < 23f)
        {
            float t = Mathf.InverseLerp(15f, 23f, hour);
            return Mathf.Lerp(90f, 200f, t);
        }

        // noche
        if (hour >= 23f || hour < 6f)
        {
            return 200f;
        }

        return -5f;
    }

    private float ObtenerAnguloLuna(float hour)
    {
        float adjustedHour = hour;

        if (adjustedHour < 6f)
        {
            adjustedHour += 24f;
        }

        // 18:00 -> 30:00 (06:00 del dia siguiente)
        if (adjustedHour >= 18f && adjustedHour <= 30f)
        {
            float t = Mathf.InverseLerp(18f, 30f, adjustedHour);
            return Mathf.Lerp(-5f, 200f, t);
        }

        return 200f;
    }
}