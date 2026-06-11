using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;

public sealed class HistoriaInicioLoroUI : MonoBehaviour
{
    public static bool HayAlgunaUIAbierta { get; private set; }

    [Header("Panel")]
    [SerializeField] private GameObject panelHistoria;
    [SerializeField] private TMP_Text textoDialogo;
    [SerializeField] private TMP_Text textoAyuda;

    [Header("Botones opcionales")]
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private Button botonOmitirIntro;

    [Header("Inicio automático")]
    [SerializeField] private bool iniciarAutomaticamente = true;

    [Min(0f)]
    [SerializeField] private float retrasoInicio = 0.1f;

    [Header("Tutorial posterior de misiones")]
    [Tooltip("Objeto que contiene el componente TutorialMisionesLoro.")]
    [SerializeField] private TutorialMisionesLoro tutorialMisiones;

    [Header("Cámaras")]
    [SerializeField] private CinemachineCamera camaraLoro;
    [SerializeField] private CinemachineCamera camaraPrincipal;

    [SerializeField] private int prioridadLoroDuranteTutorial = 100;
    [SerializeField] private int prioridadCamaraPrincipal = 100;
    [SerializeField] private int prioridadCamaraInactiva = 0;

    [Min(0f)]
    [SerializeField] private float segundosBlend = 0f;

    [Header("Objeto a ocultar durante el tutorial")]
    [SerializeField] private GameObject objetoAOcultar;

    [Header("Partes del HUD")]
    [SerializeField] private GameObject panelTiempo;
    [SerializeField] private GameObject barrasEstado;
    [SerializeField] private GameObject barraInventario;
    [SerializeField] private GameObject sistemaEconomia;

    [Header("Otros elementos del HUD")]
    [Tooltip("No pongas aquí el padre HUD completo.")]
    [SerializeField] private GameObject[] otrosElementosHUD;

    [Header("Historia")]
    [TextArea(2, 6)]
    [SerializeField] private string[] frasesHistoria =
    {
        "¡Graaak! Despierta, capitán... o lo que quede de ti.",

        "Te has metido en un buen lío. El pirata se cayó del barco borracho y ha terminado tirado en esta isla.",

        "Si quieres salir de aquí, tendrás que reunir recursos, mejorar la base y construir un barco.",

        "Aunque... dicen que hay tres gemas escondidas por la isla. Si las reúnes, quizá despiertes en tu barco como si todo hubiera sido un sueño.",

        "Primero aprende lo básico. Muévete, mira alrededor y hazme caso si quieres sobrevivir. ¡Graaak!",

        "Antes de dejarte explorar, voy a explicarte todo lo que ves en pantalla. Presta atención, capitán.",

        "Este es el panel de tiempo. Aquí puedes consultar la hora actual, el momento del día y los días que han pasado en la isla.",

        "Estas son tus barras de estado. Te permiten controlar la salud y la energía del personaje. Procura que ninguna se agote.",

        "Esta es la barra de inventario. Aquí puedes ver los objetos que llevas encima y seleccionar el que quieras utilizar.",

        "Este es el sistema de economía. Aquí puedes consultar los objetos y recursos que tienes guardados para comerciar."
    };

    [Header("Índices de explicación del HUD")]
    [SerializeField] private int indiceFraseTiempo = 6;
    [SerializeField] private int indiceFraseBarrasEstado = 7;
    [SerializeField] private int indiceFraseInventario = 8;
    [SerializeField] private int indiceFraseEconomia = 9;

    [Header("Escritura")]
    [SerializeField] private float segundosPorCaracter = 0.03f;

    [Header("Animalese – Librería")]
    [SerializeField] private AudioClip animaleseLibrary;
    [SerializeField] private float segundosLetraEnLibreria = 0.15f;

    [Range(0.04f, 0.14f)]
    [SerializeField] private float segundosLetraSalida = 0.075f;

    [Range(2f, 20f)]
    [SerializeField] private float fadeMs = 8f;

    [Header("Animalese – Voz")]
    [SerializeField] private AudioSource audioSourceVoz;
    [SerializeField] private AudioMixerGroup outputVoz;

    [Range(0.4f, 3f)]
    [SerializeField] private float pitchVoz = 1.4f;

    [Range(0f, 0.25f)]
    [SerializeField] private float pitchVariacion = 0.07f;

    [Range(0f, 0.3f)]
    [SerializeField] private float pitchBoostVocales = 0.12f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenVoz = 0.45f;

    [Header("Animalese – Cadencia")]
    [SerializeField] private bool sonarEnEspacios = false;

    [Min(1)]
    [SerializeField] private int sonarCadaCaracteres = 1;

    [SerializeField] private bool sustituirLetrasSilenciosas = true;

    private int indiceFrase;
    private bool escribiendo;
    private bool historiaIniciada;
    private bool historiaTerminando;

    private Coroutine rutinaEscritura;
    private Coroutine rutinaInicio;

    private Action alTerminarHistoria;

    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private float timeScaleAnterior = 1f;

    private bool estadoOriginalObjeto;
    private bool estadoObjetoGuardado;

    private readonly Dictionary<char, AudioClip> clipsPorLetra =
        new Dictionary<char, AudioClip>();

    private static readonly Dictionary<char, char> SustitutosSilenciosos =
        new Dictionary<char, char>
        {
            { 'F', 'I' },
            { 'S', 'I' },
            { 'T', 'E' },
            { 'V', 'U' },
            { 'X', 'E' },
            { 'Z', 'I' }
        };

    private static readonly HashSet<char> Vocales =
        new HashSet<char>
        {
            'A', 'E', 'I', 'O', 'U'
        };

    private void Awake()
    {
        if (panelHistoria != null)
        {
            panelHistoria.SetActive(false);
        }

        if (tutorialMisiones == null)
        {
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();
        }

        PrepararAudioSource();
        PrepararClipsAnimalese();
        ConfigurarBotones();
        PrepararEstadoInicialCamaras();

        HayAlgunaUIAbierta = false;
    }

    private void Start()
    {
        if (iniciarAutomaticamente)
        {
            rutinaInicio = StartCoroutine(IniciarAutomaticamenteRoutine());
        }
    }

    private IEnumerator IniciarAutomaticamenteRoutine()
    {
        yield return null;

        if (retrasoInicio > 0f)
        {
            yield return new WaitForSecondsRealtime(retrasoInicio);
        }

        rutinaInicio = null;

        if (GestorPartida.Instance != null &&
            GestorPartida.Instance.TutorialCompletado)
        {
            MostrarTodoElHUD();
            RestaurarCamaraPrincipal();
            RestaurarObjetoOculto();
            yield break;
        }

        IniciarHistoria(IniciarTutorialMisiones);
    }

    private void OnDisable()
    {
        if (rutinaInicio != null)
        {
            StopCoroutine(rutinaInicio);
            rutinaInicio = null;
        }

        if (rutinaEscritura != null)
        {
            StopCoroutine(rutinaEscritura);
            rutinaEscritura = null;
        }

        if (historiaIniciada && !historiaTerminando)
        {
            RestaurarEstadoJuego();
            RestaurarCamaraPrincipal();
            RestaurarObjetoOculto();
            MostrarTodoElHUD();
        }

        escribiendo = false;
        historiaIniciada = false;
        historiaTerminando = false;
        HayAlgunaUIAbierta = false;
    }

    private void Update()
    {
        if (!HayAlgunaUIAbierta)
        {
            return;
        }

        bool avanzar = false;

        if (Keyboard.current != null)
        {
            avanzar =
                Keyboard.current.enterKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame;
        }

        if (Gamepad.current != null)
        {
            avanzar |= Gamepad.current.buttonSouth.wasPressedThisFrame;
        }

        if (avanzar)
        {
            Siguiente();
        }
    }

    public void IniciarHistoria(Action callbackTerminar)
    {
        if (historiaIniciada)
        {
            return;
        }

        historiaIniciada = true;
        historiaTerminando = false;

        alTerminarHistoria = callbackTerminar;
        indiceFrase = 0;

        ActivarCamaraLoro();
        OcultarObjeto();
        OcultarTodoElHUD();
        GuardarEstadoJuego();

        HayAlgunaUIAbierta = true;

        if (panelHistoria != null)
        {
            panelHistoria.SetActive(true);
        }

        MostrarFraseActual();
    }

    public void Siguiente()
    {
        if (!HayAlgunaUIAbierta)
        {
            return;
        }

        if (escribiendo)
        {
            CompletarEscrituraInstantanea();
            return;
        }

        indiceFrase++;

        if (frasesHistoria == null ||
            indiceFrase >= frasesHistoria.Length)
        {
            TerminarHistoria();
            return;
        }

        MostrarFraseActual();
    }

    public void OmitirIntro()
    {
        if (!HayAlgunaUIAbierta)
        {
            return;
        }

        TerminarHistoria();
    }

    private void IniciarTutorialMisiones()
    {
        if (GestorPartida.Instance != null &&
            GestorPartida.Instance.TutorialCompletado)
        {
            return;
        }

        if (tutorialMisiones == null)
        {
            tutorialMisiones =
                FindFirstObjectByType<TutorialMisionesLoro>();
        }

        if (tutorialMisiones == null)
        {
            Debug.LogWarning(
                "HistoriaInicioLoroUI: no se ha encontrado TutorialMisionesLoro.",
                this
            );

            return;
        }

        tutorialMisiones.IniciarTutorialDesdeCero();
    }

    private void PrepararEstadoInicialCamaras()
    {
        if (camaraPrincipal != null)
        {
            camaraPrincipal.gameObject.SetActive(true);
            camaraPrincipal.Priority = prioridadCamaraPrincipal;
        }

        if (camaraLoro != null)
        {
            camaraLoro.gameObject.SetActive(true);
            camaraLoro.Priority = prioridadCamaraInactiva;
        }
    }

    private void ActivarCamaraLoro()
    {
        ConfigurarBlendCinemachine();

        if (camaraPrincipal != null)
        {
            camaraPrincipal.gameObject.SetActive(true);
            camaraPrincipal.Priority = prioridadCamaraInactiva;
        }
        else
        {
            Debug.LogWarning(
                "HistoriaInicioLoroUI: falta asignar la cámara principal.",
                this
            );
        }

        if (camaraLoro != null)
        {
            camaraLoro.gameObject.SetActive(true);
            camaraLoro.Priority = prioridadLoroDuranteTutorial;
        }
        else
        {
            Debug.LogWarning(
                "HistoriaInicioLoroUI: falta asignar la cámara del loro.",
                this
            );
        }
    }

    private void RestaurarCamaraPrincipal()
    {
        ConfigurarBlendCinemachine();

        if (camaraLoro != null)
        {
            camaraLoro.Priority = prioridadCamaraInactiva;
        }

        if (camaraPrincipal != null)
        {
            camaraPrincipal.gameObject.SetActive(true);
            camaraPrincipal.Priority = prioridadCamaraPrincipal;
        }
        else
        {
            Debug.LogWarning(
                "HistoriaInicioLoroUI: no se puede volver a la cámara principal porque no está asignada.",
                this
            );
        }
    }

    private void ConfigurarBlendCinemachine()
    {
        CinemachineBrain brain = CinemachineBrain.GetActiveBrain(0);

        if (brain == null)
        {
            return;
        }

        CinemachineBlendDefinition.Styles estilo =
            segundosBlend <= 0f
                ? CinemachineBlendDefinition.Styles.Cut
                : CinemachineBlendDefinition.Styles.EaseInOut;

        brain.DefaultBlend = new CinemachineBlendDefinition(
            estilo,
            segundosBlend
        );
    }

    private void OcultarObjeto()
    {
        if (objetoAOcultar == null)
        {
            return;
        }

        estadoOriginalObjeto = objetoAOcultar.activeSelf;
        estadoObjetoGuardado = true;

        objetoAOcultar.SetActive(false);
    }

    private void RestaurarObjetoOculto()
    {
        if (objetoAOcultar == null || !estadoObjetoGuardado)
        {
            return;
        }

        objetoAOcultar.SetActive(estadoOriginalObjeto);
        estadoObjetoGuardado = false;
    }

    private void OcultarTodoElHUD()
    {
        EstablecerActivo(panelTiempo, false);
        EstablecerActivo(barrasEstado, false);
        EstablecerActivo(barraInventario, false);
        EstablecerActivo(sistemaEconomia, false);

        if (otrosElementosHUD == null)
        {
            return;
        }

        for (int i = 0; i < otrosElementosHUD.Length; i++)
        {
            EstablecerActivo(otrosElementosHUD[i], false);
        }
    }

    private void MostrarSoloSeccionHUD(GameObject seccionAMostrar)
    {
        EstablecerActivo(panelTiempo, false);
        EstablecerActivo(barrasEstado, false);
        EstablecerActivo(barraInventario, false);
        EstablecerActivo(sistemaEconomia, false);

        EstablecerActivo(seccionAMostrar, true);
    }

    private void MostrarTodoElHUD()
    {
        EstablecerActivo(panelTiempo, true);
        EstablecerActivo(barrasEstado, true);
        EstablecerActivo(barraInventario, true);
        EstablecerActivo(sistemaEconomia, true);

        if (otrosElementosHUD == null)
        {
            return;
        }

        for (int i = 0; i < otrosElementosHUD.Length; i++)
        {
            EstablecerActivo(otrosElementosHUD[i], true);
        }
    }

    private void ActualizarHUDSegunFrase()
    {
        if (indiceFrase == indiceFraseTiempo)
        {
            MostrarSoloSeccionHUD(panelTiempo);
            return;
        }

        if (indiceFrase == indiceFraseBarrasEstado)
        {
            MostrarSoloSeccionHUD(barrasEstado);
            return;
        }

        if (indiceFrase == indiceFraseInventario)
        {
            MostrarSoloSeccionHUD(barraInventario);
            return;
        }

        if (indiceFrase == indiceFraseEconomia)
        {
            MostrarSoloSeccionHUD(sistemaEconomia);
            return;
        }

        OcultarSeccionesPrincipalesHUD();
    }

    private void OcultarSeccionesPrincipalesHUD()
    {
        EstablecerActivo(panelTiempo, false);
        EstablecerActivo(barrasEstado, false);
        EstablecerActivo(barraInventario, false);
        EstablecerActivo(sistemaEconomia, false);
    }

    private static void EstablecerActivo(
        GameObject objeto,
        bool activo
    )
    {
        if (objeto != null)
        {
            objeto.SetActive(activo);
        }
    }

    private void MostrarFraseActual()
    {
        if (frasesHistoria == null ||
            frasesHistoria.Length == 0)
        {
            TerminarHistoria();
            return;
        }

        ActualizarHUDSegunFrase();

        string frase = frasesHistoria[
            Mathf.Clamp(
                indiceFrase,
                0,
                frasesHistoria.Length - 1
            )
        ];

        if (rutinaEscritura != null)
        {
            StopCoroutine(rutinaEscritura);
        }

        rutinaEscritura =
            StartCoroutine(EscribirTexto(frase));

        ActualizarAyuda();
    }

    private IEnumerator EscribirTexto(string frase)
    {
        escribiendo = true;

        if (textoDialogo != null)
        {
            textoDialogo.text = frase;
            textoDialogo.maxVisibleCharacters = 0;
            textoDialogo.ForceMeshUpdate();
        }

        int total = frase != null ? frase.Length : 0;
        int contadorSonido = 0;

        for (int i = 0; i <= total; i++)
        {
            if (textoDialogo != null)
            {
                textoDialogo.maxVisibleCharacters = i;
            }

            if (i > 0 &&
                frase != null &&
                i <= frase.Length)
            {
                char caracter = frase[i - 1];

                if (DebeSonar(caracter))
                {
                    contadorSonido++;

                    int frecuenciaSonido =
                        Mathf.Max(1, sonarCadaCaracteres);

                    if (contadorSonido % frecuenciaSonido == 0)
                    {
                        ReproducirLetra(caracter);
                    }
                }
            }

            yield return new WaitForSecondsRealtime(
                segundosPorCaracter
            );
        }

        escribiendo = false;
        rutinaEscritura = null;
    }

    private bool DebeSonar(char caracter)
    {
        if (caracter == '\0')
        {
            return false;
        }

        if (!sonarEnEspacios &&
            char.IsWhiteSpace(caracter))
        {
            return false;
        }

        if (char.IsPunctuation(caracter) ||
            char.IsSymbol(caracter) ||
            char.IsDigit(caracter))
        {
            return false;
        }

        return true;
    }

    private void ReproducirLetra(char caracter)
    {
        if (audioSourceVoz == null)
        {
            return;
        }

        char letra = NormalizarLetra(caracter);

        if (sustituirLetrasSilenciosas &&
            SustitutosSilenciosos.TryGetValue(
                letra,
                out char sustituto
            ))
        {
            letra = sustituto;
        }

        if (!clipsPorLetra.TryGetValue(
                letra,
                out AudioClip clip))
        {
            return;
        }

        float pitch = pitchVoz;

        if (Vocales.Contains(letra))
        {
            pitch += pitchBoostVocales;
        }

        pitch += UnityEngine.Random.Range(
            -pitchVariacion,
            pitchVariacion
        );

        audioSourceVoz.pitch = Mathf.Max(0.1f, pitch);
        audioSourceVoz.PlayOneShot(clip, volumenVoz);
    }

    private static char NormalizarLetra(char caracter)
    {
        char letra = char.ToUpperInvariant(caracter);

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

            case 'Ç':
                return 'C';

            default:
                return letra;
        }
    }

    private void PrepararAudioSource()
    {
        if (audioSourceVoz == null)
        {
            audioSourceVoz = GetComponent<AudioSource>();
        }

        if (audioSourceVoz == null)
        {
            audioSourceVoz =
                gameObject.AddComponent<AudioSource>();
        }

        audioSourceVoz.playOnAwake = false;
        audioSourceVoz.loop = false;
        audioSourceVoz.spatialBlend = 0f;
        audioSourceVoz.outputAudioMixerGroup = outputVoz;
    }

    private void PrepararClipsAnimalese()
    {
        clipsPorLetra.Clear();

        if (animaleseLibrary == null)
        {
            Debug.LogError(
                "HistoriaInicioLoroUI: falta asignar Animalese Library.",
                this
            );

            return;
        }

        int frecuencia = animaleseLibrary.frequency;
        int canales = animaleseLibrary.channels;

        int muestrasLetraLibreria = Mathf.RoundToInt(
            segundosLetraEnLibreria * frecuencia
        );

        int muestrasLetraSalida = Mathf.RoundToInt(
            segundosLetraSalida * frecuencia
        );

        int muestrasFade = Mathf.RoundToInt(
            fadeMs * 0.001f * frecuencia
        );

        if (muestrasLetraLibreria <= 0 ||
            muestrasLetraSalida <= 0)
        {
            Debug.LogError(
                "HistoriaInicioLoroUI: duración de letra inválida.",
                this
            );

            return;
        }

        float[] datosOriginales =
            new float[animaleseLibrary.samples * canales];

        try
        {
            animaleseLibrary.GetData(
                datosOriginales,
                0
            );
        }
        catch (Exception excepcion)
        {
            Debug.LogError(
                "HistoriaInicioLoroUI: no se pudo leer el WAV. " +
                "Pon Load Type = Decompress On Load.\n" +
                excepcion.Message,
                this
            );

            return;
        }

        for (int i = 0; i < 26; i++)
        {
            char letra = (char)('A' + i);

            int inicioMuestra =
                i * muestrasLetraLibreria;

            int muestrasDisponibles =
                animaleseLibrary.samples - inicioMuestra;

            int muestrasClip = Mathf.Min(
                muestrasLetraSalida,
                muestrasDisponibles
            );

            if (muestrasClip <= 0)
            {
                continue;
            }

            float[] datosLetra =
                new float[muestrasClip * canales];

            int origenBase = inicioMuestra * canales;

            for (int j = 0; j < datosLetra.Length; j++)
            {
                int indiceOrigen = origenBase + j;

                datosLetra[j] =
                    indiceOrigen >= 0 &&
                    indiceOrigen < datosOriginales.Length
                        ? datosOriginales[indiceOrigen]
                        : 0f;
            }

            AplicarFade(
                datosLetra,
                canales,
                muestrasFade
            );

            AudioClip clipLetra = AudioClip.Create(
                "Animalese_" + letra,
                muestrasClip,
                canales,
                frecuencia,
                false
            );

            clipLetra.SetData(datosLetra, 0);
            clipsPorLetra[letra] = clipLetra;
        }

        Debug.Log(
            "HistoriaInicioLoroUI: cargadas " +
            clipsPorLetra.Count +
            " letras Animalese.",
            this
        );
    }

    private static void AplicarFade(
        float[] datos,
        int canales,
        int muestrasFade
    )
    {
        int muestrasClip = datos.Length / canales;

        muestrasFade = Mathf.Min(
            muestrasFade,
            muestrasClip / 2
        );

        if (muestrasFade <= 0)
        {
            return;
        }

        for (int muestra = 0;
             muestra < muestrasFade;
             muestra++)
        {
            float factor =
                (float)muestra / muestrasFade;

            for (int canal = 0;
                 canal < canales;
                 canal++)
            {
                datos[muestra * canales + canal] *=
                    factor;
            }

            int muestraFinal =
                muestrasClip - 1 - muestra;

            for (int canal = 0;
                 canal < canales;
                 canal++)
            {
                datos[muestraFinal * canales + canal] *=
                    factor;
            }
        }
    }

    private void CompletarEscrituraInstantanea()
    {
        if (rutinaEscritura != null)
        {
            StopCoroutine(rutinaEscritura);
            rutinaEscritura = null;
        }

        escribiendo = false;

        if (textoDialogo != null)
        {
            textoDialogo.maxVisibleCharacters =
                int.MaxValue;
        }
    }

    private void TerminarHistoria()
    {
        if (historiaTerminando)
        {
            return;
        }

        historiaTerminando = true;

        CompletarEscrituraInstantanea();

        HayAlgunaUIAbierta = false;

        if (panelHistoria != null)
        {
            panelHistoria.SetActive(false);
        }

        RestaurarEstadoJuego();
        RestaurarCamaraPrincipal();
        RestaurarObjetoOculto();
        MostrarTodoElHUD();

        historiaIniciada = false;
        historiaTerminando = false;

        Action callback = alTerminarHistoria;
        alTerminarHistoria = null;

        callback?.Invoke();
    }

    private void ConfigurarBotones()
    {
        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveListener(
                Siguiente
            );

            botonSiguiente.onClick.AddListener(
                Siguiente
            );
        }

        if (botonOmitirIntro != null)
        {
            botonOmitirIntro.onClick.RemoveListener(
                OmitirIntro
            );

            botonOmitirIntro.onClick.AddListener(
                OmitirIntro
            );
        }
    }

    private void ActualizarAyuda()
    {
        if (textoAyuda != null)
        {
            textoAyuda.text =
                "Espacio / Enter · Continuar";
        }
    }

    private void GuardarEstadoJuego()
    {
        cursorLockAnterior = Cursor.lockState;
        cursorVisibleAnterior = Cursor.visible;
        timeScaleAnterior = Time.timeScale;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestaurarEstadoJuego()
    {
        Time.timeScale = timeScaleAnterior;
        Cursor.lockState = cursorLockAnterior;
        Cursor.visible = cursorVisibleAnterior;
    }
}