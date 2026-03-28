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
        // 🔥 IMPORTANTE: asegurar valores válidos antes de pintar
        saludMaxima = Mathf.Max(0f, saludMaxima);
        saludActual = Mathf.Clamp(saludActual, 0f, saludMaxima);

        energiaMaxima = Mathf.Max(0f, energiaMaxima);
        energiaActual = Mathf.Clamp(energiaActual, 0f, energiaMaxima);

        ActualizarBarras();
    }

    public void ActualizarBarras()
    {
        ActualizarBarraSalud();
        ActualizarBarraEnergia();
    }

    // SALUD
    public void EstablecerSaludActual(float nuevaSaludActual)
    {
        saludActual = Mathf.Clamp(nuevaSaludActual, 0f, saludMaxima);
        ActualizarBarraSalud();
    }

    public void EstablecerSalud(float nuevaSaludActual, float nuevaSaludMaxima)
    {
        saludMaxima = Mathf.Max(0f, nuevaSaludMaxima);
        saludActual = Mathf.Clamp(nuevaSaludActual, 0f, saludMaxima);
        ActualizarBarraSalud();
    }

    private void ActualizarBarraSalud()
    {
        if (rellenoBarraSalud != null)
        {
            rellenoBarraSalud.fillAmount = saludMaxima > 0f ? saludActual / saludMaxima : 0f;
        }
    }

    // ENERGIA
    public void EstablecerEnergiaActual(float nuevaEnergiaActual)
    {
        energiaActual = Mathf.Clamp(nuevaEnergiaActual, 0f, energiaMaxima);
        ActualizarBarraEnergia();
    }

    public void EstablecerEnergia(float nuevaEnergiaActual, float nuevaEnergiaMaxima)
    {
        energiaMaxima = Mathf.Max(0f, nuevaEnergiaMaxima);
        energiaActual = Mathf.Clamp(nuevaEnergiaActual, 0f, energiaMaxima);
        ActualizarBarraEnergia();
    }

    private void ActualizarBarraEnergia()
    {
        if (rellenoBarraEnergia != null)
        {
            rellenoBarraEnergia.fillAmount = energiaMaxima > 0f ? energiaActual / energiaMaxima : 0f;
        }
    }
}