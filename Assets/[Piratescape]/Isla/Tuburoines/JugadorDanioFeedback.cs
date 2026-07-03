using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class JugadorDanioFeedback : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private SistemaSaludJugador sistemaSaludJugador;

    [Header("Pantallazo rojo")]
    [SerializeField] private Image overlayDanioRojo;
    [SerializeField] private Canvas canvasOverlay;
    [SerializeField] private bool crearOverlayRojoAutomatico = true;
    [SerializeField] private Color colorOverlayDanio = new Color(1f, 0f, 0f, 0f);
    [Range(0f, 1f)]
    [SerializeField] private float alphaMaximoFlash = 0.55f;
    [SerializeField] private float duracionSubidaFlash = 0.04f;
    [SerializeField] private float duracionBajadaFlash = 0.45f;

    [Header("Sonido dolor pirata")]
    [SerializeField] private AudioSource audioSourceDolor;
    [SerializeField] private AudioMixerGroup outputDolorPirata;
    [SerializeField] private AudioClip[] sonidosDolorPirata;
    [Range(0f, 1f)]
    [SerializeField] private float volumenDolor = 0.8f;
    [SerializeField] private float pitchDolorMinimo = 0.96f;
    [SerializeField] private float pitchDolorMaximo = 1.04f;

    private Coroutine rutinaFlash;
    private int ultimoIndiceDolor = -1;

    private void Awake()
    {
        CachearReferencias();
        PrepararAudio();
        PrepararOverlay();
    }

    public void RecibirDanio(
        int cantidad,
        Vector3 origenDanio)
    {
        AplicarDanio(cantidad);
        ReproducirFeedbackDanio(origenDanio);
    }

    public void ReproducirFeedbackDanio(Vector3 origenDanio)
    {
        ReproducirSonidoDolor();
        LanzarFlashRojo();
    }

    private void CachearReferencias()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (sistemaSaludJugador == null)
        {
            sistemaSaludJugador = GetComponent<SistemaSaludJugador>();
        }

        if (sistemaSaludJugador == null)
        {
            sistemaSaludJugador = FindFirstObjectByType<SistemaSaludJugador>();
        }
    }

    private void AplicarDanio(int cantidad)
    {
        if (cantidad <= 0)
        {
            return;
        }

        CachearReferencias();

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(cantidad);
            return;
        }

        if (sistemaSaludJugador != null)
        {
            sistemaSaludJugador.ReducirSaludDirecta(cantidad);
        }
        else
        {
            Debug.LogWarning(
                "JugadorDanioFeedback: no se ha encontrado PlayerHealth ni SistemaSaludJugador para aplicar daño.",
                this);
        }
    }

    private void PrepararAudio()
    {
        if (audioSourceDolor == null)
        {
            audioSourceDolor = GetComponent<AudioSource>();
        }

        if (audioSourceDolor == null)
        {
            audioSourceDolor = gameObject.AddComponent<AudioSource>();
        }

        audioSourceDolor.playOnAwake = false;
        audioSourceDolor.loop = false;
        audioSourceDolor.spatialBlend = 0f;
        audioSourceDolor.outputAudioMixerGroup = outputDolorPirata;
    }

    private void PrepararOverlay()
    {
        if (overlayDanioRojo == null && crearOverlayRojoAutomatico)
        {
            CrearOverlayAutomatico();
        }

        SetOverlayAlpha(0f);
    }

    private void CrearOverlayAutomatico()
    {
        Canvas canvas = canvasOverlay;

        if (canvas == null)
        {
            canvas = FindFirstObjectByType<Canvas>();
        }

        if (canvas == null)
        {
            GameObject canvasGO = new GameObject(
                "Canvas_Danio_Jugador",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        GameObject overlayGO = new GameObject(
            "Overlay_Danio_Rojo",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        overlayGO.transform.SetParent(canvas.transform, false);
        overlayGO.transform.SetAsLastSibling();

        RectTransform rect = overlayGO.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;

        overlayDanioRojo = overlayGO.GetComponent<Image>();
        overlayDanioRojo.raycastTarget = false;
        overlayDanioRojo.color = colorOverlayDanio;
    }

    private void ReproducirSonidoDolor()
    {
        if (audioSourceDolor == null)
        {
            PrepararAudio();
        }

        AudioClip clip = ObtenerSonidoDolorAleatorio();

        if (audioSourceDolor == null || clip == null)
        {
            return;
        }

        audioSourceDolor.pitch = UnityEngine.Random.Range(
            pitchDolorMinimo,
            pitchDolorMaximo);

        audioSourceDolor.PlayOneShot(
            clip,
            volumenDolor);
    }

    private AudioClip ObtenerSonidoDolorAleatorio()
    {
        if (sonidosDolorPirata == null || sonidosDolorPirata.Length == 0)
        {
            return null;
        }

        if (sonidosDolorPirata.Length == 1)
        {
            ultimoIndiceDolor = 0;
            return sonidosDolorPirata[0];
        }

        int indice;

        do
        {
            indice = UnityEngine.Random.Range(
                0,
                sonidosDolorPirata.Length);
        }
        while (indice == ultimoIndiceDolor);

        ultimoIndiceDolor = indice;
        return sonidosDolorPirata[indice];
    }

    private void LanzarFlashRojo()
    {
        if (overlayDanioRojo == null)
        {
            return;
        }

        if (rutinaFlash != null)
        {
            StopCoroutine(rutinaFlash);
        }

        rutinaFlash = StartCoroutine(RutinaFlashRojo());
    }

    private IEnumerator RutinaFlashRojo()
    {
        float tiempo = 0f;

        while (tiempo < duracionSubidaFlash)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = duracionSubidaFlash <= 0f
                ? 1f
                : tiempo / duracionSubidaFlash;

            SetOverlayAlpha(Mathf.Lerp(0f, alphaMaximoFlash, t));
            yield return null;
        }

        SetOverlayAlpha(alphaMaximoFlash);

        tiempo = 0f;

        while (tiempo < duracionBajadaFlash)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = duracionBajadaFlash <= 0f
                ? 1f
                : tiempo / duracionBajadaFlash;

            SetOverlayAlpha(Mathf.Lerp(alphaMaximoFlash, 0f, t));
            yield return null;
        }

        SetOverlayAlpha(0f);
        rutinaFlash = null;
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (overlayDanioRojo == null)
        {
            return;
        }

        Color color = colorOverlayDanio;
        color.a = Mathf.Clamp01(alpha);
        overlayDanioRojo.color = color;
    }
}
