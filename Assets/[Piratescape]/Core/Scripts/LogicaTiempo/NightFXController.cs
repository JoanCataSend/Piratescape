using UnityEngine;

public class NightStarsController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private ParticleSystem starsFX;

    [Header("Horario de estrellas")]
    [SerializeField] private int startHour = 23;
    [SerializeField] private int endHour = 5;

    private bool starsActive;

    private void Awake()
    {
        if (timeSystem == null)
        {
            timeSystem = FindFirstObjectByType<GameTimeSystem>();
        }
    }

    private void OnEnable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnTimeChanged += HandleTimeChanged;
        }

        UpdateStars();
    }

    private void OnDisable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnTimeChanged -= HandleTimeChanged;
        }
    }

    private void HandleTimeChanged(int day, int hour, int minute)
    {
        UpdateStars();
    }

    private void UpdateStars()
    {
        if (timeSystem == null || starsFX == null)
        {
            return;
        }

        bool shouldBeActive = IsStarsTime();

        if (shouldBeActive == starsActive)
        {
            return;
        }

        starsActive = shouldBeActive;

        if (starsActive)
        {
            starsFX.gameObject.SetActive(true);
            starsFX.Play(true);
        }
        else
        {
            starsFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            starsFX.gameObject.SetActive(false);
        }
    }

    private bool IsStarsTime()
    {
        int hour = timeSystem.CurrentHour;

        return hour >= startHour || hour < endHour;
    }
}