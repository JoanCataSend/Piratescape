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

    private bool mostrandoPrompt;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => EstaDisponible;

    public bool EstaDisponible => interactuable && gameObject.activeInHierarchy;

    public ItemData ItemData => itemData;
    public int Amount => amount;

    public string ObjectId => objectId;
    public string ObjectName => objectName;
    public string InteractionMessage => interactionMessage;

    public void Interactuar()
    {
        // La interacción real la gestiona PlayerPickupController.
        // Este método se deja por compatibilidad con la interfaz Interactuable.
    }

    public bool EstaEnRango(Vector3 posicionJugador)
    {
        if (!EstaDisponible)
        {
            return false;
        }

        return Vector3.Distance(transform.position, posicionJugador) <= rango;
    }

    public void MostrarPrompt(string icono)
    {
        if (!EstaDisponible)
        {
            return;
        }

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Show($"Pulsa {icono} para recoger {objectName}");
        }

        mostrandoPrompt = true;
        SetIndicatorVisible(true);
    }

    public void OcultarPrompt()
    {
        if (mostrandoPrompt && InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide();
        }

        mostrandoPrompt = false;
        SetIndicatorVisible(false);
    }

    public void OnCollected()
    {
        interactuable = false;
        mostrandoPrompt = false;

        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide();
        }

        SetIndicatorVisible(false);

        RecursoGenerado recursoGenerado = GetComponent<RecursoGenerado>();

        if (recursoGenerado != null)
        {
            recursoGenerado.Consumir();
            return;
        }

        gameObject.SetActive(false);
    }

    private void SetIndicatorVisible(bool visible)
    {
        // Aquí puedes activar/desactivar un icono 3D, outline, partícula, etc.
    }
}