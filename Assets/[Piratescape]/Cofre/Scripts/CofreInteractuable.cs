using UnityEngine;

public sealed class CofreInteractuable : MonoBehaviour, Interactuable
{
    [Header("Datos")]
    [SerializeField] private string nombreObjeto = "cofre";

    [Header("Interaccion")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;
    [SerializeField] private GameObject indicadorInteraccion;

    [Header("Inventario")]
    [SerializeField] private InventarioCofre inventarioCofre;
    [SerializeField] private CofreInventarioUI cofreInventarioUI;

    private IActivador jugador;
    private bool activo;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => activo;

    private void Awake()
    {
        if (inventarioCofre == null)
        {
            inventarioCofre = GetComponent<InventarioCofre>();
        }
    }

    private void Start()
    {
        jugador = FindFirstObjectByType<JugadorActivador>();

        if (cofreInventarioUI == null)
        {
            cofreInventarioUI = FindFirstObjectByType<CofreInventarioUI>();
        }

        SetIndicadorVisible(false);
    }

    private void Update()
    {
        if (jugador == null)
        {
            jugador = FindFirstObjectByType<JugadorActivador>();
            return;
        }

        if (cofreInventarioUI == null)
        {
            cofreInventarioUI = FindFirstObjectByType<CofreInventarioUI>();
        }

        bool uiAbierta = cofreInventarioUI != null && cofreInventarioUI.EstaAbierto;
        bool nuevoActivo = interactuable && !uiAbierta && Vector3.Distance(transform.position, jugador.Position) <= rango;

        if (nuevoActivo != activo)
        {
            activo = nuevoActivo;
            ActualizarPrompt();
        }
    }

    private void OnDisable()
    {
        OcultarPrompt();
        SetIndicadorVisible(false);
    }

    public void Interactuar()
    {
        if (!interactuable)
        {
            return;
        }

        if (inventarioCofre == null)
        {
            Debug.LogWarning("CofreInteractuable: falta InventarioCofre.", this);
            return;
        }

        if (cofreInventarioUI == null)
        {
            cofreInventarioUI = FindFirstObjectByType<CofreInventarioUI>();
        }

        if (cofreInventarioUI == null)
        {
            Debug.LogWarning("CofreInteractuable: falta CofreInventarioUI en la escena.", this);
            return;
        }

        OcultarPrompt();
        SetIndicadorVisible(false);
        cofreInventarioUI.Abrir(inventarioCofre);
    }

    private void ActualizarPrompt()
    {
        if (activo)
        {
            MostrarPrompt();
        }
        else
        {
            OcultarPrompt();
        }

        SetIndicadorVisible(activo);
    }

    private void MostrarPrompt()
    {
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Show(this, "Pulsa E para abrir " + nombreObjeto);
        }
    }

    private void OcultarPrompt()
    {
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide(this);
        }
    }

    private void SetIndicadorVisible(bool visible)
    {
        if (indicadorInteraccion != null)
        {
            indicadorInteraccion.SetActive(visible);
        }
    }
}
