using System;
using UnityEngine;

public class GameTimeSystem : MonoBehaviour
{
    private const int MinutesPerHour = 60;
    private const int HoursPerDay = 24;
    private const int MinutesPerDay = HoursPerDay * MinutesPerHour;

    [Header("Initial Time")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int startHour = 8;
    [SerializeField] private int startMinute = 0;

    [Header("Time Progression")]
    [Tooltip("Cuántos segundos reales tarda en pasar 1 minuto dentro del juego.")]
    [SerializeField] private float realSecondsPerGameMinute = 1f;

    [SerializeField] private bool startPaused = false;

    [Header("Day / Night Settings")]
    [SerializeField] private int dayStartHour = 6;
    [SerializeField] private int nightStartHour = 18;

    [Header("Sleep Settings")]
    [SerializeField] private int defaultWakeHour = 8;
    [SerializeField] private int defaultWakeMinute = 0;

    private float accumulatedRealTime;
    private int currentDay;
    private int currentHour;
    private int currentMinute;
    private bool isPaused;
    private bool wasNight;

    public event Action<int> OnDayChanged;
    public event Action<int, int> OnHourMinuteChanged;
    public event Action<int, int, int> OnTimeChanged;
    public event Action<bool> OnDayNightChanged;

    public int CurrentDay => currentDay;
    public int CurrentHour => currentHour;
    public int CurrentMinute => currentMinute;
    public bool IsPaused => isPaused;

    public int DayStartHour => dayStartHour;
    public int NightStartHour => nightStartHour;

    public bool IsNight => currentHour >= nightStartHour || currentHour < dayStartHour;
    public bool IsDay => !IsNight;

    public string CurrentTimeFormatted => $"{currentHour:00}:{currentMinute:00}";
    public string CurrentDayAndTimeFormatted => $"Día {currentDay} Hora {currentHour:00}:{currentMinute:00}";

    private void Awake()
    {
        ValidateSerializedValues();

        currentDay = startDay;
        currentHour = startHour;
        currentMinute = startMinute;
        isPaused = startPaused;
        accumulatedRealTime = 0f;

        wasNight = IsNight;

        NotifyTimeChanged();
        OnDayNightChanged?.Invoke(wasNight);
    }

    private void Update()
    {
        if (isPaused)
        {
            return;
        }

        if (realSecondsPerGameMinute <= 0f)
        {
            return;
        }

        accumulatedRealTime += Time.deltaTime;

        while (accumulatedRealTime >= realSecondsPerGameMinute)
        {
            accumulatedRealTime -= realSecondsPerGameMinute;
            AdvanceOneGameMinute();
        }
    }

    public void PauseTime()
    {
        SetPaused(true);
    }

    public void ResumeTime()
    {
        SetPaused(false);
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }

    public void SetTimeScale(float newRealSecondsPerGameMinute)
    {
        if (newRealSecondsPerGameMinute <= 0f)
        {
            Debug.LogWarning($"{nameof(GameTimeSystem)}: realSecondsPerGameMinute debe ser mayor que 0.");
            return;
        }

        realSecondsPerGameMinute = newRealSecondsPerGameMinute;
    }

    public void SetTime(int hour, int minute)
    {
        SetTime(currentDay, hour, minute);
    }

    public void SetTime(int day, int hour, int minute)
    {
        day = Mathf.Max(1, day);
        hour = Mathf.Clamp(hour, 0, HoursPerDay - 1);
        minute = Mathf.Clamp(minute, 0, MinutesPerHour - 1);

        bool dayChanged = currentDay != day;

        currentDay = day;
        currentHour = hour;
        currentMinute = minute;
        accumulatedRealTime = 0f;

        if (dayChanged)
        {
            OnDayChanged?.Invoke(currentDay);
        }

        CheckDayNightChange();
        NotifyTimeChanged();
    }

    public void AddMinutes(int minutesToAdd)
    {
        if (minutesToAdd == 0)
        {
            return;
        }

        int absoluteMinutes = ((currentDay - 1) * MinutesPerDay) + GetCurrentTotalMinutes();
        absoluteMinutes += minutesToAdd;

        if (absoluteMinutes < 0)
        {
            absoluteMinutes = 0;
        }

        int newDay = (absoluteMinutes / MinutesPerDay) + 1;
        int minutesInDay = absoluteMinutes % MinutesPerDay;

        bool dayChanged = currentDay != newDay;

        currentDay = newDay;
        currentHour = minutesInDay / MinutesPerHour;
        currentMinute = minutesInDay % MinutesPerHour;
        accumulatedRealTime = 0f;

        if (dayChanged)
        {
            OnDayChanged?.Invoke(currentDay);
        }

        CheckDayNightChange();
        NotifyTimeChanged();
    }

    public void AddHours(int hoursToAdd)
    {
        AddMinutes(hoursToAdd * MinutesPerHour);
    }

    public void AddDays(int daysToAdd)
    {
        if (daysToAdd == 0)
        {
            return;
        }

        int previousDay = currentDay;
        currentDay = Mathf.Max(1, currentDay + daysToAdd);

        if (currentDay != previousDay)
        {
            OnDayChanged?.Invoke(currentDay);
        }

        CheckDayNightChange();
        NotifyTimeChanged();
    }

    public void SleepToNextDay()
    {
        SleepToNextDay(defaultWakeHour, defaultWakeMinute);
    }

    public void SleepToNextDay(int wakeHour, int wakeMinute)
    {
        wakeHour = Mathf.Clamp(wakeHour, 0, HoursPerDay - 1);
        wakeMinute = Mathf.Clamp(wakeMinute, 0, MinutesPerHour - 1);

        currentDay++;
        currentHour = wakeHour;
        currentMinute = wakeMinute;
        accumulatedRealTime = 0f;

        OnDayChanged?.Invoke(currentDay);

        CheckDayNightChange();
        NotifyTimeChanged();
    }

    public bool CanSleepFromHour(int minSleepHour)
    {
        minSleepHour = Mathf.Clamp(minSleepHour, 0, HoursPerDay - 1);
        return currentHour >= minSleepHour;
    }

    public float GetNormalizedTimeOfDay()
    {
        return (float)GetCurrentTotalMinutes() / MinutesPerDay;
    }

    public int GetCurrentTotalMinutes()
    {
        return (currentHour * MinutesPerHour) + currentMinute;
    }

    private void AdvanceOneGameMinute()
    {
        AddMinutes(1);
    }

    private void CheckDayNightChange()
    {
        bool isNightNow = IsNight;

        if (isNightNow != wasNight)
        {
            wasNight = isNightNow;
            OnDayNightChanged?.Invoke(isNightNow);
        }
    }

    private void NotifyTimeChanged()
    {
        OnHourMinuteChanged?.Invoke(currentHour, currentMinute);
        OnTimeChanged?.Invoke(currentDay, currentHour, currentMinute);
    }

    private void ValidateSerializedValues()
    {
        startDay = Mathf.Max(1, startDay);
        startHour = Mathf.Clamp(startHour, 0, HoursPerDay - 1);
        startMinute = Mathf.Clamp(startMinute, 0, MinutesPerHour - 1);

        dayStartHour = Mathf.Clamp(dayStartHour, 0, HoursPerDay - 1);
        nightStartHour = Mathf.Clamp(nightStartHour, 0, HoursPerDay - 1);

        if (dayStartHour == nightStartHour)
        {
            dayStartHour = 6;
            nightStartHour = 18;
        }

        defaultWakeHour = Mathf.Clamp(defaultWakeHour, 0, HoursPerDay - 1);
        defaultWakeMinute = Mathf.Clamp(defaultWakeMinute, 0, MinutesPerHour - 1);

        if (realSecondsPerGameMinute <= 0f)
        {
            realSecondsPerGameMinute = 1f;
        }
    }

    [ContextMenu("Debug/Set Morning 08:00")]
    private void DebugSetMorning()
    {
        SetTime(currentDay, 8, 0);
    }

    [ContextMenu("Debug/Set Noon 12:00")]
    private void DebugSetNoon()
    {
        SetTime(currentDay, 12, 0);
    }

    [ContextMenu("Debug/Set Evening 18:00")]
    private void DebugSetEvening()
    {
        SetTime(currentDay, 18, 0);
    }

    [ContextMenu("Debug/Set Night 22:30")]
    private void DebugSetNight()
    {
        SetTime(currentDay, 22, 30);
    }

    [ContextMenu("Debug/Add 10 Minutes")]
    private void DebugAdd10Minutes()
    {
        AddMinutes(10);
    }

    [ContextMenu("Debug/Add 1 Hour")]
    private void DebugAdd1Hour()
    {
        AddHours(1);
    }

    [ContextMenu("Debug/Add 1 Day")]
    private void DebugAdd1Day()
    {
        AddDays(1);
    }

    [ContextMenu("Debug/Sleep To Next Day")]
    private void DebugSleepToNextDay()
    {
        SleepToNextDay();
    }

    [ContextMenu("Debug/Pause")]
    private void DebugPause()
    {
        PauseTime();
    }

    [ContextMenu("Debug/Resume")]
    private void DebugResume()
    {
        ResumeTime();
    }
}