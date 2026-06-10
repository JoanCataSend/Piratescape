using UnityEngine;

public sealed class CerdoGemaInteractuable : MonoBehaviour, Interactuable
{
    [Header("Datos")]
    [SerializeField] private string nombreObjeto = "gema";

    [Header("Interaccion")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;
    [SerializeField] private GameObject indicadorInteraccion;

    [Header("Gema")]
    [SerializeField] private Transform gema;
    [SerializeField] private float fuerzaCaida = 1.5f;

    private IActivador jugador;
    private bool activo;
    private bool gemaSoltada;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => activo;

    private void Start()
    {
        jugador = FindFirstObjectByType<JugadorActivador>();

        SetIndicadorVisible(false);
    }

    private void Update()
    {
        if (jugador == null)
        {
            jugador = FindFirstObjectByType<JugadorActivador>();
            return;
        }

        bool nuevoActivo = interactuable && !gemaSoltada && Vector3.Distance(transform.position, jugador.Position) <= rango;

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
        if (!interactuable || gemaSoltada)
        {
            return;
        }

        if (gema == null)
        {
            Debug.LogWarning("CerdoGemaInteractuable: falta asignar la gema.", this);
            return;
        }

        OcultarPrompt();
        SetIndicadorVisible(false);

        SoltarGema();

        gemaSoltada = true;
        interactuable = false;
        activo = false;
    }

    private void SoltarGema()
    {
        gema.SetParent(null, true);

        Rigidbody rb = gema.GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gema.gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = true;
        rb.isKinematic = false;

        Collider col = gema.GetComponent<Collider>();

        if (col == null)
        {
            col = gema.gameObject.AddComponent<BoxCollider>();
        }

        col.enabled = true;

        rb.AddForce(Vector3.down * fuerzaCaida, ForceMode.Impulse);
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
            InteractionUI.Instance.Show(this, "Pulsa E para recoger la " + nombreObjeto);
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