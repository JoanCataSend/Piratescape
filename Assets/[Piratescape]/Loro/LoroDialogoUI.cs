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

    [Header("Tutorial")]
    [TextArea(2, 5)]
    [SerializeField] private string[] frasesTutorial = new string[]
    {
        "¡Graaak! Bienvenido a la isla. Muevete con WASD y mira alrededor con el raton.",
        "Recoge recursos acercandote a ellos y pulsa E cuando aparezca el mensaje.",
        "Tu inventario esta abajo. Cambia de slot con los numeros o con la rueda del raton.",
        "Habla con el fantasma para abrir la tienda. Compra objetos utiles para mejorar tu base.",
        "El cofre sirve para guardar objetos. Acercate y pulsa E para abrirlo.",
        "Cuando quieras guardar la partida, vuelve a hablar conmigo. ¡Graaak!"
    };

    [Header("Comportamiento")]
    [SerializeField] private bool pausarJuegoAlAbrir = true;

    private int indiceTutorial;
    private bool mostrandoTutorial;
    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private float timeScaleAnterior = 1f;
    private int frameApertura = -1;

    public bool EstaAbierto => panel != null && panel.activeSelf;

    private void Awake()
    {
        if (panel != null)
        {
            panel.SetActive(false);
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
                return;
            }

            if (mostrandoTutorial && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame))
            {
                SiguienteTutorial();
            }
        }
    }

    public void AbrirMenuLoro()
    {
        AbrirPanelBase();
        mostrandoTutorial = false;
        indiceTutorial = 0;

        EstablecerTitulo("Loro de la base");
        EstablecerDialogo("¡Graaak! ¿Que necesitas? Puedo guardar tu partida o repetir el tutorial.");
        EstablecerMensaje("");
        ActualizarBotones();
    }

    public void AbrirTutorialAutomatico()
    {
        AbrirPanelBase();
        IniciarTutorial();
    }

    public void IniciarTutorial()
    {
        mostrandoTutorial = true;
        indiceTutorial = 0;
        MostrarFraseTutorialActual();
        ActualizarBotones();
    }

    public void SiguienteTutorial()
    {
        if (!mostrandoTutorial)
        {
            return;
        }

        indiceTutorial++;

        if (indiceTutorial >= frasesTutorial.Length)
        {
            TerminarTutorial();
            return;
        }

        MostrarFraseTutorialActual();
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

        mostrandoTutorial = false;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        RestaurarEstadoJuego();
        HayAlgunaUIAbierta = false;
        FrameCierre = Time.frameCount;
    }

    private void AbrirPanelBase()
    {
        frameApertura = Time.frameCount;

        if (!EstaAbierto)
        {
            GuardarEstadoJuego();
        }

        if (panel != null)
        {
            panel.SetActive(true);
        }

        HayAlgunaUIAbierta = true;
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

    private void MostrarFraseTutorialActual()
    {
        EstablecerTitulo("Tutorial del loro");

        if (frasesTutorial == null || frasesTutorial.Length == 0)
        {
            EstablecerDialogo("No hay frases de tutorial configuradas.");
            return;
        }

        indiceTutorial = Mathf.Clamp(indiceTutorial, 0, frasesTutorial.Length - 1);
        EstablecerDialogo(frasesTutorial[indiceTutorial]);
        EstablecerMensaje("Paso " + (indiceTutorial + 1) + "/" + frasesTutorial.Length + " - Pulsa Espacio o Enter para continuar.");
    }

    private void TerminarTutorial()
    {
        mostrandoTutorial = false;

        if (GestorPartida.Instance != null)
        {
            GestorPartida.Instance.MarcarTutorialCompletado(true);
        }

        EstablecerTitulo("Tutorial completado");
        EstablecerDialogo("¡Graaak! Ya sabes lo basico. Vuelve a hablar conmigo cuando quieras guardar la partida.");
        EstablecerMensaje("Tutorial completado.");
        ActualizarBotones();
    }

    private void ActualizarBotones()
    {
        if (botonGuardar != null)
        {
            botonGuardar.gameObject.SetActive(!mostrandoTutorial);
        }

        if (botonCargar != null)
        {
            botonCargar.gameObject.SetActive(!mostrandoTutorial);
        }

        if (botonTutorial != null)
        {
            botonTutorial.gameObject.SetActive(!mostrandoTutorial);
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(mostrandoTutorial);
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
            botonSiguiente.onClick.RemoveListener(SiguienteTutorial);
            botonSiguiente.onClick.AddListener(SiguienteTutorial);
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
