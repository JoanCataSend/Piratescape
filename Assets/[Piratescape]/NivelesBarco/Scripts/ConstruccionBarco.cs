using System;
using UnityEngine;
using UnityEngine.UI;

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
    //[SerializeField] private bool ocultarBaseAlCompletarTodosLosNiveles = true;

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

    [Header("Secuencia mejora barco")]
    [SerializeField] private Camera camaraJugador;
    [SerializeField] private Camera camaraMejoraBarco;
    [SerializeField] private GameObject fxMejoraBarco;
    [SerializeField] private Transform puntoFXMejoraBarco;
    [SerializeField] private GameObject fxFuegosArtificialesNivel5;
    [SerializeField] private Transform puntoFuegosArtificialesNivel5;
    [SerializeField] private float duracionSecuenciaMejora = 7f;

    [Header("UI mejora barco")]
    [SerializeField] private GameObject panelMejoraBarco;
    [SerializeField] private Image imagenMensajeNivel;
    [SerializeField] private Sprite[] spritesMensajeNivel;

    [Header("UI a ocultar durante mejora")]
    [SerializeField] private GameObject[] objetosUIAOcultar;
    private bool[] estadosPreviosUI;

    [Header("Cinemática final")]
    [SerializeField] private VictoryCutsceneController victoryCutsceneController;
    [SerializeField] private bool iniciarCinematicaFinalAlCompletarNivel5 = true;
    [SerializeField] private float retrasoCinematicaFinal = 2f;

    public static bool HaySecuenciaMejoraBarcoEnCurso { get; private set; }

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
        if (HaySecuenciaMejoraBarcoEnCurso)
        {
            return false;
        }

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

        int nivelCompletado = indiceNivelActual;

        indiceNivelActual++;

        AplicarEstadoVisualActual();

        StartCoroutine(SecuenciaMejoraBarco(nivelCompletado));

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
            marcadorMiniMapaBarco.SetActive(false);
        }

        if (zonaConstruccionVisual != null)
        {
            zonaConstruccionVisual.SetActive(indiceNivelActual == 0);
        }
    }

    private System.Collections.IEnumerator SecuenciaMejoraBarco(int nivelCompletado)
    {
        HaySecuenciaMejoraBarcoEnCurso = true;

        if (objetosUIAOcultar != null)
        {
            estadosPreviosUI = new bool[objetosUIAOcultar.Length];

            for (int i = 0; i < objetosUIAOcultar.Length; i++)
            {
                if (objetosUIAOcultar[i] != null)
                {
                    estadosPreviosUI[i] = objetosUIAOcultar[i].activeSelf;
                    objetosUIAOcultar[i].SetActive(false);
                }
            }
        }

        if (camaraJugador != null)
        {
            camaraJugador.gameObject.SetActive(false);
        }

        if (camaraMejoraBarco != null)
        {
            camaraMejoraBarco.gameObject.SetActive(true);
        }

        if (fxMejoraBarco != null)
        {
            Vector3 posicionFX = puntoFXMejoraBarco != null
                ? puntoFXMejoraBarco.position
                : transform.position;

            Quaternion rotacionFX = puntoFXMejoraBarco != null
                ? puntoFXMejoraBarco.rotation
                : Quaternion.identity;

            GameObject fx = Instantiate(fxMejoraBarco, posicionFX, rotacionFX);
            fx.SetActive(true);
            Debug.Log("FX mejora barco instanciado en: " + posicionFX);
        }

        if (nivelCompletado == 4 && fxFuegosArtificialesNivel5 != null)
        {
            Vector3 posicionFuegos = puntoFuegosArtificialesNivel5 != null
                ? puntoFuegosArtificialesNivel5.position
                : transform.position;

            Quaternion rotacionFuegos = puntoFuegosArtificialesNivel5 != null
                ? puntoFuegosArtificialesNivel5.rotation
                : Quaternion.identity;

            GameObject fuegos = Instantiate(fxFuegosArtificialesNivel5, posicionFuegos, rotacionFuegos);
            fuegos.SetActive(true);

            Debug.Log("FX fuegos artificiales nivel 5 instanciado en: " + posicionFuegos);
        }

        if (panelMejoraBarco != null)
        {
            panelMejoraBarco.SetActive(true);
        }

        if (imagenMensajeNivel != null && spritesMensajeNivel != null)
        {
            if (nivelCompletado >= 0 && nivelCompletado < spritesMensajeNivel.Length)
            {
                imagenMensajeNivel.sprite = spritesMensajeNivel[nivelCompletado];
            }
        }

        yield return new WaitForSeconds(duracionSecuenciaMejora);

        if (panelMejoraBarco != null)
        {
            panelMejoraBarco.SetActive(false);
        }

        if (camaraMejoraBarco != null)
        {
            camaraMejoraBarco.gameObject.SetActive(false);
        }

        if (objetosUIAOcultar != null && estadosPreviosUI != null)
        {
            for (int i = 0; i < objetosUIAOcultar.Length; i++)
            {
                if (objetosUIAOcultar[i] != null && i < estadosPreviosUI.Length)
                {
                    objetosUIAOcultar[i].SetActive(estadosPreviosUI[i]);
                }
            }
        }

        if (camaraJugador != null)
        {
            camaraJugador.gameObject.SetActive(true);
        }

        HaySecuenciaMejoraBarcoEnCurso = false;
        OnConstruccionActualizada?.Invoke();

        if (iniciarCinematicaFinalAlCompletarNivel5 && nivelCompletado == 4 && victoryCutsceneController != null)
        {
            victoryCutsceneController.StartVictoryCutsceneAfterDelay(retrasoCinematicaFinal);
        }
    }
}