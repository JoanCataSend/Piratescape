using System;
using System.Collections;
using System.Collections.Generic;
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

    private int indiceFrase;
    private bool escribiendo;
    private Coroutine rutinaEscritura;
    private Action alTerminarHistoria;
    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private float timeScaleAnterior = 1f;

    private readonly Dictionary<char, AudioClip> clipsPorLetra = new Dictionary<char, AudioClip>();

    private void Awake()
    {
        if (panelHistoria != null)
        {
            panelHistoria.SetActive(false);
        }

        PrepararAudioVoz();
        PrepararClipsAnimalese();
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

    private IEnumerator EscribirTexto(string fraseOriginal)
    {
        escribiendo = true;

        string frase = shortenWords ? AcortarPalabras(fraseOriginal) : fraseOriginal;

        if (textoDialogo != null)
        {
            textoDialogo.text = fraseOriginal;
            textoDialogo.maxVisibleCharacters = 0;
            textoDialogo.ForceMeshUpdate();
        }

        int totalCaracteres = fraseOriginal != null ? fraseOriginal.Length : 0;

        for (int i = 0; i <= totalCaracteres; i++)
        {
            if (textoDialogo != null)
            {
                textoDialogo.maxVisibleCharacters = i;
            }

            if (i > 0 && fraseOriginal != null && i <= fraseOriginal.Length)
            {
                char caracterVisible = fraseOriginal[i - 1];

                if (DebeSonarCaracter(caracterVisible, i))
                {
                    char caracterSonido = ObtenerCaracterParaSonido(fraseOriginal, i - 1);
                    ReproducirSonidoLetra(caracterSonido);
                }
            }

            yield return new WaitForSecondsRealtime(segundosPorCaracter);
        }

        escribiendo = false;
        rutinaEscritura = null;
    }

    private char ObtenerCaracterParaSonido(string fraseOriginal, int indice)
    {
        if (!shortenWords)
        {
            return fraseOriginal[indice];
        }

        char actual = fraseOriginal[indice];

        if (!char.IsLetter(actual))
        {
            return actual;
        }

        int inicioPalabra = indice;
        while (inicioPalabra > 0 && char.IsLetter(fraseOriginal[inicioPalabra - 1]))
        {
            inicioPalabra--;
        }

        int finPalabra = indice;
        while (finPalabra < fraseOriginal.Length - 1 && char.IsLetter(fraseOriginal[finPalabra + 1]))
        {
            finPalabra++;
        }

        if (indice == inicioPalabra || indice == finPalabra)
        {
            return actual;
        }

        return '\0';
    }

    private string AcortarPalabras(string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return texto;
        }

        string resultado = "";
        int i = 0;

        while (i < texto.Length)
        {
            if (!char.IsLetter(texto[i]))
            {
                resultado += texto[i];
                i++;
                continue;
            }

            int inicio = i;

            while (i < texto.Length && char.IsLetter(texto[i]))
            {
                i++;
            }

            int fin = i - 1;
            int longitud = fin - inicio + 1;

            if (longitud <= 2)
            {
                resultado += texto.Substring(inicio, longitud);
            }
            else
            {
                resultado += texto[inicio];
                resultado += texto[fin];
            }
        }

        return resultado;
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
            Debug.LogWarning("HistoriaInicioLoroUI: falta asignar Animalese Library.", this);
            return;
        }

        int frecuenciaMuestreo = animaleseLibrary.frequency;
        int canales = animaleseLibrary.channels;

        int muestrasPorLetraBiblioteca = Mathf.RoundToInt(segundosLetraEnLibreria * frecuenciaMuestreo);
        int muestrasPorLetraSalida = Mathf.RoundToInt(segundosLetraSalida * frecuenciaMuestreo);

        if (muestrasPorLetraBiblioteca <= 0 || muestrasPorLetraSalida <= 0)
        {
            Debug.LogWarning("HistoriaInicioLoroUI: duración de letra inválida.", this);
            return;
        }

        float[] datosOriginales = new float[animaleseLibrary.samples * canales];

        try
        {
            animaleseLibrary.GetData(datosOriginales, 0);
        }
        catch
        {
            Debug.LogWarning("HistoriaInicioLoroUI: no se pudo leer animalese.wav. En Import Settings pon Load Type = Decompress On Load.", this);
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