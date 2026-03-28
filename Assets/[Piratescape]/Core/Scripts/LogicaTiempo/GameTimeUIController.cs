using TMPro;
using UnityEngine;
using UnityEngine.UI;

    public sealed class GameTimeUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameTimeSystem gameTimeSystem;
        [SerializeField] private TextMeshProUGUI timeText;
        [SerializeField] private Image dayNightIcon;

        [Header("Sprites")]
        [SerializeField] private Sprite daySprite;
        [SerializeField] private Sprite nightSprite;

        [Header("Day Night Hours")]
        [SerializeField] private int dayStartHour = 6;
        [SerializeField] private int nightStartHour = 18;

        private void OnEnable()
        {
            if (gameTimeSystem == null)
            {
                Debug.LogWarning($"{nameof(GameTimeUIController)}: missing GameTimeSystem reference.");
                return;
            }

            gameTimeSystem.OnTimeChanged += HandleTimeChanged;
            RefreshUI();
        }

        private void OnDisable()
        {
            if (gameTimeSystem == null)
            {
                return;
            }

            gameTimeSystem.OnTimeChanged -= HandleTimeChanged;
        }

        private void HandleTimeChanged(int day, int hour, int minute)
        {
            UpdateTimeText(day, hour, minute);
            UpdateDayNightIcon(hour);
        }

        private void RefreshUI()
        {
            UpdateTimeText(
                gameTimeSystem.CurrentDay,
                gameTimeSystem.CurrentHour,
                gameTimeSystem.CurrentMinute);

            UpdateDayNightIcon(gameTimeSystem.CurrentHour);
        }

        private void UpdateTimeText(int day, int hour, int minute)
        {
            if (timeText == null)
            {
                return;
            }

            timeText.text = $"dia {day} hora {hour:00}:{minute:00}";
        }

        private void UpdateDayNightIcon(int hour)
        {
            if (dayNightIcon == null)
            {
                return;
            }

            bool isDayTime = hour >= dayStartHour && hour < nightStartHour;
            dayNightIcon.sprite = isDayTime ? daySprite : nightSprite;
        }
    }
