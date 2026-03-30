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
        ValidarValoresIniciales();
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
        saludActual = LimitarValorActual(nuevaSaludActual, saludMaxima);
        ActualizarBarraSalud();
    }

    public void EstablecerSalud(float nuevaSaludActual, float nuevaSaludMaxima)
    {
        saludMaxima = LimitarValorMaximo(nuevaSaludMaxima);
        saludActual = LimitarValorActual(nuevaSaludActual, saludMaxima);
        ActualizarBarraSalud();
    }

    private void ActualizarBarraSalud()
    {
        ActualizarRellenoBarra(rellenoBarraSalud, saludActual, saludMaxima);
    }

    // ENERGIA
    public void EstablecerEnergiaActual(float nuevaEnergiaActual)
    {
        energiaActual = LimitarValorActual(nuevaEnergiaActual, energiaMaxima);
        ActualizarBarraEnergia();
    }

    public void EstablecerEnergia(float nuevaEnergiaActual, float nuevaEnergiaMaxima)
    {
        energiaMaxima = LimitarValorMaximo(nuevaEnergiaMaxima);
        energiaActual = LimitarValorActual(nuevaEnergiaActual, energiaMaxima);
        ActualizarBarraEnergia();
    }

    private void ActualizarBarraEnergia()
    {
        ActualizarRellenoBarra(rellenoBarraEnergia, energiaActual, energiaMaxima);
    }

    // METODOS AUXILIARES
    private void ValidarValoresIniciales()
    {
        saludMaxima = LimitarValorMaximo(saludMaxima);
        saludActual = LimitarValorActual(saludActual, saludMaxima);

        energiaMaxima = LimitarValorMaximo(energiaMaxima);
        energiaActual = LimitarValorActual(energiaActual, energiaMaxima);
    }

    private float LimitarValorMaximo(float valorMaximo)
    {
        return Mathf.Max(0f, valorMaximo);
    }

    private float LimitarValorActual(float valorActual, float valorMaximo)
    {
        return Mathf.Clamp(valorActual, 0f, valorMaximo);
    }

    private void ActualizarRellenoBarra(Image barra, float valorActual, float valorMaximo)
    {
        if (barra == null)
        {
            return;
        }

        barra.fillAmount = valorMaximo > 0f ? valorActual / valorMaximo : 0f;
    }
}