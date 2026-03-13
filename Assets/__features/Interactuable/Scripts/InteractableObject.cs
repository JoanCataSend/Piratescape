using UnityEngine;

/// <summary>
/// Define el comportamiento base de un objeto interactuable.
/// </summary>
public class InteractableObject : MonoBehaviour
{
    [Header("Datos del objeto")]
    [SerializeField] private string objectId = "objeto_001";
    [SerializeField] private string objectName = "Objeto interactuable";
    [SerializeField] private bool isInteractable = true;

    [Header("Indicador visual")]
    [SerializeField] private GameObject interactionIndicator;

    private bool isPlayerInRange = false;

    private void Start()
    {
        SetIndicatorVisible(false);
    }

    /// <summary>
    /// Se llama cuando el jugador entra en rango.
    /// </summary>
    public void OnPlayerEnterRange()
    {
        isPlayerInRange = true;

        if (isInteractable)
        {
            SetIndicatorVisible(true);
        }
    }

    /// <summary>
    /// Se llama cuando el jugador sale del rango.
    /// </summary>
    public void OnPlayerExitRange()
    {
        isPlayerInRange = false;
        SetIndicatorVisible(false);
    }

    /// <summary>
    /// Ejecuta la interacción principal del objeto.
    /// </summary>
    public void Interact()
    {
        if (!isInteractable)
        {
            Debug.Log($"El objeto '{objectName}' no está disponible.");
            return;
        }

        if (!isPlayerInRange)
        {
            Debug.Log($"El jugador no está dentro del rango de '{objectName}'.");
            return;
        }

        Debug.Log($"Interacción realizada con: {objectName} (ID: {objectId})");
    }

    private void SetIndicatorVisible(bool isVisible)
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(isVisible);
        }
    }
}