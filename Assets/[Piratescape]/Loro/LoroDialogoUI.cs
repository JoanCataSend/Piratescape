using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class LoroDialogoUI : MonoBehaviour
{
    public static bool HayAlgunaUIAbierta { get; private set; }
    public static int FrameCierre { get; private set; } = -1;

    [Header("Panel")]
    [SerializeField] private GameObject panelMenuLoro;
    [SerializeField] private TMP_Text textoDialogo;
    [SerializeField] private TMP_Text textoMensaje;

    [Header("Botones")]
    [SerializeField] private Button botonGuardar;
    [SerializeField] private Button botonRepetirTutorial;
    [SerializeField] private Button botonCerrar;

    [Header("Referencias")]
    [SerializeField] private TutorialMisionesLoro tutorialMisiones;

    [Header("Comportamiento")]
    [SerializeField] private bool pausarJuegoAlAbrir = true;

    [Header("Escritura")]
    [SerializeField] private float segundosPorCaracter = 0.025f;

    [Header("Sonido estilo diálogo")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoLetra;
    [SerializeField] private float volumenSonido = 0.35f;
    [SerializeField] private float frecuenciaBase = 900f;
    [SerializeField] private float duracionBeep = 0.035f;
    [SerializeField] private bool sonarEnEspacios = false;
    [SerializeField] private int sonarCadaCaracteres = 2;

    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private float timeScaleAnterior = 1f;
    private int frameApertura = -1;

    private bool escribiendo;
    private Coroutine rutinaEscritura;

    public bool EstaAbierto => panelMenuLoro != null && panelMenuLoro.activeSelf;

    private void Awake()
    {
        if (panelMenuLoro != null)
        {
            panelMenuLoro.SetActive(false);
        }

        if (tutorialMisiones == null)
        {
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();
        }

        CachearAudioDialogo();
        ConfigurarBotones();
        HayAlgunaUIAbierta = false;
    }

    private void OnDisable()
    {
        if (EstaAbierto)
        {
            RestaurarEstadoJuego();
        }

        if (rutinaEscritura != null)
        {
            StopCoroutine(rutinaEscritura);
            rutinaEscritura = null;
        }

        escribiendo = false;
        HayAlgunaUIAbierta = false;
    }

    private void Update()
    {
        if (!EstaAbierto)
        {
            return;
        }

        if (Time.frameCount == frameApertura)
        {
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
            {
                Cerrar();
            }
        }

        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            Cerrar();
        }
    }

    public void AbrirMenuLoro()
    {
        frameApertura = Time.frameCount;
        GuardarEstadoJuego();
        HayAlgunaUIAbierta = true;

        if (panelMenuLoro != null)
        {
            panelMenuLoro.SetActive(true);
        }

        EstablecerDialogo(CrearTextoMenu());
        EstablecerMensaje("");
    }

    public void GuardarPartida()
    {
        if (GestorPartida.Instance == null)
        {
            EstablecerMensaje("No se ha encontrado el gestor de partida.");
            return;
        }

        GestorPartida.Instance.GuardarPartida();
        EstablecerMensaje("Partida guardada correctamente.");

        if (tutorialMisiones == null)
        {
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();
        }

        if (tutorialMisiones != null)
        {
            tutorialMisiones.NotificarPartidaGuardada();
        }
    }

    public void RepetirTutorial()
    {
        CerrarSinMarcarFrame();

        if (tutorialMisiones == null)
        {
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();
        }

        if (tutorialMisiones != null)
        {
            tutorialMisiones.IniciarTutorialDesdeCero();
        }
    }

    public void Cerrar()
    {
        if (!EstaAbierto)
        {
            return;
        }

        CerrarSinMarcarFrame();
        FrameCierre = Time.frameCount;
    }

    private void CerrarSinMarcarFrame()
    {
        bool estabaAbierto = EstaAbierto;

        if (rutinaEscritura != null)
        {
            StopCoroutine(rutinaEscritura);
            rutinaEscritura = null;
        }

        escribiendo = false;

        if (panelMenuLoro != null)
        {
            panelMenuLoro.SetActive(false);
        }

        HayAlgunaUIAbierta = false;

        if (estabaAbierto)
        {
            RestaurarEstadoJuego();
        }
    }

    private void ConfigurarBotones()
    {
        if (botonGuardar != null)
        {
            botonGuardar.onClick.RemoveListener(GuardarPartida);
            botonGuardar.onClick.AddListener(GuardarPartida);
        }

        if (botonRepetirTutorial != null)
        {
            botonRepetirTutorial.onClick.RemoveListener(RepetirTutorial);
            botonRepetirTutorial.onClick.AddListener(RepetirTutorial);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveListener(Cerrar);
            botonCerrar.onClick.AddListener(Cerrar);
        }
    }

    private string CrearTextoMenu()
    {
        bool tieneGuardar = botonGuardar != null;
        bool tieneTutorial = botonRepetirTutorial != null;

        if (tieneGuardar && tieneTutorial)
        {
            return "¡Graaak! ¿Qué necesitas? Puedo guardar tu partida o repetir el tutorial.";
        }

        if (tieneGuardar)
        {
            return "¡Graaak! ¿Qué necesitas? Puedo guardar tu partida.";
        }

        if (tieneTutorial)
        {
            return "¡Graaak! ¿Qué necesitas? Puedo repetir el tutorial.";
        }

        return "¡Graaak! Ahora mismo no tengo ninguna acción configurada.";
    }

    private void GuardarEstadoJuego()
    {
        cursorLockAnterior = Cursor.lockState;
        cursorVisibleAnterior = Cursor.visible;
        timeScaleAnterior = Time.timeScale;

        if (pausarJuegoAlAbrir)
        {
            Time.timeScale = 0f;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestaurarEstadoJuego()
    {
        if (pausarJuegoAlAbrir)
        {
            Time.timeScale = timeScaleAnterior;
        }

        Cursor.lockState = cursorLockAnterior;
        Cursor.visible = cursorVisibleAnterior;
    }

    private void EstablecerDialogo(string texto)
    {
        if (rutinaEscritura != null)
        {
            StopCoroutine(rutinaEscritura);
            rutinaEscritura = null;
        }

        rutinaEscritura = StartCoroutine(EscribirTexto(texto));
    }

    private IEnumerator EscribirTexto(string texto)
    {
        escribiendo = true;

        if (textoDialogo != null)
        {
            textoDialogo.text = texto;
            textoDialogo.maxVisibleCharacters = 0;
            textoDialogo.ForceMeshUpdate();
        }

        int totalCaracteres = texto != null ? texto.Length : 0;

        for (int i = 0; i <= totalCaracteres; i++)
        {
            if (textoDialogo != null)
            {
                textoDialogo.maxVisibleCharacters = i;
            }

            if (i > 0 && texto != null && i <= texto.Length)
            {
                char caracterActual = texto[i - 1];

                if (DebeSonarCaracter(caracterActual, i))
                {
                    ReproducirSonidoLetra();
                }
            }

            yield return new WaitForSecondsRealtime(segundosPorCaracter);
        }

        escribiendo = false;
        rutinaEscritura = null;
    }

    private bool DebeSonarCaracter(char caracter, int indiceCaracter)
    {
        if (!sonarEnEspacios && char.IsWhiteSpace(caracter))
        {
            return false;
        }

        if (sonarCadaCaracteres <= 0)
        {
            sonarCadaCaracteres = 1;
        }

        return indiceCaracter % sonarCadaCaracteres == 0;
    }

    private void ReproducirSonidoLetra()
    {
        if (audioSource == null || sonidoLetra == null)
        {
            return;
        }

        audioSource.pitch = 1f;
        audioSource.PlayOneShot(sonidoLetra, volumenSonido);
    }

    private void CachearAudioDialogo()
    {
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
        audioSource.spatialBlend = 0f;

        if (sonidoLetra == null)
        {
            sonidoLetra = CrearBeepDialogo();
        }
    }

    private AudioClip CrearBeepDialogo()
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duracionBeep);

        AudioClip clip = AudioClip.Create(
            "Loro_Beep_AutoGenerado",
            sampleCount,
            1,
            sampleRate,
            false
        );

        float[] samples = new float[sampleCount];

        float frecuencia = frecuenciaBase;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;
            float normalized = (float)i / sampleCount;

            float envelope = 1f;

            if (normalized < 0.15f)
            {
                envelope = normalized / 0.15f;
            }
            else if (normalized > 0.75f)
            {
                envelope = Mathf.Lerp(1f, 0f, (normalized - 0.75f) / 0.25f);
            }

            float ondaPrincipal = Mathf.Sin(2f * Mathf.PI * frecuencia * t);
            float ondaAguda = Mathf.Sin(2f * Mathf.PI * frecuencia * 1.8f * t) * 0.35f;

            samples[i] = (ondaPrincipal + ondaAguda) * 0.35f * envelope;
        }

        clip.SetData(samples, 0);
        return clip;
    }

    private void EstablecerMensaje(string texto)
    {
        if (textoMensaje != null)
        {
            textoMensaje.text = texto;
        }
    }
}