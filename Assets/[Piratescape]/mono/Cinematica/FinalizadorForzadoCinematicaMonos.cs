using System.Collections;
using UnityEngine;

public sealed class FinalizadorForzadoCinematicaMonos : MonoBehaviour
{
    [Header("Cámaras")]
    [Tooltip("La cámara normal del jugador.")]
    [SerializeField] private GameObject camaraJugador;

    [Tooltip("La cámara utilizada durante la cinemática de los monos.")]
    [SerializeField] private GameObject camaraMonos;

    [Header("Duración obligatoria")]
    [Tooltip("Tiempo máximo que permanecerá activa la cámara de los monos.")]
    [Min(0.1f)]
    [SerializeField] private float duracionCinematica = 15f;

    [Header("Opcional: componentes del jugador")]
    [Tooltip("Scripts que deben volver a activarse al terminar la cinemática.")]
    [SerializeField] private Behaviour[] componentesJugadorAReactivar;

    private Coroutine rutinaTemporizador;
    private bool camaraDetectadaActiva;

    private void Awake()
    {
        RestaurarCamaraJugador();
    }

    private void Update()
    {
        if (camaraMonos == null)
        {
            return;
        }

        bool camaraMonosActiva =
            camaraMonos.activeInHierarchy;

        /*
         * Detectamos el momento exacto en el que CamaraMonos
         * pasa de estar apagada a estar encendida.
         */
        if (camaraMonosActiva && !camaraDetectadaActiva)
        {
            camaraDetectadaActiva = true;
            IniciarTemporizadorForzado();
        }

        /*
         * Si otro script la desactiva normalmente,
         * reiniciamos el detector para la siguiente cinemática.
         */
        if (!camaraMonosActiva && camaraDetectadaActiva)
        {
            camaraDetectadaActiva = false;

            if (rutinaTemporizador != null)
            {
                StopCoroutine(rutinaTemporizador);
                rutinaTemporizador = null;
            }
        }
    }

    private void IniciarTemporizadorForzado()
    {
        if (rutinaTemporizador != null)
        {
            StopCoroutine(rutinaTemporizador);
        }

        rutinaTemporizador =
            StartCoroutine(TemporizadorForzadoRoutine());
    }

    private IEnumerator TemporizadorForzadoRoutine()
    {
        /*
         * WaitForSecondsRealtime funciona aunque Time.timeScale
         * sea 0 o el juego esté temporalmente pausado.
         */
        yield return new WaitForSecondsRealtime(
            duracionCinematica
        );

        rutinaTemporizador = null;

        FinalizarCinematicaForzosamente();
    }

    public void FinalizarCinematicaForzosamente()
    {
        /*
         * Primero apagamos obligatoriamente la cámara de los monos.
         */
        if (camaraMonos != null)
        {
            camaraMonos.SetActive(false);
        }

        /*
         * Después encendemos la cámara del jugador.
         */
        if (camaraJugador != null)
        {
            camaraJugador.SetActive(true);
        }
        else
        {
            Debug.LogWarning(
                "FinalizadorForzadoCinematicaMonos: " +
                "falta asignar Camara Jugador.",
                this
            );
        }

        /*
         * Reactivamos opcionalmente los scripts de movimiento
         * o control del jugador.
         */
        if (componentesJugadorAReactivar != null)
        {
            for (
                int i = 0;
                i < componentesJugadorAReactivar.Length;
                i++
            )
            {
                if (componentesJugadorAReactivar[i] != null)
                {
                    componentesJugadorAReactivar[i].enabled = true;
                }
            }
        }

        camaraDetectadaActiva = false;

        Debug.Log(
            "Cinemática de monos finalizada obligatoriamente. " +
            "CamaraMonos apagada y cámara del jugador restaurada.",
            this
        );
    }

    private void RestaurarCamaraJugador()
    {
        if (camaraMonos != null)
        {
            camaraMonos.SetActive(false);
        }

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(true);
        }

        camaraDetectadaActiva = false;
    }

    private void OnDisable()
    {
        if (rutinaTemporizador != null)
        {
            StopCoroutine(rutinaTemporizador);
            rutinaTemporizador = null;
        }
    }

    [ContextMenu("Debug/Finalizar cinemática ahora")]
    private void DebugFinalizarAhora()
    {
        FinalizarCinematicaForzosamente();
    }
}