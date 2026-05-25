using System;
using UnityEngine;

[Serializable]
public sealed class NivelConstruccionBarco
{
    [Header("Datos del nivel")]
    [SerializeField] private string nombreNivel = "Nivel 1";
    [SerializeField] private string mensajeCompletado = "Felicidades, completaste el nivel de construccion";

    [Header("Visual")]
    [SerializeField] private GameObject modeloBarco;

    [Header("Requisitos")]
    [SerializeField] private RequisitoConstruccion[] requisitos;

    public string NombreNivel => nombreNivel;
    public string MensajeCompletado => mensajeCompletado;
    public GameObject ModeloBarco => modeloBarco;
    public RequisitoConstruccion[] Requisitos => requisitos;




    public bool EstaCompletado
    {
        get
        {
            if (requisitos == null || requisitos.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < requisitos.Length; i++)
            {
                RequisitoConstruccion requisito = requisitos[i];

                if (requisito == null)
                {
                    continue;
                }

                if (!requisito.EstaCompletado)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public void ActivarModelo()
    {
        if (modeloBarco == null)
        {
            return;
        }

        modeloBarco.SetActive(true);
    }

    public void DesactivarModelo()
    {
        if (modeloBarco == null)
        {
            return;
        }

        modeloBarco.SetActive(false);
    }

    public RequisitoConstruccion BuscarRequisito(ItemData itemData)
    {
        if (itemData == null || requisitos == null)
        {
            return null;
        }

        for (int i = 0; i < requisitos.Length; i++)
        {
            RequisitoConstruccion requisito = requisitos[i];

            if (requisito == null)
            {
                continue;
            }

            if (requisito.ItemRequerido == itemData)
            {
                return requisito;
            }
        }

        return null;
    }

    public RequisitoConstruccion ObtenerPrimerRequisitoPendienteConMaterialDisponible(PlayerInventory inventario)
    {
        if (inventario == null || requisitos == null)
        {
            return null;
        }

        for (int i = 0; i < requisitos.Length; i++)
        {
            RequisitoConstruccion requisito = requisitos[i];

            if (requisito == null || requisito.ItemRequerido == null)
            {
                continue;
            }

            if (requisito.EstaCompletado)
            {
                continue;
            }

            int cantidadDisponible = inventario.ObtenerCantidad(requisito.ItemRequerido);

            if (cantidadDisponible > 0)
            {
                return requisito;
            }
        }

        return null;
    }
}