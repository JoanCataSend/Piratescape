using System;
using UnityEngine;

public sealed class ConstruccionBarco : MonoBehaviour
{
    public event Action OnConstruccionActualizada;
    public event Action OnConstruccionCompletada;

    [Header("Referencias")]
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private GameObject barcoNivel1;
    [SerializeField] private GameObject zonaConstruccionVisual;
    [SerializeField] private GameObject marcadorMiniMapaBarco;

    [Header("Requisitos")]
    [SerializeField] private RequisitoConstruccion[] requisitos;

    [Header("Configuracion")]
    [SerializeField] private bool ocultarBaseAlCompletar = true;

    private bool construccionCompletada;

    public bool ConstruccionCompletada => construccionCompletada;
    public RequisitoConstruccion[] Requisitos => requisitos;

    private void Awake()
    {
        if (inventarioJugador == null)
        {
            inventarioJugador = FindFirstObjectByType<PlayerInventory>();
        }

        AplicarEstadoVisualInicial();
    }

    public bool IntentarEntregarUnaUnidad()
    {
        if (construccionCompletada)
        {
            return false;
        }

        if (inventarioJugador == null || requisitos == null || requisitos.Length == 0)
        {
            return false;
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

            int cantidadRemovida = inventarioJugador.RemoverHasta(requisito.ItemRequerido, 1);

            if (cantidadRemovida <= 0)
            {
                continue;
            }

            requisito.Entregar(cantidadRemovida);
            OnConstruccionActualizada?.Invoke();
            IntentarCompletarConstruccion();
            return true;
        }

        return false;
    }

    public int ObtenerCantidadDisponible(ItemData itemData)
    {
        if (inventarioJugador == null || itemData == null)
        {
            return 0;
        }

        return inventarioJugador.ObtenerCantidad(itemData);
    }

    public int ObtenerCantidadEntregada(ItemData itemData)
    {
        RequisitoConstruccion requisito = BuscarRequisito(itemData);
        return requisito == null ? 0 : requisito.CantidadEntregada;
    }

    public int ObtenerCantidadNecesaria(ItemData itemData)
    {
        RequisitoConstruccion requisito = BuscarRequisito(itemData);
        return requisito == null ? 0 : requisito.CantidadNecesaria;
    }

    public int ObtenerCantidadPendiente(ItemData itemData)
    {
        RequisitoConstruccion requisito = BuscarRequisito(itemData);
        return requisito == null ? 0 : requisito.CantidadPendiente;
    }

    private void IntentarCompletarConstruccion()
    {
        if (!EstanTodosLosRequisitosCompletados())
        {
            return;
        }

        construccionCompletada = true;
        AplicarEstadoVisualFinal();
        OnConstruccionCompletada?.Invoke();
    }

    private bool EstanTodosLosRequisitosCompletados()
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

    private RequisitoConstruccion BuscarRequisito(ItemData itemData)
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

    private void AplicarEstadoVisualInicial()
    {
        if (barcoNivel1 != null)
        {
            barcoNivel1.SetActive(false);
        }

        if (marcadorMiniMapaBarco != null)
        {
            marcadorMiniMapaBarco.SetActive(false);
        }
    }

    private void AplicarEstadoVisualFinal()
    {
        if (barcoNivel1 != null)
        {
            barcoNivel1.SetActive(true);
        }

        if (marcadorMiniMapaBarco != null)
        {
            marcadorMiniMapaBarco.SetActive(true);
        }

        if (ocultarBaseAlCompletar && zonaConstruccionVisual != null)
        {
            zonaConstruccionVisual.SetActive(false);
        }
    }
}