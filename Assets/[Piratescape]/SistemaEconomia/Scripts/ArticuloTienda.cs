using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ArticuloTienda : MonoBehaviour
{
    [Header("Datos del articulo")]
    [SerializeField] private string nombreArticulo;
    [SerializeField] private Sprite iconoArticulo;
    [SerializeField] private GameObject prefabObjeto;
    [SerializeField] private CosteTienda[] costes;

    [Header("Referencias compra")]
    [SerializeField] private SistemaEconomia sistemaEconomia;
    [SerializeField] private Transform puntoAparicion;
    [SerializeField] private MensajeTienda mensajeTienda;
    [SerializeField] private GhostMerchant vendedorFantasma;

    [Header("Referencias UI")]
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
    [SerializeField] private bool cerrarTiendaAlComprar = true;

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
            if (mensajeTienda != null)
            {
                mensajeTienda.MostrarError("No tienes suficiente dinero");
            }

            RefrescarUI();
            return;
        }

        bool compraCorrecta = CrearObjetoComprado();

        if (!compraCorrecta)
        {
            if (mensajeTienda != null)
            {
                mensajeTienda.MostrarError("No se pudo comprar " + nombreArticulo);
            }

            return;
        }

        Cobrar();

        if (mensajeTienda != null)
        {
            mensajeTienda.MostrarConfirmacion("Has comprado " + nombreArticulo);
        }

        RefrescarUI();

        if (cerrarTiendaAlComprar && vendedorFantasma != null)
        {
            vendedorFantasma.CerrarTiendaDesdeUI();
        }
    }

    private bool CrearObjetoComprado()
    {
        if (esEspantamonos)
        {
            if (sistemaEspantamonos == null)
            {
                return false;
            }

            return sistemaEspantamonos.ComprarEspantamonos(prefabObjeto, puntoAparicion);
        }

        Instantiate(prefabObjeto, puntoAparicion.position, puntoAparicion.rotation);
        return true;
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

        if (gameObject.activeSelf == ocultar)
        {
            gameObject.SetActive(!ocultar);
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

            if (tieneSuficiente)
            {
                texto.color = colorPrecioNormal;
            }
            else
            {
                texto.color = colorPrecioInsuficiente;
            }
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

        if (prefabObjeto == null)
        {
            Debug.LogError("Falta prefabObjeto en " + nombreArticulo, this);
            return false;
        }

        if (puntoAparicion == null)
        {
            Debug.LogError("Falta puntoAparicion en " + nombreArticulo, this);
            return false;
        }

        if (botonComprar == null)
        {
            Debug.LogError("Falta botonComprar en " + nombreArticulo, this);
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