using System.Collections;
using UnityEngine;

public sealed class ControladorInicioLoro : MonoBehaviour
{
    [Header("Inicio")]
    [SerializeField] private bool iniciarAlEmpezar = true;
    [SerializeField] private float retrasoInicio = 0.25f;

    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private Transform puntoAparicionFrenteLoro;
    [SerializeField] private Transform loro;
    [SerializeField] private HistoriaInicioLoroUI historiaInicioUI;
    [SerializeField] private TutorialMisionesLoro tutorialMisiones;

    [Header("Aparicion del jugador")]
    [SerializeField] private bool colocarJugadorFrenteAlLoro = true;
    [SerializeField] private bool mirarAlLoroAlAparecer = true;

    private CharacterController characterControllerJugador;

    private IEnumerator Start()
    {
        if (!iniciarAlEmpezar)
        {
            yield break;
        }

        yield return new WaitForSecondsRealtime(retrasoInicio);

        BuscarReferenciasSiFaltan();

        if (DebeOmitirInicioPorPartidaYaAvanzada())
        {
            yield break;
        }

        ColocarJugadorSiHaceFalta();

        if (historiaInicioUI != null)
        {
            historiaInicioUI.IniciarHistoria(EmpezarTutorial);
        }
        else
        {
            EmpezarTutorial();
        }
    }

    private bool DebeOmitirInicioPorPartidaYaAvanzada()
    {
        if (GestorPartida.PartidaCargadaEnEsteInicio)
        {
            return true;
        }

        if (GestorPartida.Instance != null && GestorPartida.Instance.TutorialCompletado)
        {
            return true;
        }

        return false;
    }

    public void EmpezarTutorial()
    {
        BuscarReferenciasSiFaltan();

        if (tutorialMisiones != null)
        {
            tutorialMisiones.IniciarTutorialDesdeCero();
        }
    }

    private void ColocarJugadorSiHaceFalta()
    {
        if (!colocarJugadorFrenteAlLoro || jugador == null || puntoAparicionFrenteLoro == null)
        {
            return;
        }

        if (characterControllerJugador == null)
        {
            characterControllerJugador = jugador.GetComponent<CharacterController>();
        }

        bool controllerEstabaActivo = characterControllerJugador != null && characterControllerJugador.enabled;

        if (characterControllerJugador != null)
        {
            characterControllerJugador.enabled = false;
        }

        jugador.position = puntoAparicionFrenteLoro.position;

        if (mirarAlLoroAlAparecer && loro != null)
        {
            Vector3 direccion = loro.position - jugador.position;
            direccion.y = 0f;

            if (direccion.sqrMagnitude > 0.01f)
            {
                jugador.rotation = Quaternion.LookRotation(direccion.normalized, Vector3.up);
            }
        }

        if (characterControllerJugador != null)
        {
            characterControllerJugador.enabled = controllerEstabaActivo;
        }
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (jugador == null)
        {
            JugadorActivador activador = FindFirstObjectByType<JugadorActivador>();

            if (activador != null)
            {
                jugador = activador.transform;
            }
        }

        if (characterControllerJugador == null && jugador != null)
        {
            characterControllerJugador = jugador.GetComponent<CharacterController>();
        }

        if (historiaInicioUI == null)
        {
            historiaInicioUI = FindFirstObjectByType<HistoriaInicioLoroUI>();
        }

        if (tutorialMisiones == null)
        {
            tutorialMisiones = FindFirstObjectByType<TutorialMisionesLoro>();
        }
    }
}
