using System;
using UnityEngine;

public class GameTimeSystem : MonoBehaviour
{
    private const int MinutesPerHour = 60;
    private const int HoursPerDay = 24;
    private const int MinutesPerDay = HoursPerDay * MinutesPerHour;

    [Header("Initial Time")]
    [SerializeField] private int startDay = 1;
    [SerializeField] private int startHour = 23;
    [SerializeField] private int startMinute = 58;

    [Header("Time Progression")]
    [SerializeField] private float realSecondsPerGameMinute = 1f;
    [SerializeField] private bool startPaused = false;

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

    public bool IsNight => currentHour >= 18 || currentHour < 6;
    public bool IsDay => currentHour >= 6 && currentHour < 18;

    public string CurrentTimeFormatted => $"{currentHour:00}:{currentMinute:00}";
    public string CurrentDayAndTimeFormatted => $"dia {currentDay} hora {currentHour:00}:{currentMinute:00}";

    private void Awake()
    {
        ValidateSerializedValues();

        currentDay = startDay;
        currentHour = startHour;
        currentMinute = startMinute;
        isPaused = startPaused;

        wasNight = IsNight;

        NotifyTimeChanged();
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
        isPaused = true;
    }

    public void ResumeTime()
    {
        isPaused = false;
    }

    public void SetPaused(bool value)
    {
        isPaused = value;
    }

    public void SetTimeScale(float newRealSecondsPerGameMinute)
    {
        if (newRealSecondsPerGameMinute <= 0f)
        {
            Debug.LogWarning($"{nameof(GameTimeSystem)}: realSecondsPerGameMinute must be greater than 0.");
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
        if (day < 1)
        {
            day = 1;
        }

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

        int totalMinutes = GetCurrentTotalMinutes() + minutesToAdd;

        while (totalMinutes < 0)
        {
            if (currentDay > 1)
            {
                currentDay--;
                totalMinutes += MinutesPerDay;
                OnDayChanged?.Invoke(currentDay);
            }
            else
            {
                totalMinutes = 0;
                break;
            }
        }

        while (totalMinutes >= MinutesPerDay)
        {
            totalMinutes -= MinutesPerDay;
            currentDay++;
            OnDayChanged?.Invoke(currentDay);
        }

        currentHour = totalMinutes / MinutesPerHour;
        currentMinute = totalMinutes % MinutesPerHour;

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

        currentDay = Mathf.Max(1, currentDay + daysToAdd);

        CheckDayNightChange();
        NotifyTimeChanged();
        OnDayChanged?.Invoke(currentDay);
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

        wasNight = IsNight;

        OnDayChanged?.Invoke(currentDay);
        OnDayNightChanged?.Invoke(wasNight);
        NotifyTimeChanged();
    }

    public bool CanSleepFromHour(int minSleepHour)
    {
        return currentHour >= minSleepHour;
    }

    public float GetNormalizedTimeOfDay()
    {
        int totalMinutes = GetCurrentTotalMinutes();
        return (float)totalMinutes / MinutesPerDay;
    }

    private void AdvanceOneGameMinute()
    {
        currentMinute++;

        if (currentMinute >= MinutesPerHour)
        {
            currentMinute = 0;
            currentHour++;
        }

        if (currentHour >= HoursPerDay)
        {
            currentHour = 0;
            currentDay++;
            OnDayChanged?.Invoke(currentDay);
        }

        CheckDayNightChange();
        NotifyTimeChanged();
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

    private int GetCurrentTotalMinutes()
    {
        return (currentHour * MinutesPerHour) + currentMinute;
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

        defaultWakeHour = Mathf.Clamp(defaultWakeHour, 0, HoursPerDay - 1);
        defaultWakeMinute = Mathf.Clamp(defaultWakeMinute, 0, MinutesPerHour - 1);

        if (realSecondsPerGameMinute <= 0f)
        {
            realSecondsPerGameMinute = 1f;
        }
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