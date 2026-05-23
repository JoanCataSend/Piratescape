using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class HistoriaInicioLoroUI : MonoBehaviour
{
    public static bool HayAlgunaUIAbierta { get; private set; }

    [Header("Panel de historia")]
    [SerializeField] private GameObject panelHistoria;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoDialogo;
    [SerializeField] private TMP_Text textoAyuda;

    [Header("Botones")]
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private Button botonOmitirIntro;

    [Header("Texto")]
    [TextArea(2, 6)]
    [SerializeField] private string[] frasesHistoria = new string[]
    {
        "¡Graaak! Despierta, capitán... o lo que quede de ti.",
        "Te has metido en un buen lío. El pirata se cayó del barco borracho y ha terminado tirado en esta isla.",
        "Si quieres salir de aquí, tendrás que reunir recursos, mejorar la base y construir un barco.",
        "Aunque... dicen que hay tres gemas escondidas por la isla. Si las reúnes, quizá despiertes en tu barco como si todo hubiera sido un sueño.",
        "Primero aprende lo básico. Muévete, mira alrededor y hazme caso si quieres sobrevivir. ¡Graaak!"
    };

    [Header("Efecto escritura")]
    [SerializeField] private float segundosPorCaracter = 0.025f;
    [SerializeField] private string tituloHistoria = "El loro de la isla";

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

        if (textoTitulo != null)
        {
            textoTitulo.text = tituloHistoria;
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

        if (textoAyuda != null)
        {
            textoAyuda.text = "Espacio / Enter: continuar | Omitir intro: saltar al tutorial";
        }
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

            yield return new WaitForSecondsRealtime(segundosPorCaracter);
        }

        escribiendo = false;
        rutinaEscritura = null;
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
