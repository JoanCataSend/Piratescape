using System;
using UnityEngine;

public sealed class SistemaEconomia : MonoBehaviour
{
    [Header("Valores iniciales")]
    [SerializeField] private int conchas;
    [SerializeField] private int tulipanes;
    [SerializeField] private int pinyas;

    public event Action<int, int, int> OnEconomiaActualizada;

    public int Conchas => conchas;
    public int Tulipanes => tulipanes;
    public int Pinyas => pinyas;

    private void Start()
    {
        NotificarCambio();
    }

    public void AnadirMoneda(TipoMoneda tipoMoneda, int cantidad)
    {
        if (cantidad <= 0)
        {
            return;
        }

        if (tipoMoneda == TipoMoneda.Concha)
        {
            conchas += cantidad;
            NotificarCambio();
            return;
        }

        if (tipoMoneda == TipoMoneda.Tulipan)
        {
            tulipanes += cantidad;
            NotificarCambio();
            return;
        }

        pinyas += cantidad;
        NotificarCambio();
    }

    private void NotificarCambio()
    {
        OnEconomiaActualizada?.Invoke(conchas, tulipanes, pinyas);
    }

    public bool TieneMonedasSuficientes(TipoMoneda tipoMoneda, int cantidad)
    {
        if (cantidad <= 0)
        {
            return true;
        }

        if (tipoMoneda == TipoMoneda.Concha)
        {
            return conchas >= cantidad;
        }

        if (tipoMoneda == TipoMoneda.Tulipan)
        {
            return tulipanes >= cantidad;
        }

        return pinyas >= cantidad;
    }

    public bool GastarMoneda(TipoMoneda tipoMoneda, int cantidad)
    {
        if (!TieneMonedasSuficientes(tipoMoneda, cantidad))
        {
            return false;
        }

        if (tipoMoneda == TipoMoneda.Concha)
        {
            conchas -= cantidad;
        }
        else if (tipoMoneda == TipoMoneda.Tulipan)
        {
            tulipanes -= cantidad;
        }
        else
        {
            pinyas -= cantidad;
        }

        NotificarCambio();
        return true;
    }
}