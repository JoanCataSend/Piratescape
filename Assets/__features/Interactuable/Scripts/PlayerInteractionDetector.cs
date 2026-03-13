using UnityEngine;

/// <summary>
/// Detecta objetos interactuables cercanos al jugador.
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerInteractionDetector : MonoBehaviour
{
    private InteractableObject currentInteractable;

    private void Update()
    {
        if (currentInteractable == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentInteractable.Interact();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractableObject interactableObject = other.GetComponent<InteractableObject>();

        if (interactableObject == null)
        {
            return;
        }

        currentInteractable = interactableObject;
        currentInteractable.OnPlayerEnterRange();
    }

    private void OnTriggerExit(Collider other)
    {
        InteractableObject interactableObject = other.GetComponent<InteractableObject>();

        if (interactableObject == null)
        {
            return;
        }

        if (interactableObject == currentInteractable)
        {
            currentInteractable.OnPlayerExitRange();
            currentInteractable = null;
        }
    }
}