using UnityEngine;

public sealed class InventoryStatsPreview : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private VisualizadorBarrasEstado visualizadorBarras;

    [Header("Iconos con palpito")]
    [SerializeField] private RectTransform iconoSalud;
    [SerializeField] private RectTransform iconoEnergia;

    [SerializeField] private float escalaPalpito = 1.18f;
    [SerializeField] private float velocidadPalpito = 5f;

    private Vector3 escalaOriginalSalud;
    private Vector3 escalaOriginalEnergia;

    private bool palpitarSalud;
    private bool palpitarEnergia;

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

        if (iconoSalud != null)
        {
            escalaOriginalSalud = iconoSalud.localScale;
        }

        if (iconoEnergia != null)
        {
            escalaOriginalEnergia = iconoEnergia.localScale;
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

        palpitarSalud = false;
        palpitarEnergia = false;
        ResetearIconos();
    }

    private void Update()
    {
        ActualizarPreview();
        ActualizarPalpitoIconos();
    }

    private void ActualizarPreview()
    {
        palpitarSalud = false;
        palpitarEnergia = false;

        if (playerInventory == null || visualizadorBarras == null)
        {
            ResetearIconos();
            return;
        }

        InventorySlot slot = playerInventory.GetSlot(playerInventory.SelectedSlotIndex);

        if (slot == null || slot.IsEmpty())
        {
            visualizadorBarras.OcultarPreview();
            ResetearIconos();
            return;
        }

        ConsumibleItemData consumible = slot.itemData as ConsumibleItemData;

        if (consumible == null)
        {
            visualizadorBarras.OcultarPreview();
            ResetearIconos();
            return;
        }

        bool tienePreview = false;

        if (consumible.HealthRestore > 0 && playerHealth != null)
        {
            float saludPreview = playerHealth.CurrentHealth + consumible.HealthRestore;
            visualizadorBarras.MostrarPreviewSalud(saludPreview);
            tienePreview = true;
            palpitarSalud = true;
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
            palpitarEnergia = true;
        }
        else
        {
            visualizadorBarras.MostrarPreviewEnergia(0f);
        }

        if (!tienePreview)
        {
            visualizadorBarras.OcultarPreview();
            ResetearIconos();
        }
    }

    private void ActualizarPalpitoIconos()
    {
        if (iconoSalud != null)
        {
            if (palpitarSalud)
            {
                float t = (Mathf.Sin(Time.unscaledTime * velocidadPalpito) + 1f) * 0.5f;
                float escala = Mathf.Lerp(1f, escalaPalpito, t);
                iconoSalud.localScale = escalaOriginalSalud * escala;
            }
            else
            {
                iconoSalud.localScale = Vector3.Lerp(iconoSalud.localScale, escalaOriginalSalud, Time.unscaledDeltaTime * 12f);
            }
        }

        if (iconoEnergia != null)
        {
            if (palpitarEnergia)
            {
                float t = (Mathf.Sin(Time.unscaledTime * velocidadPalpito) + 1f) * 0.5f;
                float escala = Mathf.Lerp(1f, escalaPalpito, t);
                iconoEnergia.localScale = escalaOriginalEnergia * escala;
            }
            else
            {
                iconoEnergia.localScale = Vector3.Lerp(iconoEnergia.localScale, escalaOriginalEnergia, Time.unscaledDeltaTime * 12f);
            }
        }
    }

    private void ResetearIconos()
    {
        if (iconoSalud != null)
        {
            iconoSalud.localScale = escalaOriginalSalud;
        }

        if (iconoEnergia != null)
        {
            iconoEnergia.localScale = escalaOriginalEnergia;
        }
    }
}