using UnityEngine;

public sealed class ObjetoRecogibleInteractuable : MonoBehaviour, Interactuable, ICollectible
{
    [Header("Datos del objeto")]
    [SerializeField] private string objectId = "item_001";
    [SerializeField] private string objectName = "Objeto recogible";
    [SerializeField] private string interactionMessage = "Objeto recogido";

    [Header("Configuracion de interaccion")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;

    [Header("Datos de recogida")]
    [SerializeField] private ItemData itemData;
    [SerializeField] private int amount = 1;

    [Header("Indicador visual")]
    [SerializeField] private GameObject interactionIndicator;

    [Header("Opcional")]
    [SerializeField] private GameObject visualRoot;

    private bool activo;
    private IActivador jugador;
    private PlayerPickupController playerPickupController;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => activo;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    private void Start()
    {
        JugadorActivador jugadorActivador = FindFirstObjectByType<JugadorActivador>();

        if (jugadorActivador == null)
        {
            Debug.LogError("ObjetoRecogibleInteractuable: JugadorActivador not found.", this);
            return;
        }

        jugador = jugadorActivador;
        playerPickupController = jugadorActivador.GetComponent<PlayerPickupController>();

        if (playerPickupController == null)
        {
            Debug.LogError("ObjetoRecogibleInteractuable: PlayerPickupController not found on player.", this);
        }

        SetIndicatorVisible(false);
    }

    private void Update()
    {
        if (jugador == null || !interactuable)
        {
            return;
        }

        bool nuevoEstado = Vector3.Distance(transform.position, jugador.Position) < rango;

        if (nuevoEstado != activo)
        {
            activo = nuevoEstado;

            if (activo)
            {
                InteractionUI.Instance.Show();
                SetIndicatorVisible(true);
            }
            else
            {
                InteractionUI.Instance.Hide();
                SetIndicatorVisible(false);
            }
        }
    }

    public void Interactuar()
    {
        if (!interactuable)
        {
            return;
        }

        if (!activo)
        {
            return;
        }

        if (playerPickupController == null)
        {
            Debug.LogWarning("ObjetoRecogibleInteractuable: PlayerPickupController is missing.");
            return;
        }

        bool collected = playerPickupController.TryCollect(this);

        if (collected)
        {
            Debug.Log($"{interactionMessage} -> {objectName} (ID: {objectId})");
        }
    }

    public void OnCollected()
    {
        interactuable = false;
        activo = false;

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide();
        }

        SetIndicatorVisible(false);

        if (visualRoot != null)
        {
            visualRoot.SetActive(false);
        }

        gameObject.SetActive(false);
    }

    private void SetIndicatorVisible(bool visible)
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(visible);
        }
    }
}