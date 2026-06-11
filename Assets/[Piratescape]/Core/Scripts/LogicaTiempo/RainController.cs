using System;
using UnityEngine;


public class RainController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private ParticleSystem rainFX;

    [Header("Probabilidad diaria")]
    [Range(0f, 1f)]
    [SerializeField] private float rainChancePerDay = 0.08f;

    [Header("Debug / Pruebas")]
    [SerializeField] private bool alwaysRain = false;

    private int lastCheckedDay = -1;
    private bool shouldRainToday;
    private bool rainActive;
    private bool lastAlwaysRain;
    public event Action<bool> OnRainChanged;

    public bool EstaLloviendo => rainActive;

    private void Awake()
    {
        if (timeSystem == null)
        {
            timeSystem = FindFirstObjectByType<GameTimeSystem>();
        }

        lastAlwaysRain = alwaysRain;

        UpdateRainDecision();
        UpdateRainState();
    }

    private void OnEnable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnDayChanged += HandleDayChanged;
            timeSystem.OnTimeChanged += HandleTimeChanged;
        }

        UpdateRainDecision();
        UpdateRainState();
    }

    private void OnDisable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnDayChanged -= HandleDayChanged;
            timeSystem.OnTimeChanged -= HandleTimeChanged;
        }
    }
    private void Update()
    {
        if (lastAlwaysRain != alwaysRain)
        {
            lastAlwaysRain = alwaysRain;
            UpdateRainState();
        }
    }

    private void HandleDayChanged(int newDay)
    {
        UpdateRainDecision();
        UpdateRainState();
    }

    private void HandleTimeChanged(int day, int hour, int minute)
    {
        UpdateRainState();
    }

    private void UpdateRainDecision()
    {
        if (timeSystem == null)
        {
            return;
        }

        if (timeSystem.CurrentDay == lastCheckedDay)
        {
            return;
        }

        lastCheckedDay = timeSystem.CurrentDay;
        shouldRainToday = UnityEngine.Random.value <= rainChancePerDay;
        Debug.Log($"D�a {timeSystem.CurrentDay}: lluvia = {shouldRainToday}");
    }

    private void UpdateRainState()
    {
        bool shouldBeActive = alwaysRain || shouldRainToday;
        SetRain(shouldBeActive);
    }

    private void SetRain(bool active)
    {
        if (rainFX == null || rainActive == active)
        {
            return;
        }

        rainActive = active;

        OnRainChanged?.Invoke(rainActive);

        if (active)
        {
            rainFX.gameObject.SetActive(true);
            rainFX.Play(true);
        }
        else
        {
            rainFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            rainFX.gameObject.SetActive(false);
        }
    }

    [ContextMenu("Debug/Probar lluvia")]
    private void DebugStartRain()
    {
        SetRain(true);
    }

    [ContextMenu("Debug/Parar lluvia")]
    private void DebugStopRain()
    {
        SetRain(false);
    }

    [ContextMenu("Debug/Sortear lluvia hoy")]
    private void DebugRollRainToday()
    {
        shouldRainToday = UnityEngine.Random.value <= rainChancePerDay;
        Debug.Log($"Sorteo manual lluvia hoy: {shouldRainToday}");
        UpdateRainState();
    }
}