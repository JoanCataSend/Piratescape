using System;
using UnityEngine;

public sealed class SistemaEconomia : MonoBehaviour
{
    private const int MonedasMaximas = 999;

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
        LimitarTodasLasMonedas();
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
            conchas = LimitarMoneda(conchas + cantidad);
            NotificarCambio();
            return;
        }

        if (tipoMoneda == TipoMoneda.Tulipan)
        {
            tulipanes = LimitarMoneda(tulipanes + cantidad);
            NotificarCambio();
            return;
        }

        pinyas = LimitarMoneda(pinyas + cantidad);
        NotificarCambio();
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

    private int LimitarMoneda(int cantidad)
    {
        return Mathf.Clamp(cantidad, 0, MonedasMaximas);
    }

    private void LimitarTodasLasMonedas()
    {
        conchas = LimitarMoneda(conchas);
        tulipanes = LimitarMoneda(tulipanes);
        pinyas = LimitarMoneda(pinyas);
    }

    private void NotificarCambio()
    {
        OnEconomiaActualizada?.Invoke(conchas, tulipanes, pinyas);
    }
}