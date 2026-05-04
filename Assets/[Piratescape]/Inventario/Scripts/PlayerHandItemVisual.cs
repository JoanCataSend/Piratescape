using UnityEngine;

public sealed class PlayerHandItemVisual : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform handItemPoint;

    private GameObject currentVisual;
    private ItemData currentItemData;
    private int currentSelectedSlotIndex = -1;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += RefreshHandVisual;
        }

        RefreshHandVisual();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshHandVisual;
        }
    }

    private void Update()
    {
        if (playerInventory == null)
        {
            return;
        }

        if (currentSelectedSlotIndex != playerInventory.SelectedSlotIndex)
        {
            RefreshHandVisual();
        }
    }

    private void RefreshHandVisual()
    {
        if (playerInventory == null || handItemPoint == null)
        {
            ClearCurrentVisual();
            return;
        }

        currentSelectedSlotIndex = playerInventory.SelectedSlotIndex;

        InventorySlot selectedSlot = playerInventory.GetSlot(currentSelectedSlotIndex);

        if (selectedSlot == null || selectedSlot.IsEmpty() || selectedSlot.itemData == null)
        {
            currentItemData = null;
            ClearCurrentVisual();
            return;
        }

        ItemData selectedItem = selectedSlot.itemData;

        if (selectedItem == currentItemData && currentVisual != null)
        {
            return;
        }

        currentItemData = selectedItem;
        ClearCurrentVisual();

        if (selectedItem.HandPrefab == null)
        {
            Debug.LogWarning("El item " + selectedItem.DisplayName + " no tiene Hand Prefab asignado.", selectedItem);
            return;
        }

        currentVisual = Instantiate(selectedItem.HandPrefab, handItemPoint);

        currentVisual.transform.localPosition = selectedItem.HandLocalPosition;
        currentVisual.transform.localEulerAngles = selectedItem.HandLocalRotation;
        currentVisual.transform.localScale = selectedItem.HandLocalScale;

        DisableInteraction(currentVisual);
        DisablePhysics(currentVisual);
    }

    private void ClearCurrentVisual()
    {
        if (currentVisual != null)
        {
            Destroy(currentVisual);
            currentVisual = null;
        }
    }

    private void DisableInteraction(GameObject visual)
    {
        ObjetoRecogibleInteractuable[] recogibles = visual.GetComponentsInChildren<ObjetoRecogibleInteractuable>(true);

        for (int i = 0; i < recogibles.Length; i++)
        {
            recogibles[i].OcultarPrompt();
            recogibles[i].enabled = false;
        }
    }

    private void DisablePhysics(GameObject visual)
    {
        Collider[] colliders = visual.GetComponentsInChildren<Collider>(true);

        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = false;
        }

        Rigidbody[] rigidbodies = visual.GetComponentsInChildren<Rigidbody>(true);

        for (int i = 0; i < rigidbodies.Length; i++)
        {
            rigidbodies[i].isKinematic = true;
            rigidbodies[i].useGravity = false;
        }
    }
}