using System.Collections;
using UnityEngine;

public class CinematicBarsUIA : MonoBehaviour
{
    [Header("Barras")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;

    [Header("Configuración")]
    [SerializeField] private float barHeight = 120f;
    [SerializeField] private float animationDuration = 0.5f;

    [Header("Inicio automático")]
    [Tooltip("Actívalo para que las barras aparezcan al comenzar la escena.")]
    [SerializeField] private bool showOnStart = true;

    private Coroutine currentRoutine;

    private void Awake()
    {
        // El objeto permanece activo, pero las barras empiezan ocultas.
        HideInstant();
    }

    private void Start()
    {
        if (showOnStart)
        {
            ShowBars();
        }
    }

    public void ShowBars()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(AnimateBars(true));
    }

    public void HideBars()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(AnimateBars(false));
    }

    public void ShowInstant()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        SetBarHeight(barHeight);
    }

    public void HideInstant()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        SetBarHeight(0f);
    }

    private IEnumerator AnimateBars(bool show)
    {
        float startHeight = ObtenerAlturaActual();
        float endHeight = show ? barHeight : 0f;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(timer / animationDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float height = Mathf.Lerp(startHeight, endHeight, smoothT);

            SetBarHeight(height);

            yield return null;
        }

        SetBarHeight(endHeight);
        currentRoutine = null;
    }

    private float ObtenerAlturaActual()
    {
        if (topBar != null)
        {
            return topBar.sizeDelta.y;
        }

        if (bottomBar != null)
        {
            return bottomBar.sizeDelta.y;
        }

        return 0f;
    }

    private void SetBarHeight(float height)
    {
        if (topBar != null)
        {
            topBar.sizeDelta = new Vector2(
                topBar.sizeDelta.x,
                height
            );
        }

        if (bottomBar != null)
        {
            bottomBar.sizeDelta = new Vector2(
                bottomBar.sizeDelta.x,
                height
            );
        }
    }
}