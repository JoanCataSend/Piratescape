using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;

public sealed class SistemaPescaJugador : MonoBehaviour
{
    private enum EstadoPesca
    {
        Inactivo,
        Lanzando,
        EsperandoPicada,
        Picando,
        Recogiendo
    }

    private enum RarezaPez
    {
        Comun,
        PocoComun,
        Raro,
        Epico,
        Legendario
    }

    [System.Serializable]
    private sealed class PezPescable
    {
        [Header("Item")]
        [SerializeField] private string nombreDebug = "Pez";
        [SerializeField] private ConsumibleItemData itemPez;
        [SerializeField] private int cantidad = 1;

        [Header("Rareza")]
        [SerializeField] private RarezaPez rareza = RarezaPez.Comun;
        [Tooltip("Cuanto mayor sea este numero, mas probable es que salga este pez. Ejemplo: comun 70, raro 15, legendario 1.")]
        [SerializeField] private int pesoProbabilidad = 50;

        [Header("Mensaje opcional")]
        [SerializeField] private string mensajeCapturaPersonalizado;

        public string NombreDebug => nombreDebug;
        public ConsumibleItemData ItemPez => itemPez;
        public int Cantidad => Mathf.Max(1, cantidad);
        public RarezaPez Rareza => rareza;
        public int PesoProbabilidad => Mathf.Max(0, pesoProbabilidad);
        public string MensajeCapturaPersonalizado => mensajeCapturaPersonalizado;
        public bool EsValido => itemPez != null && PesoProbabilidad > 0;
    }

    [Header("Referencias")]
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private Animator animatorJugador;
    [SerializeField] private Transform puntoSalidaAnzuelo;
    [SerializeField] private Camera camaraJugador;

    [Header("Items")]
    [SerializeField] private bool requerirCanaSeleccionada = true;
    [SerializeField] private ItemData itemCanaPescar;
    [SerializeField] private ConsumibleItemData itemPescado;
    [SerializeField] private int cantidadPescadoGanado = 1;

    [Header("Peces diferentes y rareza")]
    [SerializeField] private bool usarTablaPecesConRareza = true;
    [SerializeField] private bool usarPescadoBaseComoFallback = true;
    [SerializeField] private bool mostrarRarezaEnMensaje = true;
    [SerializeField] private List<PezPescable> pecesDisponibles = new List<PezPescable>();

    [Header("Deteccion de agua y prompt")]
    [SerializeField] private bool mostrarPromptAlEstarCercaDelAgua = true;
    [SerializeField] private string mensajePromptLanzar = "Lanzar caña";
    [SerializeField] private string mensajePromptRecoger = "Tirar de la caña";
    [SerializeField] private float intervaloActualizarPuntoAgua = 0.08f;

    [Header("Input")]
    [SerializeField] private bool usarTeclaE = true;
    [SerializeField] private bool permitirClickComoAlternativa = false;
    [SerializeField] private bool permitirMando = true;

    [Header("Anzuelo")]
    [SerializeField] private GameObject prefabAnzuelo;
    [SerializeField] private bool usarDireccionCamaraParaLanzar = false;
    [SerializeField] private bool usarRotacionOriginalPrefabAnzuelo = true;
    [SerializeField] private Vector3 rotacionExtraAnzueloEuler = Vector3.zero;
    [SerializeField] private float distanciaLanzamientoMinima = 2.2f;
    [SerializeField] private float distanciaLanzamientoMaxima = 5.5f;
    [SerializeField] private float pasoBusquedaAgua = 0.4f;
    [SerializeField] private float alturaOrigenRaycastAgua = 2.5f;
    [SerializeField] private float distanciaRaycastAgua = 6f;
    [SerializeField] private LayerMask capasAgua;
    [SerializeField] private bool autoConfigurarCapasAgua = true;
    [SerializeField] private bool detectarTriggersAgua = true;

    [Header("Suelo que bloquea la pesca")]
    [Tooltip("Evita lanzar la caña si el raycast encuentra agua, pero hay suelo/isla/arena por encima de esa agua.")]
    [SerializeField] private bool bloquearPescaSiSueloOcultaAgua = true;
    [SerializeField] private LayerMask capasSueloBloqueanPesca;
    [SerializeField] private bool autoConfigurarCapasSueloBloqueanPesca = true;
    [SerializeField] private bool ignorarTriggersSueloBloqueoPesca = true;
    [SerializeField] private float margenSueloSobreAgua = 0.08f;

    [Header("Movimiento del anzuelo")]
    [SerializeField] private float duracionLanzamientoAnzuelo = 0.45f;
    [SerializeField] private float alturaArcoLanzamiento = 0.65f;
    [SerializeField] private float duracionRetornoAnzuelo = 0.32f;
    [SerializeField] private float alturaArcoRetorno = 0.35f;
    [SerializeField] private AnimationCurve curvaMovimientoAnzuelo = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Tiempos estilo Animal Crossing")]
    [SerializeField] private float tiempoMinimoPicada = 2f;
    [SerializeField] private float tiempoMaximoPicada = 5f;
    [SerializeField] private float tiempoParaReaccionar = 1.15f;
    [SerializeField] private bool fallarSiPulsaAntesDePicar = true;

    [Header("Cancelacion")]
    [SerializeField] private bool cancelarSiJugadorSeAleja = true;
    [SerializeField] private float distanciaMaximaAlAnzuelo = 7f;

    [Header("Animaciones jugador opcionales")]
    [SerializeField] private string triggerLanzar = "FishCast";
    [SerializeField] private string triggerRecogerBien = "FishCatch";
    [SerializeField] private string triggerRecogerMal = "FishFail";

    [Header("Audio")]
    [SerializeField] private AudioSource audioSourcePesca;
    [SerializeField] private AudioMixerGroup outputPesca;
    [SerializeField] private AudioClip sonidoLanzar;
    [SerializeField] private AudioClip sonidoFlotarAgua;
    [SerializeField] private AudioClip sonidoPicada;
    [SerializeField] private AudioClip sonidoPescaConseguida;
    [SerializeField] private AudioClip sonidoPescaFallida;
    [SerializeField] private float volumenAudio = 0.8f;

    [Header("Mensajes")]
    [SerializeField] private bool mostrarMensajes = true;
    [SerializeField] private float duracionMensajes = 1.4f;

    [Header("Debug")]
    [SerializeField] private bool mostrarLogs;

    private EstadoPesca estadoActual = EstadoPesca.Inactivo;
    private GameObject anzueloActualGO;
    private AnzueloPesca anzueloActual;
    private Coroutine rutinaPesca;
    private Vector3 posicionAnzueloActual;

    private ConsumibleItemData itemPescadoResultadoActual;
    private int cantidadPescadoResultadoActual = 1;
    private PezPescable pezResultadoActual;

    private Vector3 ultimoPuntoAguaDetectado;
    private bool hayPuntoAguaDetectado;
    private float proximaActualizacionAgua;
    private bool promptMostrado;

    public bool EstaPescando => estadoActual != EstadoPesca.Inactivo;
    public bool EstaPicando => estadoActual == EstadoPesca.Picando;

    private void Awake()
    {
        CachearReferencias();
        PrepararAudio();
        AutoConfigurarLayerAguaSiHaceFalta();
        AutoConfigurarLayersSueloBloqueanPescaSiHaceFalta();
        ValidarValores();
    }

    private void OnDisable()
    {
        OcultarPromptPesca();
        LimpiarAnzuelo();

        if (rutinaPesca != null)
        {
            StopCoroutine(rutinaPesca);
            rutinaPesca = null;
        }

        estadoActual = EstadoPesca.Inactivo;
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            return;
        }

        ActualizarDeteccionAguaYPrompt();

        if (cancelarSiJugadorSeAleja && EstaPescando && anzueloActualGO != null)
        {
            float distancia = Vector3.Distance(transform.position, posicionAnzueloActual);

            if (distancia > distanciaMaximaAlAnzuelo)
            {
                FallarPesca("Te has alejado demasiado.");
                return;
            }
        }

        if (!SeHaPulsadoInteraccionPesca())
        {
            return;
        }

        ProcesarInteraccionPesca();
    }

    private void ActualizarDeteccionAguaYPrompt()
    {
        if (estadoActual == EstadoPesca.Picando)
        {
            hayPuntoAguaDetectado = false;
            MostrarPromptPesca(mensajePromptRecoger);
            return;
        }

        if (estadoActual != EstadoPesca.Inactivo)
        {
            hayPuntoAguaDetectado = false;
            OcultarPromptPesca();
            return;
        }

        if (Time.time < proximaActualizacionAgua)
        {
            ActualizarPromptConEstadoActual();
            return;
        }

        proximaActualizacionAgua = Time.time + intervaloActualizarPuntoAgua;
        hayPuntoAguaDetectado = false;

        if (PuedeIntentarPescar() && BuscarPuntoAguaParaLanzar(out Vector3 puntoAgua))
        {
            ultimoPuntoAguaDetectado = puntoAgua;
            hayPuntoAguaDetectado = true;
        }

        ActualizarPromptConEstadoActual();
    }

    private void ActualizarPromptConEstadoActual()
    {
        if (!mostrarPromptAlEstarCercaDelAgua || !mostrarMensajes)
        {
            return;
        }

        if (estadoActual == EstadoPesca.Inactivo && hayPuntoAguaDetectado)
        {
            MostrarPromptPesca(mensajePromptLanzar);
        }
        else if (estadoActual == EstadoPesca.Picando)
        {
            MostrarPromptPesca(mensajePromptRecoger);
        }
        else
        {
            OcultarPromptPesca();
        }
    }

    private void ProcesarInteraccionPesca()
    {
        if (estadoActual == EstadoPesca.Inactivo)
        {
            if (!PuedeIntentarPescar())
            {
                return;
            }

            if (!hayPuntoAguaDetectado && !BuscarPuntoAguaParaLanzar(out ultimoPuntoAguaDetectado))
            {
                MostrarMensajeTemporal("Acércate y mira hacia una superficie de agua.");
                return;
            }

            IntentarLanzarAnzuelo(ultimoPuntoAguaDetectado);
            return;
        }

        if (estadoActual == EstadoPesca.EsperandoPicada)
        {
            if (fallarSiPulsaAntesDePicar)
            {
                FallarPesca("Has tirado demasiado pronto.");
            }

            return;
        }

        if (estadoActual == EstadoPesca.Picando)
        {
            CompletarPescaConExito();
        }
    }

    private bool PuedeIntentarPescar()
    {
        if (!requerirCanaSeleccionada)
        {
            return true;
        }

        if (itemCanaPescar == null)
        {
            return false;
        }

        if (inventarioJugador == null)
        {
            return false;
        }

        InventorySlot slot = inventarioJugador.GetSlot(inventarioJugador.SelectedSlotIndex);

        if (slot == null || slot.IsEmpty())
        {
            return false;
        }

        return slot.itemData == itemCanaPescar;
    }

    private void IntentarLanzarAnzuelo(Vector3 puntoAgua)
    {
        OcultarPromptPesca();

        posicionAnzueloActual = puntoAgua;
        Vector3 posicionSalida = ObtenerPosicionSalidaAnzuelo();
        CrearAnzuelo(posicionSalida);

        if (anzueloActual == null)
        {
            MostrarMensajeTemporal("Falta el prefab del anzuelo.");
            LimpiarAnzuelo();
            return;
        }

        ReproducirTrigger(triggerLanzar);
        ReproducirSonido(sonidoLanzar);
        MostrarMensajeTemporal("Espera a que pique...");

        estadoActual = EstadoPesca.Lanzando;
        rutinaPesca = StartCoroutine(LanzarYEsperarPicadaRoutine(puntoAgua));
    }

    private IEnumerator LanzarYEsperarPicadaRoutine(Vector3 puntoAgua)
    {
        if (anzueloActualGO != null)
        {
            yield return MoverAnzueloConArcoRoutine(
                anzueloActualGO.transform,
                anzueloActualGO.transform.position,
                puntoAgua,
                duracionLanzamientoAnzuelo,
                alturaArcoLanzamiento
            );
        }

        if (estadoActual != EstadoPesca.Lanzando)
        {
            rutinaPesca = null;
            yield break;
        }

        if (anzueloActual != null)
        {
            anzueloActual.Inicializar(puntoAgua, puntoSalidaAnzuelo);
        }

        ReproducirSonido(sonidoFlotarAgua);
        estadoActual = EstadoPesca.EsperandoPicada;
        yield return EsperarPicadaRoutine();
    }

    private IEnumerator EsperarPicadaRoutine()
    {
        float tiempoPicada = UnityEngine.Random.Range(tiempoMinimoPicada, tiempoMaximoPicada);
        float tiempo = 0f;

        while (tiempo < tiempoPicada)
        {
            if (estadoActual != EstadoPesca.EsperandoPicada)
            {
                rutinaPesca = null;
                yield break;
            }

            tiempo += Time.deltaTime;
            yield return null;
        }

        if (estadoActual != EstadoPesca.EsperandoPicada)
        {
            rutinaPesca = null;
            yield break;
        }

        estadoActual = EstadoPesca.Picando;

        if (anzueloActual != null)
        {
            anzueloActual.ActivarPicada();
        }

        ReproducirSonido(sonidoPicada);
        MostrarPromptPesca(mensajePromptRecoger);

        float tiempoReaccion = 0f;

        while (tiempoReaccion < tiempoParaReaccionar)
        {
            if (estadoActual != EstadoPesca.Picando)
            {
                rutinaPesca = null;
                yield break;
            }

            tiempoReaccion += Time.deltaTime;
            yield return null;
        }

        if (estadoActual == EstadoPesca.Picando)
        {
            FallarPesca("El pez se ha escapado.");
        }

        rutinaPesca = null;
    }

    private void CompletarPescaConExito()
    {
        if (rutinaPesca != null)
        {
            StopCoroutine(rutinaPesca);
            rutinaPesca = null;
        }

        estadoActual = EstadoPesca.Recogiendo;
        OcultarPromptPesca();
        ReproducirTrigger(triggerRecogerBien);
        PrepararResultadoPescaConExito();

        rutinaPesca = StartCoroutine(TerminarPescaRoutine(true, CrearMensajeResultadoPesca()));
    }

    private void FallarPesca(string mensaje)
    {
        if (rutinaPesca != null)
        {
            StopCoroutine(rutinaPesca);
            rutinaPesca = null;
        }

        if (estadoActual == EstadoPesca.Inactivo)
        {
            return;
        }

        estadoActual = EstadoPesca.Recogiendo;
        OcultarPromptPesca();
        ReproducirTrigger(triggerRecogerMal);
        ReproducirSonido(sonidoPescaFallida);
        MostrarMensajeTemporal(mensaje);

        rutinaPesca = StartCoroutine(TerminarPescaRoutine(false, mensaje));
    }

    private IEnumerator TerminarPescaRoutine(bool pescaConExito, string mensajeExito)
    {
        if (anzueloActual != null)
        {
            anzueloActual.DetenerVisuales(true);
            anzueloActual.ActivarLineaCana(true);
        }

        if (anzueloActualGO != null)
        {
            yield return MoverAnzueloConArcoRoutine(
                anzueloActualGO.transform,
                anzueloActualGO.transform.position,
                ObtenerPosicionSalidaAnzuelo(),
                duracionRetornoAnzuelo,
                alturaArcoRetorno
            );
        }

        LimpiarAnzuelo();

        if (pescaConExito)
        {
            bool pescadoAñadido = false;
            ConsumibleItemData itemResultado = ObtenerItemPescadoResultadoActual();
            int cantidadResultado = ObtenerCantidadPescadoResultadoActual();

            if (inventarioJugador != null && itemResultado != null)
            {
                pescadoAñadido = inventarioJugador.TryAddItem(itemResultado, cantidadResultado);
            }

            if (pescadoAñadido)
            {
                ReproducirSonido(sonidoPescaConseguida);
                MostrarMensajeTemporal(mensajeExito);
            }
            else
            {
                SoltarPescadoEnSueloSiHaceFalta();
                MostrarMensajeTemporal("Bolsillos llenos. El pescado cayó al suelo.");
            }
        }

        estadoActual = EstadoPesca.Inactivo;
        LimpiarResultadoPesca();
        rutinaPesca = null;
    }

    private void PrepararResultadoPescaConExito()
    {
        pezResultadoActual = SeleccionarPezPorRareza();

        if (pezResultadoActual != null && pezResultadoActual.ItemPez != null)
        {
            itemPescadoResultadoActual = pezResultadoActual.ItemPez;
            cantidadPescadoResultadoActual = pezResultadoActual.Cantidad;
            return;
        }

        itemPescadoResultadoActual = itemPescado;
        cantidadPescadoResultadoActual = Mathf.Max(1, cantidadPescadoGanado);
    }

    private PezPescable SeleccionarPezPorRareza()
    {
        if (!usarTablaPecesConRareza || pecesDisponibles == null || pecesDisponibles.Count == 0)
        {
            return null;
        }

        int pesoTotal = 0;

        for (int i = 0; i < pecesDisponibles.Count; i++)
        {
            PezPescable pez = pecesDisponibles[i];

            if (pez == null || !pez.EsValido)
            {
                continue;
            }

            pesoTotal += pez.PesoProbabilidad;
        }

        if (pesoTotal <= 0)
        {
            return null;
        }

        int tirada = UnityEngine.Random.Range(0, pesoTotal);
        int acumulado = 0;

        for (int i = 0; i < pecesDisponibles.Count; i++)
        {
            PezPescable pez = pecesDisponibles[i];

            if (pez == null || !pez.EsValido)
            {
                continue;
            }

            acumulado += pez.PesoProbabilidad;

            if (tirada < acumulado)
            {
                return pez;
            }
        }

        return null;
    }

    private string CrearMensajeResultadoPesca()
    {
        if (pezResultadoActual != null && !string.IsNullOrWhiteSpace(pezResultadoActual.MensajeCapturaPersonalizado))
        {
            return pezResultadoActual.MensajeCapturaPersonalizado;
        }

        ConsumibleItemData itemResultado = ObtenerItemPescadoResultadoActual();
        string nombre = ObtenerNombreItem(itemResultado);

        if (pezResultadoActual != null && mostrarRarezaEnMensaje)
        {
            return "¡Has pescado " + nombre + "! (" + ObtenerTextoRareza(pezResultadoActual.Rareza) + ")";
        }

        return "¡Has pescado " + nombre + "!";
    }

    private string ObtenerTextoRareza(RarezaPez rareza)
    {
        switch (rareza)
        {
            case RarezaPez.PocoComun:
                return "Poco común";
            case RarezaPez.Raro:
                return "Raro";
            case RarezaPez.Epico:
                return "Épico";
            case RarezaPez.Legendario:
                return "Legendario";
            default:
                return "Común";
        }
    }

    private ConsumibleItemData ObtenerItemPescadoResultadoActual()
    {
        if (itemPescadoResultadoActual != null)
        {
            return itemPescadoResultadoActual;
        }

        if (usarPescadoBaseComoFallback)
        {
            return itemPescado;
        }

        return null;
    }

    private int ObtenerCantidadPescadoResultadoActual()
    {
        if (itemPescadoResultadoActual != null)
        {
            return Mathf.Max(1, cantidadPescadoResultadoActual);
        }

        return Mathf.Max(1, cantidadPescadoGanado);
    }

    private void LimpiarResultadoPesca()
    {
        itemPescadoResultadoActual = null;
        cantidadPescadoResultadoActual = 1;
        pezResultadoActual = null;
    }

    private IEnumerator MoverAnzueloConArcoRoutine(
        Transform objetivo,
        Vector3 inicio,
        Vector3 destino,
        float duracion,
        float alturaArco)
    {
        if (objetivo == null)
        {
            yield break;
        }

        if (duracion <= 0f)
        {
            objetivo.position = destino;
            yield break;
        }

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;
            float t = Mathf.Clamp01(tiempo / duracion);
            float evaluado = curvaMovimientoAnzuelo != null ? curvaMovimientoAnzuelo.Evaluate(t) : t;
            Vector3 posicionLineal = Vector3.Lerp(inicio, destino, evaluado);
            float arco = Mathf.Sin(evaluado * Mathf.PI) * alturaArco;
            objetivo.position = posicionLineal + Vector3.up * arco;
            yield return null;
        }

        objetivo.position = destino;
    }

    private bool BuscarPuntoAguaParaLanzar(out Vector3 puntoAgua)
    {
        puntoAgua = Vector3.zero;

        Vector3 direccion = ObtenerDireccionLanzamiento();
        QueryTriggerInteraction triggerModeAgua = detectarTriggersAgua
            ? QueryTriggerInteraction.Collide
            : QueryTriggerInteraction.Ignore;

        for (float distancia = distanciaLanzamientoMinima; distancia <= distanciaLanzamientoMaxima; distancia += pasoBusquedaAgua)
        {
            Vector3 origen = transform.position + direccion * distancia + Vector3.up * alturaOrigenRaycastAgua;

            if (!Physics.Raycast(origen, Vector3.down, out RaycastHit hitAgua, distanciaRaycastAgua, capasAgua, triggerModeAgua))
            {
                continue;
            }

            if (AguaEstaTapadaPorSuelo(origen, hitAgua.distance))
            {
                continue;
            }

            puntoAgua = hitAgua.point;
            return true;
        }

        return false;
    }

    private bool AguaEstaTapadaPorSuelo(Vector3 origenRaycast, float distanciaHastaAgua)
    {
        if (!bloquearPescaSiSueloOcultaAgua)
        {
            return false;
        }

        if (capasSueloBloqueanPesca.value == 0)
        {
            return false;
        }

        QueryTriggerInteraction triggerModeSuelo = ignorarTriggersSueloBloqueoPesca
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;

        float distanciaChequeoSuelo = Mathf.Max(0.01f, distanciaHastaAgua - margenSueloSobreAgua);

        return Physics.Raycast(
            origenRaycast,
            Vector3.down,
            distanciaChequeoSuelo,
            capasSueloBloqueanPesca,
            triggerModeSuelo
        );
    }

    private Vector3 ObtenerDireccionLanzamiento()
    {
        Vector3 direccion = transform.forward;
        direccion.y = 0f;

        if (usarDireccionCamaraParaLanzar && camaraJugador != null)
        {
            direccion = camaraJugador.transform.forward;
            direccion.y = 0f;
        }

        if (direccion.sqrMagnitude < 0.01f)
        {
            direccion = transform.forward;
            direccion.y = 0f;
        }

        if (direccion.sqrMagnitude < 0.01f)
        {
            direccion = Vector3.forward;
        }

        return direccion.normalized;
    }

    private Vector3 ObtenerPosicionSalidaAnzuelo()
    {
        if (puntoSalidaAnzuelo != null)
        {
            return puntoSalidaAnzuelo.position;
        }

        Vector3 direccion = ObtenerDireccionLanzamiento();
        return transform.position + Vector3.up * 1.25f + direccion * 0.45f;
    }

    private void CrearAnzuelo(Vector3 posicionInicial)
    {
        if (prefabAnzuelo != null)
        {
            Quaternion rotacionInicial = ObtenerRotacionInicialAnzuelo();
            anzueloActualGO = Instantiate(prefabAnzuelo, posicionInicial, rotacionInicial);
        }
        else
        {
            anzueloActualGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            anzueloActualGO.name = "AnzueloPesca_Auto";
            anzueloActualGO.transform.localScale = Vector3.one * 0.18f;
            anzueloActualGO.transform.position = posicionInicial;
            Collider colliderAnzuelo = anzueloActualGO.GetComponent<Collider>();

            if (colliderAnzuelo != null)
            {
                colliderAnzuelo.enabled = false;
            }
        }

        anzueloActual = anzueloActualGO.GetComponent<AnzueloPesca>();

        if (anzueloActual == null)
        {
            anzueloActual = anzueloActualGO.AddComponent<AnzueloPesca>();
        }

        anzueloActual.PrepararParaLanzamiento(posicionInicial, puntoSalidaAnzuelo);
    }

    private Quaternion ObtenerRotacionInicialAnzuelo()
    {
        Quaternion rotacion = Quaternion.identity;

        if (usarRotacionOriginalPrefabAnzuelo && prefabAnzuelo != null)
        {
            rotacion = prefabAnzuelo.transform.rotation;
        }

        if (rotacionExtraAnzueloEuler != Vector3.zero)
        {
            rotacion *= Quaternion.Euler(rotacionExtraAnzueloEuler);
        }

        return rotacion;
    }

    private void LimpiarAnzuelo()
    {
        if (anzueloActualGO != null)
        {
            Destroy(anzueloActualGO);
        }

        anzueloActualGO = null;
        anzueloActual = null;
    }

    private void SoltarPescadoEnSueloSiHaceFalta()
    {
        ConsumibleItemData itemResultado = ObtenerItemPescadoResultadoActual();

        if (itemResultado == null || itemResultado.WorldPrefab == null)
        {
            return;
        }

        Vector3 posicion = transform.position + transform.forward * 1.2f + Vector3.up * 0.6f;
        Instantiate(itemResultado.WorldPrefab, posicion, Quaternion.identity);
    }

    private bool SeHaPulsadoInteraccionPesca()
    {
        if (usarTeclaE && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }

        if (permitirClickComoAlternativa && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        if (permitirMando && Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private void ReproducirTrigger(string triggerName)
    {
        if (animatorJugador == null || string.IsNullOrWhiteSpace(triggerName))
        {
            return;
        }

        animatorJugador.SetTrigger(triggerName);
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSourcePesca == null || clip == null)
        {
            return;
        }

        audioSourcePesca.PlayOneShot(clip, volumenAudio);
    }

    private void MostrarPromptPesca(string mensaje)
    {
        if (!mostrarMensajes || string.IsNullOrWhiteSpace(mensaje))
        {
            return;
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Show(this, mensaje);
            promptMostrado = true;
        }
    }

    private void OcultarPromptPesca()
    {
        if (!promptMostrado)
        {
            return;
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide(this);
        }

        promptMostrado = false;
    }

    private void MostrarMensajeTemporal(string mensaje)
    {
        if (!mostrarMensajes || string.IsNullOrWhiteSpace(mensaje))
        {
            return;
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.ShowTemporary(this, mensaje, duracionMensajes);
            promptMostrado = false;
        }

        if (mostrarLogs)
        {
            Debug.Log("SistemaPescaJugador: " + mensaje, this);
        }
    }

    private string ObtenerNombreItem(ItemData item)
    {
        if (item == null)
        {
            return "pescado";
        }

        if (!string.IsNullOrWhiteSpace(item.DisplayName))
        {
            return item.DisplayName;
        }

        return item.name;
    }

    private void CachearReferencias()
    {
        if (inventarioJugador == null)
        {
            inventarioJugador = GetComponent<PlayerInventory>();
        }

        if (animatorJugador == null)
        {
            animatorJugador = GetComponentInChildren<Animator>();
        }

        if (camaraJugador == null)
        {
            camaraJugador = Camera.main;
        }
    }

    private void PrepararAudio()
    {
        if (audioSourcePesca == null)
        {
            audioSourcePesca = gameObject.AddComponent<AudioSource>();
        }

        audioSourcePesca.playOnAwake = false;
        audioSourcePesca.loop = false;
        audioSourcePesca.spatialBlend = 0f;
        audioSourcePesca.outputAudioMixerGroup = outputPesca;
    }

    private void AutoConfigurarLayerAguaSiHaceFalta()
    {
        if (!autoConfigurarCapasAgua || capasAgua.value != 0)
        {
            return;
        }

        int agua = LayerMask.NameToLayer("Agua");
        int water = LayerMask.NameToLayer("Water");

        if (agua >= 0)
        {
            capasAgua = 1 << agua;
            return;
        }

        if (water >= 0)
        {
            capasAgua = 1 << water;
        }
    }

    private void AutoConfigurarLayersSueloBloqueanPescaSiHaceFalta()
    {
        if (!autoConfigurarCapasSueloBloqueanPesca || capasSueloBloqueanPesca.value != 0)
        {
            return;
        }

        string[] nombresCapasSuelo =
        {
            "Suelo",
            "Isla",
            "Ground",
            "Terrain",
            "Terreno"
        };

        int mascara = 0;

        for (int i = 0; i < nombresCapasSuelo.Length; i++)
        {
            int layer = LayerMask.NameToLayer(nombresCapasSuelo[i]);

            if (layer >= 0)
            {
                mascara |= 1 << layer;
            }
        }

        if (mascara != 0)
        {
            capasSueloBloqueanPesca = mascara;
        }
    }

    private void ValidarValores()
    {
        tiempoMinimoPicada = Mathf.Max(0.1f, tiempoMinimoPicada);
        tiempoMaximoPicada = Mathf.Max(tiempoMinimoPicada, tiempoMaximoPicada);
        tiempoParaReaccionar = Mathf.Max(0.1f, tiempoParaReaccionar);
        pasoBusquedaAgua = Mathf.Max(0.1f, pasoBusquedaAgua);
        cantidadPescadoGanado = Mathf.Max(1, cantidadPescadoGanado);
        cantidadPescadoResultadoActual = Mathf.Max(1, cantidadPescadoResultadoActual);
        intervaloActualizarPuntoAgua = Mathf.Max(0.02f, intervaloActualizarPuntoAgua);
        duracionLanzamientoAnzuelo = Mathf.Max(0.01f, duracionLanzamientoAnzuelo);
        duracionRetornoAnzuelo = Mathf.Max(0.01f, duracionRetornoAnzuelo);
        margenSueloSobreAgua = Mathf.Max(0f, margenSueloSobreAgua);
    }
}
