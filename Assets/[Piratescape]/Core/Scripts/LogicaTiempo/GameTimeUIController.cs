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

    private void Awake()
    {
        BuscarReferencias();
    }

    private void OnEnable()
    {
        BuscarReferencias();
        SuscribirseEventos();
        RefreshUI();
    }

    private void OnDisable()
    {
        DesuscribirseEventos();
    }

    private void BuscarReferencias()
    {
        if (gameTimeSystem == null)
        {
            gameTimeSystem = FindFirstObjectByType<GameTimeSystem>();
        }
    }

    private void SuscribirseEventos()
    {
        if (gameTimeSystem == null)
        {
            Debug.LogWarning($"{nameof(GameTimeUIController)}: falta referencia a {nameof(GameTimeSystem)}.", this);
            return;
        }

        gameTimeSystem.OnTimeChanged -= HandleTimeChanged;
        gameTimeSystem.OnDayNightChanged -= HandleDayNightChanged;

        gameTimeSystem.OnTimeChanged += HandleTimeChanged;
        gameTimeSystem.OnDayNightChanged += HandleDayNightChanged;
    }

    private void DesuscribirseEventos()
    {
        if (gameTimeSystem == null)
        {
            return;
        }

        gameTimeSystem.OnTimeChanged -= HandleTimeChanged;
        gameTimeSystem.OnDayNightChanged -= HandleDayNightChanged;
    }

    private void HandleTimeChanged(int day, int hour, int minute)
    {
        RefreshUI();
    }

    private void HandleDayNightChanged(bool isNight)
    {
        UpdateDayNightIcon();
    }

    public void RefreshUI()
    {
        if (gameTimeSystem == null)
        {
            BuscarReferencias();
        }

        if (gameTimeSystem == null)
        {
            return;
        }

        UpdateTimeText();
        UpdateDayNightIcon();
    }

    private void UpdateTimeText()
    {
        if (timeText == null || gameTimeSystem == null)
        {
            return;
        }

        timeText.text = $"Día {gameTimeSystem.CurrentDay}  -  {gameTimeSystem.CurrentHour:00}:{gameTimeSystem.CurrentMinute:00}";
    }

    private void UpdateDayNightIcon()
    {
        if (dayNightIcon == null || gameTimeSystem == null)
        {
            return;
        }

        Sprite spriteCorrecto = gameTimeSystem.IsDay ? daySprite : nightSprite;

        if (spriteCorrecto == null)
        {
            Debug.LogWarning($"{nameof(GameTimeUIController)}: falta asignar el sprite de {(gameTimeSystem.IsDay ? "día" : "noche")}. Espacio actual: {gameTimeSystem.CurrentTimeFormatted}", this);
            return;
        }

        dayNightIcon.sprite = spriteCorrecto;
        dayNightIcon.enabled = true;
    }
}
