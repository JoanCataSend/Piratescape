using UnityEngine;

public class DayNightVisual : MonoBehaviour
{
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private Light directionalLight;

    [Header("Lighting")]
    [SerializeField] private Gradient lightColor;
    [SerializeField] private AnimationCurve lightIntensity;

    private void Update()
    {
        if (timeSystem == null || directionalLight == null || lightColor == null || lightIntensity == null)
            return; // Evita NullReferenceException

        float t = timeSystem.GetNormalizedTimeOfDay();
        directionalLight.color = lightColor.Evaluate(t);
        directionalLight.intensity = lightIntensity.Evaluate(t);
    }
}