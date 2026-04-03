using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private float interactDistance = 3f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(transform.position, transform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            {
                ShelterSleep shelter = hit.collider.GetComponent<ShelterSleep>();

                if (shelter != null)
                {
                    shelter.TrySleep();
                }
            }
        }
    }
}