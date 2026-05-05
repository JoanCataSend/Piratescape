using System;
using UnityEngine;

public class SistemaEspantamonos : MonoBehaviour
{
    public static SistemaEspantamonos Instance { get; private set; }

    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;

    [Header("Configuracion")]
    [SerializeField] private int diasHastaRomperse = 2;
    [SerializeField] private string mensajeRotura = "El espantamonos se ha roto";

    private GameObject espantamonosActual;
    private int diaColocado = -1;
    private bool estaActivo;

    public bool EstaActivo => estaActivo;

    public event Action<bool> OnEstadoCambiado;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (timeSystem == null)
        {
            timeSystem = FindFirstObjectByType<GameTimeSystem>();
        }
    }

    private void OnEnable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnDayChanged += RevisarDias;
        }
    }

    private void OnDisable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnDayChanged -= RevisarDias;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool ComprarEspantamonos(GameObject prefabEspantamonos, Transform puntoAparicion)
    {
        if (estaActivo)
        {
            return false;
        }

        if (prefabEspantamonos == null || puntoAparicion == null)
        {
            Debug.LogWarning("SistemaEspantamonos: falta prefab o punto de aparicion.");
            return false;
        }

        espantamonosActual = Instantiate(
            prefabEspantamonos,
            puntoAparicion.position,
            puntoAparicion.rotation
        );

        diaColocado = timeSystem != null ? timeSystem.CurrentDay : 1;
        estaActivo = true;

        OnEstadoCambiado?.Invoke(estaActivo);

        Debug.Log("Espantamonos colocado en la base.");

        return true;
    }

    private void RevisarDias(int diaActual)
    {
        if (!estaActivo)
        {
            return;
        }

        int diasPasados = diaActual - diaColocado;

        if (diasPasados >= diasHastaRomperse)
        {
            RomperEspantamonos();
        }
    }

    private void RomperEspantamonos()
    {
        if (espantamonosActual != null)
        {
            Destroy(espantamonosActual);
        }

        espantamonosActual = null;
        diaColocado = -1;
        estaActivo = false;

        MostrarMensaje(mensajeRotura);

        OnEstadoCambiado?.Invoke(estaActivo);

        Debug.Log(mensajeRotura);
    }

    private void MostrarMensaje(string texto)
    {
        if (NightMessageUI.Instance != null)
        {
            NightMessageUI.Instance.ShowMessage(texto);
        }
        else
        {
            Debug.Log(texto);
        }
    }

    [ContextMenu("Debug/Romper Espantamonos")]
    private void DebugRomperEspantamonos()
    {
        if (estaActivo)
        {
            RomperEspantamonos();
        }
    }
}