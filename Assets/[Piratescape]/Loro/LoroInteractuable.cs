using UnityEngine;

public sealed class LoroInteractuable : MonoBehaviour, Interactuable
{
    [Header("Datos")]
    [SerializeField] private string nombreObjeto = "loro";

    [Header("Interaccion")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;
    [SerializeField] private GameObject indicadorInteraccion;

    [Header("UI")]
    [SerializeField] private LoroDialogoUI loroDialogoUI;

    private IActivador jugador;
    private bool activo;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => activo;

    private void Start()
    {
        jugador = FindFirstObjectByType<JugadorActivador>();

        if (loroDialogoUI == null)
        {
            loroDialogoUI = FindFirstObjectByType<LoroDialogoUI>();
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

        if (loroDialogoUI == null)
        {
            loroDialogoUI = FindFirstObjectByType<LoroDialogoUI>();
        }

        bool hayUIBloqueante = HayUIBloqueanteAbierta();
        bool nuevoActivo = interactuable && !hayUIBloqueante && Vector3.Distance(transform.position, jugador.Position) <= rango;

        if (nuevoActivo != activo)
        {
            activo = nuevoActivo;
            ActualizarPrompt();
        }
    }

    private void OnDisable()
    {
        activo = false;
        OcultarPrompt();
        SetIndicadorVisible(false);
    }

    public void Interactuar()
    {
        if (!interactuable || HayUIBloqueanteAbierta())
        {
            return;
        }

        if (loroDialogoUI == null)
        {
            loroDialogoUI = FindFirstObjectByType<LoroDialogoUI>();
        }

        if (loroDialogoUI == null)
        {
            Debug.LogWarning("LoroInteractuable: falta LoroDialogoUI en la escena.", this);
            return;
        }

        OcultarPrompt();
        SetIndicadorVisible(false);
        loroDialogoUI.AbrirMenuLoro();
    }

    private bool HayUIBloqueanteAbierta()
    {
        bool cofreAbierto = CofreInventarioUI.HayAlgunaUIAbierta;
        bool loroMenuAbierto = LoroDialogoUI.HayAlgunaUIAbierta;
        bool historiaAbierta = HistoriaInicioLoroUI.HayAlgunaUIAbierta;

        return cofreAbierto || loroMenuAbierto || historiaAbierta;
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
            InteractionUI.Instance.Show(this, "Pulsa E para hablar con " + nombreObjeto);
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
