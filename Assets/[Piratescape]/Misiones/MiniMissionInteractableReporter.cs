using UnityEngine;
using UnityEngine.InputSystem;

public sealed class MiniMissionInteractableReporter : MonoBehaviour
{
    [Header("Evento")]
    [SerializeField] private string eventoAReportar = "hablar_comerciante";
    [SerializeField] private bool reportarUnaSolaVez = true;

    [Header("Jugador")]
    [SerializeField] private string tagJugador = "Player";
    [SerializeField] private Transform jugador;
    [SerializeField] private float rangoInteraccion = 2.5f;
    [SerializeField] private bool usarTrigger = true;

    [Header("Input")]
    [SerializeField] private bool requerirPulsarInteraccion = true;
    [SerializeField] private bool usarTeclaE = true;
    [SerializeField] private bool permitirMando = true;

    [Header("Prompt opcional")]
    [SerializeField] private bool mostrarPrompt = true;
    [SerializeField] private string textoPrompt = "Hablar";

    private bool jugadorDentro;
    private bool yaReportado;
    private bool promptMostrado;

    private void Awake()
    {
        BuscarJugadorSiFalta();
    }

    private void Update()
    {
        if (yaReportado && reportarUnaSolaVez)
        {
            OcultarPrompt();
            return;
        }

        bool cerca = EstaJugadorCerca();

        if (!cerca)
        {
            OcultarPrompt();
            return;
        }

        MostrarPrompt();

        if (!requerirPulsarInteraccion || SeHaPulsadoInteraccion())
        {
            ReportarEvento();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!usarTrigger || !EsJugador(other))
        {
            return;
        }

        jugadorDentro = true;
        jugador = other.transform;

        if (!requerirPulsarInteraccion)
        {
            ReportarEvento();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!usarTrigger || !EsJugador(other))
        {
            return;
        }

        jugadorDentro = false;
        OcultarPrompt();
    }

    public void ReportarEvento()
    {
        if (yaReportado && reportarUnaSolaVez)
        {
            return;
        }

        MiniMissionManager.ReportarEventoGlobal(eventoAReportar);
        yaReportado = true;
        OcultarPrompt();
    }

    private bool EstaJugadorCerca()
    {
        if (usarTrigger && jugadorDentro)
        {
            return true;
        }

        BuscarJugadorSiFalta();

        if (jugador == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, jugador.position) <= rangoInteraccion;
    }

    private bool EsJugador(Collider other)
    {
        if (other == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(tagJugador) && other.CompareTag(tagJugador))
        {
            return true;
        }

        return other.GetComponentInParent<PlayerInventory>() != null;
    }

    private void BuscarJugadorSiFalta()
    {
        if (jugador != null)
        {
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag(tagJugador);

        if (player != null)
        {
            jugador = player.transform;
        }
    }

    private bool SeHaPulsadoInteraccion()
    {
        if (usarTeclaE && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }

        if (permitirMando && Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private void MostrarPrompt()
    {
        if (!mostrarPrompt || promptMostrado || InteractionUI.Instance == null)
        {
            return;
        }

        InteractionUI.Instance.Show(this, textoPrompt);
        promptMostrado = true;
    }

    private void OcultarPrompt()
    {
        if (!promptMostrado || InteractionUI.Instance == null)
        {
            return;
        }

        InteractionUI.Instance.Hide(this);
        promptMostrado = false;
    }
}
