using UnityEngine;

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

    [Header("Debug")]
    [SerializeField] private bool modoDiosActivo;

    private bool estaMuerto;

    public float SaludActual => saludActual;
    public float SaludMaxima => saludMaxima;
    public bool EstaMuerto => estaMuerto;
    public bool ModoDiosActivo => modoDiosActivo;

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
        if (estaMuerto || modoDiosActivo)
        {
            return;
        }

        ReducirSaludPorTiempo();
    }

    private void ReducirSaludPorTiempo()
    {
        if (modoDiosActivo)
        {
            return;
        }

        saludActual -= perdidaSaludPorMinuto;
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);

        ActualizarHUD();
        ComprobarMuerte();
    }

    public void ReducirSaludDirecta(float cantidad)
    {
        if (cantidad <= 0f || estaMuerto || modoDiosActivo)
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

    public void EstablecerModoDios(bool activo)
    {
        modoDiosActivo = activo;

        if (modoDiosActivo)
        {
            estaMuerto = false;
            saludActual = saludMaxima;
            ActualizarHUD();
        }

        Debug.Log("SistemaSaludJugador: GodMode = " + modoDiosActivo);
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
        if (estaMuerto || modoDiosActivo)
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