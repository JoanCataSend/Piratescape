using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MonoAmistadPlatano : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private ItemData itemPlatano;
    [SerializeField] private Animator animatorMono;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Interaccion")]
    [SerializeField] private float rangoInteraccion = 2.5f;
    [SerializeField] private int platanosNecesariosParaAmistad = 2;
    [SerializeField] private string itemIdPlatano = "platano";
    [SerializeField] private string mensajeSinPlatanos = "Necesitas platanos";
    [SerializeField] private string mensajeDarPlatano = "Dar platano";
    [SerializeField] private string mensajeMonoAmigo = "El mono ahora es tu amigo";
    [SerializeField] private float duracionMensajeTemporal = 1.8f;

    [Header("Seguimiento")]
    [SerializeField] private bool seguirAlSerAmigo = true;
    [SerializeField] private float distanciaParar = 1.8f;
    [SerializeField] private float distanciaPararAmigo = 3.2f;
    [SerializeField] private float velocidadSinNavMesh = 2.5f;
    [SerializeField] private float velocidadGiro = 8f;
    [SerializeField] private bool mantenerAlturaInicialSinNavMesh = true;

    [Header("Deteccion de suelo")]
    [SerializeField] private bool ajustarAlturaAlSuelo = true;
    [SerializeField] private LayerMask capasSuelo = ~0;
    [SerializeField] private float alturaSobreSuelo = 0.05f;
    [SerializeField] private float alturaOrigenRaycastSuelo = 2f;
    [SerializeField] private float distanciaRaycastSuelo = 8f;
    [SerializeField] private float suavizadoAlturaSuelo = 18f;
    [SerializeField] private bool corregirAlturaConNavMeshAgent = true;
    [SerializeField] private bool ignorarTriggersSuelo = true;
    [SerializeField] private bool ignorarOtrosMonosComoSuelo = true;
    [SerializeField] private bool ignorarJugadorComoSuelo = true;
    [SerializeField] private bool ignorarItemsComoSuelo = true;

    [Header("Separacion entre monos")]
    [SerializeField] private bool ignorarColisionesFisicasConOtrosMonos = true;
    [SerializeField] private float intervaloActualizarColisionesMonos = 2f;

    [Header("Orientacion")]
    [SerializeField] private bool mirarSiempreAlJugador = true;
    [SerializeField] private float correccionRotacionYModelo = 0f;

    [Header("Modos del mono amigo")]
    [SerializeField] private ModoMonoAmigo modoAmigo = ModoMonoAmigo.Sigueme;
    [SerializeField] private bool permitirCambiarModoConInteraccion = true;
    [SerializeField] private string textoPromptCambiarModo = "Cambiar orden";
    [SerializeField] private string mensajeModoSigueme = "Modo: sigueme";
    [SerializeField] private string mensajeModoEsperarAqui = "Modo: esperar aqui";
    [SerializeField] private string mensajeModoBuscarObjetos = "Modo: buscar objetos";
    [SerializeField] private string mensajeModoDefenderme = "Modo: defenderme";
    [SerializeField] private bool mostrarModoActualEnPrompt = true;

    [Header("Recolector de items")]
    [SerializeField] private bool recogerItemsSiEsAmigo = true;
    [SerializeField] private float rangoBusquedaItems = 8f;
    [SerializeField] private float intervaloBusquedaItems = 0.75f;
    [SerializeField] private float distanciaRecogerItem = 1.1f;
    [SerializeField] private float distanciaSoltarCercaJugador = 2.2f;
    [SerializeField] private float distanciaDelanteJugadorAlSoltar = 1.25f;
    [SerializeField] private float alturaSoltarItem = 0.35f;
    [SerializeField] private Transform puntoLlevarItem;
    [SerializeField] private Transform puntoSoltarItem;
    [SerializeField] private LayerMask capasItems = ~0;
    [SerializeField] private bool ignorarItemsQueSeanHijosDelJugador = true;
    [SerializeField] private bool noVolverARecogerItemsYaEntregados = true;
    [SerializeField] private float margenExtraRecogida = 0.35f;
    [SerializeField] private bool usarColliderParaMedirDistancia = true;
    [SerializeField] private bool ignorarCanaDePescarComoItem = true;
    [SerializeField] private string[] palabrasItemsIgnoradosPorMono = new string[] { "cana", "caña", "pescar", "fishing rod", "fishing_rod", "fishingrod", "rod", "hook", "anzuelo" };

    [Header("Defensa del jugador")]
    [SerializeField] private bool defenderJugadorSiRecibeDanio = true;
    [SerializeField] private LayerMask capasEnemigos = ~0;
    [SerializeField] private string[] tagsEnemigos = new string[] { "Enemy", "Enemigo", "Tiburon", "Cangrejo", "Pirana", "Piraña" };
    [SerializeField] private float rangoMaximoEscucharDanio = 25f;
    [SerializeField] private float radioBuscarEnemigoCercaDanio = 4f;
    [SerializeField] private float tiempoPerseguirEnemigoTrasDanio = 8f;
    [SerializeField] private float distanciaAtaqueEnemigo = 1.1f;
    [SerializeField] private int danioAlEnemigo = 10;
    [SerializeField] private float tiempoEntreAtaquesEnemigo = 0.8f;
    [SerializeField] private bool cancelarObjetivoDeRecoleccionParaDefender = true;
    [SerializeField] private string triggerAtacarEnemigo = "Attack";
    [SerializeField] private bool mostrarLogsDefensa = false;
    [SerializeField] private float radioDefensaActiva = 8f;
    [SerializeField] private float intervaloBusquedaEnemigosDefensa = 0.45f;

    [Header("Depuracion opcional")]
    [SerializeField] private bool mostrarLogsRecolector = false;

    [Header("Estado")]
    [SerializeField] private int platanosRecibidos;
    [SerializeField] private bool esAmigo;

    [Header("Animacion opcional")]
    [SerializeField] private string triggerRecibirPlatano = "RecibirPlatano";
    [SerializeField] private string triggerAmigo = "Amigo";
    [SerializeField] private string boolSiguiendo = "Siguiendo";
    [SerializeField] private string triggerRecogerItem = "RecogerItem";
    [SerializeField] private string triggerSoltarItem = "SoltarItem";

    [Header("FX amistad corazones")]
    [SerializeField] private bool reproducirCorazonesAlHacerseAmigo = true;
    [SerializeField] private bool crearParticulasCorazonesAutomaticas = true;
    [SerializeField] private bool configurarCorazonesPorCodigo = true;
    [SerializeField] private ParticleSystem particulasCorazonesAmistad;
    [SerializeField] private Transform puntoFXCorazones;
    [SerializeField] private Vector3 offsetCorazones = new Vector3(0f, 1.45f, 0f);
    [SerializeField] private bool usarPrefabCorazon3D = true;
    [SerializeField] private GameObject prefabCorazon3D;
    [SerializeField] private float escalaPrefabCorazonMin = 0.18f;
    [SerializeField] private float escalaPrefabCorazonMax = 0.28f;
    [SerializeField] private float radioSpawnPrefabCorazones = 0.28f;
    [SerializeField] private float duracionPrefabCorazones = 1.55f;
    [SerializeField] private bool usarRotacionOriginalPrefabCorazon = true;
    [SerializeField] private Vector3 rotacionExtraPrefabCorazonEuler = Vector3.zero;
    [SerializeField] private bool aplicarGiroAleatorioSuaveCorazon = true;
    [SerializeField] private float inclinacionAleatoriaCorazon = 10f;
    [SerializeField] private float giroAleatorioYCorazon = 360f;
    [SerializeField] private bool colorearPrefabCorazonDeRojo = true;
    [SerializeField] private bool desactivarCollidersPrefabCorazon = true;
    [SerializeField] private Material materialCorazones;
    [SerializeField] private bool usarTexturaCorazonSuave = true;
    [SerializeField] private bool usarTexturaCorazonPixel = false;
    [SerializeField] private int corazonesMin = 10;
    [SerializeField] private int corazonesMax = 14;
    [SerializeField] private float tamanoCorazonMin = 0.1f;
    [SerializeField] private float tamanoCorazonMax = 0.18f;
    [SerializeField] private float velocidadVerticalCorazonesMin = 0.75f;
    [SerializeField] private float velocidadVerticalCorazonesMax = 1.35f;
    [SerializeField] private float expansionHorizontalCorazones = 0.28f;
    [SerializeField] private float duracionCorazones = 1.6f;
    [SerializeField] private Color colorCorazon = new Color(1f, 0.05f, 0.05f, 1f);

    private float alturaInicial;
    private Collider[] collidersPropios;
    private float siguienteActualizacionColisionesMonos;
    private bool promptMostrado;
    private float tiempoHastaPermitirOcultarPrompt;

    private ObjetoRecogibleInteractuable itemObjetivo;
    private ObjetoRecogibleInteractuable itemRecogido;
    private Rigidbody rigidbodyItemRecogido;
    private bool itemUsabaGravedad;
    private bool itemEraKinematic;
    private Collider[] collidersItemRecogido;
    private bool[] collidersItemRecogidoActivos;
    private float siguienteBusquedaItems;
    private readonly List<ObjetoRecogibleInteractuable> itemsYaEntregados = new List<ObjetoRecogibleInteractuable>();
    private Transform enemigoObjetivo;
    private float tiempoHastaAbandonarDefensa;
    private float tiempoSiguienteAtaqueEnemigo;
    private float siguienteBusquedaDefensaActiva;
    private Material materialCorazonesGenerado;
    private static Texture2D texturaCorazonPixelCompartida;
    private static Texture2D texturaCorazonSuaveCompartida;

    private enum ModoMonoAmigo
    {
        Sigueme,
        EsperarAqui,
        BuscarObjetos,
        Defenderme
    }

    private enum EstadoRecolector
    {
        SiguiendoJugador,
        YendoAItem,
        VolviendoConItem,
        DefendiendoJugador
    }

    private EstadoRecolector estadoRecolector = EstadoRecolector.SiguiendoJugador;

    private void Awake()
    {
        alturaInicial = transform.position.y;
        collidersPropios = GetComponentsInChildren<Collider>(true);

        if (jugador == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                jugador = playerObject.transform;
            }
        }

        if (inventarioJugador == null && jugador != null)
        {
            inventarioJugador = jugador.GetComponent<PlayerInventory>();
        }

        if (animatorMono == null)
        {
            animatorMono = GetComponentInChildren<Animator>();
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        ConfigurarNavMeshAgent();
        ConfigurarColisionesConOtrosMonos();
        ConfigurarParticulasCorazones();
    }

    private void OnEnable()
    {
        ConfigurarNavMeshAgent();
        ConfigurarColisionesConOtrosMonos();
        JugadorDanioFeedback.OnJugadorDaniado += AlJugadorLeHanHechoDanio;
    }

    private void OnDisable()
    {
        JugadorDanioFeedback.OnJugadorDaniado -= AlJugadorLeHanHechoDanio;
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            OcultarPrompt();
            return;
        }

        if (CofreInventarioUI.HayAlgunaUIAbierta)
        {
            OcultarPrompt();
            return;
        }

        if (jugador == null)
        {
            BuscarJugador();
        }

        if (jugador == null)
        {
            return;
        }

        ActualizarColisionesConOtrosMonosSiHaceFalta();

        if (esAmigo)
        {
            GestionarInteraccionModoAmigo();
            GestionarComportamientoAmigo();
            return;
        }

        GestionarInteraccion();
    }

    private void LateUpdate()
    {
        if (ajustarAlturaAlSuelo)
        {
            AjustarAlturaAlSuelo();
        }
    }

    private void GestionarInteraccion()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > rangoInteraccion)
        {
            OcultarPrompt();
            return;
        }

        MostrarPrompt();

        if (SeHaPulsadoInteraccion())
        {
            IntentarDarPlatano();
        }
    }

    private void IntentarDarPlatano()
    {
        if (inventarioJugador == null)
        {
            inventarioJugador = jugador.GetComponent<PlayerInventory>();
        }

        if (inventarioJugador == null)
        {
            MostrarMensajeTemporal("No encuentro el inventario");
            return;
        }

        ItemData platanoEncontrado = ObtenerItemPlatanoDelInventario();

        if (platanoEncontrado == null)
        {
            MostrarMensajeTemporal(mensajeSinPlatanos);
            return;
        }

        bool eliminado = inventarioJugador.RemoveItem(platanoEncontrado, 1);

        if (!eliminado)
        {
            MostrarMensajeTemporal(mensajeSinPlatanos);
            return;
        }

        platanosRecibidos++;
        ReproducirTrigger(triggerRecibirPlatano);

        if (platanosRecibidos >= platanosNecesariosParaAmistad)
        {
            ConvertirEnAmigo();
            return;
        }

        MostrarMensajeTemporal(mensajeDarPlatano + " " + platanosRecibidos + "/" + platanosNecesariosParaAmistad);
    }

    private ItemData ObtenerItemPlatanoDelInventario()
    {
        if (inventarioJugador == null)
        {
            return null;
        }

        if (itemPlatano != null && inventarioJugador.ObtenerCantidad(itemPlatano) > 0)
        {
            return itemPlatano;
        }

        foreach (InventorySlot slot in inventarioJugador.GetSlots())
        {
            if (slot == null || slot.IsEmpty() || slot.itemData == null)
            {
                continue;
            }

            if (EsPlatano(slot.itemData))
            {
                return slot.itemData;
            }
        }

        return null;
    }

    private bool EsPlatano(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        string id = item.ItemId != null ? item.ItemId.ToLowerInvariant() : "";
        string nombreAsset = item.name != null ? item.name.ToLowerInvariant() : "";
        string nombreMostrar = item.DisplayName != null ? item.DisplayName.ToLowerInvariant() : "";
        string idBuscado = itemIdPlatano != null ? itemIdPlatano.ToLowerInvariant() : "platano";

        return id == idBuscado ||
               nombreAsset.Contains(idBuscado) ||
               nombreMostrar.Contains(idBuscado) ||
               nombreAsset.Contains("plátano") ||
               nombreMostrar.Contains("plátano") ||
               nombreAsset.Contains("banana") ||
               nombreMostrar.Contains("banana");
    }

    private void ConvertirEnAmigo()
    {
        esAmigo = true;
        platanosRecibidos = Mathf.Max(platanosRecibidos, platanosNecesariosParaAmistad);
        modoAmigo = ModoMonoAmigo.Sigueme;
        estadoRecolector = EstadoRecolector.SiguiendoJugador;

        ReproducirTrigger(triggerAmigo);
        CambiarBool(boolSiguiendo, seguirAlSerAmigo);
        ReproducirFXCorazonesAmistad();
        MostrarMensajeTemporal(mensajeMonoAmigo);
        ConfigurarNavMeshAgent();
    }

    private void GestionarComportamientoAmigo()
    {
        if (!seguirAlSerAmigo)
        {
            CambiarBool(boolSiguiendo, false);
            PararNavMeshAgent();
            return;
        }

        if (itemRecogido != null)
        {
            VolverAlJugadorConItem();
            return;
        }

        switch (modoAmigo)
        {
            case ModoMonoAmigo.Sigueme:
                GestionarModoSigueme();
                break;

            case ModoMonoAmigo.EsperarAqui:
                GestionarModoEsperarAqui();
                break;

            case ModoMonoAmigo.BuscarObjetos:
                GestionarModoBuscarObjetos();
                break;

            case ModoMonoAmigo.Defenderme:
                GestionarModoDefenderme();
                break;
        }
    }

    private void GestionarModoSigueme()
    {
        if (defenderJugadorSiRecibeDanio && EnemigoObjetivoValido())
        {
            DefenderJugadorContraEnemigo();
            return;
        }

        itemObjetivo = null;
        estadoRecolector = EstadoRecolector.SiguiendoJugador;
        SeguirJugador();
    }

    private void GestionarModoEsperarAqui()
    {
        itemObjetivo = null;
        enemigoObjetivo = null;
        estadoRecolector = EstadoRecolector.SiguiendoJugador;
        CambiarBool(boolSiguiendo, false);
        PararNavMeshAgent();

        if (mirarSiempreAlJugador && jugador != null)
        {
            MirarHacia(jugador.position);
        }
    }

    private void GestionarModoBuscarObjetos()
    {
        enemigoObjetivo = null;

        if (!recogerItemsSiEsAmigo)
        {
            SeguirJugador();
            return;
        }

        if (itemObjetivo == null || !ItemPuedeSerRecogido(itemObjetivo))
        {
            itemObjetivo = null;
            estadoRecolector = EstadoRecolector.SiguiendoJugador;
        }

        if (itemObjetivo == null && Time.time >= siguienteBusquedaItems)
        {
            siguienteBusquedaItems = Time.time + intervaloBusquedaItems;
            itemObjetivo = BuscarItemMasCercanoParaMono();

            if (itemObjetivo != null)
            {
                estadoRecolector = EstadoRecolector.YendoAItem;
            }
        }

        if (itemObjetivo != null)
        {
            IrARecogerItem();
            return;
        }

        SeguirJugador();
    }

    private void GestionarModoDefenderme()
    {
        if (defenderJugadorSiRecibeDanio)
        {
            BuscarEnemigoActivoParaDefenderSiHaceFalta();
        }

        if (defenderJugadorSiRecibeDanio && EnemigoObjetivoValido())
        {
            DefenderJugadorContraEnemigo();
            return;
        }

        itemObjetivo = null;
        estadoRecolector = EstadoRecolector.SiguiendoJugador;
        SeguirJugador();
    }

    private void IrARecogerItem()
    {
        if (itemObjetivo == null || !ItemPuedeSerRecogido(itemObjetivo))
        {
            itemObjetivo = null;
            estadoRecolector = EstadoRecolector.SiguiendoJugador;
            SeguirJugador();
            return;
        }

        float distancia = ObtenerDistanciaHorizontalAItem(itemObjetivo);
        float distanciaNecesaria = distanciaRecogerItem + margenExtraRecogida;

        if (mostrarLogsRecolector)
        {
            Debug.Log($"[MonoAmistadPlatano] Yendo a item '{itemObjetivo.name}'. Distancia: {distancia:0.00}. Necesaria: {distanciaNecesaria:0.00}", this);
        }

        if (distancia <= distanciaNecesaria || NavMeshAgentYaHaLlegadoAlDestino())
        {
            RecogerItemObjetivo();
            return;
        }

        MoverHacia(ObtenerDestinoItem(itemObjetivo), 0.15f, false);
    }

    private void RecogerItemObjetivo()
    {
        if (itemObjetivo == null || !ItemPuedeSerRecogido(itemObjetivo))
        {
            itemObjetivo = null;
            estadoRecolector = EstadoRecolector.SiguiendoJugador;
            return;
        }

        itemRecogido = itemObjetivo;
        itemObjetivo = null;
        estadoRecolector = EstadoRecolector.VolviendoConItem;

        if (mostrarLogsRecolector)
        {
            Debug.Log($"[MonoAmistadPlatano] Item recogido: '{itemRecogido.name}'", this);
        }

        ReproducirTrigger(triggerRecogerItem);

        itemRecogido.OcultarPrompt();
        itemRecogido.enabled = false;

        Transform punto = ObtenerPuntoLlevarItem();
        Transform itemTransform = itemRecogido.transform;

        rigidbodyItemRecogido = itemRecogido.GetComponent<Rigidbody>();

        if (rigidbodyItemRecogido != null)
        {
            itemUsabaGravedad = rigidbodyItemRecogido.useGravity;
            itemEraKinematic = rigidbodyItemRecogido.isKinematic;
            rigidbodyItemRecogido.linearVelocity = Vector3.zero;
            rigidbodyItemRecogido.angularVelocity = Vector3.zero;
            rigidbodyItemRecogido.useGravity = false;
            rigidbodyItemRecogido.isKinematic = true;
        }

        collidersItemRecogido = itemRecogido.GetComponentsInChildren<Collider>(true);
        collidersItemRecogidoActivos = new bool[collidersItemRecogido.Length];

        for (int i = 0; i < collidersItemRecogido.Length; i++)
        {
            if (collidersItemRecogido[i] == null)
            {
                continue;
            }

            collidersItemRecogidoActivos[i] = collidersItemRecogido[i].enabled;
            collidersItemRecogido[i].enabled = false;
        }

        itemTransform.SetParent(punto, false);
        itemTransform.localPosition = Vector3.zero;
        itemTransform.localRotation = Quaternion.identity;
    }

    private void VolverAlJugadorConItem()
    {
        if (itemRecogido == null)
        {
            LimpiarDatosItemRecogido();
            estadoRecolector = EstadoRecolector.SiguiendoJugador;
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > distanciaSoltarCercaJugador)
        {
            MoverHacia(jugador.position, distanciaSoltarCercaJugador, true);
            return;
        }

        SoltarItemCercaDelJugador();
    }

    private void SoltarItemCercaDelJugador()
    {
        if (itemRecogido == null)
        {
            LimpiarDatosItemRecogido();
            estadoRecolector = EstadoRecolector.SiguiendoJugador;
            return;
        }

        ReproducirTrigger(triggerSoltarItem);

        Transform itemTransform = itemRecogido.transform;
        Vector3 posicionSoltar = ObtenerPosicionSoltarItem();

        itemTransform.SetParent(null, true);
        itemTransform.position = posicionSoltar;
        itemTransform.rotation = Quaternion.identity;

        if (noVolverARecogerItemsYaEntregados && !itemsYaEntregados.Contains(itemRecogido))
        {
            itemsYaEntregados.Add(itemRecogido);
        }

        if (collidersItemRecogido != null)
        {
            for (int i = 0; i < collidersItemRecogido.Length; i++)
            {
                if (collidersItemRecogido[i] == null)
                {
                    continue;
                }

                bool estabaActivo = collidersItemRecogidoActivos != null && i < collidersItemRecogidoActivos.Length && collidersItemRecogidoActivos[i];
                collidersItemRecogido[i].enabled = estabaActivo;
            }
        }

        if (rigidbodyItemRecogido != null)
        {
            rigidbodyItemRecogido.isKinematic = itemEraKinematic;
            rigidbodyItemRecogido.useGravity = itemUsabaGravedad;
            rigidbodyItemRecogido.linearVelocity = Vector3.zero;
            rigidbodyItemRecogido.angularVelocity = Vector3.zero;
        }

        itemRecogido.enabled = true;

        if (mostrarLogsRecolector)
        {
            Debug.Log($"[MonoAmistadPlatano] Item soltado cerca del jugador: '{itemRecogido.name}'", this);
        }

        itemRecogido = null;
        LimpiarDatosItemRecogido();
        estadoRecolector = EstadoRecolector.SiguiendoJugador;
        siguienteBusquedaItems = Time.time + intervaloBusquedaItems;
    }

    private Vector3 ObtenerPosicionSoltarItem()
    {
        if (puntoSoltarItem != null)
        {
            return puntoSoltarItem.position;
        }

        Vector3 direccionDelante = jugador.forward;
        direccionDelante.y = 0f;

        if (direccionDelante.sqrMagnitude <= 0.001f)
        {
            direccionDelante = transform.forward;
            direccionDelante.y = 0f;
        }

        if (direccionDelante.sqrMagnitude <= 0.001f)
        {
            direccionDelante = Vector3.forward;
        }

        direccionDelante.Normalize();

        return jugador.position + direccionDelante * distanciaDelanteJugadorAlSoltar + Vector3.up * alturaSoltarItem;
    }

    private Transform ObtenerPuntoLlevarItem()
    {
        if (puntoLlevarItem != null)
        {
            return puntoLlevarItem;
        }

        GameObject punto = new GameObject("PuntoLlevarItemMono");
        punto.transform.SetParent(transform, false);
        punto.transform.localPosition = new Vector3(0f, 0.9f, 0.35f);
        punto.transform.localRotation = Quaternion.identity;
        puntoLlevarItem = punto.transform;
        return puntoLlevarItem;
    }

    private ObjetoRecogibleInteractuable BuscarItemMasCercanoParaMono()
    {
        ObjetoRecogibleInteractuable[] todos = FindObjectsByType<ObjetoRecogibleInteractuable>(FindObjectsSortMode.None);

        ObjetoRecogibleInteractuable mejor = null;
        float mejorDistancia = float.MaxValue;

        foreach (ObjetoRecogibleInteractuable item in todos)
        {
            if (!ItemPuedeSerRecogido(item))
            {
                continue;
            }

            float distancia = ObtenerDistanciaHorizontalAItem(item);

            if (distancia > rangoBusquedaItems)
            {
                continue;
            }

            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = item;
            }
        }

        return mejor;
    }

    private Vector3 ObtenerDestinoItem(ObjetoRecogibleInteractuable item)
    {
        if (item == null)
        {
            return transform.position;
        }

        Collider colliderItem = ObtenerColliderPrincipalItem(item);

        if (colliderItem != null)
        {
            Vector3 puntoCercano = colliderItem.ClosestPoint(transform.position);
            puntoCercano.y = item.transform.position.y;
            return puntoCercano;
        }

        return item.transform.position;
    }

    private float ObtenerDistanciaHorizontalAItem(ObjetoRecogibleInteractuable item)
    {
        if (item == null)
        {
            return float.MaxValue;
        }

        Vector3 posicionReferencia = item.transform.position;
        Collider colliderItem = ObtenerColliderPrincipalItem(item);

        if (usarColliderParaMedirDistancia && colliderItem != null && colliderItem.enabled)
        {
            posicionReferencia = colliderItem.ClosestPoint(transform.position);
        }

        Vector3 origen = transform.position;
        origen.y = 0f;
        posicionReferencia.y = 0f;

        return Vector3.Distance(origen, posicionReferencia);
    }

    private Collider ObtenerColliderPrincipalItem(ObjetoRecogibleInteractuable item)
    {
        if (item == null)
        {
            return null;
        }

        Collider colliderItem = item.GetComponent<Collider>();

        if (colliderItem != null && colliderItem.enabled && !colliderItem.isTrigger)
        {
            return colliderItem;
        }

        Collider[] colliders = item.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col != null && col.enabled && !col.isTrigger)
            {
                return col;
            }
        }

        foreach (Collider col in colliders)
        {
            if (col != null && col.enabled)
            {
                return col;
            }
        }

        return null;
    }

    private bool NavMeshAgentYaHaLlegadoAlDestino()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
        {
            return false;
        }

        if (navMeshAgent.pathPending || !navMeshAgent.hasPath)
        {
            return false;
        }

        if (navMeshAgent.remainingDistance == Mathf.Infinity)
        {
            return false;
        }

        return navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance + margenExtraRecogida;
    }

    private bool ItemPuedeSerRecogido(ObjetoRecogibleInteractuable item)
    {
        if (item == null || !item.isActiveAndEnabled || !item.EstaDisponible)
        {
            return false;
        }

        if (item == itemRecogido)
        {
            return false;
        }

        if (item.transform.IsChildOf(transform))
        {
            return false;
        }

        if (ignorarItemsQueSeanHijosDelJugador && jugador != null && item.transform.IsChildOf(jugador))
        {
            return false;
        }

        if (noVolverARecogerItemsYaEntregados && itemsYaEntregados.Contains(item))
        {
            return false;
        }

        if (EsItemIgnoradoPorMono(item))
        {
            return false;
        }

        if ((capasItems.value & (1 << item.gameObject.layer)) == 0)
        {
            return false;
        }

        return true;
    }

    private bool EsItemIgnoradoPorMono(ObjetoRecogibleInteractuable item)
    {
        if (!ignorarCanaDePescarComoItem || item == null)
        {
            return false;
        }

        if (palabrasItemsIgnoradosPorMono == null || palabrasItemsIgnoradosPorMono.Length == 0)
        {
            return false;
        }

        string textoItem = ObtenerTextoBusquedaItem(item);

        if (string.IsNullOrWhiteSpace(textoItem))
        {
            return false;
        }

        foreach (string palabra in palabrasItemsIgnoradosPorMono)
        {
            if (string.IsNullOrWhiteSpace(palabra))
            {
                continue;
            }

            string palabraNormalizada = NormalizarTextoBusqueda(palabra);

            if (textoItem.Contains(palabraNormalizada))
            {
                return true;
            }
        }

        return false;
    }

    private string ObtenerTextoBusquedaItem(ObjetoRecogibleInteractuable item)
    {
        if (item == null)
        {
            return "";
        }

        string texto = item.name + " " + item.gameObject.name;

        ItemData itemData = ObtenerItemDataDesdeRecogible(item);

        if (itemData != null)
        {
            texto += " " + itemData.name + " " + itemData.ItemId + " " + itemData.DisplayName;
        }

        MonoBehaviour[] componentes = item.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour componente in componentes)
        {
            if (componente == null)
            {
                continue;
            }

            texto += " " + componente.GetType().Name;
        }

        return NormalizarTextoBusqueda(texto);
    }

    private ItemData ObtenerItemDataDesdeRecogible(ObjetoRecogibleInteractuable item)
    {
        if (item == null)
        {
            return null;
        }

        System.Type tipo = item.GetType();
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        foreach (FieldInfo campo in tipo.GetFields(flags))
        {
            if (campo == null || !typeof(ItemData).IsAssignableFrom(campo.FieldType))
            {
                continue;
            }

            return campo.GetValue(item) as ItemData;
        }

        foreach (PropertyInfo propiedad in tipo.GetProperties(flags))
        {
            if (propiedad == null || !typeof(ItemData).IsAssignableFrom(propiedad.PropertyType) || !propiedad.CanRead)
            {
                continue;
            }

            try
            {
                return propiedad.GetValue(item, null) as ItemData;
            }
            catch
            {
                // Algunas propiedades pueden lanzar excepción si no están listas. Las ignoramos.
            }
        }

        return null;
    }

    private string NormalizarTextoBusqueda(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return "";
        }

        return texto.ToLowerInvariant()
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ü", "u");
    }

    private void LimpiarDatosItemRecogido()
    {
        rigidbodyItemRecogido = null;
        itemUsabaGravedad = false;
        itemEraKinematic = false;
        collidersItemRecogido = null;
        collidersItemRecogidoActivos = null;
    }

    private void AjustarAlturaAlSuelo()
    {
        if (!TryObtenerAlturaSuelo(out float alturaSuelo))
        {
            return;
        }

        float alturaObjetivo = alturaSuelo + alturaSobreSuelo;
        float diferencia = alturaObjetivo - transform.position.y;

        if (Mathf.Abs(diferencia) <= 0.002f)
        {
            return;
        }

        float factorSuavizado = 1f - Mathf.Exp(-suavizadoAlturaSuelo * Time.deltaTime);

        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh && corregirAlturaConNavMeshAgent)
        {
            float baseOffsetObjetivo = navMeshAgent.baseOffset + diferencia;
            navMeshAgent.baseOffset = Mathf.Lerp(navMeshAgent.baseOffset, baseOffsetObjetivo, factorSuavizado);
            return;
        }

        Vector3 posicion = transform.position;
        posicion.y = Mathf.Lerp(posicion.y, alturaObjetivo, factorSuavizado);
        transform.position = posicion;
    }

    private bool TryObtenerAlturaSuelo(out float alturaSuelo)
    {
        alturaSuelo = transform.position.y;

        Vector3 origen = transform.position + Vector3.up * alturaOrigenRaycastSuelo;
        float distanciaMaxima = alturaOrigenRaycastSuelo + distanciaRaycastSuelo;
        QueryTriggerInteraction triggerInteraction = ignorarTriggersSuelo ? QueryTriggerInteraction.Ignore : QueryTriggerInteraction.Collide;
        RaycastHit[] impactos = Physics.RaycastAll(origen, Vector3.down, distanciaMaxima, capasSuelo, triggerInteraction);

        if (impactos == null || impactos.Length == 0)
        {
            return false;
        }

        bool encontrado = false;
        float mejorDistancia = float.MaxValue;
        RaycastHit mejorImpacto = new RaycastHit();

        foreach (RaycastHit impacto in impactos)
        {
            if (impacto.collider == null)
            {
                continue;
            }

            if (impacto.collider.transform == transform || impacto.collider.transform.IsChildOf(transform))
            {
                continue;
            }

            if (itemRecogido != null && impacto.collider.transform.IsChildOf(itemRecogido.transform))
            {
                continue;
            }

            if (!EsImpactoValidoComoSuelo(impacto.collider))
            {
                continue;
            }

            if (impacto.distance < mejorDistancia)
            {
                mejorDistancia = impacto.distance;
                mejorImpacto = impacto;
                encontrado = true;
            }
        }

        if (!encontrado)
        {
            return false;
        }

        alturaSuelo = mejorImpacto.point.y;
        return true;
    }


    private bool EsImpactoValidoComoSuelo(Collider colliderImpacto)
    {
        if (colliderImpacto == null)
        {
            return false;
        }

        if (ignorarOtrosMonosComoSuelo)
        {
            MonoAmistadPlatano monoDelCollider = colliderImpacto.GetComponentInParent<MonoAmistadPlatano>();

            if (monoDelCollider != null && monoDelCollider != this)
            {
                return false;
            }
        }

        if (ignorarJugadorComoSuelo && jugador != null)
        {
            if (colliderImpacto.transform == jugador || colliderImpacto.transform.IsChildOf(jugador))
            {
                return false;
            }
        }

        if (ignorarItemsComoSuelo)
        {
            ObjetoRecogibleInteractuable itemDelCollider = colliderImpacto.GetComponentInParent<ObjetoRecogibleInteractuable>();

            if (itemDelCollider != null)
            {
                return false;
            }
        }

        return true;
    }

    private void ActualizarColisionesConOtrosMonosSiHaceFalta()
    {
        if (!ignorarColisionesFisicasConOtrosMonos)
        {
            return;
        }

        if (Time.time < siguienteActualizacionColisionesMonos)
        {
            return;
        }

        siguienteActualizacionColisionesMonos = Time.time + Mathf.Max(0.25f, intervaloActualizarColisionesMonos);
        ConfigurarColisionesConOtrosMonos();
    }

    private void ConfigurarColisionesConOtrosMonos()
    {
        if (!ignorarColisionesFisicasConOtrosMonos)
        {
            return;
        }

        if (collidersPropios == null || collidersPropios.Length == 0)
        {
            collidersPropios = GetComponentsInChildren<Collider>(true);
        }

        MonoAmistadPlatano[] monos = FindObjectsByType<MonoAmistadPlatano>(FindObjectsSortMode.None);

        foreach (MonoAmistadPlatano otroMono in monos)
        {
            if (otroMono == null || otroMono == this)
            {
                continue;
            }

            Collider[] collidersOtroMono = otroMono.GetComponentsInChildren<Collider>(true);

            foreach (Collider colliderPropio in collidersPropios)
            {
                if (colliderPropio == null)
                {
                    continue;
                }

                foreach (Collider colliderOtro in collidersOtroMono)
                {
                    if (colliderOtro == null || colliderOtro == colliderPropio)
                    {
                        continue;
                    }

                    Physics.IgnoreCollision(colliderPropio, colliderOtro, true);
                }
            }
        }
    }

    private void SeguirJugador()
    {
        if (!seguirAlSerAmigo || jugador == null)
        {
            CambiarBool(boolSiguiendo, false);
            PararNavMeshAgent();
            return;
        }

        float distanciaParada = ObtenerDistanciaPararSeguimiento();
        float distancia = Vector3.Distance(transform.position, jugador.position);
        bool debeMoverse = distancia > distanciaParada;

        CambiarBool(boolSiguiendo, debeMoverse);

        if (!debeMoverse)
        {
            PararNavMeshAgent();
            MirarHacia(jugador.position);
            return;
        }

        MoverHacia(jugador.position, distanciaParada, true);
    }

    private float ObtenerDistanciaPararSeguimiento()
    {
        if (esAmigo)
        {
            return Mathf.Max(0.1f, distanciaPararAmigo);
        }

        return Mathf.Max(0.1f, distanciaParar);
    }

    private void MoverHacia(Vector3 destino, float distanciaMinima, bool mirarAlJugadorMientrasSeMueve)
    {
        CambiarBool(boolSiguiendo, true);

        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.stoppingDistance = distanciaMinima;
            navMeshAgent.SetDestination(destino);

            if (mirarAlJugadorMientrasSeMueve && mirarSiempreAlJugador)
            {
                MirarHacia(jugador.position);
            }
            else
            {
                MirarHaciaDireccionMovimiento(destino);
            }

            return;
        }

        MoverSinNavMesh(destino, distanciaMinima, mirarAlJugadorMientrasSeMueve);
    }

    private void MoverSinNavMesh(Vector3 destino, float distanciaMinima, bool mirarAlJugadorMientrasSeMueve)
    {
        if (mantenerAlturaInicialSinNavMesh)
        {
            destino.y = alturaInicial;
        }

        Vector3 direccion = destino - transform.position;
        direccion.y = 0f;

        if (direccion.magnitude <= distanciaMinima)
        {
            CambiarBool(boolSiguiendo, false);
            return;
        }

        Vector3 movimiento = direccion.normalized * velocidadSinNavMesh * Time.deltaTime;

        if (movimiento.sqrMagnitude > direccion.sqrMagnitude)
        {
            movimiento = direccion;
        }

        transform.position += movimiento;

        if (mirarAlJugadorMientrasSeMueve && mirarSiempreAlJugador)
        {
            MirarHacia(jugador.position);
        }
        else
        {
            MirarHacia(destino);
        }
    }

    private void MirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = objetivo - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        rotacionObjetivo *= Quaternion.Euler(0f, correccionRotacionYModelo, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * velocidadGiro);
    }

    private void MirarHaciaDireccionMovimiento(Vector3 destinoFallback)
    {
        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            if (navMeshAgent.velocity.sqrMagnitude > 0.01f)
            {
                MirarHacia(transform.position + navMeshAgent.velocity);
                return;
            }

            if (navMeshAgent.hasPath)
            {
                MirarHacia(navMeshAgent.steeringTarget);
                return;
            }
        }

        MirarHacia(destinoFallback);
    }

    private void ConfigurarNavMeshAgent()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled)
        {
            return;
        }

        navMeshAgent.stoppingDistance = ObtenerDistanciaPararSeguimiento();
        navMeshAgent.updateRotation = false;

        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = !esAmigo;
        }
    }

    private void PararNavMeshAgent()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    private void GestionarInteraccionModoAmigo()
    {
        if (!permitirCambiarModoConInteraccion || jugador == null)
        {
            OcultarPrompt();
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > rangoInteraccion)
        {
            OcultarPrompt();
            return;
        }

        if (Time.time < tiempoHastaPermitirOcultarPrompt)
        {
            return;
        }

        MostrarPromptModoAmigo();

        if (SeHaPulsadoInteraccion())
        {
            CambiarAlSiguienteModoAmigo();
        }
    }

    private void MostrarPromptModoAmigo()
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

        string texto = textoPromptCambiarModo;

        if (mostrarModoActualEnPrompt)
        {
            texto += ": " + ObtenerNombreModoAmigo(modoAmigo);
        }

        InteractionUI.Instance.Show(this, texto);
        promptMostrado = true;
    }

    private void CambiarAlSiguienteModoAmigo()
    {
        ModoMonoAmigo modoAnterior = modoAmigo;

        switch (modoAmigo)
        {
            case ModoMonoAmigo.Sigueme:
                modoAmigo = ModoMonoAmigo.EsperarAqui;
                break;

            case ModoMonoAmigo.EsperarAqui:
                modoAmigo = ModoMonoAmigo.BuscarObjetos;
                break;

            case ModoMonoAmigo.BuscarObjetos:
                modoAmigo = ModoMonoAmigo.Defenderme;
                break;

            default:
                modoAmigo = ModoMonoAmigo.Sigueme;
                break;
        }

        PrepararCambioDeModo(modoAnterior, modoAmigo);
        MostrarMensajeTemporal(ObtenerMensajeModoAmigo(modoAmigo));
    }

    private void PrepararCambioDeModo(ModoMonoAmigo modoAnterior, ModoMonoAmigo modoNuevo)
    {
        itemObjetivo = null;
        enemigoObjetivo = null;
        tiempoHastaAbandonarDefensa = 0f;
        tiempoSiguienteAtaqueEnemigo = 0f;
        siguienteBusquedaDefensaActiva = 0f;

        if (modoNuevo == ModoMonoAmigo.EsperarAqui)
        {
            CambiarBool(boolSiguiendo, false);
            PararNavMeshAgent();
            return;
        }

        if (modoNuevo == ModoMonoAmigo.BuscarObjetos)
        {
            siguienteBusquedaItems = 0f;
        }

        if (modoNuevo == ModoMonoAmigo.Sigueme || modoNuevo == ModoMonoAmigo.Defenderme)
        {
            estadoRecolector = EstadoRecolector.SiguiendoJugador;
        }
    }

    private string ObtenerNombreModoAmigo(ModoMonoAmigo modo)
    {
        switch (modo)
        {
            case ModoMonoAmigo.EsperarAqui:
                return "Esperar aqui";

            case ModoMonoAmigo.BuscarObjetos:
                return "Buscar objetos";

            case ModoMonoAmigo.Defenderme:
                return "Defenderme";

            default:
                return "Sigueme";
        }
    }

    private string ObtenerMensajeModoAmigo(ModoMonoAmigo modo)
    {
        switch (modo)
        {
            case ModoMonoAmigo.EsperarAqui:
                return mensajeModoEsperarAqui;

            case ModoMonoAmigo.BuscarObjetos:
                return mensajeModoBuscarObjetos;

            case ModoMonoAmigo.Defenderme:
                return mensajeModoDefenderme;

            default:
                return mensajeModoSigueme;
        }
    }

    private void MostrarPrompt()
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

        string texto = mensajeDarPlatano + " " + platanosRecibidos + "/" + platanosNecesariosParaAmistad;
        InteractionUI.Instance.Show(this, texto);
        promptMostrado = true;
    }

    private void MostrarMensajeTemporal(string texto)
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

        InteractionUI.Instance.ShowTemporary(this, texto, duracionMensajeTemporal);
        tiempoHastaPermitirOcultarPrompt = Time.time + duracionMensajeTemporal;
        promptMostrado = true;
    }

    private void OcultarPrompt()
    {
        if (!promptMostrado || InteractionUI.Instance == null)
        {
            return;
        }

        if (Time.time < tiempoHastaPermitirOcultarPrompt)
        {
            return;
        }

        InteractionUI.Instance.Hide(this);
        promptMostrado = false;
    }

    private bool SeHaPulsadoInteraccion()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }



    private bool ModoActualPermiteDefenderPorDanio()
    {
        return modoAmigo == ModoMonoAmigo.Sigueme || modoAmigo == ModoMonoAmigo.Defenderme;
    }

    private void BuscarEnemigoActivoParaDefenderSiHaceFalta()
    {
        if (Time.time < siguienteBusquedaDefensaActiva)
        {
            return;
        }

        siguienteBusquedaDefensaActiva = Time.time + Mathf.Max(0.1f, intervaloBusquedaEnemigosDefensa);

        Transform objetivo = BuscarEnemigoCercaDe(jugador.position, Mathf.Max(0.25f, radioDefensaActiva));

        if (objetivo == null)
        {
            return;
        }

        enemigoObjetivo = objetivo;
        tiempoHastaAbandonarDefensa = Time.time + Mathf.Max(0.5f, tiempoPerseguirEnemigoTrasDanio);
        estadoRecolector = EstadoRecolector.DefendiendoJugador;

        if (mostrarLogsDefensa)
        {
            Debug.Log($"[MonoAmistadPlatano] Modo defenderme: enemigo detectado cerca del jugador: {enemigoObjetivo.name}", this);
        }
    }

    private void AlJugadorLeHanHechoDanio(Transform atacante, Vector3 posicionDanio, int cantidadDanio)
    {
        if (!defenderJugadorSiRecibeDanio || !esAmigo || jugador == null || !ModoActualPermiteDefenderPorDanio())
        {
            return;
        }

        float distanciaAJugador = Vector3.Distance(transform.position, jugador.position);
        float distanciaAlDanio = Vector3.Distance(transform.position, posicionDanio);

        if (distanciaAJugador > rangoMaximoEscucharDanio && distanciaAlDanio > rangoMaximoEscucharDanio)
        {
            return;
        }

        Transform objetivo = EsTransformEnemigoValido(atacante)
            ? atacante
            : BuscarEnemigoCercaDe(posicionDanio);

        if (objetivo == null)
        {
            if (mostrarLogsDefensa)
            {
                Debug.Log("[MonoAmistadPlatano] El jugador ha recibido daño, pero no se ha encontrado enemigo cercano.", this);
            }

            return;
        }

        enemigoObjetivo = objetivo;
        tiempoHastaAbandonarDefensa = Time.time + Mathf.Max(0.5f, tiempoPerseguirEnemigoTrasDanio);
        estadoRecolector = EstadoRecolector.DefendiendoJugador;

        if (cancelarObjetivoDeRecoleccionParaDefender && itemRecogido == null)
        {
            itemObjetivo = null;
        }

        if (mostrarLogsDefensa)
        {
            Debug.Log($"[MonoAmistadPlatano] Defendiendo al jugador contra: {enemigoObjetivo.name}", this);
        }
    }

    private void DefenderJugadorContraEnemigo()
    {
        if (!EnemigoObjetivoValido())
        {
            enemigoObjetivo = null;
            estadoRecolector = EstadoRecolector.SiguiendoJugador;
            SeguirJugador();
            return;
        }

        if (Time.time > tiempoHastaAbandonarDefensa)
        {
            enemigoObjetivo = null;
            estadoRecolector = EstadoRecolector.SiguiendoJugador;
            SeguirJugador();
            return;
        }

        Vector3 posicionEnemigo = enemigoObjetivo.position;
        float distancia = Vector3.Distance(transform.position, posicionEnemigo);

        if (distancia > distanciaAtaqueEnemigo)
        {
            MoverHacia(posicionEnemigo, distanciaAtaqueEnemigo, false);
            MirarHacia(posicionEnemigo);
            return;
        }

        PararNavMeshAgent();
        CambiarBool(boolSiguiendo, false);
        MirarHacia(posicionEnemigo);

        if (Time.time < tiempoSiguienteAtaqueEnemigo)
        {
            return;
        }

        tiempoSiguienteAtaqueEnemigo = Time.time + Mathf.Max(0.1f, tiempoEntreAtaquesEnemigo);
        ReproducirTrigger(triggerAtacarEnemigo);
        AplicarDanioAEnemigo(enemigoObjetivo, danioAlEnemigo);
    }

    private bool EnemigoObjetivoValido()
    {
        return EsTransformEnemigoValido(enemigoObjetivo);
    }

    private Transform BuscarEnemigoCercaDe(Vector3 posicion)
    {
        return BuscarEnemigoCercaDe(posicion, Mathf.Max(0.25f, radioBuscarEnemigoCercaDanio));
    }

    private Transform BuscarEnemigoCercaDe(Vector3 posicion, float radio)
    {
        radio = Mathf.Max(0.25f, radio);
        Collider[] impactos = Physics.OverlapSphere(posicion, radio, capasEnemigos, QueryTriggerInteraction.Collide);

        Transform mejor = null;
        float mejorDistancia = float.MaxValue;

        foreach (Collider impacto in impactos)
        {
            if (impacto == null)
            {
                continue;
            }

            Transform candidato = impacto.attachedRigidbody != null
                ? impacto.attachedRigidbody.transform
                : impacto.transform.root;

            if (!EsTransformEnemigoValido(candidato))
            {
                candidato = impacto.GetComponentInParent<Transform>();
            }

            if (!EsTransformEnemigoValido(candidato))
            {
                continue;
            }

            float distancia = Vector3.Distance(posicion, candidato.position);

            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = candidato;
            }
        }

        return mejor;
    }

    private bool EsTransformEnemigoValido(Transform candidato)
    {
        if (candidato == null || !candidato.gameObject.activeInHierarchy)
        {
            return false;
        }

        if (candidato == transform || candidato.IsChildOf(transform) || transform.IsChildOf(candidato))
        {
            return false;
        }

        if (jugador != null && (candidato == jugador || candidato.IsChildOf(jugador) || jugador.IsChildOf(candidato)))
        {
            return false;
        }

        if (candidato.GetComponentInParent<MonoAmistadPlatano>() != null)
        {
            return false;
        }

        if (candidato.GetComponentInParent<ObjetoRecogibleInteractuable>() != null)
        {
            return false;
        }

        bool mascaraEnemigosConfigurada = capasEnemigos.value != 0 && capasEnemigos.value != ~0;

        if (mascaraEnemigosConfigurada && (capasEnemigos.value & (1 << candidato.gameObject.layer)) != 0)
        {
            return true;
        }

        if (TieneTagEnemigo(candidato))
        {
            return true;
        }

        return TieneComponenteConNombreDeEnemigo(candidato);
    }

    private bool TieneTagEnemigo(Transform candidato)
    {
        if (candidato == null || tagsEnemigos == null)
        {
            return false;
        }

        foreach (string tagEnemigo in tagsEnemigos)
        {
            if (string.IsNullOrWhiteSpace(tagEnemigo))
            {
                continue;
            }

            try
            {
                if (candidato.CompareTag(tagEnemigo))
                {
                    return true;
                }
            }
            catch (UnityException)
            {
                // El tag puede no existir en el proyecto. Lo ignoramos para no romper el juego.
            }
        }

        return false;
    }

    private bool TieneComponenteConNombreDeEnemigo(Transform candidato)
    {
        if (candidato == null)
        {
            return false;
        }

        MonoBehaviour[] componentes = candidato.GetComponentsInChildren<MonoBehaviour>(true);

        foreach (MonoBehaviour componente in componentes)
        {
            if (componente == null)
            {
                continue;
            }

            string nombreTipo = componente.GetType().Name.ToLowerInvariant();

            if (nombreTipo.Contains("enemigo") ||
                nombreTipo.Contains("enemy") ||
                nombreTipo.Contains("tiburon") ||
                nombreTipo.Contains("shark") ||
                nombreTipo.Contains("cangrejo") ||
                nombreTipo.Contains("crab") ||
                nombreTipo.Contains("pirana") ||
                nombreTipo.Contains("piraña"))
            {
                return true;
            }
        }

        return false;
    }

    private void AplicarDanioAEnemigo(Transform enemigo, int cantidad)
    {
        if (enemigo == null || cantidad <= 0)
        {
            return;
        }

        if (InvocarMetodoDanio(enemigo, cantidad))
        {
            return;
        }

        enemigo.gameObject.SendMessage("RecibirDanio", cantidad, SendMessageOptions.DontRequireReceiver);
        enemigo.gameObject.SendMessage("TakeDamage", cantidad, SendMessageOptions.DontRequireReceiver);
        enemigo.gameObject.SendMessage("AplicarDanio", cantidad, SendMessageOptions.DontRequireReceiver);

        if (mostrarLogsDefensa)
        {
            Debug.Log($"[MonoAmistadPlatano] Ataque al enemigo '{enemigo.name}'. Si quieres que pierda vida, añade VidaEnemigoSimple o un método RecibirDanio/TakeDamage.", this);
        }
    }

    private bool InvocarMetodoDanio(Transform enemigo, int cantidad)
    {
        MonoBehaviour[] componentes = enemigo.GetComponentsInChildren<MonoBehaviour>(true);
        string[] nombresMetodos = new string[]
        {
            "RecibirDanio",
            "RecibirDano",
            "TakeDamage",
            "AplicarDanio",
            "Damage",
            "ReducirSaludDirecta"
        };

        foreach (MonoBehaviour componente in componentes)
        {
            if (componente == null || componente == this)
            {
                continue;
            }

            System.Type tipo = componente.GetType();

            foreach (string nombreMetodo in nombresMetodos)
            {
                MethodInfo metodo = tipo.GetMethod(
                    nombreMetodo,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                    null,
                    new System.Type[] { typeof(int) },
                    null);

                if (metodo == null)
                {
                    continue;
                }

                metodo.Invoke(componente, new object[] { cantidad });
                return true;
            }
        }

        return false;
    }

    private void ConfigurarParticulasCorazones()
    {
        if (!reproducirCorazonesAlHacerseAmigo)
        {
            return;
        }

        if (usarPrefabCorazon3D)
        {
            return;
        }

        if (particulasCorazonesAmistad == null && crearParticulasCorazonesAutomaticas)
        {
            Transform padreFX = puntoFXCorazones != null ? puntoFXCorazones : transform;
            GameObject fxObject = new GameObject("FX_Corazones_Amistad_Codigo");
            fxObject.transform.SetParent(padreFX, false);
            fxObject.transform.localPosition = puntoFXCorazones != null ? Vector3.zero : offsetCorazones;
            fxObject.transform.localRotation = Quaternion.identity;
            particulasCorazonesAmistad = fxObject.AddComponent<ParticleSystem>();
        }

        if (particulasCorazonesAmistad == null)
        {
            return;
        }

        if (configurarCorazonesPorCodigo)
        {
            ConfigurarSistemaParticulasCorazones(particulasCorazonesAmistad);
        }
    }

    private void ReproducirFXCorazonesAmistad()
    {
        if (!reproducirCorazonesAlHacerseAmigo)
        {
            return;
        }

        if (usarPrefabCorazon3D)
        {
            ReproducirCorazonesPrefab3D();
            return;
        }

        if (particulasCorazonesAmistad == null)
        {
            ConfigurarParticulasCorazones();
        }

        if (particulasCorazonesAmistad == null)
        {
            return;
        }

        if (puntoFXCorazones != null)
        {
            particulasCorazonesAmistad.transform.position = puntoFXCorazones.position;
        }
        else if (particulasCorazonesAmistad.transform.parent == transform)
        {
            particulasCorazonesAmistad.transform.localPosition = offsetCorazones;
        }

        particulasCorazonesAmistad.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        particulasCorazonesAmistad.Play(true);
    }

    private void ReproducirCorazonesPrefab3D()
    {
        if (prefabCorazon3D == null)
        {
            return;
        }

        int min = Mathf.Max(1, corazonesMin);
        int max = Mathf.Max(min, corazonesMax);
        int cantidad = Random.Range(min, max + 1);
        Vector3 posicionBase = ObtenerPosicionBaseCorazones();

        for (int i = 0; i < cantidad; i++)
        {
            Vector2 circulo = Random.insideUnitCircle * Mathf.Max(0f, radioSpawnPrefabCorazones);
            Vector3 posicion = posicionBase + new Vector3(circulo.x, Random.Range(-0.04f, 0.08f), circulo.y);
            Quaternion rotacion = ObtenerRotacionInicialCorazonPrefab();

            GameObject corazon = Instantiate(prefabCorazon3D, posicion, rotacion);
            corazon.name = prefabCorazon3D.name + "_FX_Amistad";

            if (desactivarCollidersPrefabCorazon)
            {
                DesactivarFisicasCorazonTemporal(corazon);
            }

            float escala = Random.Range(
                Mathf.Max(0.01f, escalaPrefabCorazonMin),
                Mathf.Max(Mathf.Max(0.01f, escalaPrefabCorazonMin), escalaPrefabCorazonMax));

            corazon.transform.localScale = prefabCorazon3D.transform.localScale * escala;

            Vector3 velocidad = new Vector3(
                Random.Range(-expansionHorizontalCorazones, expansionHorizontalCorazones),
                Random.Range(velocidadVerticalCorazonesMin, velocidadVerticalCorazonesMax),
                Random.Range(-expansionHorizontalCorazones, expansionHorizontalCorazones));

            Vector3 rotacionPorSegundo = new Vector3(
                Random.Range(-45f, 45f),
                Random.Range(-95f, 95f),
                Random.Range(-45f, 45f));

            CorazonAmistadTemporal corazonTemporal = corazon.GetComponent<CorazonAmistadTemporal>();

            if (corazonTemporal == null)
            {
                corazonTemporal = corazon.AddComponent<CorazonAmistadTemporal>();
            }

            corazonTemporal.Inicializar(
                velocidad,
                rotacionPorSegundo,
                Mathf.Max(0.35f, duracionPrefabCorazones),
                colorCorazon,
                colorearPrefabCorazonDeRojo);
        }
    }

    private Quaternion ObtenerRotacionInicialCorazonPrefab()
    {
        Quaternion rotacion = Quaternion.identity;

        if (usarRotacionOriginalPrefabCorazon && prefabCorazon3D != null)
        {
            rotacion = prefabCorazon3D.transform.rotation;
        }

        if (rotacionExtraPrefabCorazonEuler != Vector3.zero)
        {
            rotacion *= Quaternion.Euler(rotacionExtraPrefabCorazonEuler);
        }

        if (aplicarGiroAleatorioSuaveCorazon)
        {
            float inclinacion = Mathf.Max(0f, inclinacionAleatoriaCorazon);
            float giroY = Mathf.Max(0f, giroAleatorioYCorazon);

            rotacion *= Quaternion.Euler(
                Random.Range(-inclinacion, inclinacion),
                Random.Range(-giroY * 0.5f, giroY * 0.5f),
                Random.Range(-inclinacion, inclinacion));
        }

        return rotacion;
    }

    private Vector3 ObtenerPosicionBaseCorazones()
    {
        if (puntoFXCorazones != null)
        {
            return puntoFXCorazones.position;
        }

        return transform.TransformPoint(offsetCorazones);
    }

    private void DesactivarFisicasCorazonTemporal(GameObject corazon)
    {
        if (corazon == null)
        {
            return;
        }

        Collider[] colliders = corazon.GetComponentsInChildren<Collider>(true);

        foreach (Collider col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }

        Rigidbody[] rigidbodies = corazon.GetComponentsInChildren<Rigidbody>(true);

        foreach (Rigidbody rb in rigidbodies)
        {
            if (rb == null)
            {
                continue;
            }

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    private void ConfigurarSistemaParticulasCorazones(ParticleSystem ps)
    {
        if (ps == null)
        {
            return;
        }

        int min = Mathf.Max(1, corazonesMin);
        int max = Mathf.Max(min, corazonesMax);
        short minBurst = (short)Mathf.Clamp(min, 1, short.MaxValue);
        short maxBurst = (short)Mathf.Clamp(max, minBurst, short.MaxValue);

        var main = ps.main;
        main.duration = Mathf.Max(0.3f, duracionCorazones);
        main.loop = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(1.0f, Mathf.Max(1.05f, duracionCorazones));
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoCorazonMin, tamanoCorazonMax);
        main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.startColor = colorCorazon;
        main.gravityModifier = 0f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.maxParticles = 80;
        main.scalingMode = ParticleSystemScalingMode.Hierarchy;

        var emission = ps.emission;
        emission.enabled = true;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[]
        {
            new ParticleSystem.Burst(0f, minBurst, maxBurst)
        });

        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.22f;
        shape.arc = 360f;
        shape.randomDirectionAmount = 0.15f;

        var velocity = ps.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-expansionHorizontalCorazones, expansionHorizontalCorazones);
        velocity.y = new ParticleSystem.MinMaxCurve(velocidadVerticalCorazonesMin, velocidadVerticalCorazonesMax);
        velocity.z = new ParticleSystem.MinMaxCurve(-expansionHorizontalCorazones, expansionHorizontalCorazones);

        var limitVelocity = ps.limitVelocityOverLifetime;
        limitVelocity.enabled = true;
        limitVelocity.space = ParticleSystemSimulationSpace.World;
        limitVelocity.dampen = 0.28f;

        var size = ps.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve curvaTamano = new AnimationCurve();
        curvaTamano.AddKey(0f, 0.25f);
        curvaTamano.AddKey(0.15f, 1f);
        curvaTamano.AddKey(0.75f, 0.9f);
        curvaTamano.AddKey(1f, 0f);
        size.size = new ParticleSystem.MinMaxCurve(1f, curvaTamano);

        var color = ps.colorOverLifetime;
        color.enabled = true;
        Gradient gradiente = new Gradient();
        gradiente.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(colorCorazon, 0f),
                new GradientColorKey(colorCorazon, 0.75f),
                new GradientColorKey(new Color(1f, 0.25f, 0.25f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.12f),
                new GradientAlphaKey(1f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = gradiente;

        var rotation = ps.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-70f * Mathf.Deg2Rad, 70f * Mathf.Deg2Rad);

        var renderer = ps.GetComponent<ParticleSystemRenderer>();
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = 2f;
        renderer.minParticleSize = 0.01f;
        renderer.maxParticleSize = 0.5f;

        Material material = materialCorazones != null ? materialCorazones : ObtenerMaterialCorazones();
        if (material != null)
        {
            renderer.material = material;
        }
    }

    private Material ObtenerMaterialCorazones()
    {
        if (materialCorazonesGenerado != null)
        {
            return materialCorazonesGenerado;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
        {
            shader = Shader.Find("Particles/Standard Unlit");
        }

        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }

        if (shader == null)
        {
            return null;
        }

        materialCorazonesGenerado = new Material(shader);
        materialCorazonesGenerado.name = "M_Corazones_Mono_Generado";
        materialCorazonesGenerado.renderQueue = 3000;

        Texture2D textura = null;

        if (usarTexturaCorazonSuave)
        {
            textura = ObtenerTexturaCorazonSuave();
        }
        else if (usarTexturaCorazonPixel)
        {
            textura = ObtenerTexturaCorazonPixel();
        }

        if (textura != null)
        {
            if (materialCorazonesGenerado.HasProperty("_BaseMap"))
            {
                materialCorazonesGenerado.SetTexture("_BaseMap", textura);
            }

            if (materialCorazonesGenerado.HasProperty("_MainTex"))
            {
                materialCorazonesGenerado.SetTexture("_MainTex", textura);
            }
        }

        if (materialCorazonesGenerado.HasProperty("_BaseColor"))
        {
            materialCorazonesGenerado.SetColor("_BaseColor", Color.white);
        }

        if (materialCorazonesGenerado.HasProperty("_Color"))
        {
            materialCorazonesGenerado.SetColor("_Color", Color.white);
        }

        return materialCorazonesGenerado;
    }


    private static Texture2D ObtenerTexturaCorazonSuave()
    {
        if (texturaCorazonSuaveCompartida != null)
        {
            return texturaCorazonSuaveCompartida;
        }

        const int resolucion = 64;
        const int muestras = 4;
        texturaCorazonSuaveCompartida = new Texture2D(resolucion, resolucion, TextureFormat.RGBA32, false);
        texturaCorazonSuaveCompartida.name = "T_Corazon_Suave_Generado";
        texturaCorazonSuaveCompartida.filterMode = FilterMode.Bilinear;
        texturaCorazonSuaveCompartida.wrapMode = TextureWrapMode.Clamp;

        Color blanco = Color.white;
        Color transparente = new Color(1f, 1f, 1f, 0f);

        for (int y = 0; y < resolucion; y++)
        {
            for (int x = 0; x < resolucion; x++)
            {
                int dentro = 0;

                for (int sy = 0; sy < muestras; sy++)
                {
                    for (int sx = 0; sx < muestras; sx++)
                    {
                        float u = (x + (sx + 0.5f) / muestras) / resolucion;
                        float v = (y + (sy + 0.5f) / muestras) / resolucion;

                        float px = Mathf.Lerp(-1.35f, 1.35f, u);
                        float py = Mathf.Lerp(-1.25f, 1.45f, v);

                        // Corazon matematico suave. El pico queda abajo y los lobulos arriba.
                        float a = px * px + py * py - 1f;
                        float valor = a * a * a - px * px * py * py * py;

                        if (valor <= 0f)
                        {
                            dentro++;
                        }
                    }
                }

                float alpha = dentro / (float)(muestras * muestras);
                Color color = alpha > 0f ? blanco : transparente;
                color.a = alpha;
                texturaCorazonSuaveCompartida.SetPixel(x, y, color);
            }
        }

        texturaCorazonSuaveCompartida.Apply(false, true);
        return texturaCorazonSuaveCompartida;
    }

    private static Texture2D ObtenerTexturaCorazonPixel()
    {
        if (texturaCorazonPixelCompartida != null)
        {
            return texturaCorazonPixelCompartida;
        }

        texturaCorazonPixelCompartida = new Texture2D(16, 16, TextureFormat.RGBA32, false);
        texturaCorazonPixelCompartida.name = "T_Corazon_Pixel_Generado";
        texturaCorazonPixelCompartida.filterMode = FilterMode.Point;
        texturaCorazonPixelCompartida.wrapMode = TextureWrapMode.Clamp;

        Color transparente = new Color(1f, 1f, 1f, 0f);
        Color blanco = Color.white;

        for (int y = 0; y < 16; y++)
        {
            for (int x = 0; x < 16; x++)
            {
                texturaCorazonPixelCompartida.SetPixel(x, y, transparente);
            }
        }

        string[] patron = new string[]
        {
            "................",
            "................",
            "...XXX..XXX.....",
            "..XXXXX.XXXXX...",
            ".XXXXXXXXXXXXX..",
            ".XXXXXXXXXXXXX..",
            ".XXXXXXXXXXXXX..",
            "..XXXXXXXXXXX...",
            "...XXXXXXXXX....",
            "....XXXXXXX.....",
            ".....XXXXX......",
            "......XXX.......",
            ".......X........",
            "................",
            "................",
            "................"
        };

        for (int fila = 0; fila < patron.Length; fila++)
        {
            int y = 15 - fila;
            string linea = patron[fila];

            for (int x = 0; x < Mathf.Min(16, linea.Length); x++)
            {
                if (linea[x] == 'X')
                {
                    texturaCorazonPixelCompartida.SetPixel(x, y, blanco);
                }
            }
        }

        texturaCorazonPixelCompartida.Apply(false, true);
        return texturaCorazonPixelCompartida;
    }

    private void BuscarJugador()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        jugador = playerObject.transform;

        if (inventarioJugador == null)
        {
            inventarioJugador = jugador.GetComponent<PlayerInventory>();
        }
    }

    private void ReproducirTrigger(string nombreParametro)
    {
        if (animatorMono == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return;
        }

        if (TieneParametroAnimator(nombreParametro, AnimatorControllerParameterType.Trigger))
        {
            animatorMono.SetTrigger(nombreParametro);
        }
    }

    private void CambiarBool(string nombreParametro, bool valor)
    {
        if (animatorMono == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return;
        }

        if (TieneParametroAnimator(nombreParametro, AnimatorControllerParameterType.Bool))
        {
            animatorMono.SetBool(nombreParametro, valor);
        }
    }

    private bool TieneParametroAnimator(string nombreParametro, AnimatorControllerParameterType tipo)
    {
        if (animatorMono == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parametro in animatorMono.parameters)
        {
            if (parametro.type == tipo && parametro.name == nombreParametro)
            {
                return true;
            }
        }

        return false;
    }
}

public class CorazonAmistadTemporal : MonoBehaviour
{
    private Vector3 velocidad;
    private Vector3 rotacionPorSegundo;
    private float duracion = 1.5f;
    private float tiempo;
    private Vector3 escalaInicial;
    private Renderer[] renderers;
    private Material[][] materialesInstanciados;
    private Color colorBase = Color.red;
    private bool aplicarColor;

    public void Inicializar(Vector3 nuevaVelocidad, Vector3 nuevaRotacionPorSegundo, float nuevaDuracion, Color nuevoColor, bool nuevoAplicarColor)
    {
        velocidad = nuevaVelocidad;
        rotacionPorSegundo = nuevaRotacionPorSegundo;
        duracion = Mathf.Max(0.1f, nuevaDuracion);
        colorBase = nuevoColor;
        aplicarColor = nuevoAplicarColor;
        tiempo = 0f;
        escalaInicial = transform.localScale;
        PrepararMateriales();
    }

    private void Awake()
    {
        escalaInicial = transform.localScale;
    }

    private void Update()
    {
        tiempo += Time.deltaTime;
        float t = Mathf.Clamp01(tiempo / duracion);

        transform.position += velocidad * Time.deltaTime;
        transform.Rotate(rotacionPorSegundo * Time.deltaTime, Space.Self);

        velocidad.x = Mathf.Lerp(velocidad.x, 0f, Time.deltaTime * 1.2f);
        velocidad.z = Mathf.Lerp(velocidad.z, 0f, Time.deltaTime * 1.2f);
        velocidad.y = Mathf.Lerp(velocidad.y, velocidad.y * 0.82f, Time.deltaTime * 1.5f);

        float escala = EvaluarEscala(t);
        transform.localScale = escalaInicial * escala;

        ActualizarColor(t);

        if (t >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private float EvaluarEscala(float t)
    {
        if (t < 0.15f)
        {
            return Mathf.Lerp(0.35f, 1f, t / 0.15f);
        }

        return Mathf.Lerp(1f, 0.05f, Mathf.InverseLerp(0.72f, 1f, t));
    }

    private void PrepararMateriales()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        materialesInstanciados = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null)
            {
                continue;
            }

            materialesInstanciados[i] = renderers[i].materials;
        }

        ActualizarColor(0f);
    }

    private void ActualizarColor(float t)
    {
        if (materialesInstanciados == null)
        {
            return;
        }

        float alpha = t < 0.12f ? Mathf.InverseLerp(0f, 0.12f, t) : Mathf.Lerp(1f, 0f, Mathf.InverseLerp(0.72f, 1f, t));
        Color color = aplicarColor ? colorBase : Color.white;
        color.a = alpha;

        for (int i = 0; i < materialesInstanciados.Length; i++)
        {
            Material[] materiales = materialesInstanciados[i];

            if (materiales == null)
            {
                continue;
            }

            foreach (Material mat in materiales)
            {
                if (mat == null)
                {
                    continue;
                }

                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor("_BaseColor", color);
                }
                else if (mat.HasProperty("_Color"))
                {
                    mat.SetColor("_Color", color);
                }
            }
        }
    }
}
