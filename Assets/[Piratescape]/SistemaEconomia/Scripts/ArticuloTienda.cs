using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ArticuloTienda : MonoBehaviour
{
    [Header("Datos del articulo")]
    [SerializeField] private string nombreArticulo;
    [SerializeField] private Sprite iconoArticulo;
    [SerializeField] private ItemData itemInventario;
    [SerializeField] private int cantidadItem = 1;
    [SerializeField] private GameObject prefabObjeto;
    [SerializeField] private CosteTienda[] costes;

    [Header("Referencias compra")]
    [SerializeField] private SistemaEconomia sistemaEconomia;
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private Transform puntoAparicion;
    [SerializeField] private MensajeTienda mensajeTienda;
    [SerializeField] private GhostMerchant vendedorFantasma;

    [Header("Referencias UI")]
    [SerializeField] private GameObject contenedorArticulo;
    [SerializeField] private Image imagenArticulo;
    [SerializeField] private TMP_Text textoNombre;
    [SerializeField] private Transform contenedorPrecios;
    [SerializeField] private Button botonComprar;
    [SerializeField] private TMP_Text textoBotonComprar;

    [Header("Colores")]
    [SerializeField] private Color colorPrecioNormal = Color.white;
    [SerializeField] private Color colorPrecioInsuficiente = Color.red;

    [Header("Iconos monedas")]
    [SerializeField] private Sprite iconoConcha;
    [SerializeField] private Sprite iconoTulipan;
    [SerializeField] private Sprite iconoPinya;

    [Header("Configuracion")]
    [SerializeField] private bool desactivarBotonSiNoPuedeComprar = true;
    [SerializeField] private bool cerrarTiendaAlComprar = false;

    [Header("Espantamonos")]
    [SerializeField] private bool esEspantamonos;
    [SerializeField] private SistemaEspantamonos sistemaEspantamonos;

    private readonly List<TMP_Text> textosPrecio = new List<TMP_Text>();
    private readonly List<CosteTienda> costesPrecio = new List<CosteTienda>();

    private void Awake()
    {
        BuscarReferenciasSiFaltan();
        PrepararUI();

        if (botonComprar != null)
        {
            botonComprar.onClick.AddListener(Comprar);
        }

        if (sistemaEconomia != null)
        {
            sistemaEconomia.OnEconomiaActualizada += AlActualizarEconomia;
        }

        if (esEspantamonos && sistemaEspantamonos != null)
        {
            sistemaEspantamonos.OnEstadoCambiado += AlCambiarEstadoEspantamonos;
        }

        RefrescarUI();
    }

    private void OnEnable()
    {
        RefrescarUI();
    }

    private void OnDestroy()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.RemoveListener(Comprar);
        }

        if (sistemaEconomia != null)
        {
            sistemaEconomia.OnEconomiaActualizada -= AlActualizarEconomia;
        }

        if (esEspantamonos && sistemaEspantamonos != null)
        {
            sistemaEspantamonos.OnEstadoCambiado -= AlCambiarEstadoEspantamonos;
        }
    }

    private void Comprar()
    {
        BuscarReferenciasSiFaltan();

        if (!ReferenciasValidas())
        {
            return;
        }

        if (DebeOcultarsePorEspantamonos())
        {
            RefrescarUI();
            return;
        }

        if (!PuedeComprar())
        {
            MostrarError("No tienes suficiente dinero");
            RefrescarUI();
            return;
        }

        if (!esEspantamonos && !PuedeEntrarEnInventario())
        {
            MostrarError("Inventario lleno");
            return;
        }

        bool compraCorrecta;

        if (esEspantamonos)
        {
            compraCorrecta = ComprarEspantamonos();
        }
        else
        {
            compraCorrecta = ComprarItemInventario();
        }

        if (!compraCorrecta)
        {
            return;
        }

        Cobrar();
        RefrescarUI();

        if (cerrarTiendaAlComprar && vendedorFantasma != null)
        {
            vendedorFantasma.CerrarTiendaDesdeUI();
        }
    }

    private bool ComprarItemInventario()
    {
        if (inventarioJugador == null || itemInventario == null)
        {
            MostrarError("Falta configurar el item de inventario");
            return false;
        }

        bool anadido = inventarioJugador.TryAddItem(itemInventario, cantidadItem);

        if (!anadido)
        {
            MostrarError("Inventario lleno");
            return false;
        }

        MostrarConfirmacion("Has comprado " + nombreArticulo);
        return true;
    }

    private bool ComprarEspantamonos()
    {
        if (sistemaEspantamonos == null)
        {
            MostrarError("Falta SistemaEspantamonos");
            return false;
        }

        bool colocado = sistemaEspantamonos.ComprarEspantamonos(prefabObjeto, puntoAparicion);

        if (!colocado)
        {
            MostrarError("No se pudo colocar el espantamonos");
            return false;
        }

        MostrarConfirmacion("Espantamonos colocado en base");
        return true;
    }

    private bool PuedeEntrarEnInventario()
    {
        if (inventarioJugador == null || itemInventario == null)
        {
            return false;
        }

        return inventarioJugador.CanAddItem(itemInventario, cantidadItem);
    }

    private void PrepararUI()
    {
        if (textoNombre != null)
        {
            textoNombre.text = nombreArticulo;
        }

        if (imagenArticulo != null)
        {
            imagenArticulo.sprite = iconoArticulo;
            imagenArticulo.preserveAspect = true;
        }

        if (textoBotonComprar != null)
        {
            textoBotonComprar.text = "Comprar";
        }

        CrearUIPrecios();
    }

    private void CrearUIPrecios()
    {
        textosPrecio.Clear();
        costesPrecio.Clear();

        if (contenedorPrecios == null)
        {
            return;
        }

        for (int i = contenedorPrecios.childCount - 1; i >= 0; i--)
        {
            Destroy(contenedorPrecios.GetChild(i).gameObject);
        }

        if (costes == null || costes.Length == 0)
        {
            CrearPrecioGratis();
            return;
        }

        foreach (CosteTienda coste in costes)
        {
            if (coste == null)
            {
                continue;
            }

            CrearPrecioVisual(coste);
        }
    }

    private GameObject contenorPreciosGetChild(int index)
    {
        return contenedorPrecios.GetChild(index).gameObject;
    }

    private void CrearPrecioGratis()
    {
        GameObject textoGO = new GameObject("Precio_Gratis");
        textoGO.transform.SetParent(contenedorPrecios, false);

        RectTransform rect = textoGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(100f, 30f);

        TextMeshProUGUI texto = textoGO.AddComponent<TextMeshProUGUI>();
        texto.text = "Gratis";
        texto.fontSize = 24f;
        texto.alignment = TextAlignmentOptions.Center;
        texto.color = colorPrecioNormal;
        texto.raycastTarget = false;
    }

    private void CrearPrecioVisual(CosteTienda coste)
    {
        GameObject fila = new GameObject("Precio_" + coste.TipoMoneda);
        fila.transform.SetParent(contenedorPrecios, false);

        RectTransform filaRect = fila.AddComponent<RectTransform>();
        filaRect.sizeDelta = new Vector2(100f, 32f);

        HorizontalLayoutGroup layout = fila.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 5f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = false;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;

        LayoutElement filaLayout = fila.AddComponent<LayoutElement>();
        filaLayout.preferredWidth = 100f;
        filaLayout.preferredHeight = 32f;

        GameObject iconoGO = new GameObject("Icono");
        iconoGO.transform.SetParent(fila.transform, false);

        RectTransform iconoRect = iconoGO.AddComponent<RectTransform>();
        iconoRect.sizeDelta = new Vector2(26f, 26f);

        LayoutElement iconoLayout = iconoGO.AddComponent<LayoutElement>();
        iconoLayout.preferredWidth = 26f;
        iconoLayout.preferredHeight = 26f;

        Image imagen = iconoGO.AddComponent<Image>();
        imagen.sprite = ObtenerIcono(coste.TipoMoneda);
        imagen.preserveAspect = true;
        imagen.raycastTarget = false;

        GameObject textoGO = new GameObject("Cantidad");
        textoGO.transform.SetParent(fila.transform, false);

        RectTransform textoRect = textoGO.AddComponent<RectTransform>();
        textoRect.sizeDelta = new Vector2(50f, 30f);

        LayoutElement textoLayout = textoGO.AddComponent<LayoutElement>();
        textoLayout.preferredWidth = 50f;
        textoLayout.preferredHeight = 30f;

        TextMeshProUGUI texto = textoGO.AddComponent<TextMeshProUGUI>();
        texto.text = coste.Cantidad.ToString();
        texto.fontSize = 24f;
        texto.alignment = TextAlignmentOptions.MidlineLeft;
        texto.raycastTarget = false;
        texto.color = colorPrecioNormal;

        textosPrecio.Add(texto);
        costesPrecio.Add(coste);
    }

    private Sprite ObtenerIcono(TipoMoneda tipoMoneda)
    {
        if (tipoMoneda == TipoMoneda.Concha)
        {
            return iconoConcha;
        }

        if (tipoMoneda == TipoMoneda.Tulipan)
        {
            return iconoTulipan;
        }

        return iconoPinya;
    }

    private void RefrescarUI()
    {
        BuscarReferenciasSiFaltan();

        bool ocultar = DebeOcultarsePorEspantamonos();
        GameObject objetoAOcultar = contenedorArticulo != null ? contenedorArticulo : gameObject;

        if (objetoAOcultar.activeSelf == ocultar)
        {
            objetoAOcultar.SetActive(!ocultar);
        }

        if (ocultar)
        {
            return;
        }

        ActualizarColoresPrecio();
        ActualizarBotonComprar();
    }

    private void ActualizarColoresPrecio()
    {
        for (int i = 0; i < textosPrecio.Count; i++)
        {
            TMP_Text texto = textosPrecio[i];
            CosteTienda coste = costesPrecio[i];

            if (texto == null || coste == null || sistemaEconomia == null)
            {
                continue;
            }

            bool tieneSuficiente = sistemaEconomia.TieneMonedasSuficientes(coste.TipoMoneda, coste.Cantidad);
            texto.color = tieneSuficiente ? colorPrecioNormal : colorPrecioInsuficiente;
        }
    }

    private void ActualizarBotonComprar()
    {
        if (botonComprar == null)
        {
            return;
        }

        if (desactivarBotonSiNoPuedeComprar)
        {
            botonComprar.interactable = PuedeComprar();
        }
    }

    private bool DebeOcultarsePorEspantamonos()
    {
        if (!esEspantamonos)
        {
            return false;
        }

        if (sistemaEspantamonos == null)
        {
            sistemaEspantamonos = SistemaEspantamonos.Instance;
        }

        return sistemaEspantamonos != null && sistemaEspantamonos.EstaActivo;
    }

    private bool PuedeComprar()
    {
        if (sistemaEconomia == null)
        {
            return false;
        }

        if (costes == null || costes.Length == 0)
        {
            return true;
        }

        foreach (CosteTienda coste in costes)
        {
            if (coste == null)
            {
                continue;
            }

            if (!sistemaEconomia.TieneMonedasSuficientes(coste.TipoMoneda, coste.Cantidad))
            {
                return false;
            }
        }

        return true;
    }

    private void Cobrar()
    {
        if (costes == null)
        {
            return;
        }

        foreach (CosteTienda coste in costes)
        {
            if (coste == null)
            {
                continue;
            }

            sistemaEconomia.GastarMoneda(coste.TipoMoneda, coste.Cantidad);
        }
    }

    private bool ReferenciasValidas()
    {
        if (sistemaEconomia == null)
        {
            Debug.LogError("Falta SistemaEconomia en " + nombreArticulo, this);
            return false;
        }

        if (!esEspantamonos && inventarioJugador == null)
        {
            Debug.LogError("Falta PlayerInventory en " + nombreArticulo, this);
            return false;
        }

        if (!esEspantamonos && itemInventario == null)
        {
            Debug.LogError("Falta Item Inventario en " + nombreArticulo, this);
            return false;
        }

        if (esEspantamonos && prefabObjeto == null)
        {
            Debug.LogError("Falta Prefab Objeto en " + nombreArticulo, this);
            return false;
        }

        if (esEspantamonos && puntoAparicion == null)
        {
            Debug.LogError("Falta Punto Aparicion en " + nombreArticulo, this);
            return false;
        }

        if (botonComprar == null)
        {
            Debug.LogError("Falta Boton Comprar en " + nombreArticulo, this);
            return false;
        }

        if (esEspantamonos && sistemaEspantamonos == null)
        {
            Debug.LogError("Falta SistemaEspantamonos en " + nombreArticulo, this);
            return false;
        }

        return true;
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (sistemaEconomia == null)
        {
            sistemaEconomia = FindFirstObjectByType<SistemaEconomia>();
        }

        if (inventarioJugador == null)
        {
            inventarioJugador = FindFirstObjectByType<PlayerInventory>();
        }

        if (mensajeTienda == null)
        {
            mensajeTienda = FindFirstObjectByType<MensajeTienda>();
        }

        if (esEspantamonos && sistemaEspantamonos == null)
        {
            sistemaEspantamonos = FindFirstObjectByType<SistemaEspantamonos>();
        }

        if (botonComprar == null)
        {
            Transform botonTransform = transform.Find("BuyButton");

            if (botonTransform != null)
            {
                botonComprar = botonTransform.GetComponent<Button>();
            }

            if (botonComprar == null)
            {
                botonComprar = GetComponentInChildren<Button>(true);
            }
        }

        if (contenedorPrecios == null)
        {
            Transform precioTransform = transform.Find("PrecioContenedor");

            if (precioTransform != null)
            {
                contenedorPrecios = precioTransform;
            }
        }

        if (contenedorArticulo == null)
        {
            contenedorArticulo = gameObject;
        }
    }

    private void MostrarError(string mensaje)
    {
        if (mensajeTienda != null)
        {
            mensajeTienda.MostrarError(mensaje);
        }

        Debug.LogWarning(mensaje);
    }

    private void MostrarConfirmacion(string mensaje)
    {
        if (mensajeTienda != null)
        {
            mensajeTienda.MostrarConfirmacion(mensaje);
        }

        Debug.Log(mensaje);
    }

    private void AlActualizarEconomia(int conchas, int tulipanes, int pinyas)
    {
        RefrescarUI();
    }

    private void AlCambiarEstadoEspantamonos(bool espantamonosActivo)
    {
        RefrescarUI();
    }
}