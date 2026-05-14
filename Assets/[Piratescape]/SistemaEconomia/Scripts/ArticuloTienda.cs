using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ArticuloTienda : MonoBehaviour
{
    [Header("Datos nuevos del articulo")]
    [SerializeField] private ArticuloTiendaData articuloData;

    [Header("Compatibilidad con tienda antigua")]
    [SerializeField] private string nombreArticulo;
    [SerializeField] private Sprite iconoArticulo;
    [SerializeField] private ItemData itemInventario;
    [SerializeField] private int cantidadItem = 1;
    [SerializeField] private GameObject prefabObjeto;
    [SerializeField] private CosteTienda[] costes;
    [SerializeField] private bool esEspantamonos;

    [Header("Referencias compra")]
    [SerializeField] private SistemaEconomia sistemaEconomia;
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private SistemaObjetosBase sistemaObjetosBase;
    [SerializeField] private SistemaEspantamonos sistemaEspantamonos;
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

        if (sistemaEspantamonos != null)
        {
            sistemaEspantamonos.OnEstadoCambiado += AlCambiarEstadoEspantamonos;
        }

        if (sistemaObjetosBase != null)
        {
            sistemaObjetosBase.OnObjetosBaseActualizados += AlActualizarObjetosBase;
        }

        RefrescarUI();
    }

    private void OnEnable()
    {
        BuscarReferenciasSiFaltan();
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

        if (sistemaEspantamonos != null)
        {
            sistemaEspantamonos.OnEstadoCambiado -= AlCambiarEstadoEspantamonos;
        }

        if (sistemaObjetosBase != null)
        {
            sistemaObjetosBase.OnObjetosBaseActualizados -= AlActualizarObjetosBase;
        }
    }

    public void Configurar(ArticuloTiendaData nuevoArticuloData)
    {
        articuloData = nuevoArticuloData;
        BuscarReferenciasSiFaltan();
        PrepararUI();
        RefrescarUI();
    }

    private void Comprar()
    {
        BuscarReferenciasSiFaltan();

        if (!ReferenciasValidas())
        {
            return;
        }

        if (DebeOcultarse())
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

        if (EsItemInventario() && !PuedeEntrarEnInventario())
        {
            MostrarError("Inventario lleno");
            return;
        }

        bool compraCorrecta = false;

        if (EsItemInventario())
        {
            compraCorrecta = ComprarItemInventario();
        }
        else if (EsObjetoBase())
        {
            compraCorrecta = ComprarObjetoBase();
        }
        else
        {
            MostrarError("Articulo de tienda sin tipo valido");
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
        ItemData item = ObtenerItemInventario();
        int cantidad = ObtenerCantidadItem();

        if (inventarioJugador != null && item != null)
        {
            bool anadido = inventarioJugador.TryAddItem(item, cantidad);

            if (!anadido)
            {
                MostrarError("Inventario lleno");
                return false;
            }

            MostrarConfirmacion("Has comprado " + ObtenerNombre());
            return true;
        }

        if (articuloData == null && prefabObjeto != null)
        {
            return ComprarPrefabAntiguo();
        }

        MostrarError("Falta configurar el ItemData de " + ObtenerNombre());
        return false;
    }

    private bool ComprarPrefabAntiguo()
    {
        Transform puntoFinal = puntoAparicion;

        if (puntoFinal == null && inventarioJugador != null)
        {
            puntoFinal = inventarioJugador.transform;
        }

        if (puntoFinal == null)
        {
            MostrarError("Falta punto de aparicion para " + ObtenerNombre());
            return false;
        }

        Instantiate(prefabObjeto, puntoFinal.position, puntoFinal.rotation);
        MostrarConfirmacion("Has comprado " + ObtenerNombre());
        return true;
    }

    private bool ComprarObjetoBase()
    {
        ObjetoBaseData objetoBase = ObtenerObjetoBase();

        if (objetoBase != null)
        {
            if (objetoBase.TipoObjetoBase == TipoObjetoBase.Espantamonos)
            {
                return ComprarEspantamonos(objetoBase.Prefab, objetoBase.MensajeCompra, objetoBase.MensajeYaColocado);
            }

            return ComprarObjetoBaseNormal(objetoBase);
        }

        if (articuloData == null && esEspantamonos)
        {
            return ComprarEspantamonos(prefabObjeto, "Espantamonos colocado en base", "Ya tienes un espantamonos en la base");
        }

        MostrarError("Falta configurar el objeto de base");
        return false;
    }

    private bool ComprarEspantamonos(GameObject prefab, string mensajeCompra, string mensajeYaColocado)
    {
        if (sistemaEspantamonos == null)
        {
            MostrarError("Falta SistemaEspantamonos");
            return false;
        }

        bool colocado = sistemaEspantamonos.ComprarEspantamonos(prefab, puntoAparicion);

        if (!colocado)
        {
            MostrarError(mensajeYaColocado);
            return false;
        }

        MostrarConfirmacion(mensajeCompra);
        return true;
    }

    private bool ComprarObjetoBaseNormal(ObjetoBaseData objetoBase)
    {
        if (sistemaObjetosBase == null)
        {
            MostrarError("Falta SistemaObjetosBase");
            return false;
        }

        bool colocado = sistemaObjetosBase.ColocarObjeto(objetoBase, puntoAparicion);

        if (!colocado)
        {
            MostrarError(objetoBase.MensajeYaColocado);
            return false;
        }

        MostrarConfirmacion(objetoBase.MensajeCompra);
        return true;
    }

    private bool PuedeEntrarEnInventario()
    {
        ItemData item = ObtenerItemInventario();

        if (articuloData == null && item == null && prefabObjeto != null)
        {
            return true;
        }

        if (inventarioJugador == null || item == null)
        {
            return false;
        }

        return inventarioJugador.CanAddItem(item, ObtenerCantidadItem());
    }

    private void PrepararUI()
    {
        BuscarReferenciasSiFaltan();

        if (textoNombre != null)
        {
            textoNombre.text = ObtenerNombre();
        }

        if (imagenArticulo != null)
        {
            Sprite icono = ObtenerIconoArticulo();
            if (icono != null)
            {
                imagenArticulo.sprite = icono;
            }

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

        CosteTienda[] costesArticulo = ObtenerCostes();

        if (costesArticulo == null || costesArticulo.Length == 0)
        {
            CrearPrecioGratis();
            return;
        }

        foreach (CosteTienda coste in costesArticulo)
        {
            if (coste == null)
            {
                continue;
            }

            CrearPrecioVisual(coste);
        }
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
        imagen.sprite = ObtenerIconoMoneda(coste.TipoMoneda);
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

    private Sprite ObtenerIconoMoneda(TipoMoneda tipoMoneda)
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

        bool ocultar = DebeOcultarse();
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

    private bool DebeOcultarse()
    {
        if (!EsObjetoBase())
        {
            return false;
        }

        ObjetoBaseData objetoBase = ObtenerObjetoBase();

        if (objetoBase != null)
        {
            if (!objetoBase.OcultarEnTiendaMientrasEsteColocado)
            {
                return false;
            }

            if (objetoBase.TipoObjetoBase == TipoObjetoBase.Espantamonos)
            {
                if (sistemaEspantamonos == null)
                {
                    sistemaEspantamonos = SistemaEspantamonos.Instance;
                }

                return sistemaEspantamonos != null && sistemaEspantamonos.EstaActivo;
            }

            if (sistemaObjetosBase == null)
            {
                sistemaObjetosBase = SistemaObjetosBase.Instance;
            }

            return sistemaObjetosBase != null && sistemaObjetosBase.DebeOcultarseEnTienda(objetoBase);
        }

        if (articuloData == null && esEspantamonos)
        {
            if (sistemaEspantamonos == null)
            {
                sistemaEspantamonos = SistemaEspantamonos.Instance;
            }

            return sistemaEspantamonos != null && sistemaEspantamonos.EstaActivo;
        }

        return false;
    }

    private bool PuedeComprar()
    {
        if (sistemaEconomia == null)
        {
            return false;
        }

        CosteTienda[] costesArticulo = ObtenerCostes();

        if (costesArticulo == null || costesArticulo.Length == 0)
        {
            return true;
        }

        foreach (CosteTienda coste in costesArticulo)
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
        CosteTienda[] costesArticulo = ObtenerCostes();

        if (costesArticulo == null)
        {
            return;
        }

        foreach (CosteTienda coste in costesArticulo)
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
            Debug.LogError("Falta SistemaEconomia en " + ObtenerNombre(), this);
            return false;
        }

        if (botonComprar == null)
        {
            Debug.LogError("Falta Boton Comprar en " + ObtenerNombre(), this);
            return false;
        }

        if (EsItemInventario())
        {
            if (ObtenerItemInventario() == null && prefabObjeto == null)
            {
                Debug.LogError("Falta ItemData o Prefab Objeto en " + ObtenerNombre(), this);
                return false;
            }

            if (ObtenerItemInventario() != null && inventarioJugador == null)
            {
                Debug.LogError("Falta PlayerInventory en " + ObtenerNombre(), this);
                return false;
            }
        }

        if (EsObjetoBase())
        {
            ObjetoBaseData objetoBase = ObtenerObjetoBase();

            if (objetoBase != null)
            {
                if (objetoBase.Prefab == null)
                {
                    Debug.LogError("Falta prefab en " + ObtenerNombre(), this);
                    return false;
                }

                if (puntoAparicion == null)
                {
                    Debug.LogError("Falta Punto Aparicion en " + ObtenerNombre(), this);
                    return false;
                }

                if (objetoBase.TipoObjetoBase == TipoObjetoBase.Espantamonos && sistemaEspantamonos == null)
                {
                    Debug.LogError("Falta SistemaEspantamonos en " + ObtenerNombre(), this);
                    return false;
                }

                if (objetoBase.TipoObjetoBase == TipoObjetoBase.Normal && sistemaObjetosBase == null)
                {
                    Debug.LogError("Falta SistemaObjetosBase en " + ObtenerNombre(), this);
                    return false;
                }
            }
            else if (esEspantamonos)
            {
                if (prefabObjeto == null)
                {
                    Debug.LogError("Falta Prefab Objeto en " + ObtenerNombre(), this);
                    return false;
                }

                if (puntoAparicion == null)
                {
                    Debug.LogError("Falta Punto Aparicion en " + ObtenerNombre(), this);
                    return false;
                }

                if (sistemaEspantamonos == null)
                {
                    Debug.LogError("Falta SistemaEspantamonos en " + ObtenerNombre(), this);
                    return false;
                }
            }
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

        if (sistemaObjetosBase == null)
        {
            sistemaObjetosBase = FindFirstObjectByType<SistemaObjetosBase>();
        }

        if (sistemaEspantamonos == null)
        {
            sistemaEspantamonos = FindFirstObjectByType<SistemaEspantamonos>();
        }

        if (mensajeTienda == null)
        {
            mensajeTienda = FindFirstObjectByType<MensajeTienda>();
        }

        if (vendedorFantasma == null)
        {
            vendedorFantasma = FindFirstObjectByType<GhostMerchant>();
        }

        if (contenedorArticulo == null)
        {
            contenedorArticulo = gameObject;
        }

        if (imagenArticulo == null)
        {
            Transform iconoTransform = BuscarHijoRecursivo(transform, "Icono");
            if (iconoTransform != null)
            {
                imagenArticulo = iconoTransform.GetComponent<Image>();
            }
        }

        if (textoNombre == null)
        {
            Transform nombreTransform = BuscarHijoRecursivo(transform, "Nombre");
            if (nombreTransform != null)
            {
                textoNombre = nombreTransform.GetComponent<TMP_Text>();
            }
        }

        if (contenedorPrecios == null)
        {
            Transform precioTransform = BuscarHijoRecursivo(transform, "PrecioContenedor");
            if (precioTransform != null)
            {
                contenedorPrecios = precioTransform;
            }
        }

        if (botonComprar == null)
        {
            Transform botonTransform = BuscarHijoRecursivo(transform, "BuyButton");
            if (botonTransform != null)
            {
                botonComprar = botonTransform.GetComponent<Button>();
            }

            if (botonComprar == null)
            {
                botonComprar = GetComponentInChildren<Button>(true);
            }
        }

        if (textoBotonComprar == null && botonComprar != null)
        {
            Transform textoBotonTransform = BuscarHijoRecursivo(botonComprar.transform, "TxtComprar");
            if (textoBotonTransform != null)
            {
                textoBotonComprar = textoBotonTransform.GetComponent<TMP_Text>();
            }

            if (textoBotonComprar == null)
            {
                textoBotonComprar = botonComprar.GetComponentInChildren<TMP_Text>(true);
            }
        }
    }

    private Transform BuscarHijoRecursivo(Transform padre, string nombre)
    {
        if (padre == null)
        {
            return null;
        }

        for (int i = 0; i < padre.childCount; i++)
        {
            Transform hijo = padre.GetChild(i);

            if (hijo.name == nombre)
            {
                return hijo;
            }

            Transform encontrado = BuscarHijoRecursivo(hijo, nombre);

            if (encontrado != null)
            {
                return encontrado;
            }
        }

        return null;
    }

    private bool EsItemInventario()
    {
        if (articuloData != null)
        {
            return articuloData.TipoArticulo == TipoArticuloTienda.ItemInventario;
        }

        return !esEspantamonos;
    }

    private bool EsObjetoBase()
    {
        if (articuloData != null)
        {
            return articuloData.TipoArticulo == TipoArticuloTienda.ObjetoBase;
        }

        return esEspantamonos;
    }

    private ItemData ObtenerItemInventario()
    {
        if (articuloData != null)
        {
            return articuloData.ItemInventario;
        }

        return itemInventario;
    }

    private ObjetoBaseData ObtenerObjetoBase()
    {
        if (articuloData != null)
        {
            return articuloData.ObjetoBase;
        }

        return null;
    }

    private int ObtenerCantidadItem()
    {
        if (articuloData != null)
        {
            return articuloData.CantidadItem;
        }

        return cantidadItem;
    }

    private CosteTienda[] ObtenerCostes()
    {
        if (articuloData != null)
        {
            return articuloData.Costes;
        }

        return costes;
    }

    private string ObtenerNombre()
    {
        if (articuloData != null)
        {
            return articuloData.Nombre;
        }

        if (!string.IsNullOrWhiteSpace(nombreArticulo))
        {
            return nombreArticulo;
        }

        if (itemInventario != null)
        {
            return itemInventario.DisplayName;
        }

        if (prefabObjeto != null)
        {
            return prefabObjeto.name;
        }

        return gameObject.name;
    }

    private Sprite ObtenerIconoArticulo()
    {
        if (articuloData != null)
        {
            return articuloData.Icono;
        }

        if (iconoArticulo != null)
        {
            return iconoArticulo;
        }

        if (itemInventario != null)
        {
            return itemInventario.Icon;
        }

        return null;
    }

    private void MostrarError(string mensaje)
    {
        if (mensajeTienda != null)
        {
            mensajeTienda.MostrarError(mensaje);
        }

        Debug.LogWarning(mensaje, this);
    }

    private void MostrarConfirmacion(string mensaje)
    {
        if (mensajeTienda != null)
        {
            mensajeTienda.MostrarConfirmacion(mensaje);
        }

        Debug.Log(mensaje, this);
    }

    private void AlActualizarEconomia(int conchas, int tulipanes, int pinyas)
    {
        RefrescarUI();
    }

    private void AlCambiarEstadoEspantamonos(bool espantamonosActivo)
    {
        RefrescarUI();
    }

    private void AlActualizarObjetosBase()
    {
        RefrescarUI();
    }
}
