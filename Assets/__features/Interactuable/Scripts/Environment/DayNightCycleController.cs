using UnityEngine;
using GuevaraVideojocs.TimeSystem;

namespace GuevaraVideojocs.Environment
{
    public sealed class DayNightCycleController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameTimeSystem gameTimeSystem;
        [SerializeField] private Light sunLight;
        [SerializeField] private Light moonLight;
        [SerializeField] private GameObject sunVisual;
        [SerializeField] private GameObject moonVisual;

        [Header("Skybox")]
        [SerializeField] private Material daySkybox;
        [SerializeField] private Material nightSkybox;

        [Header("Day Night Hours")]
        [SerializeField] private int dayStartHour = 6;
        [SerializeField] private int nightStartHour = 18;

        [Header("Ambient Lighting")]
        [SerializeField] private Color dayAmbientColor = new Color(0.85f, 0.85f, 0.85f);
        [SerializeField] private Color nightAmbientColor = new Color(0.18f, 0.20f, 0.28f);

        [Header("Light Intensity")]
        [SerializeField] private float daySunIntensity = 1.2f;
        [SerializeField] private float nightSunIntensity = 0.08f;
        [SerializeField] private float nightMoonIntensity = 0.35f;

        [Header("Light Colors")]
        [SerializeField] private Color daySunColor = Color.white;
        [SerializeField] private Color nightSunColor = new Color(0.20f, 0.25f, 0.40f);
        [SerializeField] private Color moonColor = new Color(0.60f, 0.70f, 1.00f);

        [Header("Rotation")]
        [SerializeField] private Vector3 sunDayRotation = new Vector3(50f, -30f, 0f);
        [SerializeField] private Vector3 moonNightRotation = new Vector3(220f, -30f, 0f);

        private bool isSubscribed = false;

        private void Start()
        {
            RefreshLighting();
        }

        private void OnEnable()
        {
            SubscribeToTimeSystem();
            RefreshLighting();
        }

        private void OnDisable()
        {
            UnsubscribeFromTimeSystem();
        }

        private void SubscribeToTimeSystem()
        {
            if (gameTimeSystem == null)
            {
                Debug.LogWarning($"{nameof(DayNightCycleController)}: falta la referencia a GameTimeSystem.");
                return;
            }

            if (isSubscribed)
            {
                return;
            }

            gameTimeSystem.OnTimeChanged += HandleTimeChanged;
            isSubscribed = true;
        }

        private void UnsubscribeFromTimeSystem()
        {
            if (gameTimeSystem == null || !isSubscribed)
            {
                return;
            }

            gameTimeSystem.OnTimeChanged -= HandleTimeChanged;
            isSubscribed = false;
        }

        private void HandleTimeChanged(int day, int hour, int minute)
        {
            UpdateLighting(hour);
        }

        private void RefreshLighting()
        {
            if (gameTimeSystem == null)
            {
                Debug.LogWarning($"{nameof(DayNightCycleController)}: no se puede refrescar porque GameTimeSystem es null.");
                return;
            }

            UpdateLighting(gameTimeSystem.CurrentHour);
        }

        private void UpdateLighting(int hour)
        {
            bool isDayTime = hour >= dayStartHour && hour < nightStartHour;

            UpdateSun(isDayTime);
            UpdateMoon(isDayTime);
            UpdateAmbientLight(isDayTime);
            UpdateSkybox(isDayTime);
        }

        private void UpdateSun(bool isDayTime)
        {
            if (sunLight == null)
            {
                return;
            }

            sunLight.enabled = true;
            sunLight.intensity = isDayTime ? daySunIntensity : nightSunIntensity;
            sunLight.color = isDayTime ? daySunColor : nightSunColor;
            sunLight.transform.rotation = Quaternion.Euler(sunDayRotation);

            if (sunVisual != null)
            {
                sunVisual.SetActive(isDayTime);
            }
        }

        private void UpdateMoon(bool isDayTime)
        {
            bool showMoon = !isDayTime;

            if (moonLight != null)
            {
                moonLight.enabled = showMoon;
                moonLight.intensity = showMoon ? nightMoonIntensity : 0f;
                moonLight.color = moonColor;
                moonLight.transform.rotation = Quaternion.Euler(moonNightRotation);
            }

            if (moonVisual != null)
            {
                moonVisual.SetActive(showMoon);
            }
        }

        private void UpdateAmbientLight(bool isDayTime)
        {
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = isDayTime ? dayAmbientColor : nightAmbientColor;
        }

        private void UpdateSkybox(bool isDayTime)
        {
            Material targetSkybox = isDayTime ? daySkybox : nightSkybox;

            if (targetSkybox == null)
            {
                Debug.LogWarning($"{nameof(DayNightCycleController)}: falta asignar el skybox de {(isDayTime ? "día" : "noche")}.");
                return;
            }

            if (RenderSettings.skybox != targetSkybox)
            {
                RenderSettings.skybox = targetSkybox;
                DynamicGI.UpdateEnvironment();
            }
        }
    }
}