using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public sealed class TutorialMisionesLoro : MonoBehaviour
{
    public static TutorialMisionesLoro Instance { get; private set; }
    public static bool TutorialActivoGlobal { get; private set; }
    public static bool BloquearMenuPausa { get; private set; }
    public static bool BloquearInteraccionLoro => Instance != null && Instance.tutorialActivo && Instance.pasoActual != PasoTutorial.GuardarPartida;

    private enum PasoTutorial
    {
        MoverYMirar,
        Saltar,
        RecogerItem,
        GuardarPartida,
        Terminado
    }

    [Header("Panel")]
    [SerializeField] private GameObject panelTutorial;
    [SerializeField] private TMP_Text textoObjetivo;
    [SerializeField] private TMP_Text textoProgreso;

    [Header("Boton opcional")]
    [SerializeField] private Button botonOmitirTutorial;

    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private PlayerInventory inventarioJugador;

    [Header("Mision 1 - Movimiento")]
    [SerializeField] private float distanciaNecesaria = 3f;
    [SerializeField] private float movimientoRatonNecesario = 80f;

    [Header("Comportamiento")]
    [SerializeField] private float esperaEntreMisiones = 1f;
    [SerializeField] private bool usarTeclaOParaOmitir = false;
    [SerializeField] private bool bloquearMenuPausaDuranteTutorial = false;

    private PasoTutorial pasoActual = PasoTutorial.Terminado;
    private bool tutorialActivo;
    private bool cambiandoPaso;
    private Vector3 posicionInicialPaso;
    private float movimientoRatonAcumulado;
    private int totalItemsInicial;
    private bool partidaGuardadaEnTutorial;

    public bool TutorialActivo => tutorialActivo;

    private void Awake()
    {
        Instance = this;

        if (panelTutorial != null)
        {
            panelTutorial.SetActive(false);
        }

        ConfigurarBotones();
        TutorialActivoGlobal = false;
        BloquearMenuPausa = false;
    }

    private void OnDisable()
    {
        TutorialActivoGlobal = false;
        BloquearMenuPausa = false;
    }

    private void Start()
    {
        BuscarReferenciasSiFaltan();
    }

    private void Update()
    {
        if (!tutorialActivo)
        {
            return;
        }

        if (usarTeclaOParaOmitir && Keyboard.current != null && Keyboard.current.oKey.wasPressedThisFrame)
        {
            OmitirTutorial();
            return;
        }

        if (cambiandoPaso)
        {
            return;
        }

        switch (pasoActual)
        {
            case PasoTutorial.MoverYMirar:
                ActualizarPasoMoverYMirar();
                break;

            case PasoTutorial.Saltar:
                ActualizarPasoSaltar();
                break;

            case PasoTutorial.RecogerItem:
                ActualizarPasoRecogerItem();
                break;

            case PasoTutorial.GuardarPartida:
                ActualizarPasoGuardarPartida();
                break;
        }
    }

    public void IniciarTutorialDesdeCero()
    {
        BuscarReferenciasSiFaltan();

        tutorialActivo = true;
        TutorialActivoGlobal = true;
        BloquearMenuPausa = bloquearMenuPausaDuranteTutorial;
        cambiandoPaso = false;
        partidaGuardadaEnTutorial = false;

        if (panelTutorial != null)
        {
            panelTutorial.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CambiarAPaso(PasoTutorial.MoverYMirar);
    }

    public void OmitirTutorial()
    {
        if (!tutorialActivo)
        {
            return;
        }

        tutorialActivo = false;
        TutorialActivoGlobal = false;
        BloquearMenuPausa = false;
        cambiandoPaso = false;
        pasoActual = PasoTutorial.Terminado;

        if (panelTutorial != null)
        {
            panelTutorial.SetActive(false);
        }

        if (GestorPartida.Instance != null)
        {
            GestorPartida.Instance.MarcarTutorialCompletado(true);
        }
    }

    public void NotificarPartidaGuardada()
    {
        if (!tutorialActivo)
        {
            return;
        }

        partidaGuardadaEnTutorial = true;
    }

    private void CambiarAPaso(PasoTutorial nuevoPaso)
    {
        pasoActual = nuevoPaso;

        switch (pasoActual)
        {
            case PasoTutorial.MoverYMirar:
                PrepararPasoMoverYMirar();
                break;

            case PasoTutorial.Saltar:
                PrepararPasoSaltar();
                break;

            case PasoTutorial.RecogerItem:
                PrepararPasoRecogerItem();
                break;

            case PasoTutorial.GuardarPartida:
                PrepararPasoGuardarPartida();
                break;

            case PasoTutorial.Terminado:
                TerminarTutorial();
                break;
        }
    }

    private void PrepararPasoMoverYMirar()
    {
        if (jugador != null)
        {
            posicionInicialPaso = jugador.position;
        }

        movimientoRatonAcumulado = 0f;
        MostrarObjetivo("Misión 1: muévete y mira alrededor.", "Usa WASD para caminar y mueve el ratón para mirar." + TextoOmitir());
    }

    private void ActualizarPasoMoverYMirar()
    {
        if (jugador == null)
        {
            BuscarReferenciasSiFaltan();
            return;
        }

        if (Mouse.current != null)
        {
            movimientoRatonAcumulado += Mouse.current.delta.ReadValue().magnitude;
        }

        float distancia = Vector3.Distance(posicionInicialPaso, jugador.position);
        bool seMovio = distancia >= distanciaNecesaria;
        bool miro = movimientoRatonAcumulado >= movimientoRatonNecesario;

        float progresoMovimiento = Mathf.Clamp01(distancia / distanciaNecesaria);
        float progresoMirada = Mathf.Clamp01(movimientoRatonAcumulado / movimientoRatonNecesario);
        float progresoTotal = (progresoMovimiento + progresoMirada) * 0.5f;

        string textoMovimiento = seMovio ? "Movimiento completado" : "Muévete: " + Mathf.RoundToInt(progresoMovimiento * 100f) + "%";
        string textoMirada = miro ? "Mirada completada" : "Mira alrededor: " + Mathf.RoundToInt(progresoMirada * 100f) + "%";

        MostrarObjetivo(
            "Misión 1: muévete y mira alrededor.",
            textoMovimiento + " | " + textoMirada + " | Total: " + Mathf.RoundToInt(progresoTotal * 100f) + "%"
        );

        if (seMovio && miro)
        {
            CompletarPaso(PasoTutorial.Saltar, "¡Bien! Ya sabes moverte y mirar alrededor.");
        }
    }

    private void PrepararPasoSaltar()
    {
        MostrarObjetivo("Misión 2: salta una vez.", "Pulsa Espacio para saltar." + TextoOmitir());
    }

    private void ActualizarPasoSaltar()
    {
        bool salto = false;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            salto = true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            salto = true;
        }

        if (salto)
        {
            CompletarPaso(PasoTutorial.RecogerItem, "¡Perfecto! Ahora recoge un recurso.");
        }
    }

    private void PrepararPasoRecogerItem()
    {
        totalItemsInicial = ContarItemsInventario();
        MostrarObjetivo("Misión 3: recoge un objeto.", "Acércate a un recurso y pulsa E cuando salga el mensaje." + TextoOmitir());
    }

    private void ActualizarPasoRecogerItem()
    {
        int totalActual = ContarItemsInventario();

        MostrarObjetivo(
            "Misión 3: recoge un objeto.",
            "Objetos en inventario: " + totalActual + " | Recoge cualquier recurso para continuar."
        );

        if (totalActual > totalItemsInicial)
        {
            CompletarPaso(PasoTutorial.GuardarPartida, "¡Recurso conseguido! Ahora aprende a guardar la partida.");
        }
    }

    private void PrepararPasoGuardarPartida()
    {
        partidaGuardadaEnTutorial = false;
        MostrarObjetivo("Misión 4: guarda la partida con el loro.", "Vuelve al loro, pulsa E y dale a Guardar partida." + TextoOmitir());
    }

    private void ActualizarPasoGuardarPartida()
    {
        if (partidaGuardadaEnTutorial)
        {
            CompletarPaso(PasoTutorial.Terminado, "¡Partida guardada! Tutorial terminado.");
        }
    }

    private void CompletarPaso(PasoTutorial siguientePaso, string mensaje)
    {
        if (cambiandoPaso)
        {
            return;
        }

        StartCoroutine(CompletarPasoRoutine(siguientePaso, mensaje));
    }

    private IEnumerator CompletarPasoRoutine(PasoTutorial siguientePaso, string mensaje)
    {
        cambiandoPaso = true;
        MostrarObjetivo("Objetivo completado", mensaje);

        if (esperaEntreMisiones > 0f)
        {
            yield return new WaitForSecondsRealtime(esperaEntreMisiones);
        }

        cambiandoPaso = false;
        CambiarAPaso(siguientePaso);
    }

    private void TerminarTutorial()
    {
        tutorialActivo = false;
        TutorialActivoGlobal = false;
        BloquearMenuPausa = false;
        cambiandoPaso = false;

        if (GestorPartida.Instance != null)
        {
            GestorPartida.Instance.MarcarTutorialCompletado(true);
        }

        MostrarObjetivo("Tutorial completado", "¡Graaak! Ya sabes lo básico para empezar. Puedes volver a hablar conmigo para guardar o repetir el tutorial.");
        StartCoroutine(OcultarPanelDespuesDeUnMomento());
    }

    private IEnumerator OcultarPanelDespuesDeUnMomento()
    {
        yield return new WaitForSecondsRealtime(3f);

        if (!tutorialActivo && panelTutorial != null)
        {
            panelTutorial.SetActive(false);
        }
    }

    private void MostrarObjetivo(string objetivo, string progreso)
    {
        if (panelTutorial != null && !panelTutorial.activeSelf)
        {
            panelTutorial.SetActive(true);
        }

        if (textoObjetivo != null)
        {
            textoObjetivo.text = objetivo;
        }

        if (textoProgreso != null)
        {
            textoProgreso.text = progreso;
        }
    }

    private string TextoOmitir()
    {
        if (usarTeclaOParaOmitir)
        {
            return " Pulsa O para omitir.";
        }

        if (botonOmitirTutorial != null)
        {
            return " Puedes omitir desde el botón.";
        }

        return "";
    }

    private int ContarItemsInventario()
    {
        if (inventarioJugador == null)
        {
            BuscarReferenciasSiFaltan();
        }

        if (inventarioJugador == null)
        {
            return 0;
        }

        int total = 0;

        for (int i = 0; i < inventarioJugador.GetSlots().Count; i++)
        {
            InventorySlot slot = inventarioJugador.GetSlot(i);

            if (slot == null || slot.IsEmpty())
            {
                continue;
            }

            total += slot.amount;
        }

        return total;
    }

    private void ConfigurarBotones()
    {
        if (botonOmitirTutorial != null)
        {
            botonOmitirTutorial.onClick.RemoveListener(OmitirTutorial);
            botonOmitirTutorial.onClick.AddListener(OmitirTutorial);
        }
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (inventarioJugador == null)
        {
            inventarioJugador = FindFirstObjectByType<PlayerInventory>();
        }

        if (jugador == null)
        {
            PlayerInventory inventario = inventarioJugador != null ? inventarioJugador : FindFirstObjectByType<PlayerInventory>();

            if (inventario != null)
            {
                jugador = inventario.transform;
            }
        }
    }
}
