using UnityEngine;

public class ShelterSleep : MonoBehaviour
{
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private SleepFadeUI fadeUI;

    public void TrySleep()
    {
        if (!timeSystem.IsNight)
        {
            Debug.Log("No puedes dormir hasta las 18:00");
            return;
        }

        // Opcional: animación fade
        if (fadeUI != null)
        {
            fadeUI.Sleep();
        }

        // 🔥 IMPORTANTE
        timeSystem.SleepToNextDay();
    }
}