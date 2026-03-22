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

    // Actualiza todas las barras
    public void ActualizarBarras()
    {
        ActualizarBarraSalud();
        ActualizarBarraEnergia();
    }

    // Salud
    public void EstablecerSaludActual(float nuevaSaludActual)
    {
        saludActual = Mathf.Clamp(nuevaSaludActual, 0f, saludMaxima);
        ActualizarBarraSalud();
    }

    private void ActualizarBarraSalud()
    {
        if (rellenoBarraSalud != null)
            rellenoBarraSalud.fillAmount = saludMaxima > 0f ? saludActual / saludMaxima : 0f;
    }

    // Energía
    public void EstablecerEnergiaActual(float nuevaEnergiaActual)
    {
        energiaActual = Mathf.Clamp(nuevaEnergiaActual, 0f, energiaMaxima);
        ActualizarBarraEnergia();
    }

    private void ActualizarBarraEnergia()
    {
        if (rellenoBarraEnergia != null)
            rellenoBarraEnergia.fillAmount = energiaMaxima > 0f ? energiaActual / energiaMaxima : 0f;
    }
    public void EstablecerSalud(float nuevaSaludActual, float nuevaSaludMaxima)
    {
        saludMaxima = Mathf.Max(0f, nuevaSaludMaxima);
        saludActual = Mathf.Clamp(nuevaSaludActual, 0f, saludMaxima);
        ActualizarBarraSalud();
    }
}