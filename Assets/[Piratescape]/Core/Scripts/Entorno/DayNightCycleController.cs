using UnityEngine;

public class DayNightController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;

    [Header("Skybox")]
    [SerializeField] private Material daySkybox;
    [SerializeField] private Material nightSkybox;

    private void Update()
    {
        if (timeSystem == null) return;

        float t = timeSystem.GetNormalizedTimeOfDay();

        UpdateLighting(t);
        UpdateSkybox(t);
    }

    private void UpdateLighting(float t)
    {
        // 🌙 AMBIENT (noche más azul)
        Color nightAmbient = new Color(0.08f, 0.12f, 0.35f);
        Color dayAmbient = Color.white;

        Color sunriseColor = new Color(1f, 0.75f, 0.5f);
        Color dayColor = new Color(1f, 0.95f, 0.7f);
        // 🌞 SOL
        if (t >= 0.25f && t <= 0.75f)
        {
            float sunT = Mathf.InverseLerp(0.25f, 0.75f, t);

            sunLight.intensity = Mathf.Lerp(0f, 1.2f, Mathf.Sin(sunT * Mathf.PI));

            // 🔥 CAMBIO AQUÍ
            sunLight.color = Color.Lerp(sunriseColor, dayColor, sunT);
        }
        else
        {
            sunLight.intensity = 0f;
        }

        // 🌙 LUNA
        if (t <= 0.25f || t >= 0.75f)
        {
            float moonT = (t < 0.25f)
                ? Mathf.InverseLerp(0.25f, 0f, t)
                : Mathf.InverseLerp(0.75f, 1f, t);

            moonLight.intensity = Mathf.Lerp(0f, 0.4f, moonT);
            moonLight.color = new Color(0.6f, 0.7f, 1f);
        }
        else
        {
            moonLight.intensity = 0f;
        }

        // 🌌 AMBIENT
        float dayFactor = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.8f, t));
        RenderSettings.ambientLight = Color.Lerp(nightAmbient, dayAmbient, dayFactor);

        // 🔄 ROTACIONES
        float sunAngle = Mathf.Lerp(-90f, 270f, t);
        sunLight.transform.rotation = Quaternion.Euler(sunAngle, 170f, 0f);

        float moonAngle = Mathf.Lerp(90f, 450f, t);
        moonLight.transform.rotation = Quaternion.Euler(moonAngle, 170f, 0f);
    }

    private void UpdateSkybox(float t)
    {
        if (daySkybox == null || nightSkybox == null) return;

        // transición continua todo el día
        float factor = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.2f, 0.8f, t));

        RenderSettings.skybox.Lerp(nightSkybox, daySkybox, factor);
        DynamicGI.UpdateEnvironment();
    }
}

