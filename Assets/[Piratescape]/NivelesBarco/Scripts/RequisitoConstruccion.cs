using System;
using UnityEngine;

[Serializable]
public sealed class RequisitoConstruccion
{
    [SerializeField] private ItemData itemRequerido;
    [SerializeField] private int cantidadNecesaria = 1;
    [SerializeField] private int cantidadEntregada = 0;

    public ItemData ItemRequerido => itemRequerido;
    public int CantidadNecesaria => cantidadNecesaria;
    public int CantidadEntregada => cantidadEntregada;

    public int CantidadPendiente
    {
        get
        {
            return Mathf.Max(0, cantidadNecesaria - cantidadEntregada);
        }
    }

    public bool EstaCompletado
    {
        get
        {
            return cantidadEntregada >= cantidadNecesaria;
        }
    }

    public void Entregar(int cantidad)
    {
        if (cantidad <= 0)
        {
            return;
        }

        cantidadEntregada = Mathf.Clamp(cantidadEntregada + cantidad, 0, cantidadNecesaria);
    }
}