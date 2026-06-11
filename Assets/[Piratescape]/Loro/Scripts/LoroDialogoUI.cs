using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Audio;
public sealed class LoroDialogoUI : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    // Estado global
    // ─────────────────────────────────────────────────────────────
    public static bool HayAlgunaUIAbierta { get; private set; }
    public static int  FrameCierre        { get; private set; } = -1;

    // ─────────────────────────────────────────────────────────────
    // Inspector – Panel
    // ─────────────────────────────────────────────────────────────
    [Header("Panel")]
    [SerializeField] private GameObject panelMenuLoro;
    [SerializeField] private TMP_Text   textoDialogo;
    [SerializeField] private TMP_Text   textoMensaje;

    [Header("Botones")]
    [SerializeField] private Button botonGuardar;
    [SerializeField] private Button botonRepetirTutorial;
    [SerializeField] private Button botonCerrar;

    [Header("Referencias")]
    [SerializeField] private TutorialMisionesLoro tutorialMisiones;

    [Header("Comportamiento")]
    [SerializeField] private bool pausarJuegoAlAbrir = true;

    // ─────────────────────────────────────────────────────────────
    // Sonidos 2D de guardar y tutorial
    // ─────────────────────────────────────────────────────────────

    [Header("Sonidos acciones")]
    [SerializeField] private AudioSource audioSourceAcciones;
    [SerializeField] private AudioClip[] sonidosAccion;
    [SerializeField] private float volumenAccion = 0.6f;
    [SerializeField] private AudioMixerGroup outputAcciones;

    // ─────────────────────────────────────────────────────────────
    // Inspector – Escritura
    // ─────────────────────────────────────────────────────────────
    [Header("Escritura")]
    [Tooltip("Segundos que tarda en aparecer cada carácter. Valores recomendados: 0.02–0.04.")]
    [SerializeField] private float segundosPorCaracter = 0.03f;

    // ─────────────────────────────────────────────────────────────
    // Inspector – Animalese
    // ─────────────────────────────────────────────────────────────
    [Header("Animalese – Librería")]
    [Tooltip("El WAV de 3,9 s con las 26 letras A-Z. Import Settings: Decompress On Load.")]
    [SerializeField] private AudioClip animaleseLibrary;

    [Tooltip("Duración de cada letra EN EL WAV (no tocar si usas el pack estándar).")]
    [SerializeField] private float segundosLetraEnLibreria = 0.15f;

    [Tooltip("Cuántos segundos del clip se reproducen realmente. 0.06–0.10 suena más natural.")]
    [SerializeField] [Range(0.04f, 0.14f)] private float segundosLetraSalida = 0.075f;

    [Tooltip("Milisegundos de fade-in/out para eliminar clicks digitales. Recomendado: 6–12 ms.")]
    [SerializeField] [Range(2f, 20f)] private float fadeMs = 8f;

    [Header("Animalese – Voz")]
    [SerializeField] private AudioSource audioSourceVoz;
    [SerializeField] private AudioMixerGroup outputVoz;

    [Tooltip("Pitch base del personaje. Loro ↔ 1.3–1.6 | Neutro = 1.0 | Grave = 0.65–0.85.")]
    [SerializeField] [Range(0.4f, 3.0f)] private float pitchVoz = 1.4f;

    [Tooltip("Variación aleatoria de pitch por letra para que no suene robótico. 0 = ninguna.")]
    [SerializeField] [Range(0f, 0.25f)] private float pitchVariacion = 0.07f;

    [Tooltip("Las vocales suenan ligeramente más agudas que las consonantes (efecto AC).")]
    [SerializeField] [Range(0f, 0.3f)] private float pitchBoostVocales = 0.12f;

    [SerializeField] [Range(0f, 1f)] private float volumenVoz = 0.45f;

    [Header("Animalese – Cadencia")]
    [Tooltip("Sonar en espacios en blanco (no recomendado).")]
    [SerializeField] private bool sonarEnEspacios = false;

    [Tooltip("Reproducir sonido cada N caracteres visibles (1 = todos).")]
    [SerializeField] [Min(1)] private int sonarCadaCaracteres = 1;

    [Tooltip("Las letras con poco audio (F,S,T,V,X,Z) se sustituyen por una vocal cercana.")]
    [SerializeField] private bool sustituirLetrasSilenciosas = true;

    // ─────────────────────────────────────────────────────────────
    // Estado interno
    // ─────────────────────────────────────────────────────────────
    private CursorLockMode cursorLockAnterior;
    private bool           cursorVisibleAnterior;
    private float          timeScaleAnterior = 1f;
    private int            frameApertura     = -1;

    private bool      escribiendo;
    private Coroutine rutinaEscritura;

    private readonly Dictionary<char, AudioClip> clipsPorLetra = new Dictionary<char, AudioClip>();

    // Letras del WAV casi silenciosas → se mapean a un sustituto más audible
    private static readonly Dictionary<char, char> SustitutosSilenciosos = new Dictionary<char, char>
    {
        { 'F', 'I' },
        { 'S', 'I' },
        { 'T', 'E' },
        { 'V', 'U' },
        { 'X', 'E' },
        { 'Z', 'I' },
    };

    private static readonly HashSet<char> Vocales = new HashSet<char> { 'A', 'E', 'I', 'O', 'U' };

    // ─────────────────────────────────────────────────────────────
    // Propiedad pública
    // ─────────────────────────────────────────────────────────────
    public bool EstaAbierto => panelMenuLoro != null && panelMenuLoro.activeSelf;

    // ─────────────────────────────────────────────────────────────
    // Unity lifecycle
    // ─────────────────────────────────────────────────────────────
    private void Awake()
    {
        panelMenuLoro?.SetActive(false);

        if (tutorialMisiones == null)
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();

        PrepararAudioSource();
        PrepararClipsAnimalese();
        ConfigurarBotones();

        HayAlgunaUIAbierta = false;
    }

    private void OnDisable()
    {
        if (EstaAbierto) RestaurarEstadoJuego();

        DetenerEscritura();
        HayAlgunaUIAbierta = false;
    }

    private void Update()
    {
        if (!EstaAbierto) return;
        if (Time.frameCount == frameApertura) return;

        bool cerrar = false;

        if (Keyboard.current != null)
            cerrar |= Keyboard.current.escapeKey.wasPressedThisFrame
                   || Keyboard.current.eKey.wasPressedThisFrame;

        if (Gamepad.current != null)
            cerrar |= Gamepad.current.buttonEast.wasPressedThisFrame;

        if (cerrar) Cerrar();
    }

    // ─────────────────────────────────────────────────────────────
    // API pública
    // ─────────────────────────────────────────────────────────────
    public void AbrirMenuLoro()
    {
        frameApertura = Time.frameCount;
        GuardarEstadoJuego();
        HayAlgunaUIAbierta = true;
        panelMenuLoro?.SetActive(true);

        EstablecerDialogo(CrearTextoMenu());
        EstablecerMensaje("");
    }

    public void GuardarPartida()
    {
        ReproducirSonidoAccion();

        if (GestorPartida.Instance == null)
        {
            EstablecerMensaje("No se ha encontrado el gestor de partida.");
            return;
        }

        GestorPartida.Instance.GuardarPartida();
        EstablecerMensaje("Partida guardada correctamente.");

        if (tutorialMisiones == null)
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();

        tutorialMisiones?.NotificarPartidaGuardada();
    }

    public void RepetirTutorial()
    {
        ReproducirSonidoAccion();

        CerrarSinMarcarFrame();

        if (tutorialMisiones == null)
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();

        tutorialMisiones?.IniciarTutorialDesdeCero();
    }

    public void Cerrar()
    {
        if (!EstaAbierto) return;
        CerrarSinMarcarFrame();
        FrameCierre = Time.frameCount;
    }

    // ─────────────────────────────────────────────────────────────
    // Cierre interno
    // ─────────────────────────────────────────────────────────────
    private void CerrarSinMarcarFrame()
    {
        bool estabaAbierto = EstaAbierto;

        DetenerEscritura();
        panelMenuLoro?.SetActive(false);
        HayAlgunaUIAbierta = false;

        if (estabaAbierto) RestaurarEstadoJuego();
    }

    // ─────────────────────────────────────────────────────────────
    // Escritura de texto + audio
    // ─────────────────────────────────────────────────────────────
    private void EstablecerDialogo(string texto)
    {
        DetenerEscritura();
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

        int total           = textoOriginal != null ? textoOriginal.Length : 0;
        int contadorSonido  = 0;

        for (int i = 0; i <= total; i++)
        {
            if (textoDialogo != null)
                textoDialogo.maxVisibleCharacters = i;

            if (i > 0 && textoOriginal != null && i <= textoOriginal.Length)
            {
                char c = textoOriginal[i - 1];

                if (DebeSonar(c))
                {
                    contadorSonido++;
                    if (contadorSonido % sonarCadaCaracteres == 0)
                        ReproducirLetra(c);
                }
            }

            yield return new WaitForSecondsRealtime(segundosPorCaracter);
        }

        escribiendo     = false;
        rutinaEscritura = null;
    }

    private void DetenerEscritura()
    {
        if (rutinaEscritura != null) { StopCoroutine(rutinaEscritura); rutinaEscritura = null; }
        escribiendo = false;
    }

    // ─────────────────────────────────────────────────────────────
    // Lógica de audio
    // ─────────────────────────────────────────────────────────────
    private bool DebeSonar(char c)
    {
        if (c == '\0') return false;
        if (!sonarEnEspacios && char.IsWhiteSpace(c)) return false;
        if (char.IsPunctuation(c) || char.IsSymbol(c) || char.IsDigit(c)) return false;
        return true;
    }

    private void ReproducirLetra(char caracter)
    {
        if (audioSourceVoz == null) return;

        char letra = NormalizarLetra(caracter);

        if (sustituirLetrasSilenciosas && SustitutosSilenciosos.TryGetValue(letra, out char sustituto))
            letra = sustituto;

        if (!clipsPorLetra.TryGetValue(letra, out AudioClip clip)) return;

        float pitch = pitchVoz;
        if (Vocales.Contains(letra)) pitch += pitchBoostVocales;
        pitch += UnityEngine.Random.Range(-pitchVariacion, pitchVariacion);
        pitch = Mathf.Max(0.1f, pitch);

        audioSourceVoz.pitch = pitch;
        audioSourceVoz.PlayOneShot(clip, volumenVoz);
    }

    private static char NormalizarLetra(char c)
    {
        char upper = char.ToUpperInvariant(c);
        return upper switch
        {
            'Á'      => 'A',
            'É'      => 'E',
            'Í'      => 'I',
            'Ó'      => 'O',
            'Ú'or'Ü' => 'U',
            'Ñ'      => 'N',
            'Ç'      => 'C',
            _        => upper
        };
    }

    // ─────────────────────────────────────────────────────────────
    // Preparación de clips (Awake)
    // ─────────────────────────────────────────────────────────────
    private void PrepararAudioSource()
    {
        if (audioSourceVoz == null) audioSourceVoz = GetComponent<AudioSource>();
        if (audioSourceVoz == null) audioSourceVoz = gameObject.AddComponent<AudioSource>();

        audioSourceVoz.playOnAwake  = false;
        audioSourceVoz.loop         = false;
        audioSourceVoz.spatialBlend = 0f;
        audioSourceVoz.outputAudioMixerGroup = outputVoz;

        if (audioSourceAcciones == null)
        {
            audioSourceAcciones = gameObject.AddComponent<AudioSource>();
        }

        audioSourceAcciones.playOnAwake = false;
        audioSourceAcciones.loop = false;
        audioSourceAcciones.spatialBlend = 0f;
        audioSourceAcciones.outputAudioMixerGroup = outputAcciones;
    }

    private void PrepararClipsAnimalese()
    {
        clipsPorLetra.Clear();

        if (animaleseLibrary == null)
        {
            Debug.LogError("[Animalese] Falta asignar Animalese Library en el inspector.", this);
            return;
        }

        int freq    = animaleseLibrary.frequency;
        int canales = animaleseLibrary.channels;

        int muestrasLetraLib    = Mathf.RoundToInt(segundosLetraEnLibreria * freq);
        int muestrasLetraSalida = Mathf.RoundToInt(segundosLetraSalida     * freq);
        int muestrasFade        = Mathf.RoundToInt(fadeMs * 0.001f         * freq);

        if (muestrasLetraLib <= 0 || muestrasLetraSalida <= 0)
        {
            Debug.LogError("[Animalese] Duración de letra inválida.", this);
            return;
        }

        float[] raw = new float[animaleseLibrary.samples * canales];

        try { animaleseLibrary.GetData(raw, 0); }
        catch (Exception e)
        {
            Debug.LogError($"[Animalese] No se pudo leer el WAV. Import Settings → Load Type = Decompress On Load.\n{e.Message}", this);
            return;
        }

        for (int i = 0; i < 26; i++)
        {
            char letra = (char)('A' + i);

            int inicioMuestra   = i * muestrasLetraLib;
            int muestrasDisp    = animaleseLibrary.samples - inicioMuestra;
            int muestrasClip    = Mathf.Min(muestrasLetraSalida, muestrasDisp);

            if (muestrasClip <= 0) continue;

            float[] datos = new float[muestrasClip * canales];
            int origenBase = inicioMuestra * canales;

            for (int j = 0; j < datos.Length; j++)
            {
                int idx = origenBase + j;
                datos[j] = (idx >= 0 && idx < raw.Length) ? raw[idx] : 0f;
            }

            AplicarFade(datos, canales, muestrasFade);

            AudioClip clip = AudioClip.Create($"Animalese_{letra}", muestrasClip, canales, freq, false);
            clip.SetData(datos, 0);
            clipsPorLetra[letra] = clip;
        }

        Debug.Log($"[Animalese] {clipsPorLetra.Count} clips cargados ({segundosLetraSalida * 1000:F0} ms c/u, fade {fadeMs:F0} ms).");
    }

    /// <summary>
    /// Rampa lineal de fade-in al principio y fade-out al final para eliminar clicks digitales.
    /// </summary>
    private static void AplicarFade(float[] datos, int canales, int muestrasFade)
    {
        int muestrasClip = datos.Length / canales;
        muestrasFade = Mathf.Min(muestrasFade, muestrasClip / 2);
        if (muestrasFade <= 0) return;

        for (int m = 0; m < muestrasFade; m++)
        {
            float t = (float)m / muestrasFade;

            for (int c = 0; c < canales; c++)
                datos[m * canales + c] *= t;

            int mFinal = muestrasClip - 1 - m;
            for (int c = 0; c < canales; c++)
                datos[mFinal * canales + c] *= t;
        }
    }

    // ─────────────────────────────────────────────────────────────
    // Texto del menú
    // ─────────────────────────────────────────────────────────────
    private string CrearTextoMenu()
    {
        bool tieneGuardar  = botonGuardar         != null;
        bool tieneTutorial = botonRepetirTutorial != null;

        if (tieneGuardar && tieneTutorial)
            return "¡Graaak! ¿Qué necesitas? Puedo guardar tu partida o repetir el tutorial.";

        if (tieneGuardar)
            return "¡Graaak! ¿Qué necesitas? Puedo guardar tu partida.";

        if (tieneTutorial)
            return "¡Graaak! ¿Qué necesitas? Puedo repetir el tutorial.";

        return "¡Graaak! Ahora mismo no tengo ninguna acción configurada.";
    }

    // ─────────────────────────────────────────────────────────────
    // Helpers UI
    // ─────────────────────────────────────────────────────────────
    private void EstablecerMensaje(string texto)
    {
        if (textoMensaje != null) textoMensaje.text = texto;
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

    // ─────────────────────────────────────────────────────────────
    // Estado del juego (cursor / tiempo)
    // ─────────────────────────────────────────────────────────────
    private void GuardarEstadoJuego()
    {
        cursorLockAnterior    = Cursor.lockState;
        cursorVisibleAnterior = Cursor.visible;
        timeScaleAnterior     = Time.timeScale;

        if (pausarJuegoAlAbrir) Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
    }

    private void RestaurarEstadoJuego()
    {
        if (pausarJuegoAlAbrir) Time.timeScale = timeScaleAnterior;
        Cursor.lockState = cursorLockAnterior;
        Cursor.visible   = cursorVisibleAnterior;
    }

    // ─────────────────────────────────────────────────────────────
    // Método para audio
    // ─────────────────────────────────────────────────────────────
    private void ReproducirSonidoAccion()
    {
        if (audioSourceAcciones == null ||
            sonidosAccion == null ||
            sonidosAccion.Length == 0)
        {
            return;
        }

        AudioClip clip = sonidosAccion[
            UnityEngine.Random.Range(0, sonidosAccion.Length)
        ];

        audioSourceAcciones.pitch = 1f;
        audioSourceAcciones.PlayOneShot(clip, volumenAccion);
    }
}