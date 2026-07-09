using UnityEngine;

public class NightStarsController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private RainController rainController;
    [SerializeField] private ParticleSystem starsFX;

    [Header("Horario de estrellas")]
    [SerializeField] private int startHour = 23;
    [SerializeField] private int endHour = 5;
    [Tooltip("Minutos de juego que tardan las estrellas en aparecer/desaparecer.")]
    [SerializeField] private int fadeMinutes = 60;

    [Header("Aspecto")]
    [SerializeField] private float maxRateOverTime = 45f;
    [SerializeField] private Color starColor = new Color(0.75f, 0.86f, 1f, 1f);
    [SerializeField] private bool apagarObjetoCuandoNoSeVe = true;

    [Header("Clima")]
    [SerializeField] private bool ocultarConMalClima = true;
    [SerializeField] private float visibilidadMinimaConTormenta = 0.05f;
    [SerializeField] private float visibilidadMinimaConLluvia = 0.15f;
    [SerializeField] private float visibilidadMinimaConNiebla = 0.08f;
    [SerializeField] private float visibilidadMinimaConNublado = 0.35f;

    [Header("Suavizado")]
    [SerializeField] private float velocidadSuavizado = 2.5f;

    [Header("Debug")]
    [SerializeField] private float intensidadActual;
    [SerializeField] private float intensidadObjetivo;

    private void Awake()
    {
        if (timeSystem == null)
        {
            timeSystem = FindFirstObjectByType<GameTimeSystem>();
        }

        if (rainController == null)
        {
            rainController = FindFirstObjectByType<RainController>();
        }
    }

    private void OnEnable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnTimeChanged += HandleTimeChanged;
        }

        PrepararEstrellas();
        CalcularIntensidadObjetivo();
        AplicarIntensidad(true);
    }

    private void OnDisable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnTimeChanged -= HandleTimeChanged;
        }
    }

    private void Update()
    {
        CalcularIntensidadObjetivo();
        AplicarIntensidad(false);
    }

    private void HandleTimeChanged(int day, int hour, int minute)
    {
        CalcularIntensidadObjetivo();
    }

    private void PrepararEstrellas()
    {
        if (starsFX == null)
        {
            return;
        }

        var main = starsFX.main;
        main.loop = true;
        main.playOnAwake = false;
        main.startColor = starColor;

        var emission = starsFX.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
    }

    private void CalcularIntensidadObjetivo()
    {
        if (timeSystem == null)
        {
            intensidadObjetivo = 0f;
            return;
        }

        intensidadObjetivo = CalcularIntensidadPorHora(timeSystem.CurrentHour, timeSystem.CurrentMinute);
        intensidadObjetivo *= CalcularMultiplicadorClima();
    }

    private float CalcularIntensidadPorHora(int hour, int minute)
    {
        int current = hour * 60 + minute;
        int start = Mathf.Clamp(startHour, 0, 23) * 60;
        int end = Mathf.Clamp(endHour, 0, 23) * 60;
        int dayMinutes = 24 * 60;
        int fade = Mathf.Max(1, fadeMinutes);

        if (start == end)
        {
            return 0f;
        }

        bool crossesMidnight = end <= start;

        if (crossesMidnight)
        {
            if (current < end)
            {
                current += dayMinutes;
            }

            end += dayMinutes;
        }

        if (current < start || current > end)
        {
            return 0f;
        }

        float fadeIn = Mathf.InverseLerp(start, start + fade, current);
        float fadeOut = Mathf.InverseLerp(end, end - fade, current);
        return Mathf.Clamp01(Mathf.Min(fadeIn, fadeOut));
    }

    private float CalcularMultiplicadorClima()
    {
        if (!ocultarConMalClima || rainController == null)
        {
            return 1f;
        }

        float intensidadClima = rainController.IntensidadVisualClima;
        float visibilidadMinima = 1f;

        switch (rainController.ClimaActual)
        {
            case RainController.EstadoClima.Tormenta:
                visibilidadMinima = visibilidadMinimaConTormenta;
                break;

            case RainController.EstadoClima.LluviaSuave:
                visibilidadMinima = visibilidadMinimaConLluvia;
                break;

            case RainController.EstadoClima.Niebla:
                visibilidadMinima = visibilidadMinimaConNiebla;
                break;

            case RainController.EstadoClima.Nublado:
                visibilidadMinima = visibilidadMinimaConNublado;
                break;
        }

        return Mathf.Lerp(1f, Mathf.Clamp01(visibilidadMinima), intensidadClima);
    }

    private void AplicarIntensidad(bool instantaneo)
    {
        if (starsFX == null)
        {
            return;
        }

        if (instantaneo)
        {
            intensidadActual = intensidadObjetivo;
        }
        else
        {
            float factor = 1f - Mathf.Exp(-velocidadSuavizado * Time.deltaTime);
            intensidadActual = Mathf.Lerp(intensidadActual, intensidadObjetivo, factor);
        }

        bool visible = intensidadActual > 0.01f;

        if (visible && !starsFX.gameObject.activeSelf)
        {
            starsFX.gameObject.SetActive(true);
        }

        if (visible && !starsFX.isPlaying)
        {
            starsFX.Play(true);
        }

        var emission = starsFX.emission;
        emission.rateOverTime = maxRateOverTime * intensidadActual;

        var main = starsFX.main;
        Color color = starColor;
        color.a *= Mathf.Clamp01(intensidadActual);
        main.startColor = color;

        if (!visible && apagarObjetoCuandoNoSeVe)
        {
            starsFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            starsFX.gameObject.SetActive(false);
        }
    }
}
