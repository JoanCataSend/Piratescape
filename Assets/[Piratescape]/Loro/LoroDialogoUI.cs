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

    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private float timeScaleAnterior = 1f;
    private int frameApertura = -1;

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

        ConfigurarBotones();
        HayAlgunaUIAbierta = false;
    }

    private void OnDisable()
    {
        if (EstaAbierto)
        {
            RestaurarEstadoJuego();
        }

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
        if (textoDialogo != null)
        {
            textoDialogo.text = texto;
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
