using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GhostIntroDialogueUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private TMP_Text textoDialogo;
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private Button botonOmitir;
    [SerializeField] private AudioSource audioSource;

    [Header("Contenido")]
    [SerializeField] private string nombreFantasma = "Fantasma";

    [TextArea(4, 10)]
    [SerializeField] private string[] frases =
    {
        "Hola... soy el fantasma de esta isla.",
        "Solo aparezco por la noche, a partir de las 22:00.",
        "Cada noche cambiaré de lugar, así que tendrás que buscarme.",
        "Si me encuentras, podré comerciar contigo.",
        "Quizá juntos podamos descubrir qué ocurrió aquí..."
    };

    [Header("Typewriter")]
    [SerializeField] private float tiempoEntreLetras = 0.035f;

    [Header("Controles")]
    [SerializeField] private bool permitirContinuarConTecla = false;

    [Header("Sonido estilo diálogo")]
    [SerializeField] private AudioClip sonidoLetra;
    [SerializeField] private float volumenSonido = 0.35f;
    [SerializeField] private float frecuenciaBase = 720f;
    [SerializeField] private float variacionFrecuencia = 180f;
    [SerializeField] private float duracionBeep = 0.035f;
    [SerializeField] private bool sonarEnEspacios = false;

    private Coroutine rutinaActual;
    private bool fraseCompleta;
    private bool dialogoActivo;
    private int indiceFrase;
    private string fraseActual;
    private System.Action alTerminarDialogo;

    private void Awake()
    {
        CachearReferencias();
        ConfigurarBotones();

        if (sonidoLetra == null)
        {
            sonidoLetra = CrearBeepAnimalCrossingLike();
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private void Update()
    {
        if (!dialogoActivo || panelRoot == null || !panelRoot.activeSelf)
        {
            return;
        }

        if (permitirContinuarConTecla && SeHaPulsadoContinuar())
        {
            AvanzarDialogo();
        }
    }

    private void CachearReferencias()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void ConfigurarBotones()
    {
        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveListener(AvanzarDialogo);
            botonSiguiente.onClick.AddListener(AvanzarDialogo);
        }

        if (botonOmitir != null)
        {
            botonOmitir.onClick.RemoveListener(OmitirDialogo);
            botonOmitir.onClick.AddListener(OmitirDialogo);
        }
    }

    public void IniciarDialogo(System.Action callbackFinal)
    {
        alTerminarDialogo = callbackFinal;

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        indiceFrase = 0;
        fraseCompleta = false;
        dialogoActivo = true;

        if (textoNombre != null)
        {
            textoNombre.text = nombreFantasma;
        }

        if (textoDialogo != null)
        {
            textoDialogo.text = "";
        }

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(true);
        }

        if (botonOmitir != null)
        {
            botonOmitir.gameObject.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        rutinaActual = StartCoroutine(EscribirFraseActual());
    }

    public void AvanzarDialogo()
    {
        if (!dialogoActivo)
        {
            return;
        }

        if (!fraseCompleta)
        {
            CompletarFraseActual();
            return;
        }

        indiceFrase++;

        if (indiceFrase >= frases.Length)
        {
            TerminarDialogo();
            return;
        }

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        rutinaActual = StartCoroutine(EscribirFraseActual());
    }

    public void OmitirDialogo()
    {
        if (!dialogoActivo)
        {
            return;
        }

        TerminarDialogo();
    }

    private IEnumerator EscribirFraseActual()
    {
        fraseCompleta = false;

        if (textoDialogo == null || frases == null || frases.Length == 0)
        {
            TerminarDialogo();
            yield break;
        }

        fraseActual = frases[indiceFrase];
        textoDialogo.text = "";

        for (int i = 0; i < fraseActual.Length; i++)
        {
            if (fraseCompleta)
            {
                textoDialogo.text = fraseActual;
                yield break;
            }

            char caracter = fraseActual[i];
            textoDialogo.text += caracter;

            if (DebeSonar(caracter))
            {
                ReproducirSonidoLetra();
            }

            yield return new WaitForSecondsRealtime(tiempoEntreLetras);
        }

        fraseCompleta = true;
    }

    private void CompletarFraseActual()
    {
        fraseCompleta = true;

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
            rutinaActual = null;
        }

        if (textoDialogo != null)
        {
            textoDialogo.text = fraseActual;
        }
    }

    private void TerminarDialogo()
    {
        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
            rutinaActual = null;
        }

        dialogoActivo = false;
        fraseCompleta = false;

        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(false);
        }

        if (botonOmitir != null)
        {
            botonOmitir.gameObject.SetActive(false);
        }

        System.Action callback = alTerminarDialogo;
        alTerminarDialogo = null;

        callback?.Invoke();
    }

    private bool DebeSonar(char caracter)
    {
        if (sonarEnEspacios)
        {
            return true;
        }

        return !char.IsWhiteSpace(caracter);
    }

    private void ReproducirSonidoLetra()
    {
        if (audioSource == null || sonidoLetra == null)
        {
            return;
        }

        audioSource.pitch = Random.Range(0.92f, 1.08f);
        audioSource.PlayOneShot(sonidoLetra, volumenSonido);
    }

    private AudioClip CrearBeepAnimalCrossingLike()
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duracionBeep);

        AudioClip clip = AudioClip.Create(
            "Ghost_Dialogue_Blip",
            sampleCount,
            1,
            sampleRate,
            false
        );

        float[] samples = new float[sampleCount];

        float frecuencia = frecuenciaBase + Random.Range(-variacionFrecuencia, variacionFrecuencia);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            float ondaPrincipal = Mathf.Sin(2f * Mathf.PI * frecuencia * t);
            float ondaSecundaria = Mathf.Sin(2f * Mathf.PI * frecuencia * 1.5f * t) * 0.35f;

            float envolvente = 1f - ((float)i / sampleCount);
            samples[i] = (ondaPrincipal + ondaSecundaria) * envolvente * 0.25f;
        }

        clip.SetData(samples, 0);

        return clip;
    }

    private bool SeHaPulsadoContinuar()
    {
        bool tecladoNuevo = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool mando = Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame;
        bool tecladoViejo = Input.GetKeyDown(KeyCode.E);

        return tecladoNuevo || mando || tecladoViejo;
    }
}