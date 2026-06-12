using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public sealed class PlayerInventory : MonoBehaviour, IItemReceiver
{
    public event Action OnInventoryChanged;
    public event Action<string> OnInventoryMessageRequested;

    [Header("Configuracion del inventario")]
    [SerializeField] private int inventorySize = 5;

    [Header("Seleccion")]
    [SerializeField] private int selectedSlotIndex = 0;

    [Header("Drop")]
    [SerializeField] private Transform dropPoint;
    [SerializeField] private float dropDistance = 1.5f;
    [SerializeField] private float dropHeight = 1.1f;
    [SerializeField] private float throwForceForward = 4f;
    [SerializeField] private float throwForceUp = 1.5f;

    [Header("Animación")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private string dropTriggerName = "Drop";
    [SerializeField] private float dropDelayBeforeSpawn = 0.35f;

    [Header("Sonidos inventario")]
    [SerializeField] private AudioSource audioSourceInventario;
    [SerializeField] private AudioMixerGroup outputInventario;
    [SerializeField] private AudioClip sonidoCambiarSlot;
    [SerializeField] private AudioClip sonidoConsumirItem;
    [SerializeField] private float volumenCambiarSlot = 0.45f;
    [SerializeField] private float volumenConsumirItem = 0.6f;
    [SerializeField] private AudioClip sonidoSoltarItem;
    [SerializeField] private float volumenSoltarItem = 0.45f;

    private List<InventorySlot> slots = new List<InventorySlot>();
    private PlayerHealth playerHealth;
    private PlayerEnergy playerEnergy;
    private bool isDropping;
    private int frameInputBloqueado = -1;

    public int SelectedSlotIndex => selectedSlotIndex;

    private void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerEnergy = GetComponent<PlayerEnergy>();

        if (playerAnimator == null)
        {
            playerAnimator = GetComponentInChildren<Animator>();
        }

        InitializeSlots();

        PrepararAudioInventario();
    }

    private void Update()
    {
        if (DebeBloquearInputInventario())
        {
            return;
        }

        HandleKeyboardInput();
        HandleGamepadInput();
    }

    public void BloquearInputUnFrame()
    {
        frameInputBloqueado = Time.frameCount;
    }

    private bool DebeBloquearInputInventario()
    {
        if (Time.frameCount == frameInputBloqueado)
        {
            return true;
        }

        if (Time.timeScale == 0f)
        {
            return true;
        }

        if (CofreInventarioUI.HayAlgunaUIAbierta)
        {
            return true;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return true;
        }

        return false;
    }

    private void InitializeSlots()
    {
        slots.Clear();

        for (int i = 0; i < inventorySize; i++)
        {
            slots.Add(new InventorySlot());
        }

        if (selectedSlotIndex < 0)
        {
            selectedSlotIndex = 0;
        }

        if (selectedSlotIndex >= slots.Count)
        {
            selectedSlotIndex = slots.Count - 1;
        }
    }

    private void HandleKeyboardInput()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) SelectSlot(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SelectSlot(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) SelectSlot(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) SelectSlot(3);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) SelectSlot(4);

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                DropSelectedItem();
            }
        }

        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                UseSelectedItem();
            }

            float scroll = -Mouse.current.scroll.ReadValue().y;

            if (scroll > 0f)
            {
                SelectNextSlot();
            }
            else if (scroll < 0f)
            {
                SelectPreviousSlot();
            }
        }
    }

    private void HandleGamepadInput()
    {
        if (Gamepad.current == null)
        {
            return;
        }

        if (Gamepad.current.leftShoulder.wasPressedThisFrame)
        {
            SelectPreviousSlot();
        }

        if (Gamepad.current.rightShoulder.wasPressedThisFrame)
        {
            SelectNextSlot();
        }

        if (Gamepad.current.buttonNorth.wasPressedThisFrame)
        {
            UseSelectedItem();
        }

        if (Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            DropSelectedItem();
        }
    }

    public bool CanAddItem(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            return false;
        }

        int freeSpaceTotal = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }

            if (!slots[i].IsEmpty() && slots[i].itemData == itemData && slots[i].amount < InventorySlot.DefaultMaxStack)
            {
                freeSpaceTotal += InventorySlot.DefaultMaxStack - slots[i].amount;
            }
            else if (slots[i].IsEmpty())
            {
                freeSpaceTotal += InventorySlot.DefaultMaxStack;
            }
        }

        return freeSpaceTotal >= amount;
    }

    public bool TryAddItem(ItemData itemData, int amount)
    {
        if (itemData == null)
        {
            Debug.LogWarning("PlayerInventory: itemData es null.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning("PlayerInventory: amount debe ser mayor que 0.");
            return false;
        }

        if (!CanAddItem(itemData, amount))
        {
            OnInventoryMessageRequested?.Invoke("¡Oh no! Mis bolsillos están llenos");
            return false;
        }

        int remainingAmount = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }

            if (!slots[i].IsEmpty() && slots[i].itemData == itemData && slots[i].amount < InventorySlot.DefaultMaxStack)
            {
                int freeSpace = InventorySlot.DefaultMaxStack - slots[i].amount;
                int amountToAdd = Mathf.Min(remainingAmount, freeSpace);

                slots[i].amount += amountToAdd;
                remainingAmount -= amountToAdd;

                if (remainingAmount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] == null)
            {
                continue;
            }

            if (slots[i].IsEmpty())
            {
                int amountToAdd = Mathf.Min(remainingAmount, InventorySlot.DefaultMaxStack);

                slots[i].itemData = itemData;
                slots[i].amount = amountToAdd;
                remainingAmount -= amountToAdd;

                if (remainingAmount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        NotifyInventoryChanged();
        return false;
    }

    public bool RemoveItem(ItemData itemData, int amount = 1)
    {
        if (itemData == null || amount <= 0)
        {
            return false;
        }

        int remainingAmount = amount;

        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].itemData == itemData)
            {
                int amountToRemove = Mathf.Min(remainingAmount, slots[i].amount);
                slots[i].amount -= amountToRemove;
                remainingAmount -= amountToRemove;

                if (slots[i].amount <= 0)
                {
                    slots[i].Clear();
                }

                if (remainingAmount <= 0)
                {
                    NotifyInventoryChanged();
                    return true;
                }
            }
        }

        NotifyInventoryChanged();
        return remainingAmount <= 0;
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return;
        }

        if (!SlotTieneItem(index))
        {
            return;
        }

        if (selectedSlotIndex == index)
        {
            return;
        }

        selectedSlotIndex = index;
        ReproducirSonidoInventario(sonidoCambiarSlot, volumenCambiarSlot);
        NotifyInventoryChanged();
    }

    public void SelectNextSlot()
    {
        if (slots == null || slots.Count == 0)
        {
            return;
        }

        int nuevoIndice = BuscarSiguienteSlotConItem(selectedSlotIndex, 1);

        if (nuevoIndice == -1)
        {
            return;
        }

        if (nuevoIndice == selectedSlotIndex)
        {
            return;
        }

        selectedSlotIndex = nuevoIndice;
        ReproducirSonidoInventario(sonidoCambiarSlot, volumenCambiarSlot);
        NotifyInventoryChanged();
    }

    public void SelectPreviousSlot()
    {
        if (slots == null || slots.Count == 0)
        {
            return;
        }

        int nuevoIndice = BuscarSiguienteSlotConItem(selectedSlotIndex, -1);

        if (nuevoIndice == -1)
        {
            return;
        }

        if (nuevoIndice == selectedSlotIndex)
        {
            return;
        }

        selectedSlotIndex = nuevoIndice;
        ReproducirSonidoInventario(sonidoCambiarSlot, volumenCambiarSlot);
        NotifyInventoryChanged();
    }

    public void UseSelectedItem()
    {
        InventorySlot slot = GetSlot(selectedSlotIndex);

        if (slot == null || slot.IsEmpty())
        {
            Debug.Log("No hay item en el slot seleccionado.");
            return;
        }

        ItemData item = slot.itemData;
        ConsumibleItemData consumibleItem = item as ConsumibleItemData;

        if (consumibleItem == null)
        {
            Debug.Log(item.DisplayName + " no es consumible.");
            return;
        }

        bool used = ConsumeItem(consumibleItem);

        if (used)
        {
            slot.amount--;

            if (slot.amount <= 0)
            {
                slot.Clear();
            }

            ReproducirSonidoInventario(sonidoConsumirItem, volumenConsumirItem);
            Debug.Log("Consumido: " + item.DisplayName);
            NotifyInventoryChanged();
        }
    }

    public void DropSelectedItem()
    {
        if (isDropping)
        {
            return;
        }

        InventorySlot slot = GetSlot(selectedSlotIndex);

        if (slot == null || slot.IsEmpty())
        {
            Debug.Log("No hay item para tirar.");
            return;
        }

        ItemData item = slot.itemData;

        if (item.WorldPrefab == null)
        {
            Debug.LogWarning("El item " + item.DisplayName + " no tiene WorldPrefab asignado.");
            return;
        }

        StartCoroutine(DropSelectedItemRoutine());
    }

    private IEnumerator DropSelectedItemRoutine()
    {
        isDropping = true;

        InventorySlot slot = GetSlot(selectedSlotIndex);

        if (slot == null || slot.IsEmpty())
        {
            isDropping = false;
            yield break;
        }

        ItemData item = slot.itemData;

        if (item == null || item.WorldPrefab == null)
        {
            isDropping = false;
            yield break;
        }

        if (playerAnimator != null && !string.IsNullOrWhiteSpace(dropTriggerName))
        {
            playerAnimator.SetTrigger(dropTriggerName);
        }

        if (dropDelayBeforeSpawn > 0f)
        {
            yield return new WaitForSeconds(dropDelayBeforeSpawn);
        }

        Vector3 spawnPosition = GetDropPosition();
        Quaternion spawnRotation = Quaternion.identity;

        GameObject droppedObject = Instantiate(
            item.WorldPrefab,
            spawnPosition,
            spawnRotation
        );

        Rigidbody rb = droppedObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            Vector3 fuerzaLanzamiento =
                transform.forward * throwForceForward +
                Vector3.up * throwForceUp;

            rb.AddForce(fuerzaLanzamiento, ForceMode.Impulse);
        }

        slot.amount--;

        if (slot.amount <= 0)
        {
            slot.Clear();
        }

        ReproducirSonidoInventario(sonidoSoltarItem, volumenSoltarItem);

        Debug.Log("Tirado al suelo: " + item.DisplayName);
        NotifyInventoryChanged();

        isDropping = false;
    }

    private Vector3 GetDropPosition()
    {
        if (dropPoint != null)
        {
            return dropPoint.position;
        }

        return transform.position
            + transform.forward * dropDistance
            + Vector3.up * dropHeight;
    }

    private bool ConsumeItem(ConsumibleItemData item)
    {
        if (item == null)
        {
            return false;
        }

        bool usedSomething = false;

        if (item.HealthRestore > 0)
        {
            if (playerHealth != null)
            {
                playerHealth.Heal(item.HealthRestore);
                usedSomething = true;
            }
            else
            {
                Debug.LogWarning("No hay PlayerHealth en el jugador.");
            }
        }

        if (item.EnergyRestore > 0)
        {
            if (playerEnergy != null)
            {
                playerEnergy.RestoreEnergy(item.EnergyRestore);
                usedSomething = true;
            }
            else
            {
                Debug.LogWarning("No hay PlayerEnergy en el jugador.");
            }
        }

        return usedSomething;
    }

    private bool SlotTieneItem(int index)
    {
        InventorySlot slot = GetSlot(index);
        return slot != null && !slot.IsEmpty();
    }

    private int BuscarPrimerSlotConItem()
    {
        if (slots == null)
        {
            return -1;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            if (SlotTieneItem(i))
            {
                return i;
            }
        }

        return -1;
    }

    private int BuscarSiguienteSlotConItem(int indiceActual, int direccion)
    {
        if (slots == null || slots.Count == 0)
        {
            return -1;
        }

        for (int i = 1; i <= slots.Count; i++)
        {
            int nuevoIndice = indiceActual + direccion * i;

            if (nuevoIndice >= slots.Count)
            {
                nuevoIndice = nuevoIndice % slots.Count;
            }

            while (nuevoIndice < 0)
            {
                nuevoIndice += slots.Count;
            }

            if (SlotTieneItem(nuevoIndice))
            {
                return nuevoIndice;
            }
        }

        return -1;
    }

    private void AsegurarSeleccionValida()
    {
        if (slots == null || slots.Count == 0)
        {
            selectedSlotIndex = 0;
            return;
        }

        if (selectedSlotIndex < 0 || selectedSlotIndex >= slots.Count)
        {
            selectedSlotIndex = 0;
        }

        if (SlotTieneItem(selectedSlotIndex))
        {
            return;
        }

        int primerSlotConItem = BuscarPrimerSlotConItem();

        if (primerSlotConItem != -1)
        {
            selectedSlotIndex = primerSlotConItem;
        }
    }

    public List<InventorySlot> GetSlots()
    {
        return slots;
    }

    public InventorySlot GetSlot(int index)
    {
        if (index < 0 || index >= slots.Count)
        {
            return null;
        }

        return slots[index];
    }

    public void NotifyInventoryChanged()
    {
        AsegurarSeleccionValida();
        OnInventoryChanged?.Invoke();
    }

    public void ForceUpdateUI()
    {
        NotifyInventoryChanged();
    }

    public int ObtenerCantidad(ItemData itemData)
    {
        if (itemData == null)
        {
            return 0;
        }

        int cantidadTotal = 0;

        for (int i = 0; i < slots.Count; i++)
        {
            InventorySlot slot = slots[i];

            if (slot == null || slot.IsEmpty())
            {
                continue;
            }

            if (slot.itemData != itemData)
            {
                continue;
            }

            cantidadTotal += slot.amount;
        }

        return cantidadTotal;
    }

    public int RemoverHasta(ItemData itemData, int cantidadSolicitada)
    {
        if (itemData == null || cantidadSolicitada <= 0)
        {
            return 0;
        }

        int cantidadDisponible = ObtenerCantidad(itemData);
        int cantidadARemover = Mathf.Min(cantidadDisponible, cantidadSolicitada);

        if (cantidadARemover <= 0)
        {
            return 0;
        }

        RemoveItem(itemData, cantidadARemover);
        return cantidadARemover;
    }

    private void PrepararAudioInventario()
    {
        if (audioSourceInventario == null)
        {
            audioSourceInventario = gameObject.AddComponent<AudioSource>();
        }

        audioSourceInventario.playOnAwake = false;
        audioSourceInventario.loop = false;
        audioSourceInventario.spatialBlend = 0f;
        audioSourceInventario.outputAudioMixerGroup = outputInventario;
    }

    private void ReproducirSonidoInventario(AudioClip clip, float volumen)
    {
        if (audioSourceInventario == null || clip == null)
        {
            return;
        }

        audioSourceInventario.pitch = 1f;
        audioSourceInventario.PlayOneShot(clip, volumen);
    }
}