using UnityEngine;

namespace GuevaraVideojocs.TimeSystem
{
    public class GameTimeDebugLogger : MonoBehaviour
    {
        [SerializeField] private GameTimeSystem gameTimeSystem;
        [SerializeField] private bool logEveryMinute = false;

        private void OnEnable()
        {
            if (gameTimeSystem == null)
            {
                Debug.LogWarning($"{nameof(GameTimeDebugLogger)}: GameTimeSystem reference is missing.");
                return;
            }

            gameTimeSystem.OnDayChanged += HandleDayChanged;
            gameTimeSystem.OnTimeChanged += HandleTimeChanged;
        }

        private void OnDisable()
        {
            if (gameTimeSystem == null)
            {
                return;
            }

            gameTimeSystem.OnDayChanged -= HandleDayChanged;
            gameTimeSystem.OnTimeChanged -= HandleTimeChanged;
        }

        private void HandleDayChanged(int newDay)
        {
            Debug.Log($"[GameTime] Day changed to {newDay}");
        }

        private void HandleTimeChanged(int day, int hour, int minute)
        {
            if (!logEveryMinute)
            {
                return;
            }

            Debug.Log($"[GameTime] Day {day} - {hour:00}:{minute:00}");
        }
    }
}