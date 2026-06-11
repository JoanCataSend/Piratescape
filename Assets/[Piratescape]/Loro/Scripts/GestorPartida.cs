using System.IO;
using UnityEngine;

public sealed class GestorPartida : MonoBehaviour
{
    public static GestorPartida Instance { get; private set; }

    private enum ModoInicioPartida
    {
        Ninguno,
        CargarPartida,
        NuevaPartida
    }

    private const string NombreArchivo = "partida_pirata.json";
    private const string ClaveTutorialCompletado = "tutorial_loro_completado";
    private static ModoInicioPartida modoInicioPendiente = ModoInicioPartida.Ninguno;

    public static bool PartidaCargadaEnEsteInicio { get; private set; }

    [Header("Bases de datos")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private BaseObjectDatabase baseObjectDatabase;

    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private GameTimeSystem sistemaTiempo;
    [SerializeField] private SistemaEconomia sistemaEconomia;
    [SerializeField] private SistemaObjetosBase sistemaObjetosBase;

    public bool TutorialCompletado
    {
        get => PlayerPrefs.GetInt(ClaveTutorialCompletado, 0) == 1;
    }

    private static string RutaArchivoEstatica => Path.Combine(Application.persistentDataPath, NombreArchivo);
    private string RutaArchivo => RutaArchivoEstatica;

    public static void SolicitarCargarAlEntrar()
    {
        PartidaCargadaEnEsteInicio = false;
        modoInicioPendiente = ModoInicioPartida.CargarPartida;
    }

    public static void SolicitarNuevaPartidaAlEntrar()
    {
        PartidaCargadaEnEsteInicio = false;
        modoInicioPendiente = ModoInicioPartida.NuevaPartida;
    }

    public static bool ExistePartidaGuardadaEnDisco()
    {
        return File.Exists(RutaArchivoEstatica);
    }

    public static string ObtenerRutaArchivoGuardado()
    {
        return RutaArchivoEstatica;
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
    }

    private void Start()
    {
        ProcesarModoInicioPendiente();
    }

    public bool ExistePartidaGuardada()
    {
        return ExistePartidaGuardadaEnDisco();
    }

    public void GuardarPartida()
    {
        BuscarReferenciasSiFaltan();

        DatosPartida datos = CrearDatosPartida();
        string json = JsonUtility.ToJson(datos, true);
        File.WriteAllText(RutaArchivo, json);

        Debug.Log("Partida guardada en: " + RutaArchivo);
    }

    public bool CargarPartida()
    {
        BuscarReferenciasSiFaltan();

        if (!ExistePartidaGuardada())
        {
            Debug.LogWarning("No hay partida guardada para cargar.");
            return false;
        }

        string json = File.ReadAllText(RutaArchivo);
        DatosPartida datos = JsonUtility.FromJson<DatosPartida>(json);

        if (datos == null)
        {
            Debug.LogWarning("El archivo de guardado no se ha podido leer.");
            return false;
        }

        AplicarDatosPartida(datos);
        Debug.Log("Partida cargada desde: " + RutaArchivo);
        return true;
    }

    public void MarcarTutorialCompletado(bool completado)
    {
        PlayerPrefs.SetInt(ClaveTutorialCompletado, completado ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void BorrarPartida()
    {
        if (ExistePartidaGuardada())
        {
            File.Delete(RutaArchivo);
        }

        PlayerPrefs.DeleteKey(ClaveTutorialCompletado);
        PlayerPrefs.Save();
    }


    private void ProcesarModoInicioPendiente()
    {
        if (modoInicioPendiente == ModoInicioPartida.Ninguno)
        {
            return;
        }

        ModoInicioPartida modo = modoInicioPendiente;
        modoInicioPendiente = ModoInicioPartida.Ninguno;

        if (modo == ModoInicioPartida.NuevaPartida)
        {
            PartidaCargadaEnEsteInicio = false;
            BorrarPartida();
            Debug.Log("Nueva partida iniciada desde cero. Guardado anterior borrado si existia.");
            return;
        }

        if (modo == ModoInicioPartida.CargarPartida)
        {
            bool cargada = CargarPartida();
            PartidaCargadaEnEsteInicio = cargada;

            if (!cargada)
            {
                Debug.LogWarning("No se ha podido cargar la partida. Se mantiene la escena como partida nueva.");
            }
        }
    }

    private DatosPartida CrearDatosPartida()
    {
        DatosPartida datos = new DatosPartida();

        datos.tutorialCompletado = TutorialCompletado;

        if (jugador != null)
        {
            datos.jugador.posicion = jugador.position;
            datos.jugador.rotacionEuler = jugador.eulerAngles;
        }

        if (playerHealth != null)
        {
            datos.jugador.saludActual = playerHealth.CurrentHealth;
        }

        if (playerEnergy != null)
        {
            datos.jugador.energiaActual = Mathf.RoundToInt(playerEnergy.CurrentEnergy);
        }

        if (sistemaTiempo != null)
        {
            datos.tiempo.dia = sistemaTiempo.CurrentDay;
            datos.tiempo.hora = sistemaTiempo.CurrentHour;
            datos.tiempo.minuto = sistemaTiempo.CurrentMinute;
        }

        if (sistemaEconomia != null)
        {
            datos.economia.conchas = sistemaEconomia.Conchas;
            datos.economia.tulipanes = sistemaEconomia.Tulipanes;
            datos.economia.pinyas = sistemaEconomia.Pinyas;
        }

        datos.inventarioJugador = InventarioGuardadoUtil.CrearDesdePlayer(inventarioJugador);

        if (sistemaObjetosBase != null)
        {
            datos.objetosBase = sistemaObjetosBase.CrearDatosGuardado();
        }

        return datos;
    }

    private void AplicarDatosPartida(DatosPartida datos)
    {
        MarcarTutorialCompletado(datos.tutorialCompletado);

        AplicarJugador(datos.jugador);
        AplicarTiempo(datos.tiempo);
        AplicarEconomia(datos.economia);
        InventarioGuardadoUtil.CargarEnPlayer(inventarioJugador, datos.inventarioJugador, itemDatabase);

        if (sistemaObjetosBase != null)
        {
            sistemaObjetosBase.CargarDatosGuardado(datos.objetosBase, baseObjectDatabase, itemDatabase);
        }
    }

    private void AplicarJugador(DatosJugador datosJugador)
    {
        if (datosJugador == null)
        {
            return;
        }

        if (jugador != null)
        {
            CharacterController controller = jugador.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
            }

            jugador.position = datosJugador.posicion;
            jugador.eulerAngles = datosJugador.rotacionEuler;

            if (controller != null)
            {
                controller.enabled = true;
            }
        }

        if (playerHealth != null)
        {
            int diferenciaSalud = datosJugador.saludActual - playerHealth.CurrentHealth;

            if (diferenciaSalud > 0)
            {
                playerHealth.Heal(diferenciaSalud);
            }
            else if (diferenciaSalud < 0)
            {
                playerHealth.TakeDamage(-diferenciaSalud);
            }
        }

        if (playerEnergy != null)
        {
            playerEnergy.SetEnergy(datosJugador.energiaActual);
        }
    }

    private void AplicarTiempo(DatosTiempo datosTiempo)
    {
        if (sistemaTiempo == null || datosTiempo == null)
        {
            return;
        }

        sistemaTiempo.SetTime(datosTiempo.dia, datosTiempo.hora, datosTiempo.minuto);
    }

    private void AplicarEconomia(DatosEconomia datosEconomia)
    {
        if (sistemaEconomia == null || datosEconomia == null)
        {
            return;
        }

        AjustarMoneda(TipoMoneda.Concha, sistemaEconomia.Conchas, datosEconomia.conchas);
        AjustarMoneda(TipoMoneda.Tulipan, sistemaEconomia.Tulipanes, datosEconomia.tulipanes);
        AjustarMoneda(TipoMoneda.Pinya, sistemaEconomia.Pinyas, datosEconomia.pinyas);
    }

    private void AjustarMoneda(TipoMoneda tipoMoneda, int cantidadActual, int cantidadObjetivo)
    {
        cantidadObjetivo = Mathf.Max(0, cantidadObjetivo);
        int diferencia = cantidadObjetivo - cantidadActual;

        if (diferencia > 0)
        {
            sistemaEconomia.AnadirMoneda(tipoMoneda, diferencia);
        }
        else if (diferencia < 0)
        {
            sistemaEconomia.GastarMoneda(tipoMoneda, -diferencia);
        }
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (jugador == null)
        {
            JugadorActivador jugadorActivador = FindFirstObjectByType<JugadorActivador>();
            if (jugadorActivador != null)
            {
                jugador = jugadorActivador.transform;
            }
        }

        if (inventarioJugador == null)
        {
            inventarioJugador = FindFirstObjectByType<PlayerInventory>();
        }

        if (playerHealth == null)
        {
            playerHealth = FindFirstObjectByType<PlayerHealth>();
        }

        if (playerEnergy == null)
        {
            playerEnergy = FindFirstObjectByType<PlayerEnergy>();
        }

        if (sistemaTiempo == null)
        {
            sistemaTiempo = FindFirstObjectByType<GameTimeSystem>();
        }

        if (sistemaEconomia == null)
        {
            sistemaEconomia = FindFirstObjectByType<SistemaEconomia>();
        }

        if (sistemaObjetosBase == null)
        {
            sistemaObjetosBase = SistemaObjetosBase.Instance;
        }
    }
}
