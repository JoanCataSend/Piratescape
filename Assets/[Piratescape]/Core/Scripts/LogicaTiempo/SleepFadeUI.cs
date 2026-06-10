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

    [Header("Sonido al dormir")]
    [Tooltip("AudioSource utilizado para el sonido del fade de dormir.")]
    [SerializeField] private AudioSource audioSourceDormir;

    [Tooltip("Sonido que se reproduce cuando el personaje empieza a dormirse.")]
    [SerializeField] private AudioClip sonidoFadeDormir;

    [Range(0f, 1f)]
    [SerializeField] private float volumenSonidoDormir = 0.8f;

    [Tooltip("Retraso entre el inicio del sonido y el cierre del iris.")]
    [Min(0f)]
    [SerializeField] private float retrasoFadeTrasSonido = 0f;

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

        PrepararAudioDormir();

        // Evita que la imagen bloquee botones o interacciones.
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

        // Cada instancia utiliza su propio material.
        runtimeMaterial = new Material(fadeImage.material);
        fadeImage.material = runtimeMaterial;

        return true;
    }

    private void PrepararAudioDormir()
    {
        if (audioSourceDormir == null)
        {
            audioSourceDormir = GetComponent<AudioSource>();
        }

        if (audioSourceDormir == null)
        {
            audioSourceDormir =
                gameObject.AddComponent<AudioSource>();
        }

        audioSourceDormir.playOnAwake = false;
        audioSourceDormir.loop = false;

        // Es un sonido de interfaz/transición, por eso es 2D.
        audioSourceDormir.spatialBlend = 0f;
        audioSourceDormir.dopplerLevel = 0f;
    }

    // Cierra y vuelve a abrir el iris con sonido de dormir.
    public void Sleep()
    {
        DetenerFadeActual();
        currentRoutine = StartCoroutine(SleepRoutine());
    }

    // Fade normal sin sonido.
    public void FadeOut()
    {
        DetenerFadeActual();
        currentRoutine = StartCoroutine(FadeOutRoutine());
    }

    // Fade de dormir con sonido.
    public void FadeOutDormir()
    {
        DetenerFadeActual();
        currentRoutine = StartCoroutine(FadeOutDormirRoutine());
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

    public IEnumerator FadeOutDormirRoutine()
    {
        IsFading = true;

        ReproducirSonidoDormir();

        if (retrasoFadeTrasSonido > 0f)
        {
            yield return new WaitForSecondsRealtime(
                retrasoFadeTrasSonido
            );
        }

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

        ReproducirSonidoDormir();

        if (retrasoFadeTrasSonido > 0f)
        {
            yield return new WaitForSecondsRealtime(
                retrasoFadeTrasSonido
            );
        }

        yield return FadeTo(closedRadius);
        yield return FadeTo(openRadius);

        IsFading = false;
        currentRoutine = null;
    }

    private void ReproducirSonidoDormir()
    {
        if (audioSourceDormir == null ||
            sonidoFadeDormir == null)
        {
            return;
        }

        audioSourceDormir.Stop();
        audioSourceDormir.pitch = 1f;

        audioSourceDormir.PlayOneShot(
            sonidoFadeDormir,
            volumenSonidoDormir
        );
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

        if (audioSourceDormir != null)
        {
            audioSourceDormir.Stop();
        }
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

    [ContextMenu("Debug/Fade Out Dormir")]
    private void DebugFadeOutDormir()
    {
        FadeOutDormir();
    }
}