using System;
using UnityEngine;

public sealed class ConstruccionBarco : MonoBehaviour
{
    public event Action OnConstruccionActualizada;
    public event Action OnConstruccionCompletada;
    public event Action<string, Color> OnMensajeConstruccionSolicitado;

    [Header("Referencias")]
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private PlayerEnergy energiaJugador;
    [SerializeField] private GameObject zonaConstruccionVisual;
    [SerializeField] private GameObject marcadorMiniMapaBarco;

    [Header("Niveles de construccion")]
    [SerializeField] private NivelConstruccionBarco[] nivelesConstruccion;

    [Header("Configuracion")]
    [SerializeField] private bool ocultarBaseAlCompletarTodosLosNiveles = true;

    [Header("Coste al construir")]
    [SerializeField] private float energiaGastadaPorMaterial = 5f;
    [SerializeField] private int vidaGastadaPorMaterialSinEnergia = 2;

    private int indiceNivelActual;
    private string ultimoMensajeCompletado = "";

    public bool ConstruccionCompletada => EstanTodosLosNivelesCompletados;
    public bool NivelActualCompletado => ObtenerNivelActual() == null;
    public int IndiceNivelActual => indiceNivelActual;
    public string UltimoMensajeCompletado => ultimoMensajeCompletado;

    public NivelConstruccionBarco NivelActual => ObtenerNivelActual();

    private bool EstanTodosLosNivelesCompletados
    {
        get
        {
            return nivelesConstruccion != null && indiceNivelActual >= nivelesConstruccion.Length;
        }
    }

    private void Awake()
    {
        CachearReferencias();
    }

    private void OnEnable()
    {
        AplicarEstadoVisualActual();
    }

    public bool IntentarEntregarUnaUnidad()
    {
        NivelConstruccionBarco nivelActual = ObtenerNivelActual();

        if (nivelActual == null || inventarioJugador == null)
        {
            return false;
        }

        InventorySlot slotSeleccionado = inventarioJugador.GetSlot(inventarioJugador.SelectedSlotIndex);

        if (slotSeleccionado == null || slotSeleccionado.IsEmpty() || slotSeleccionado.itemData == null)
        {
            SolicitarMensajeConstruccion("         Selecciona un material del inventario.", Color.white);
            return false;
        }

        ItemData itemSeleccionado = slotSeleccionado.itemData;
        RequisitoConstruccion requisito = nivelActual.BuscarRequisito(itemSeleccionado);

        if (requisito == null)
        {
            SolicitarMensajeConstruccion("     Ese material no sirve para esta construccion.", new Color(0.8f, 0.25f, 0.25f));
            return false;
        }

        if (requisito.EstaCompletado)
        {
            SolicitarMensajeConstruccion("Ya has entregado todo ese material.", Color.white);
            return false;
        }

        int cantidadRemovida = inventarioJugador.RemoverHasta(itemSeleccionado, 1);

        if (cantidadRemovida <= 0)
        {
            SolicitarMensajeConstruccion("No tienes suficiente material.", new Color(0.8f, 0.25f, 0.25f));
            return false;
        }

        requisito.Entregar(cantidadRemovida);
        AplicarCosteConstruccion(cantidadRemovida);

        OnConstruccionActualizada?.Invoke();
        IntentarCompletarNivelActual();

        return true;
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
        RequisitoConstruccion requisito = BuscarRequisitoEnNivelActual(itemData);
        return requisito == null ? 0 : requisito.CantidadEntregada;
    }

    public int ObtenerCantidadNecesaria(ItemData itemData)
    {
        RequisitoConstruccion requisito = BuscarRequisitoEnNivelActual(itemData);
        return requisito == null ? 0 : requisito.CantidadNecesaria;
    }

    public int ObtenerCantidadPendiente(ItemData itemData)
    {
        RequisitoConstruccion requisito = BuscarRequisitoEnNivelActual(itemData);
        return requisito == null ? 0 : requisito.CantidadPendiente;
    }

    public RequisitoConstruccion[] ObtenerRequisitosNivelActual()
    {
        NivelConstruccionBarco nivelActual = ObtenerNivelActual();
        return nivelActual == null ? null : nivelActual.Requisitos;
    }

    private void CachearReferencias()
    {
        if (inventarioJugador == null)
        {
            inventarioJugador = FindFirstObjectByType<PlayerInventory>();
        }

        if (energiaJugador == null)
        {
            energiaJugador = FindFirstObjectByType<PlayerEnergy>();
        }
    }

    private NivelConstruccionBarco ObtenerNivelActual()
    {
        if (nivelesConstruccion == null)
        {
            return null;
        }

        if (indiceNivelActual < 0 || indiceNivelActual >= nivelesConstruccion.Length)
        {
            return null;
        }

        return nivelesConstruccion[indiceNivelActual];
    }

    private RequisitoConstruccion BuscarRequisitoEnNivelActual(ItemData itemData)
    {
        NivelConstruccionBarco nivelActual = ObtenerNivelActual();
        return nivelActual == null ? null : nivelActual.BuscarRequisito(itemData);
    }

    private void AplicarCosteConstruccion(int cantidadEntregada)
    {
        if (energiaJugador == null || cantidadEntregada <= 0)
        {
            return;
        }

        for (int i = 0; i < cantidadEntregada; i++)
        {
            energiaJugador.TryUseEnergyOrHealth(energiaGastadaPorMaterial, vidaGastadaPorMaterialSinEnergia);
        }
    }

    private void IntentarCompletarNivelActual()
    {
        NivelConstruccionBarco nivelActual = ObtenerNivelActual();

        if (nivelActual == null || !nivelActual.EstaCompletado)
        {
            return;
        }

        ultimoMensajeCompletado = nivelActual.MensajeCompletado;

        indiceNivelActual++;

        AplicarEstadoVisualActual();

        OnConstruccionCompletada?.Invoke();
        OnConstruccionActualizada?.Invoke();
    }

    private void AplicarEstadoVisualInicial()
    {
        if (nivelesConstruccion != null)
        {
            for (int i = 0; i < nivelesConstruccion.Length; i++)
            {
                NivelConstruccionBarco nivel = nivelesConstruccion[i];

                if (nivel == null)
                {
                    continue;
                }

                nivel.DesactivarModelo();
            }
        }

        if (marcadorMiniMapaBarco != null)
        {
            marcadorMiniMapaBarco.SetActive(false);
        }
    }

    private void AplicarEstadoVisualTrasCompletarNivel()
    {
        if (marcadorMiniMapaBarco != null)
        {
            marcadorMiniMapaBarco.SetActive(true);
        }

        if (indiceNivelActual >= 1 && zonaConstruccionVisual != null)
        {
            zonaConstruccionVisual.SetActive(false);
        }
    }

    private void SolicitarMensajeConstruccion(string mensaje, Color color)
    {
        OnMensajeConstruccionSolicitado?.Invoke(mensaje, color);
    }

    private void OcultarModelosAnteriores(int indiceNivelCompletado)
    {
        if (nivelesConstruccion == null)
        {
            return;
        }

        for (int i = 0; i < indiceNivelCompletado; i++)
        {
            NivelConstruccionBarco nivelAnterior = nivelesConstruccion[i];

            if (nivelAnterior == null)
            {
                continue;
            }

            nivelAnterior.DesactivarModelo();
        }
    }

    private void AplicarEstadoVisualActual()
    {
        if (nivelesConstruccion != null)
        {
            for (int i = 0; i < nivelesConstruccion.Length; i++)
            {
                NivelConstruccionBarco nivel = nivelesConstruccion[i];

                if (nivel == null)
                {
                    continue;
                }

                nivel.DesactivarModelo();
            }
        }

        if (indiceNivelActual > 0 && nivelesConstruccion != null)
        {
            int indiceModeloVisible = indiceNivelActual - 1;

            if (indiceModeloVisible >= 0 && indiceModeloVisible < nivelesConstruccion.Length)
            {
                NivelConstruccionBarco nivelVisible = nivelesConstruccion[indiceModeloVisible];

                if (nivelVisible != null)
                {
                    nivelVisible.ActivarModelo();
                }
            }
        }

        if (marcadorMiniMapaBarco != null)
        {
            marcadorMiniMapaBarco.SetActive(indiceNivelActual > 0);
        }

        if (zonaConstruccionVisual != null)
        {
            zonaConstruccionVisual.SetActive(indiceNivelActual == 0);
        }
    }
}