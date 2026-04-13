using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SleepFadeUI : MonoBehaviour
{
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        if (fadeImage != null)
        {
            SetAlphaImmediate(0f);
        }
    }

    public void Sleep()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(SleepRoutine());
    }

    public IEnumerator FadeOutRoutine()
    {
        yield return FadeTo(1f);
    }

    public IEnumerator FadeInRoutine()
    {
        yield return FadeTo(0f);
    }

    private IEnumerator SleepRoutine()
    {
        yield return FadeOutRoutine();
        yield return FadeInRoutine();
        currentRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (fadeImage == null)
        {
            yield break;
        }

        float elapsed = 0f;
        float startAlpha = fadeImage.color.a;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = fadeDuration <= 0f ? 1f : elapsed / fadeDuration;
            float newAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetAlphaImmediate(newAlpha);
            yield return null;
        }

        SetAlphaImmediate(targetAlpha);
    }

    private void SetAlphaImmediate(float alpha)
    {
        if (fadeImage == null)
        {
            return;
        }

        Color color = fadeImage.color;
        color.a = Mathf.Clamp01(alpha);
        fadeImage.color = color;
    }
}