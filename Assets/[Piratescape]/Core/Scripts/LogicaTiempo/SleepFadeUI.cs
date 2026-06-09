using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SleepFadeUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image fadeImage;

    [Header("Duración")]
    [Min(0f)]
    [SerializeField] private float fadeDuration = 1f;

    [Header("Estado inicial")]
    [Tooltip(
        "DESACTIVADO en MainDemo. " +
        "ACTIVADO en EscenaCabina y FinalAlternativo."
    )]
    [SerializeField] private bool empezarCerrado = false;

    [Header("Iris Transition")]
    [SerializeField] private float openRadius = 1.5f;
    [SerializeField] private float closedRadius = -0.15f;
    [SerializeField] private float softness = 0.08f;
    [SerializeField] private Vector2 center = new Vector2(0.5f, 0.5f);

    public bool IsFading { get; private set; }

    private Coroutine currentRoutine;
    private Material runtimeMaterial;

    private static readonly int RadiusID =
        Shader.PropertyToID("_Radius");

    private static readonly int SoftnessID =
        Shader.PropertyToID("_Softness");

    private static readonly int CenterID =
        Shader.PropertyToID("_Center");

    private static readonly int AspectID =
        Shader.PropertyToID("_Aspect");

    private void Awake()
    {
        if (!PrepararMaterial())
        {
            enabled = false;
            return;
        }

        // Evita que la imagen del fade bloquee botones o interacciones.
        fadeImage.raycastTarget = false;

        ConfigurarMaterial();

        if (empezarCerrado)
        {
            AplicarRadioInmediato(closedRadius);
        }
        else
        {
            AplicarRadioInmediato(openRadius);
        }

        IsFading = false;
    }

    private void Update()
    {
        ConfigurarMaterial();
    }

    private bool PrepararMaterial()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning(
                "SleepFadeUI: falta asignar Fade Image.",
                this
            );

            return false;
        }

        if (fadeImage.material == null)
        {
            Debug.LogWarning(
                "SleepFadeUI: Fade Image no tiene material asignado.",
                this
            );

            return false;
        }

        // Cada escena y cada instancia tendrá su propio material.
        // Así una transición no modifica el material de otra escena.
        runtimeMaterial = new Material(fadeImage.material);
        fadeImage.material = runtimeMaterial;

        return true;
    }

    // Sistema original de dormir:
    // cierra el iris y después vuelve a abrirlo.
    public void Sleep()
    {
        DetenerFadeActual();
        currentRoutine = StartCoroutine(SleepRoutine());
    }

    public void FadeOut()
    {
        DetenerFadeActual();
        currentRoutine = StartCoroutine(FadeOutRoutine());
    }

    public void FadeIn()
    {
        DetenerFadeActual();
        currentRoutine = StartCoroutine(FadeInRoutine());
    }

    public IEnumerator FadeOutRoutine()
    {
        IsFading = true;

        yield return FadeTo(closedRadius);

        IsFading = false;
        currentRoutine = null;
    }

    public IEnumerator FadeInRoutine()
    {
        IsFading = true;

        yield return FadeTo(openRadius);

        IsFading = false;
        currentRoutine = null;
    }

    public void SetClosedImmediate()
    {
        DetenerFadeActual();
        AplicarRadioInmediato(closedRadius);
        IsFading = false;
    }

    public void SetOpenImmediate()
    {
        DetenerFadeActual();
        AplicarRadioInmediato(openRadius);
        IsFading = false;
    }

    private IEnumerator SleepRoutine()
    {
        IsFading = true;

        yield return FadeTo(closedRadius);
        yield return FadeTo(openRadius);

        IsFading = false;
        currentRoutine = null;
    }

    private IEnumerator FadeTo(float targetRadius)
    {
        if (runtimeMaterial == null)
        {
            yield break;
        }

        float startRadius =
            runtimeMaterial.GetFloat(RadiusID);

        if (fadeDuration <= 0f)
        {
            AplicarRadioInmediato(targetRadius);
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / fadeDuration
            );

            float smoothT = Mathf.SmoothStep(
                0f,
                1f,
                t
            );

            float radius = Mathf.Lerp(
                startRadius,
                targetRadius,
                smoothT
            );

            AplicarRadioInmediato(radius);

            yield return null;
        }

        AplicarRadioInmediato(targetRadius);
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
            aspect =
                Screen.width / (float)Screen.height;
        }

        runtimeMaterial.SetFloat(
            SoftnessID,
            softness
        );

        runtimeMaterial.SetVector(
            CenterID,
            new Vector4(
                center.x,
                center.y,
                0f,
                0f
            )
        );

        runtimeMaterial.SetFloat(
            AspectID,
            aspect
        );
    }

    private void AplicarRadioInmediato(float radius)
    {
        if (runtimeMaterial == null)
        {
            return;
        }

        runtimeMaterial.SetFloat(
            RadiusID,
            radius
        );
    }

    private void DetenerFadeActual()
    {
        if (currentRoutine == null)
        {
            return;
        }

        StopCoroutine(currentRoutine);
        currentRoutine = null;
        IsFading = false;
    }

    private void OnDisable()
    {
        DetenerFadeActual();
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
            runtimeMaterial = null;
        }
    }

    [ContextMenu("Debug/Iris Close")]
    private void DebugIrisClose()
    {
        SetClosedImmediate();
    }

    [ContextMenu("Debug/Iris Open")]
    private void DebugIrisOpen()
    {
        SetOpenImmediate();
    }

    [ContextMenu("Debug/Iris Sleep Test")]
    private void DebugIrisSleepTest()
    {
        Sleep();
    }
}