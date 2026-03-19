using UnityEngine;
using UnityEngine.UI;

public sealed class VisualizadorBarrasEstado : MonoBehaviour
{
    [Header("Referencias visuales")]
    [SerializeField] private Image rellenoBarraSalud;
    [SerializeField] private Image rellenoBarraEnergia;

    [Header("Salud")]
    [SerializeField] private float saludActual = 100f;
    [SerializeField] private float saludMaxima = 100f;

    [Header("Energia")]
    [SerializeField] private float energiaActual = 100f;
    [SerializeField] private float energiaMaxima = 100f;

    private void Start()
    {
        ActualizarBarras();
    }

    private void OnValidate()
    {
        AjustarValores();
        ActualizarBarrasEnEditor();
    }

    public void EstablecerSalud(float saludActualNueva, float saludMaximaNueva)
    {
        saludMaxima = Mathf.Max(0f, saludMaximaNueva);
        saludActual = Mathf.Clamp(saludActualNueva, 0f, saludMaxima);
        ActualizarBarraSalud();
    }

    public void EstablecerEnergia(float energiaActualNueva, float energiaMaximaNueva)
    {
        energiaMaxima = Mathf.Max(0f, energiaMaximaNueva);
        energiaActual = Mathf.Clamp(energiaActualNueva, 0f, energiaMaxima);
        ActualizarBarraEnergia();
    }

    public void EstablecerSaludActual(float nuevaSaludActual)
    {
        saludActual = Mathf.Clamp(nuevaSaludActual, 0f, saludMaxima);
        ActualizarBarraSalud();
    }

    public void EstablecerEnergiaActual(float nuevaEnergiaActual)
    {
        energiaActual = Mathf.Clamp(nuevaEnergiaActual, 0f, energiaMaxima);
        ActualizarBarraEnergia();
    }

    public void ActualizarBarras()
    {
        AjustarValores();
        ActualizarBarraSalud();
        ActualizarBarraEnergia();
    }

    private void AjustarValores()
    {
        saludMaxima = Mathf.Max(0f, saludMaxima);
        energiaMaxima = Mathf.Max(0f, energiaMaxima);

        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);
        energiaActual = Mathf.Clamp(energiaActual, 0f, energiaMaxima);
    }

    private void ActualizarBarraSalud()
    {
        if (rellenoBarraSalud == null)
        {
            return;
        }

        if (saludMaxima <= 0f)
        {
            rellenoBarraSalud.fillAmount = 0f;
            return;
        }

        rellenoBarraSalud.fillAmount = saludActual / saludMaxima;
    }


    private void ActualizarBarraEnergia()
    {
        if (rellenoBarraEnergia == null)
        {
            return;
        }

        if (energiaMaxima <= 0f)
        {
            rellenoBarraEnergia.fillAmount = 0f;
            return;
        }

        rellenoBarraEnergia.fillAmount = energiaActual / energiaMaxima;
    }

    private void ActualizarBarrasEnEditor()
    {
        if (!Application.isPlaying)
        {
            ActualizarBarraSalud();
            ActualizarBarraEnergia();
        }
    }
}