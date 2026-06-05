using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

    [Header("Historia")]
    [TextArea(2, 6)]
    [SerializeField] private string[] frasesHistoria = new string[]
    {
        "¡Graaak! Despierta, capitán... o lo que quede de ti.",
        "Te has metido en un buen lío. El pirata se cayó del barco borracho y ha terminado tirado en esta isla.",
        "Si quieres salir de aquí, tendrás que reunir recursos, mejorar la base y construir un barco.",
        "Aunque... dicen que hay tres gemas escondidas por la isla. Si las reúnes, quizá despiertes en tu barco como si todo hubiera sido un sueño.",
        "Primero aprende lo básico. Muévete, mira alrededor y hazme caso si quieres sobrevivir. ¡Graaak!"
    };

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

    private int indiceFrase;
    private bool escribiendo;
    private Coroutine rutinaEscritura;
    private Action alTerminarHistoria;
    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private float timeScaleAnterior = 1f;

    private void Awake()
    {
        if (panelHistoria != null)
        {
            panelHistoria.SetActive(false);
        }

        CachearAudioDialogo();
        ConfigurarBotones();
        HayAlgunaUIAbierta = false;
    }

    private void OnDisable()
    {
        if (HayAlgunaUIAbierta)
        {
            RestaurarEstadoJuego();
        }

        HayAlgunaUIAbierta = false;
    }

    private void Update()
    {
        if (!HayAlgunaUIAbierta)
        {
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                Siguiente();
            }
        }

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            Siguiente();
        }
    }

    public void IniciarHistoria(Action callbackTerminar)
    {
        alTerminarHistoria = callbackTerminar;
        indiceFrase = 0;

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

        if (indiceFrase >= frasesHistoria.Length)
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

    private void MostrarFraseActual()
    {
        if (frasesHistoria == null || frasesHistoria.Length == 0)
        {
            TerminarHistoria();
            return;
        }

        string frase = frasesHistoria[Mathf.Clamp(indiceFrase, 0, frasesHistoria.Length - 1)];

        if (rutinaEscritura != null)
        {
            StopCoroutine(rutinaEscritura);
        }

        rutinaEscritura = StartCoroutine(EscribirTexto(frase));
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

        int totalCaracteres = frase != null ? frase.Length : 0;

        for (int i = 0; i <= totalCaracteres; i++)
        {
            if (textoDialogo != null)
            {
                textoDialogo.maxVisibleCharacters = i;
            }

            if (i > 0 && frase != null && i <= frase.Length)
            {
                char caracterActual = frase[i - 1];

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
            textoDialogo.maxVisibleCharacters = int.MaxValue;
        }
    }

    private void TerminarHistoria()
    {
        if (rutinaEscritura != null)
        {
            StopCoroutine(rutinaEscritura);
            rutinaEscritura = null;
        }

        escribiendo = false;
        HayAlgunaUIAbierta = false;

        if (panelHistoria != null)
        {
            panelHistoria.SetActive(false);
        }

        RestaurarEstadoJuego();
        alTerminarHistoria?.Invoke();
        alTerminarHistoria = null;
    }

    private void ConfigurarBotones()
    {
        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveListener(Siguiente);
            botonSiguiente.onClick.AddListener(Siguiente);
        }

        if (botonOmitirIntro != null)
        {
            botonOmitirIntro.onClick.RemoveListener(OmitirIntro);
            botonOmitirIntro.onClick.AddListener(OmitirIntro);
        }
    }

    private void ActualizarAyuda()
    {
        if (textoAyuda == null)
        {
            return;
        }

        textoAyuda.text = "Espacio / Enter: continuar";
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