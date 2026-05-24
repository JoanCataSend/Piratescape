using UnityEngine;

public class GemasMagicas : MonoBehaviour
{
    [SerializeField] private ItemData itemGema;

    private PlayerInventory inventario;

    private void Start()
    {
        inventario = FindFirstObjectByType<PlayerInventory>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (inventario == null || itemGema == null)
            return;

        bool recogida = inventario.TryAddItem(itemGema, 1);

        if (!recogida)
        {
            Debug.Log("Inventario lleno");
            return;
        }

        Debug.Log("Gema recogida: " + itemGema.DisplayName);

        gameObject.SetActive(false);
    }
}