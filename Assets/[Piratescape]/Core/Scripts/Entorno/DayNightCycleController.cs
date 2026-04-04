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

    [Header("Lighting")]
    [SerializeField] private Gradient sunColor;
    [SerializeField] private AnimationCurve sunIntensity;
    [SerializeField] private Gradient moonColor;
    [SerializeField] private AnimationCurve moonIntensity;
    [SerializeField] private Gradient ambientColor;

    private void Update()
    {
        if (timeSystem == null) return;

        float t = timeSystem.GetNormalizedTimeOfDay(); // 0 → 1

        UpdateSun(t);
        UpdateMoon(t);
        UpdateAmbient(t);
        UpdateSkybox(t);
    }

    private void UpdateSun(float t)
    {
        if (sunLight == null) return;

        sunLight.color = sunColor.Evaluate(t);
        sunLight.intensity = sunIntensity.Evaluate(t);

        float angle = Mathf.Lerp(-90f, 270f, t);
        sunLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
    }

    private void UpdateMoon(float t)
    {
        if (moonLight == null) return;

        moonLight.color = moonColor.Evaluate(t);
        moonLight.intensity = moonIntensity.Evaluate(t);

        float angle = Mathf.Lerp(90f, 450f, t);
        moonLight.transform.rotation = Quaternion.Euler(angle, 170f, 0f);
    }

    private void UpdateAmbient(float t)
    {
        RenderSettings.ambientLight = ambientColor.Evaluate(t);
    }

    private void UpdateSkybox(float t)
    {
        if (daySkybox == null || nightSkybox == null) return;

        // 🌅 Transición suave:
        // 0.25 = 6:00 → empieza día
        // 0.75 = 18:00 → empieza noche
        float factor = Mathf.Clamp01(Mathf.InverseLerp(0.2f, 0.8f, t));

        RenderSettings.skybox.Lerp(nightSkybox, daySkybox, factor);
        DynamicGI.UpdateEnvironment();
    }
}