using UnityEngine;
using GuevaraVideojocs.TimeSystem;

public sealed class SistemaSaludJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem sistemaTiempo;
    [SerializeField] private VisualizadorBarrasEstado visualizador;

    [Header("Salud")]
    [SerializeField] private float saludMaxima = 100f;
    [SerializeField] private float saludActual = 100f;

    [Header("Configuracion desgaste")]
    [SerializeField] private float perdidaSaludPorMinuto = 0.1f;

    private void OnEnable()
    {
        if (sistemaTiempo != null)
        {
            sistemaTiempo.OnTimeChanged += AlCambiarTiempo;
        }
    }

    private void OnDisable()
    {
        if (sistemaTiempo != null)
        {
            sistemaTiempo.OnTimeChanged -= AlCambiarTiempo;
        }
    }

    private void Start()
    {
        ActualizarHUD();
    }

    private void AlCambiarTiempo(int dia, int hora, int minuto)
    {
        ReducirSalud();
    }

    private void ReducirSalud()
    {
        saludActual -= perdidaSaludPorMinuto;
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);

        ActualizarHUD();
    }

    private void ActualizarHUD()
    {
        if (visualizador != null)
        {
            visualizador.EstablecerSalud(saludActual, saludMaxima);
        }
    }
    public void AumentarSalud(float cantidad)
    {
        saludActual += cantidad;
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);
        ActualizarHUD(); // Esto actualizará tu VisualizadorBarrasEstado
    }
}