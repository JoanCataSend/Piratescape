using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public sealed class MiniMissionManager : MonoBehaviour
{
    public enum TipoObjetivo
    {
        TenerItem,
        Evento,
        EsperarHora,
        SobrevivirCambioDia
    }

    [System.Serializable]
    public sealed class MiniMision
    {
        [Header("Identificacion")]
        [SerializeField] private string id = "mision";
        [SerializeField] private string titulo = "Nueva mision";
        [TextArea(2, 4)]
        [SerializeField] private string descripcion = "Descripcion de la mision";

        [Header("Objetivo")]
        [SerializeField] private TipoObjetivo tipoObjetivo = TipoObjetivo.Evento;

        [Header("Objetivo por item")]
        [SerializeField] private ItemData itemObjetivo;
        [SerializeField] private string itemIdObjetivo;
        [SerializeField] private int cantidadObjetivo = 1;

        [Header("Objetivo por evento")]
        [SerializeField] private string eventoObjetivo;

        [Header("Objetivo por hora")]
        [Range(0, 23)]
        [SerializeField] private int horaObjetivo = 22;
        [Range(0, 59)]
        [SerializeField] private int minutoObjetivo = 0;

        [Header("Texto")]
        [SerializeField] private string textoCompletada = "Mision completada";

        public string Id => string.IsNullOrWhiteSpace(id) ? titulo : id;
        public string Titulo => titulo;
        public string Descripcion => descripcion;
        public TipoObjetivo Tipo => tipoObjetivo;
        public ItemData ItemObjetivo => itemObjetivo;
        public string ItemIdObjetivo => itemIdObjetivo;
        public int CantidadObjetivo => Mathf.Max(1, cantidadObjetivo);
        public string EventoObjetivo => eventoObjetivo;
        public int HoraObjetivo => Mathf.Clamp(horaObjetivo, 0, 23);
        public int MinutoObjetivo => Mathf.Clamp(minutoObjetivo, 0, 59);
        public string TextoCompletada => string.IsNullOrWhiteSpace(textoCompletada) ? "Mision completada" : textoCompletada;

        public string ObtenerTextoProgreso(MiniMissionManager manager)
        {
            if (manager == null)
            {
                return string.Empty;
            }

            switch (Tipo)
            {
                case TipoObjetivo.TenerItem:
                {
                    int actual = manager.ObtenerCantidadItem(ItemObjetivo, ItemIdObjetivo);
                    return Mathf.Clamp(actual, 0, CantidadObjetivo) + "/" + CantidadObjetivo;
                }

                case TipoObjetivo.Evento:
                    return manager.EventoRegistrado(EventoObjetivo) ? "1/1" : "0/1";

                case TipoObjetivo.EsperarHora:
                    if (manager.gameTimeSystem != null)
                    {
                        return manager.gameTimeSystem.CurrentTimeFormatted + " / " + HoraObjetivo.ToString("00") + ":" + MinutoObjetivo.ToString("00");
                    }

                    return "Esperando hora";

                case TipoObjetivo.SobrevivirCambioDia:
                    if (manager.gameTimeSystem != null)
                    {
                        return "Dia " + manager.gameTimeSystem.CurrentDay;
                    }

                    return "Sobrevive";

                default:
                    return string.Empty;
            }
        }
    }

    public static MiniMissionManager Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private GameTimeSystem gameTimeSystem;
    [SerializeField] private MiniMissionUI missionUI;

    [Header("Misiones")]
    [SerializeField] private bool crearMisionesTutorialSiListaVacia = true;
    [SerializeField] private List<MiniMision> misiones = new List<MiniMision>();
    [SerializeField] private int indiceMisionActual = 0;
    [SerializeField] private bool activarMisionesAlIniciar = true;
    [SerializeField] private float retardoEntreMisiones = 1.2f;
    [SerializeField] private float intervaloComprobarObjetivos = 0.25f;

    [Header("Guardado opcional")]
    [SerializeField] private bool guardarProgresoPlayerPrefs = true;
    [SerializeField] private string claveGuardado = "PirateScape_MiniMisiones_Tutorial";

    [Header("Depuracion")]
    [SerializeField] private bool mostrarLogs = false;

    private readonly HashSet<string> eventosReportados = new HashSet<string>();
    private float siguienteComprobacion;
    private bool misionesActivas;
    private bool completandoMision;
    private int diaInicioMisionActual = 1;

    public MiniMision MisionActual
    {
        get
        {
            if (misiones == null || indiceMisionActual < 0 || indiceMisionActual >= misiones.Count)
            {
                return null;
            }

            return misiones[indiceMisionActual];
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuscarReferenciasSiFaltan();

        if (crearMisionesTutorialSiListaVacia && (misiones == null || misiones.Count == 0))
        {
            CrearMisionesTutorialPorDefecto();
        }

        if (guardarProgresoPlayerPrefs)
        {
            CargarProgreso();
        }

        misionesActivas = activarMisionesAlIniciar;
        PrepararMisionActual();
        RefrescarUI();
    }

    private void OnEnable()
    {
        BuscarReferenciasSiFaltan();

        if (gameTimeSystem != null)
        {
            gameTimeSystem.OnDayChanged += AlCambiarDia;
        }
    }

    private void OnDisable()
    {
        if (gameTimeSystem != null)
        {
            gameTimeSystem.OnDayChanged -= AlCambiarDia;
        }
    }

    private void Update()
    {
        if (!misionesActivas || completandoMision)
        {
            return;
        }

        if (Time.time < siguienteComprobacion)
        {
            return;
        }

        siguienteComprobacion = Time.time + Mathf.Max(0.05f, intervaloComprobarObjetivos);

        if (MisionActual == null)
        {
            RefrescarUI();
            return;
        }

        RefrescarUI();

        if (ObjetivoActualCompletado())
        {
            CompletarMisionActual();
        }
    }

    public static void ReportarEventoGlobal(string idEvento)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.ReportarEvento(idEvento);
    }

    public void ReportarEvento(string idEvento)
    {
        if (string.IsNullOrWhiteSpace(idEvento))
        {
            return;
        }

        string clave = Normalizar(idEvento);
        eventosReportados.Add(clave);

        if (mostrarLogs)
        {
            Debug.Log("[MiniMissionManager] Evento reportado: " + clave, this);
        }

        GuardarProgreso();
        RefrescarUI();

        if (!completandoMision && ObjetivoActualCompletado())
        {
            CompletarMisionActual();
        }
    }

    public bool EventoRegistrado(string idEvento)
    {
        if (string.IsNullOrWhiteSpace(idEvento))
        {
            return false;
        }

        return eventosReportados.Contains(Normalizar(idEvento));
    }

    public void ActivarMisiones()
    {
        misionesActivas = true;
        RefrescarUI();
    }

    public void PausarMisiones()
    {
        misionesActivas = false;
        RefrescarUI();
    }

    [ContextMenu("Debug/Resetear misiones")]
    public void ResetearMisiones()
    {
        indiceMisionActual = 0;
        eventosReportados.Clear();
        completandoMision = false;
        misionesActivas = true;
        PrepararMisionActual();

        if (guardarProgresoPlayerPrefs)
        {
            PlayerPrefs.DeleteKey(claveGuardado + "_indice");
            PlayerPrefs.DeleteKey(claveGuardado + "_eventos");
            PlayerPrefs.DeleteKey(claveGuardado + "_diaInicio");
            PlayerPrefs.Save();
        }

        RefrescarUI();
    }

    [ContextMenu("Debug/Completar mision actual")]
    public void DebugCompletarMisionActual()
    {
        CompletarMisionActual();
    }

    [ContextMenu("Debug/Reportar hablar comerciante")]
    public void DebugReportarHablarComerciante()
    {
        ReportarEvento("hablar_comerciante");
    }

    [ContextMenu("Debug/Reportar pesca primer pez")]
    public void DebugReportarPrimerPez()
    {
        ReportarEvento("pescar_primer_pez");
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (inventarioJugador == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                inventarioJugador = player.GetComponent<PlayerInventory>();
            }
        }

        if (gameTimeSystem == null)
        {
            gameTimeSystem = FindFirstObjectByType<GameTimeSystem>();
        }

        if (missionUI == null)
        {
            missionUI = FindFirstObjectByType<MiniMissionUI>();
        }
    }

    private void CrearMisionesTutorialPorDefecto()
    {
        misiones = new List<MiniMision>
        {
            CrearMisionItem("mision_cana_vieja", "Una caña abandonada", "Encuentra una caña vieja en la isla para poder empezar a pescar.", "cana_vieja", 1, "Has encontrado una caña vieja."),
            CrearMisionEvento("mision_primer_pez", "Primer pez", "Selecciona la caña y pesca cualquier pez.", "pescar_primer_pez", "Has pescado tu primer pez."),
            CrearMisionItem("mision_platanos", "Reserva de plátanos", "Consigue 4 plátanos. A los monos les gustan mucho", "platano", 4, "Ya tienes suficientes plátanos."),
            CrearMisionEvento("mision_mono_amigo", "Un amigo peludo", "Dale 2 plátanos a un mono para ganarte su confianza.", "mono_amigo", "Ahora tienes un mono amigo."),
            CrearMisionHora("mision_esperar_noche", "Espera a la noche", "Espera hasta las 22:00.", 22, 0, "Ya es hora de buscar al comerciante fantasma."),
            CrearMisionEvento("mision_comerciante", "El comerciante fantasma", "Habla con el comerciante fantasmam", "hablar_comerciante", "Has hablado con el comerciante fantasma."),
            CrearMisionSobrevivir("mision_sobrevive_dia", "Sobrevive un día", "Duerme", "Has sobrevivido un día completo."),
            CrearMisionEvento("mision_cueva_luz", "Luz en la oscuridad", "Entra en la cueva llevando una antorcha o farol para explorar con seguridad.", "entrar_cueva_con_luz", "La cueva ya no parece tan oscura.")
        };
    }

    private MiniMision CrearMisionItem(string id, string titulo, string descripcion, string itemId, int cantidad, string completada)
    {
        MiniMision mision = new MiniMision();
        SetPrivateField(mision, "id", id);
        SetPrivateField(mision, "titulo", titulo);
        SetPrivateField(mision, "descripcion", descripcion);
        SetPrivateField(mision, "tipoObjetivo", TipoObjetivo.TenerItem);
        SetPrivateField(mision, "itemIdObjetivo", itemId);
        SetPrivateField(mision, "cantidadObjetivo", cantidad);
        SetPrivateField(mision, "textoCompletada", completada);
        return mision;
    }

    private MiniMision CrearMisionEvento(string id, string titulo, string descripcion, string evento, string completada)
    {
        MiniMision mision = new MiniMision();
        SetPrivateField(mision, "id", id);
        SetPrivateField(mision, "titulo", titulo);
        SetPrivateField(mision, "descripcion", descripcion);
        SetPrivateField(mision, "tipoObjetivo", TipoObjetivo.Evento);
        SetPrivateField(mision, "eventoObjetivo", evento);
        SetPrivateField(mision, "textoCompletada", completada);
        return mision;
    }

    private MiniMision CrearMisionHora(string id, string titulo, string descripcion, int hora, int minuto, string completada)
    {
        MiniMision mision = new MiniMision();
        SetPrivateField(mision, "id", id);
        SetPrivateField(mision, "titulo", titulo);
        SetPrivateField(mision, "descripcion", descripcion);
        SetPrivateField(mision, "tipoObjetivo", TipoObjetivo.EsperarHora);
        SetPrivateField(mision, "horaObjetivo", hora);
        SetPrivateField(mision, "minutoObjetivo", minuto);
        SetPrivateField(mision, "textoCompletada", completada);
        return mision;
    }

    private MiniMision CrearMisionSobrevivir(string id, string titulo, string descripcion, string completada)
    {
        MiniMision mision = new MiniMision();
        SetPrivateField(mision, "id", id);
        SetPrivateField(mision, "titulo", titulo);
        SetPrivateField(mision, "descripcion", descripcion);
        SetPrivateField(mision, "tipoObjetivo", TipoObjetivo.SobrevivirCambioDia);
        SetPrivateField(mision, "textoCompletada", completada);
        return mision;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        if (target == null)
        {
            return;
        }

        FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        if (field != null)
        {
            field.SetValue(target, value);
        }
    }

    private void PrepararMisionActual()
    {
        BuscarReferenciasSiFaltan();

        if (MisionActual != null && MisionActual.Tipo == TipoObjetivo.SobrevivirCambioDia && gameTimeSystem != null)
        {
            diaInicioMisionActual = gameTimeSystem.CurrentDay;
        }

        GuardarProgreso();
    }

    private bool ObjetivoActualCompletado()
    {
        MiniMision mision = MisionActual;

        if (mision == null)
        {
            return false;
        }

        switch (mision.Tipo)
        {
            case TipoObjetivo.TenerItem:
                return ObtenerCantidadItem(mision.ItemObjetivo, mision.ItemIdObjetivo) >= mision.CantidadObjetivo;

            case TipoObjetivo.Evento:
                return EventoRegistrado(mision.EventoObjetivo);

            case TipoObjetivo.EsperarHora:
                return HoraObjetivoAlcanzada(mision.HoraObjetivo, mision.MinutoObjetivo);

            case TipoObjetivo.SobrevivirCambioDia:
                return gameTimeSystem != null && gameTimeSystem.CurrentDay > diaInicioMisionActual;

            default:
                return false;
        }
    }

    private bool HoraObjetivoAlcanzada(int hora, int minuto)
    {
        if (gameTimeSystem == null)
        {
            return false;
        }

        int actual = gameTimeSystem.CurrentHour * 60 + gameTimeSystem.CurrentMinute;
        int objetivo = Mathf.Clamp(hora, 0, 23) * 60 + Mathf.Clamp(minuto, 0, 59);
        return actual >= objetivo;
    }

    private void CompletarMisionActual()
    {
        if (completandoMision || MisionActual == null)
        {
            return;
        }

        StartCoroutine(CompletarMisionRoutine());
    }

    private IEnumerator CompletarMisionRoutine()
    {
        completandoMision = true;
        MiniMision completada = MisionActual;

        if (missionUI != null && completada != null)
        {
            missionUI.MostrarCompletada(completada.Titulo, completada.TextoCompletada);
        }

        if (mostrarLogs && completada != null)
        {
            Debug.Log("[MiniMissionManager] Mision completada: " + completada.Titulo, this);
        }

        indiceMisionActual++;
        GuardarProgreso();

        yield return new WaitForSeconds(retardoEntreMisiones);

        completandoMision = false;
        PrepararMisionActual();
        RefrescarUI();
    }

    private void AlCambiarDia(int nuevoDia)
    {
        if (MisionActual != null && MisionActual.Tipo == TipoObjetivo.SobrevivirCambioDia && nuevoDia > diaInicioMisionActual)
        {
            CompletarMisionActual();
        }
    }

    private void RefrescarUI()
    {
        if (missionUI == null)
        {
            return;
        }

        if (!misionesActivas)
        {
            missionUI.Ocultar();
            return;
        }

        MiniMision mision = MisionActual;

        if (mision == null)
        {
            missionUI.MostrarTodasCompletadas();
            return;
        }

        missionUI.MostrarMision(mision.Titulo, mision.Descripcion, mision.ObtenerTextoProgreso(this));
    }

    public int ObtenerCantidadItem(ItemData itemData, string itemId)
    {
        return ObtenerCantidadItemEnInventario(inventarioJugador, itemData, itemId);
    }

    public static bool InventarioTieneItem(PlayerInventory inventario, ItemData itemData, string itemId, int cantidad)
    {
        return ObtenerCantidadItemEnInventario(inventario, itemData, itemId) >= Mathf.Max(1, cantidad);
    }

    public static int ObtenerCantidadItemEnInventario(PlayerInventory inventario, ItemData itemData, string itemId)
    {
        if (inventario == null)
        {
            return 0;
        }

        int total = 0;

        if (itemData != null)
        {
            try
            {
                total += Mathf.Max(0, inventario.ObtenerCantidad(itemData));
            }
            catch
            {
                // Si el inventario no tiene este metodo en alguna version, seguimos con los slots.
            }
        }

        try
        {
            IEnumerable<InventorySlot> slots = inventario.GetSlots();

            if (slots == null)
            {
                return total;
            }

            foreach (InventorySlot slot in slots)
            {
                if (slot == null)
                {
                    continue;
                }

                bool vacio = false;

                try
                {
                    vacio = slot.IsEmpty();
                }
                catch
                {
                    vacio = false;
                }

                if (vacio || slot.itemData == null)
                {
                    continue;
                }

                if (!CoincideItem(slot.itemData, itemData, itemId))
                {
                    continue;
                }

                total += Mathf.Max(1, ObtenerCantidadSlot(slot));
            }
        }
        catch
        {
            // Si hay una version diferente del inventario, al menos devolvemos lo obtenido por ObtenerCantidad.
        }

        return total;
    }

    private static bool CoincideItem(ItemData item, ItemData itemDataObjetivo, string itemIdObjetivo)
    {
        if (item == null)
        {
            return false;
        }

        if (itemDataObjetivo != null && item == itemDataObjetivo)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(itemIdObjetivo))
        {
            return false;
        }

        string objetivo = Normalizar(itemIdObjetivo);
        string id = Normalizar(item.ItemId);
        string nombreAsset = Normalizar(item.name);
        string nombreMostrar = Normalizar(item.DisplayName);

        return id == objetivo ||
               nombreAsset == objetivo ||
               nombreMostrar == objetivo ||
               nombreAsset.Contains(objetivo) ||
               nombreMostrar.Contains(objetivo);
    }

    private static int ObtenerCantidadSlot(InventorySlot slot)
    {
        if (slot == null)
        {
            return 0;
        }

        string[] posiblesNombres =
        {
            "quantity", "cantidad", "amount", "count", "stack", "stackSize", "cantidadActual", "currentAmount"
        };

        BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        System.Type tipo = slot.GetType();

        for (int i = 0; i < posiblesNombres.Length; i++)
        {
            FieldInfo field = tipo.GetField(posiblesNombres[i], flags);

            if (field != null)
            {
                object value = field.GetValue(slot);

                if (value is int intValue)
                {
                    return Mathf.Max(0, intValue);
                }
            }

            PropertyInfo property = tipo.GetProperty(posiblesNombres[i], flags);

            if (property != null && property.CanRead)
            {
                object value = property.GetValue(slot, null);

                if (value is int intValue)
                {
                    return Mathf.Max(0, intValue);
                }
            }
        }

        return 1;
    }

    private void GuardarProgreso()
    {
        if (!guardarProgresoPlayerPrefs)
        {
            return;
        }

        PlayerPrefs.SetInt(claveGuardado + "_indice", indiceMisionActual);
        PlayerPrefs.SetInt(claveGuardado + "_diaInicio", diaInicioMisionActual);
        PlayerPrefs.SetString(claveGuardado + "_eventos", string.Join("|", eventosReportados));
        PlayerPrefs.Save();
    }

    private void CargarProgreso()
    {
        indiceMisionActual = PlayerPrefs.GetInt(claveGuardado + "_indice", indiceMisionActual);
        diaInicioMisionActual = PlayerPrefs.GetInt(claveGuardado + "_diaInicio", diaInicioMisionActual);

        string eventos = PlayerPrefs.GetString(claveGuardado + "_eventos", string.Empty);
        eventosReportados.Clear();

        if (!string.IsNullOrWhiteSpace(eventos))
        {
            string[] partes = eventos.Split('|');

            for (int i = 0; i < partes.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(partes[i]))
                {
                    eventosReportados.Add(Normalizar(partes[i]));
                }
            }
        }

        if (misiones != null && misiones.Count > 0)
        {
            indiceMisionActual = Mathf.Clamp(indiceMisionActual, 0, misiones.Count);
        }
        else
        {
            indiceMisionActual = 0;
        }
    }

    private static string Normalizar(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        return texto.Trim().ToLowerInvariant()
            .Replace("á", "a")
            .Replace("é", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ú", "u")
            .Replace("ü", "u")
            .Replace("ñ", "n");
    }
}
