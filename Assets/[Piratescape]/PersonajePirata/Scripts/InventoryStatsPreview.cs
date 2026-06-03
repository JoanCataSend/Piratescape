using UnityEngine;

public sealed class InventoryStatsPreview : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private VisualizadorBarrasEstado visualizadorBarras;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }

        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (playerEnergy == null)
        {
            playerEnergy = GetComponent<PlayerEnergy>();
        }

        if (visualizadorBarras == null)
        {
            visualizadorBarras = FindFirstObjectByType<VisualizadorBarrasEstado>();
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += ActualizarPreview;
        }

        ActualizarPreview();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= ActualizarPreview;
        }

        if (visualizadorBarras != null)
        {
            visualizadorBarras.OcultarPreview();
        }
    }

    private void Update()
    {
        ActualizarPreview();
    }

    private void ActualizarPreview()
    {
        if (playerInventory == null || visualizadorBarras == null)
        {
            return;
        }

        InventorySlot slot = playerInventory.GetSlot(playerInventory.SelectedSlotIndex);

        if (slot == null || slot.IsEmpty())
        {
            visualizadorBarras.OcultarPreview();
            return;
        }

        ConsumibleItemData consumible = slot.itemData as ConsumibleItemData;

        if (consumible == null)
        {
            visualizadorBarras.OcultarPreview();
            return;
        }

        bool tienePreview = false;

        if (consumible.HealthRestore > 0 && playerHealth != null)
        {
            float saludPreview = playerHealth.CurrentHealth + consumible.HealthRestore;
            visualizadorBarras.MostrarPreviewSalud(saludPreview);
            tienePreview = true;
        }
        else
        {
            visualizadorBarras.MostrarPreviewSalud(0f);
        }

        if (consumible.EnergyRestore > 0 && playerEnergy != null)
        {
            float energiaPreview = playerEnergy.CurrentEnergy + consumible.EnergyRestore;
            visualizadorBarras.MostrarPreviewEnergia(energiaPreview);
            tienePreview = true;
        }
        else
        {
            visualizadorBarras.MostrarPreviewEnergia(0f);
        }

        if (!tienePreview)
        {
            visualizadorBarras.OcultarPreview();
        }
    }
}