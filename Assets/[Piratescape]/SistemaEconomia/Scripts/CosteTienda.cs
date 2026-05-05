using System;
using UnityEngine;

[Serializable]
public sealed class CosteTienda
{
    [SerializeField] private TipoMoneda tipoMoneda;
    [SerializeField] private int cantidad;

    public TipoMoneda TipoMoneda => tipoMoneda;
    public int Cantidad => cantidad;
}