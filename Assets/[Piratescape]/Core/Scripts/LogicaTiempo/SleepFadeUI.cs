using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SleepFadeUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image fadeImage;

    [Header("Duración")]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Iris Transition")]
    [SerializeField] private float openRadius = 1.5f;
    [SerializeField] private float closedRadius = -0.15f;
    [SerializeField] private float softness = 0.08f;
    [SerializeField] private Vector2 center = new Vector2(0.5f, 0.5f);

    public bool IsFading { get; private set; }

    private Coroutine currentRoutine;
    private Material runtimeMaterial;

    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int SoftnessID = Shader.PropertyToID("_Softness");
    private static readonly int CenterID = Shader.PropertyToID("_Center");
    private static readonly int AspectID = Shader.PropertyToID("_Aspect");

    private void Awake()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("SleepFadeUI: falta la referencia a fadeImage.", this);
            return;
        }

        if (fadeImage.material == null)
        {
            Debug.LogWarning("SleepFadeUI: la Image no tiene material asignado.", this);
            return;
        }

        runtimeMaterial = new Material(fadeImage.material);
        fadeImage.material = runtimeMaterial;

        ConfigurarMaterial();
        SetRadiusImmediate(openRadius);
        IsFading = false;
    }

    private void Update()
    {
        ConfigurarMaterial();
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
        IsFading = true;

        yield return FadeTo(closedRadius);

        IsFading = false;
    }

    public IEnumerator FadeInRoutine()
    {
        IsFading = true;

        yield return FadeTo(openRadius);

        IsFading = false;
    }

    private IEnumerator SleepRoutine()
    {
        yield return FadeOutRoutine();
        yield return FadeInRoutine();

        currentRoutine = null;
    }

    private IEnumerator FadeTo(float targetRadius)
    {
        if (runtimeMaterial == null)
        {
            yield break;
        }

        float elapsed = 0f;
        float startRadius = runtimeMaterial.GetFloat(RadiusID);

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;

            float t = fadeDuration <= 0f ? 1f : elapsed / fadeDuration;
            float newRadius = Mathf.Lerp(startRadius, targetRadius, t);

            SetRadiusImmediate(newRadius);

            yield return null;
        }

        SetRadiusImmediate(targetRadius);
    }

    private void ConfigurarMaterial()
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        float aspect = 1f;

        if (Screen.height > 0)
        {
            aspect = Screen.width / (float)Screen.height;
        }

        runtimeMaterial.SetFloat(SoftnessID, softness);
        runtimeMaterial.SetVector(CenterID, new Vector4(center.x, center.y, 0f, 0f));
        runtimeMaterial.SetFloat(AspectID, aspect);
    }

    private void SetRadiusImmediate(float radius)
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        runtimeMaterial.SetFloat(RadiusID, radius);
    }

    [ContextMenu("Debug/Iris Close")]
    private void DebugIrisClose()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(FadeOutRoutine());
    }

    [ContextMenu("Debug/Iris Open")]
    private void DebugIrisOpen()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(FadeInRoutine());
    }

    [ContextMenu("Debug/Iris Sleep Test")]
    private void DebugIrisSleepTest()
    {
        Sleep();
    }
}