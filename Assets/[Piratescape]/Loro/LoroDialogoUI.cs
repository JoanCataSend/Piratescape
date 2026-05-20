using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class LoroDialogoUI : MonoBehaviour
{
    public static bool HayAlgunaUIAbierta { get; private set; }
    public static int FrameCierre { get; private set; } = -1;

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoDialogo;
    [SerializeField] private TMP_Text textoMensaje;

    [Header("Botones")]
    [SerializeField] private Button botonGuardar;
    [SerializeField] private Button botonCargar;
    [SerializeField] private Button botonTutorial;
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private Button botonCerrar;

    [Header("Tutorial por misiones")]
    [SerializeField] private TutorialMisionesLoro tutorialMisiones;

    [Header("Comportamiento")]
    [SerializeField] private bool pausarJuegoAlAbrir = true;

    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private float timeScaleAnterior = 1f;
    private int frameApertura = -1;
    private bool modoMenuModal;

    public bool EstaAbierto => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
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
        if (modoMenuModal)
        {
            RestaurarEstadoJuego();
        }

        modoMenuModal = false;
        HayAlgunaUIAbierta = false;
    }

    private void Update()
    {
        if (!EstaAbierto || !modoMenuModal)
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
    }

    public void AbrirMenuLoro()
    {
        AbrirPanelComoMenuModal();

        EstablecerTitulo("Loro de la base");
        EstablecerDialogo("¡Graaak! ¿Que necesitas? Puedo guardar tu partida, cargarla o repetir el tutorial.");
        EstablecerMensaje("");
        MostrarBotonesMenu(true);
    }

    public void AbrirTutorialAutomatico()
    {
        IniciarTutorial();
    }

    public void IniciarTutorial()
    {
        if (tutorialMisiones == null)
        {
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();
        }

        CerrarSinMarcarFrame();

        if (tutorialMisiones != null)
        {
            tutorialMisiones.IniciarTutorialDesdeCero();
        }
        else
        {
            MostrarPanelTutorial("Tutorial", "No se ha encontrado TutorialMisionesLoro en la escena.", "Añade el script TutorialMisionesLoro al controlador del loro.");
        }
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

    public void CargarPartida()
    {
        if (GestorPartida.Instance == null)
        {
            EstablecerMensaje("No se ha encontrado el gestor de partida.");
            return;
        }

        bool cargada = GestorPartida.Instance.CargarPartida();
        EstablecerMensaje(cargada ? "Partida cargada correctamente." : "No hay ninguna partida guardada.");
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

    public void MostrarPanelTutorial(string titulo, string dialogo, string mensaje)
    {
        if (modoMenuModal)
        {
            RestaurarEstadoJuego();
        }

        modoMenuModal = false;
        HayAlgunaUIAbierta = false;

        if (panel != null)
        {
            panel.SetActive(true);
        }

        EstablecerTitulo(titulo);
        EstablecerDialogo(dialogo);
        EstablecerMensaje(mensaje);
        MostrarBotonesMenu(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OcultarPanelTutorial()
    {
        if (modoMenuModal)
        {
            return;
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }
    }

    private void AbrirPanelComoMenuModal()
    {
        frameApertura = Time.frameCount;

        if (!modoMenuModal)
        {
            GuardarEstadoJuego();
        }

        modoMenuModal = true;
        HayAlgunaUIAbierta = true;

        if (panel != null)
        {
            panel.SetActive(true);
        }
    }

    private void CerrarSinMarcarFrame()
    {
        bool estabaEnMenuModal = modoMenuModal;

        modoMenuModal = false;
        HayAlgunaUIAbierta = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (estabaEnMenuModal)
        {
            RestaurarEstadoJuego();
        }
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

    private void MostrarBotonesMenu(bool visibles)
    {
        if (botonGuardar != null)
        {
            botonGuardar.gameObject.SetActive(visibles);
        }

        if (botonCargar != null)
        {
            botonCargar.gameObject.SetActive(visibles);
        }

        if (botonTutorial != null)
        {
            botonTutorial.gameObject.SetActive(visibles);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(false);
        }

        if (botonCerrar != null)
        {
            botonCerrar.gameObject.SetActive(visibles);
        }
    }

    private void ConfigurarBotones()
    {
        if (botonGuardar != null)
        {
            botonGuardar.onClick.RemoveListener(GuardarPartida);
            botonGuardar.onClick.AddListener(GuardarPartida);
        }

        if (botonCargar != null)
        {
            botonCargar.onClick.RemoveListener(CargarPartida);
            botonCargar.onClick.AddListener(CargarPartida);
        }

        if (botonTutorial != null)
        {
            botonTutorial.onClick.RemoveListener(IniciarTutorial);
            botonTutorial.onClick.AddListener(IniciarTutorial);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(false);
        }

        if (botonCerrar != null)
        {
            botonCerrar.onClick.RemoveListener(Cerrar);
            botonCerrar.onClick.AddListener(Cerrar);
        }
    }

    private void EstablecerTitulo(string texto)
    {
        if (textoTitulo != null)
        {
            textoTitulo.text = texto;
        }
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
