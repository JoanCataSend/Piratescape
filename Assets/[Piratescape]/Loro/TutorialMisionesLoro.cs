using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class TutorialMisionesLoro : MonoBehaviour
{
    public static TutorialMisionesLoro Instance { get; private set; }

    private enum PasoTutorial
    {
        MoverYMirar,
        Saltar,
        RecogerItem,
        GuardarPartida,
        Terminado
    }

    [Header("Referencias")]
    [SerializeField] private LoroDialogoUI loroDialogoUI;
    [SerializeField] private Transform jugador;
    [SerializeField] private PlayerInventory inventarioJugador;

    [Header("Mision 1 - Movimiento")]
    [SerializeField] private float distanciaNecesaria = 3f;
    [SerializeField] private float movimientoRatonNecesario = 80f;

    [Header("Comportamiento")]
    [SerializeField] private bool iniciarSoloUnaVez = true;
    [SerializeField] private float esperaEntreMisiones = 1f;

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
    }

    private void Start()
    {
        BuscarReferenciasSiFaltan();
    }

    private void Update()
    {
        if (!tutorialActivo || cambiandoPaso)
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
        cambiandoPaso = false;
        partidaGuardadaEnTutorial = false;

        CambiarAPaso(PasoTutorial.MoverYMirar);
    }

    public void IniciarTutorialAutomatico()
    {
        if (iniciarSoloUnaVez && GestorPartida.Instance != null && GestorPartida.Instance.TutorialCompletado)
        {
            return;
        }

        IniciarTutorialDesdeCero();
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

        MostrarObjetivo(
            "Tutorial del loro",
            "Mision 1: muévete y mira alrededor.",
            "Usa WASD para caminar y mueve el raton para mirar. Progreso: 0%"
        );
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
            "Tutorial del loro",
            "Mision 1: muévete y mira alrededor.",
            textoMovimiento + " | " + textoMirada + " | Total: " + Mathf.RoundToInt(progresoTotal * 100f) + "%"
        );

        if (seMovio && miro)
        {
            CompletarPaso(PasoTutorial.Saltar, "¡Bien! Ya sabes moverte y mirar alrededor.");
        }
    }

    private void PrepararPasoSaltar()
    {
        MostrarObjetivo(
            "Tutorial del loro",
            "Mision 2: salta una vez.",
            "Pulsa Espacio para saltar."
        );
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
            CompletarPaso(PasoTutorial.RecogerItem, "¡Perfecto! Ahora vamos a recoger un recurso.");
        }
    }

    private void PrepararPasoRecogerItem()
    {
        totalItemsInicial = ContarItemsInventario();

        MostrarObjetivo(
            "Tutorial del loro",
            "Mision 3: recoge un objeto.",
            "Acércate a un recurso y pulsa E cuando salga el mensaje de recoger."
        );
    }

    private void ActualizarPasoRecogerItem()
    {
        int totalActual = ContarItemsInventario();

        MostrarObjetivo(
            "Tutorial del loro",
            "Mision 3: recoge un objeto.",
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

        MostrarObjetivo(
            "Tutorial del loro",
            "Mision 4: guarda la partida con el loro.",
            "Vuelve al loro, pulsa E y dale a Guardar partida."
        );
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

        MostrarObjetivo("Objetivo completado", mensaje, "Siguiente mision en un momento...");

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
        cambiandoPaso = false;

        if (GestorPartida.Instance != null)
        {
            GestorPartida.Instance.MarcarTutorialCompletado(true);
        }

        MostrarObjetivo(
            "Tutorial completado",
            "¡Graaak! Ya sabes lo básico para empezar.",
            "Puedes volver a hablar conmigo para guardar, cargar o repetir el tutorial."
        );

        StartCoroutine(OcultarPanelTutorialDespuesDeUnMomento());
    }

    private IEnumerator OcultarPanelTutorialDespuesDeUnMomento()
    {
        yield return new WaitForSecondsRealtime(3f);

        if (loroDialogoUI != null && !LoroDialogoUI.HayAlgunaUIAbierta)
        {
            loroDialogoUI.OcultarPanelTutorial();
        }
    }

    private void MostrarObjetivo(string titulo, string dialogo, string mensaje)
    {
        if (loroDialogoUI == null)
        {
            BuscarReferenciasSiFaltan();
        }

        if (loroDialogoUI != null)
        {
            loroDialogoUI.MostrarPanelTutorial(titulo, dialogo, mensaje);
        }
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

    private void BuscarReferenciasSiFaltan()
    {
        if (loroDialogoUI == null)
        {
            loroDialogoUI = FindFirstObjectByType<LoroDialogoUI>();
        }

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
