using UnityEngine;

/// <summary>
/// Objeto interactuable genérico reutilizable para cualquier asset.
/// </summary>
public class ObjetoInteractuableUniversal : MonoBehaviour, Interactuable
{
    [Header("Datos del objeto")]
    [SerializeField] private string objectId = "objeto_001";
    [SerializeField] private string objectName = "Objeto interactuable";
    [SerializeField] private string interactionMessage = "Interacción realizada.";

    [Header("Configuración")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;

    [Header("Indicador visual")]
    [SerializeField] private GameObject interactionIndicator;

    private bool activo;
    private IActivador jugador;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => activo;

    private void Start()
    {
        jugador = FindFirstObjectByType<JugadorActivador>();

        SetIndicatorVisible(false);
    }

    private void Update()
    {
        if (jugador == null) return;

        bool nuevoEstado = interactuable &&
            Vector3.Distance(transform.position, jugador.Position) < rango;

        if (nuevoEstado != activo)
        {
            activo = nuevoEstado;

            if (activo)
                InteractionUI.Instance.Show();
            else
                InteractionUI.Instance.Hide();
        }
    }

    public void Interactuar()
    {
        if (!interactuable)
        {
            return;
        }

        Debug.Log($"{interactionMessage} -> {objectName} (ID: {objectId})");
    }

    private void SetIndicatorVisible(bool visible)
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(visible);
        }
    }
}