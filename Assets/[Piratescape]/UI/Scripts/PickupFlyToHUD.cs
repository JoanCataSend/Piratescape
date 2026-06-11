using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PickupFlyToHUD : MonoBehaviour
{
    [Header("Sonido")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoLlegadaHUD;
    [SerializeField] private AudioClip sonidoVueloHUD;
    [SerializeField] private float volumenSonido = 1f;

    [Header("Referencias")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform floatingIconPrefab;

    [Header("Movimiento")]
    [SerializeField] private float duration = 0.6f;
    [SerializeField] private float arcHeight = 90f;
    [SerializeField] private float startScale = 1.15f;
    [SerializeField] private float endScale = 0.35f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.7f, 0f);

    [Header("Estela")]
    [SerializeField] private bool usarEstela = true;
    [SerializeField] private float trailInterval = 0.045f;
    [SerializeField] private float trailDuration = 0.22f;
    [SerializeField] private float trailStartAlpha = 0.35f;
    [SerializeField] private float trailScaleMultiplier = 0.8f;

    [Header("Destello final")]
    [SerializeField] private float popDuration = 0.25f;
    [SerializeField] private float popScale = 1.45f;
    [SerializeField] private float popRotationSpeed = 360f;

    public void Play(Sprite iconSprite, Vector3 worldPosition, RectTransform targetHUD)
    {
        if (canvas == null || mainCamera == null || floatingIconPrefab == null || targetHUD == null)
        {
            Debug.LogWarning("Faltan referencias en PickupFlyToHUD.");
            return;
        }

        RectTransform icon = Instantiate(floatingIconPrefab, canvas.transform);

        Image image = icon.GetComponent<Image>();

        if (image != null)
        {
            image.sprite = iconSprite;
            image.enabled = true;
            image.raycastTarget = false;
        }

        CanvasGroup canvasGroup = icon.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 1f;

        Vector2 startPos = WorldToCanvasPosition(worldPosition + worldOffset);
        Vector2 endPos = TargetToCanvasPosition(targetHUD);

        icon.anchoredPosition = startPos;
        icon.localScale = Vector3.one * startScale;
        icon.localRotation = Quaternion.identity;

        if (audioSource != null && sonidoVueloHUD != null)
        {
            audioSource.PlayOneShot(sonidoVueloHUD, volumenSonido);
        }

        StartCoroutine(FlyRoutine(icon, iconSprite, startPos, endPos));
    }

    private IEnumerator FlyRoutine(RectTransform icon, Sprite iconSprite, Vector2 startPos, Vector2 endPos)
    {
        float timer = 0f;
        float trailTimer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            trailTimer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / duration);
            float smoothT = t * t * (3f - 2f * t);

            Vector2 currentPos = Vector2.Lerp(startPos, endPos, smoothT);
            currentPos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;

            float currentScale = Mathf.Lerp(startScale, endScale, smoothT);

            icon.anchoredPosition = currentPos;
            icon.localScale = Vector3.one * currentScale;

            if (usarEstela && trailTimer >= trailInterval)
            {
                trailTimer = 0f;
                CrearEstela(iconSprite, currentPos, currentScale);
            }

            yield return null;
        }

        icon.anchoredPosition = endPos;
        icon.localScale = Vector3.one * endScale;

        if (audioSource != null && sonidoLlegadaHUD != null)
        {
            audioSource.PlayOneShot(sonidoLlegadaHUD, volumenSonido);
        }

        yield return StartCoroutine(PopRoutine(icon));

        Destroy(icon.gameObject);
    }

    private void CrearEstela(Sprite iconSprite, Vector2 position, float currentScale)
    {
        RectTransform trailIcon = Instantiate(floatingIconPrefab, canvas.transform);

        trailIcon.anchoredPosition = position;
        trailIcon.localScale = Vector3.one * currentScale * trailScaleMultiplier;
        trailIcon.localRotation = Quaternion.identity;

        Image image = trailIcon.GetComponent<Image>();

        if (image != null)
        {
            image.sprite = iconSprite;
            image.enabled = true;
            image.raycastTarget = false;
        }

        CanvasGroup canvasGroup = trailIcon.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = trailIcon.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = trailStartAlpha;

        StartCoroutine(TrailRoutine(trailIcon, canvasGroup));
    }

    private IEnumerator TrailRoutine(RectTransform trailIcon, CanvasGroup canvasGroup)
    {
        float timer = 0f;

        Vector3 startScale = trailIcon.localScale;
        Vector3 endTrailScale = startScale * 0.25f;

        while (timer < trailDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / trailDuration);

            trailIcon.localScale = Vector3.Lerp(startScale, endTrailScale, t);
            canvasGroup.alpha = Mathf.Lerp(trailStartAlpha, 0f, t);

            yield return null;
        }

        Destroy(trailIcon.gameObject);
    }

    private IEnumerator PopRoutine(RectTransform icon)
    {
        float timer = 0f;

        Vector3 initialScale = Vector3.one * endScale;
        Vector3 maxScale = Vector3.one * popScale;

        CanvasGroup canvasGroup = icon.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = icon.gameObject.AddComponent<CanvasGroup>();
        }

        while (timer < popDuration)
        {
            timer += Time.deltaTime;

            float t = Mathf.Clamp01(timer / popDuration);

            float scaleT = Mathf.Sin(t * Mathf.PI * 0.5f);

            icon.localScale = Vector3.Lerp(initialScale, maxScale, scaleT);
            canvasGroup.alpha = 1f - t;

            icon.Rotate(0f, 0f, popRotationSpeed * Time.deltaTime);

            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private Vector2 WorldToCanvasPosition(Vector3 worldPosition)
    {
        Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }

    private Vector2 TargetToCanvasPosition(RectTransform target)
    {
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, target.position);

        RectTransform canvasRect = canvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }
}