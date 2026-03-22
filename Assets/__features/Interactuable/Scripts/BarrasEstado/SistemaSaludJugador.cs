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

    public float SaludActual => saludActual;
    public float SaludMaxima => saludMaxima;

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
        ReducirSaludPorTiempo();
    }

    private void ReducirSaludPorTiempo()
    {
        saludActual -= perdidaSaludPorMinuto;
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);
        ActualizarHUD();
    }

    public void ReducirSaludDirecta(float cantidad)
    {
        if (cantidad <= 0f) return;

        saludActual -= cantidad;
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);
        ActualizarHUD();
    }

    public void AumentarSalud(float cantidad)
    {
        if (cantidad <= 0f) return;

        saludActual += cantidad;
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
}