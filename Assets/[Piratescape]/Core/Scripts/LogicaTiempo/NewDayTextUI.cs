using System.Collections;
using TMPro;
using UnityEngine;

public class NewDayTextUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Tiempos")]
    [SerializeField] private float fadeInDuration = 0.4f;
    [SerializeField] private float visibleDuration = 1.4f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    private Coroutine routine;

    private void Awake()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (text == null)
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        gameObject.SetActive(false);
    }

    public IEnumerator ShowDayRoutine(int day)
    {
        if (routine != null)
        {
            StopCoroutine(routine);
        }

        gameObject.SetActive(true);

        text.text = $"Comienza el Día {day}";
        Debug.Log("NewDayTextUI activado correctamente");
        canvasGroup.alpha = 0f;

        yield return FadeTo(1f, fadeInDuration);
        yield return new WaitForSecondsRealtime(visibleDuration);
        yield return FadeTo(0f, fadeOutDuration);

        gameObject.SetActive(false);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        if (duration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}