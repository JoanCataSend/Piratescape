using System.Collections.Generic;
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

    private enum EstadoRecolector
    {
        SiguiendoJugador,
        YendoAItem,
        VolviendoConItem
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
    }

    private void OnEnable()
    {
        ConfigurarNavMeshAgent();
        ConfigurarColisionesConOtrosMonos();
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
            OcultarPrompt();
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
        estadoRecolector = EstadoRecolector.SiguiendoJugador;

        ReproducirTrigger(triggerAmigo);
        CambiarBool(boolSiguiendo, seguirAlSerAmigo);
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

        if (!recogerItemsSiEsAmigo)
        {
            SeguirJugador();
            return;
        }

        if (itemRecogido != null)
        {
            VolverAlJugadorConItem();
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

        if ((capasItems.value & (1 << item.gameObject.layer)) == 0)
        {
            return false;
        }

        return true;
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

        float distancia = Vector3.Distance(transform.position, jugador.position);
        bool debeMoverse = distancia > distanciaParar;

        CambiarBool(boolSiguiendo, debeMoverse);

        if (!debeMoverse)
        {
            PararNavMeshAgent();
            MirarHacia(jugador.position);
            return;
        }

        MoverHacia(jugador.position, distanciaParar, true);
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

        navMeshAgent.stoppingDistance = distanciaParar;
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
