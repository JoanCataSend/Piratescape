using System.Collections;
using UnityEngine;

public class CinematicBarsUI : MonoBehaviour
{
    [Header("Barras")]
    [SerializeField] private RectTransform topBar;
    [SerializeField] private RectTransform bottomBar;

    [Header("Configuración")]
    [SerializeField] private float barHeight = 120f;
    [SerializeField] private float animationDuration = 0.5f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        HideInstant();
    }

    public void ShowBars()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        gameObject.SetActive(true);
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
        gameObject.SetActive(true);

        if (topBar != null)
        {
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, barHeight);
        }

        if (bottomBar != null)
        {
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, barHeight);
        }
    }

    public void HideInstant()
    {
        if (topBar != null)
        {
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, 0f);
        }

        if (bottomBar != null)
        {
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, 0f);
        }

        gameObject.SetActive(false);
    }

    private IEnumerator AnimateBars(bool show)
    {
        float startHeight = show ? 0f : barHeight;
        float endHeight = show ? barHeight : 0f;

        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / animationDuration);
            float smoothT = Mathf.SmoothStep(0f, 1f, t);

            float height = Mathf.Lerp(startHeight, endHeight, smoothT);

            if (topBar != null)
            {
                topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, height);
            }

            if (bottomBar != null)
            {
                bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, height);
            }

            yield return null;
        }

        if (topBar != null)
        {
            topBar.sizeDelta = new Vector2(topBar.sizeDelta.x, endHeight);
        }

        if (bottomBar != null)
        {
            bottomBar.sizeDelta = new Vector2(bottomBar.sizeDelta.x, endHeight);
        }

        if (!show)
        {
            gameObject.SetActive(false);
        }

        currentRoutine = null;
    }
}