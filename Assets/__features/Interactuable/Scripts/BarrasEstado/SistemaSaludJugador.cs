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

    private bool estaMuerto;

    public float SaludActual => saludActual;
    public float SaludMaxima => saludMaxima;
    public bool EstaMuerto => estaMuerto;

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
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);
        ActualizarHUD();
        ComprobarMuerte();
    }

    private void AlCambiarTiempo(int dia, int hora, int minuto)
    {
        if (estaMuerto)
        {
            return;
        }

        ReducirSaludPorTiempo();
    }

    private void ReducirSaludPorTiempo()
    {
        saludActual -= perdidaSaludPorMinuto;
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);

        ActualizarHUD();
        ComprobarMuerte();
    }

    public void ReducirSaludDirecta(float cantidad)
    {
        if (cantidad <= 0f || estaMuerto)
        {
            return;
        }

        saludActual -= cantidad;
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);

        ActualizarHUD();
        ComprobarMuerte();
    }

    public void AumentarSalud(float cantidad)
    {
        if (cantidad <= 0f || estaMuerto)
        {
            return;
        }

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

    private void ComprobarMuerte()
    {
        if (estaMuerto)
        {
            return;
        }

        if (saludActual > 0f)
        {
            return;
        }

        estaMuerto = true;

        if (sistemaTiempo != null)
        {
            sistemaTiempo.PauseTime();
        }
    }
}