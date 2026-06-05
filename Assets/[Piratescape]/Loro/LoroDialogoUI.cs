using System.Collections;
using System.Collections.Generic;
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

    [Header("Animalese")]
    [SerializeField] private AudioSource audioSourceVoz;
    [SerializeField] private AudioClip animaleseLibrary;

    [Tooltip("Duración de cada letra dentro del WAV original. En el pack suele ser 0.15.")]
    [SerializeField] private float segundosLetraEnLibreria = 0.15f;

    [Tooltip("Duración del trozo que se usa de cada letra. En el pack suele ser 0.075.")]
    [SerializeField] private float segundosLetraSalida = 0.075f;

    [SerializeField] private float volumenVoz = 0.35f;

    [Tooltip("Menor que 1 = voz más grave/lenta. Mayor que 1 = voz más aguda/rápida.")]
    [SerializeField] private float pitchVoz = 1f;

    [Tooltip("Si está activo, las palabras largas se acortan para sonar más tipo Animal Crossing.")]
    [SerializeField] private bool shortenWords = false;

    [SerializeField] private bool sonarEnEspacios = false;
    [SerializeField] private int sonarCadaCaracteres = 1;

    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private float timeScaleAnterior = 1f;
    private int frameApertura = -1;

    private bool escribiendo;
    private Coroutine rutinaEscritura;

    private readonly Dictionary<char, AudioClip> clipsPorLetra = new Dictionary<char, AudioClip>();

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

        PrepararAudioVoz();
        PrepararClipsAnimalese();
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

    private IEnumerator EscribirTexto(string textoOriginal)
    {
        escribiendo = true;

        if (textoDialogo != null)
        {
            textoDialogo.text = textoOriginal;
            textoDialogo.maxVisibleCharacters = 0;
            textoDialogo.ForceMeshUpdate();
        }

        int totalCaracteres = textoOriginal != null ? textoOriginal.Length : 0;

        for (int i = 0; i <= totalCaracteres; i++)
        {
            if (textoDialogo != null)
            {
                textoDialogo.maxVisibleCharacters = i;
            }

            if (i > 0 && textoOriginal != null && i <= textoOriginal.Length)
            {
                char caracterVisible = textoOriginal[i - 1];

                if (DebeSonarCaracter(caracterVisible, i))
                {
                    char caracterSonido = ObtenerCaracterParaSonido(textoOriginal, i - 1);
                    ReproducirSonidoLetra(caracterSonido);
                }
            }

            yield return new WaitForSecondsRealtime(segundosPorCaracter);
        }

        escribiendo = false;
        rutinaEscritura = null;
    }

    private char ObtenerCaracterParaSonido(string textoOriginal, int indice)
    {
        if (!shortenWords)
        {
            return textoOriginal[indice];
        }

        char actual = textoOriginal[indice];

        if (!char.IsLetter(actual))
        {
            return actual;
        }

        int inicioPalabra = indice;
        while (inicioPalabra > 0 && char.IsLetter(textoOriginal[inicioPalabra - 1]))
        {
            inicioPalabra--;
        }

        int finPalabra = indice;
        while (finPalabra < textoOriginal.Length - 1 && char.IsLetter(textoOriginal[finPalabra + 1]))
        {
            finPalabra++;
        }

        if (indice == inicioPalabra || indice == finPalabra)
        {
            return actual;
        }

        return '\0';
    }

    private bool DebeSonarCaracter(char caracter, int indiceCaracter)
    {
        if (caracter == '\0')
        {
            return false;
        }

        if (!sonarEnEspacios && char.IsWhiteSpace(caracter))
        {
            return false;
        }

        if (char.IsPunctuation(caracter))
        {
            return false;
        }

        if (sonarCadaCaracteres <= 0)
        {
            sonarCadaCaracteres = 1;
        }

        return indiceCaracter % sonarCadaCaracteres == 0;
    }

    private void ReproducirSonidoLetra(char caracter)
    {
        if (audioSourceVoz == null)
        {
            return;
        }

        char letra = NormalizarLetra(caracter);

        if (!clipsPorLetra.TryGetValue(letra, out AudioClip clip))
        {
            return;
        }

        audioSourceVoz.pitch = Mathf.Max(0.1f, pitchVoz);
        audioSourceVoz.PlayOneShot(clip, volumenVoz);
    }

    private char NormalizarLetra(char caracter)
    {
        char letra = char.ToUpper(caracter);

        switch (letra)
        {
            case 'Á':
                return 'A';

            case 'É':
                return 'E';

            case 'Í':
                return 'I';

            case 'Ó':
                return 'O';

            case 'Ú':
            case 'Ü':
                return 'U';

            case 'Ñ':
                return 'N';

            default:
                return letra;
        }
    }

    private void PrepararAudioVoz()
    {
        if (audioSourceVoz == null)
        {
            audioSourceVoz = GetComponent<AudioSource>();
        }

        if (audioSourceVoz == null)
        {
            audioSourceVoz = gameObject.AddComponent<AudioSource>();
        }

        audioSourceVoz.playOnAwake = false;
        audioSourceVoz.loop = false;
        audioSourceVoz.spatialBlend = 0f;
    }

    private void PrepararClipsAnimalese()
    {
        clipsPorLetra.Clear();

        if (animaleseLibrary == null)
        {
            Debug.LogWarning("LoroDialogoUI: falta asignar Animalese Library.", this);
            return;
        }

        int frecuenciaMuestreo = animaleseLibrary.frequency;
        int canales = animaleseLibrary.channels;

        int muestrasPorLetraBiblioteca = Mathf.RoundToInt(segundosLetraEnLibreria * frecuenciaMuestreo);
        int muestrasPorLetraSalida = Mathf.RoundToInt(segundosLetraSalida * frecuenciaMuestreo);

        if (muestrasPorLetraBiblioteca <= 0 || muestrasPorLetraSalida <= 0)
        {
            Debug.LogWarning("LoroDialogoUI: duración de letra inválida.", this);
            return;
        }

        float[] datosOriginales = new float[animaleseLibrary.samples * canales];

        try
        {
            animaleseLibrary.GetData(datosOriginales, 0);
        }
        catch
        {
            Debug.LogWarning("LoroDialogoUI: no se pudo leer animalese.wav. En Import Settings pon Load Type = Decompress On Load.", this);
            return;
        }

        for (int i = 0; i < 26; i++)
        {
            char letra = (char)('A' + i);

            int inicioMuestra = i * muestrasPorLetraBiblioteca;
            int inicioDato = inicioMuestra * canales;

            int muestrasDisponibles = animaleseLibrary.samples - inicioMuestra;
            int muestrasClip = Mathf.Min(muestrasPorLetraSalida, muestrasDisponibles);

            if (muestrasClip <= 0)
            {
                continue;
            }

            float[] datosLetra = new float[muestrasClip * canales];

            for (int j = 0; j < datosLetra.Length; j++)
            {
                int indiceOriginal = inicioDato + j;

                if (indiceOriginal >= 0 && indiceOriginal < datosOriginales.Length)
                {
                    datosLetra[j] = datosOriginales[indiceOriginal];
                }
            }

            AudioClip clipLetra = AudioClip.Create(
                "Animalese_" + letra,
                muestrasClip,
                canales,
                frecuenciaMuestreo,
                false
            );

            clipLetra.SetData(datosLetra, 0);
            clipsPorLetra.Add(letra, clipLetra);
        }
    }

    private void EstablecerMensaje(string texto)
    {
        if (textoMensaje != null)
        {
            textoMensaje.text = texto;
        }
    }
}