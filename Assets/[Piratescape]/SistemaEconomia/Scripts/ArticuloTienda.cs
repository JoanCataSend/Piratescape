using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Audio;

public sealed class ArticuloTienda : MonoBehaviour
{
    [Header("Datos del articulo")]
    [SerializeField] private ArticuloTiendaData articuloData;

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
    [SerializeField] private TMP_Text textoDescripcion;
    [SerializeField] private Transform contenedorPrecios;
    [SerializeField] private PrecioMonedaUI prefabPrecioMoneda;
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

    [Header("Sonidos compra")]
    [SerializeField] private AudioSource audioSourceCompra;
    [SerializeField] private AudioMixerGroup outputCompra;
    [SerializeField] private AudioClip[] sonidosCompraCorrecta;
    [SerializeField] private AudioClip sonidoCompraFallida;
    [SerializeField] private float volumenCompraCorrecta = 0.6f;
    [SerializeField] private float volumenCompraFallida = 0.6f;

    private int ultimoIndiceCompra = -1;

    private readonly List<TMP_Text> textosPrecio = new List<TMP_Text>();
    private readonly List<CosteTienda> costesPrecio = new List<CosteTienda>();


    private bool eventosSuscritos;

    private void Awake()
    {
        BuscarReferenciasSiFaltan();
        PrepararAudioCompra();
        SuscribirseEventos();
        PrepararUI();
        RefrescarUI();
    }

    private void OnEnable()
    {
        BuscarReferenciasSiFaltan();
        SuscribirseEventos();
        RefrescarUI();
    }

    private void OnDestroy()
    {
        DesuscribirseEventos();
    }

    public void Configurar(ArticuloTiendaData nuevoArticuloData)
    {
        Configurar(nuevoArticuloData, puntoAparicion);
    }

    public void Configurar(ArticuloTiendaData nuevoArticuloData, Transform nuevoPuntoAparicion)
    {
        articuloData = nuevoArticuloData;
        puntoAparicion = nuevoPuntoAparicion;

        BuscarReferenciasSiFaltan();
        PrepararUI();
        RefrescarUI();
    }

    private void SuscribirseEventos()
    {
        if (eventosSuscritos)
        {
            return;
        }

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

        eventosSuscritos = true;
    }

    private void DesuscribirseEventos()
    {
        if (!eventosSuscritos)
        {
            return;
        }

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

        eventosSuscritos = false;
    }

    private void Comprar()
    {
        BuscarReferenciasSiFaltan();

        if (!ReferenciasValidas())
        {
            return;
        }

        if (inventarioJugador != null)
        {
            inventarioJugador.BloquearInputUnFrame();
        }

        if (DebeOcultarse())
        {
            RefrescarUI();
            return;
        }

        if (!PuedeComprar())
        {
            ReproducirCompraFallida();
            MostrarError("No tienes suficiente dinero");
            RefrescarUI();
            return;
        }

        if (articuloData.TipoArticulo == TipoArticuloTienda.ItemInventario && !PuedeEntrarEnInventario())
        {
            MostrarError("Inventario lleno");
            return;
        }

        bool compraCorrecta = false;

        if (articuloData.TipoArticulo == TipoArticuloTienda.ItemInventario)
        {
            compraCorrecta = ComprarItemInventario();
        }
        else if (articuloData.TipoArticulo == TipoArticuloTienda.ObjetoBase)
        {
            ObjetoBaseData objetoBase = articuloData.ObjetoBase;

            if (objetoBase != null && objetoBase.TipoObjetoBase == TipoObjetoBase.Espantamonos)
            {
                if (vendedorFantasma != null)
                {
                    vendedorFantasma.StartCoroutine(ComprarEspantamonosConAnimacion(objetoBase));
                }

                return;
            }

            if (objetoBase != null && objetoBase.TipoObjetoBase == TipoObjetoBase.Cofre)
            {
                if (vendedorFantasma != null)
                {
                    vendedorFantasma.StartCoroutine(ComprarObjetoBaseConAnimacion(objetoBase));
                }

                return;
            }

            compraCorrecta = ComprarObjetoBase();
        }

        if (!compraCorrecta)
        {
            return;
        }

        Cobrar();
        ReproducirCompraCorrecta();
        RefrescarUI();

        if (cerrarTiendaAlComprar && vendedorFantasma != null)
        {
            vendedorFantasma.CerrarTiendaDesdeUI();
        }
    }

    private bool ComprarItemInventario()
    {
        if (inventarioJugador == null || articuloData.ItemInventario == null)
        {
            MostrarError("Falta configurar el item de inventario");
            return false;
        }

        bool anadido = inventarioJugador.TryAddItem(articuloData.ItemInventario, articuloData.CantidadItem);

        if (!anadido)
        {
            MostrarError("Inventario lleno");
            return false;
        }

        MostrarConfirmacion("Has comprado " + articuloData.Nombre);
        return true;
    }

    private bool ComprarObjetoBase()
    {
        ObjetoBaseData objetoBase = articuloData.ObjetoBase;

        if (objetoBase == null)
        {
            MostrarError("Falta configurar el objeto de base");
            return false;
        }

        if (objetoBase.TipoObjetoBase == TipoObjetoBase.Espantamonos)
        {
            return ComprarEspantamonos(objetoBase);
        }

        return ComprarObjetoBaseNormal(objetoBase);
    }

    private bool ComprarEspantamonos(ObjetoBaseData objetoBase)
    {
        if (sistemaEspantamonos == null)
        {
            MostrarError("Falta SistemaEspantamonos");
            return false;
        }

        bool colocado = sistemaEspantamonos.ComprarEspantamonos(objetoBase.Prefab, puntoAparicion);

        if (!colocado)
        {
            MostrarError(objetoBase.MensajeYaColocado);
            return false;
        }

        MostrarConfirmacion(objetoBase.MensajeCompra);
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
        if (inventarioJugador == null || articuloData == null || articuloData.ItemInventario == null)
        {
            return false;
        }

        return inventarioJugador.CanAddItem(articuloData.ItemInventario, articuloData.CantidadItem);
    }

    private void PrepararUI()
    {
        if (articuloData == null)
        {
            return;
        }

        if (textoNombre != null)
        {
            textoNombre.text = articuloData.Nombre;
        }

        if (textoDescripcion != null)
        {
            string descripcion = articuloData.Descripcion;
            textoDescripcion.text = descripcion;
            textoDescripcion.gameObject.SetActive(!string.IsNullOrWhiteSpace(descripcion));
        }

        if (imagenArticulo != null)
        {
            imagenArticulo.sprite = articuloData.Icono;
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

        CosteTienda[] costes = articuloData != null ? articuloData.Costes : null;

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
        if (prefabPrecioMoneda == null)
        {
            Debug.LogError("ArticuloTienda: falta asignar el prefab PrecioMonedaUI.", this);
            return;
        }

        PrecioMonedaUI precioUI = Instantiate(prefabPrecioMoneda, contenedorPrecios);
        precioUI.name = "Precio_" + coste.TipoMoneda;

        Sprite icono = ObtenerIconoMoneda(coste.TipoMoneda);
        precioUI.Configurar(icono, coste.Cantidad);

        if (precioUI.TextoCantidad != null)
        {
            textosPrecio.Add(precioUI.TextoCantidad);
            costesPrecio.Add(coste);
        }
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

        botonComprar.interactable = !desactivarBotonSiNoPuedeComprar || PuedeComprar();
    }

    private bool DebeOcultarse()
    {
        if (articuloData == null || articuloData.TipoArticulo != TipoArticuloTienda.ObjetoBase)
        {
            return false;
        }

        ObjetoBaseData objetoBase = articuloData.ObjetoBase;

        if (objetoBase == null || !objetoBase.OcultarEnTiendaMientrasEsteColocado)
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

    private bool PuedeComprar()
    {
        if (sistemaEconomia == null || articuloData == null)
        {
            return false;
        }

        CosteTienda[] costes = articuloData.Costes;

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
        if (articuloData == null || articuloData.Costes == null)
        {
            return;
        }

        foreach (CosteTienda coste in articuloData.Costes)
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
        if (articuloData == null)
        {
            Debug.LogError("Falta ArticuloTiendaData en " + gameObject.name, this);
            return false;
        }

        if (sistemaEconomia == null)
        {
            Debug.LogError("Falta SistemaEconomia en " + articuloData.Nombre, this);
            return false;
        }

        if (botonComprar == null)
        {
            Debug.LogError("Falta Boton Comprar en " + articuloData.Nombre, this);
            return false;
        }

        if (articuloData.TipoArticulo == TipoArticuloTienda.ItemInventario)
        {
            if (inventarioJugador == null)
            {
                Debug.LogError("Falta PlayerInventory en " + articuloData.Nombre, this);
                return false;
            }

            if (articuloData.ItemInventario == null)
            {
                Debug.LogError("Falta ItemData en " + articuloData.Nombre, this);
                return false;
            }
        }

        if (articuloData.TipoArticulo == TipoArticuloTienda.ObjetoBase)
        {
            ObjetoBaseData objetoBase = articuloData.ObjetoBase;

            if (objetoBase == null)
            {
                Debug.LogError("Falta ObjetoBaseData en " + articuloData.Nombre, this);
                return false;
            }

            if (objetoBase.Prefab == null)
            {
                Debug.LogError("Falta prefab en " + articuloData.Nombre, this);
                return false;
            }

            if (puntoAparicion == null)
            {
                Debug.LogError("Falta Punto Aparicion en " + articuloData.Nombre, this);
                return false;
            }

            if (objetoBase.TipoObjetoBase == TipoObjetoBase.Espantamonos && sistemaEspantamonos == null)
            {
                Debug.LogError("Falta SistemaEspantamonos en " + articuloData.Nombre, this);
                return false;
            }

            if (objetoBase.TipoObjetoBase != TipoObjetoBase.Espantamonos && sistemaObjetosBase == null)
            {
                Debug.LogError("Falta SistemaObjetosBase en " + articuloData.Nombre, this);
                return false;
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

        if (textoDescripcion == null)
        {
            Transform descripcionTransform = BuscarHijoRecursivo(transform, "Descripcion");
            if (descripcionTransform != null)
            {
                textoDescripcion = descripcionTransform.GetComponent<TMP_Text>();
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

    private IEnumerator ComprarEspantamonosConAnimacion(ObjetoBaseData objetoBase)
    {
        if (sistemaEspantamonos == null)
        {
            MostrarError("Falta SistemaEspantamonos");
            yield break;
        }

        vendedorFantasma.CerrarTienda(false);

        bool colocado = sistemaEspantamonos.ComprarEspantamonos(objetoBase.Prefab, puntoAparicion);

        if (!colocado)
        {
            MostrarError(objetoBase.MensajeYaColocado);
            vendedorFantasma.AbrirTienda();
            yield break;
        }

        Cobrar();
        RefrescarUI();

        Animator animator = sistemaEspantamonos.EspantamonosActual.GetComponentInChildren<Animator>();

        if (animator != null)
        {
            yield return null;

            AnimatorStateInfo estado = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(estado.length + 0.6f);
        }

        vendedorFantasma.AbrirTienda();
    }

    private IEnumerator ComprarObjetoBaseConAnimacion(ObjetoBaseData objetoBase)
    {
        if (sistemaObjetosBase == null)
        {
            MostrarError("Falta SistemaObjetosBase");
            yield break;
        }

        vendedorFantasma.CerrarTienda(false);

        bool colocado = sistemaObjetosBase.ColocarObjeto(objetoBase, puntoAparicion);

        if (!colocado)
        {
            MostrarError(objetoBase.MensajeYaColocado);
            vendedorFantasma.AbrirTienda();
            yield break;
        }

        Cobrar();
        RefrescarUI();

        GameObject ultimoObjeto = sistemaObjetosBase.UltimoObjetoColocado;

        if (ultimoObjeto != null)
        {
            Animator animator = ultimoObjeto.GetComponentInChildren<Animator>();

            if (animator != null)
            {
                yield return null;

                AnimatorStateInfo estado = animator.GetCurrentAnimatorStateInfo(0);

                yield return new WaitForSeconds(estado.length + 0.6f);
            }
            else
            {
                yield return new WaitForSeconds(3f);
            }
        }
        else
        {
            yield return new WaitForSeconds(3f);
        }

        vendedorFantasma.AbrirTienda();
    }

    private void PrepararAudioCompra()
    {
        if (audioSourceCompra == null)
        {
            audioSourceCompra = gameObject.AddComponent<AudioSource>();
        }

        audioSourceCompra.playOnAwake = false;
        audioSourceCompra.loop = false;
        audioSourceCompra.spatialBlend = 0f;
        audioSourceCompra.outputAudioMixerGroup = outputCompra;
    }

    private void ReproducirCompraCorrecta()
    {
        AudioClip clip = ObtenerSonidoCompraCorrecta();

        if (audioSourceCompra == null || clip == null)
        {
            return;
        }

        audioSourceCompra.PlayOneShot(clip, volumenCompraCorrecta);
    }

    private void ReproducirCompraFallida()
    {
        if (audioSourceCompra == null || sonidoCompraFallida == null)
        {
            return;
        }

        audioSourceCompra.PlayOneShot(sonidoCompraFallida, volumenCompraFallida);
    }

    private AudioClip ObtenerSonidoCompraCorrecta()
    {
        if (sonidosCompraCorrecta == null || sonidosCompraCorrecta.Length == 0)
        {
            return null;
        }

        if (sonidosCompraCorrecta.Length == 1)
        {
            ultimoIndiceCompra = 0;
            return sonidosCompraCorrecta[0];
        }

        int indice;

        do
        {
            indice = Random.Range(0, sonidosCompraCorrecta.Length);
        }
        while (indice == ultimoIndiceCompra);

        ultimoIndiceCompra = indice;
        return sonidosCompraCorrecta[indice];
    }
}
