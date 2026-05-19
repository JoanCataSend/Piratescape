using UnityEngine;
using UnityEngine.UI;

public sealed class SlotCofreUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private InventorySlotUI slotVisual;
    [SerializeField] private Button boton;

    private CofreInventarioUI cofreInventarioUI;
    private bool esSlotJugador;
    private int indiceSlot;

    private void Awake()
    {
        BuscarReferencias();
        PrepararBoton();
    }

    private void OnDestroy()
    {
        if (boton != null)
        {
            boton.onClick.RemoveListener(AlHacerClick);
        }
    }

    public void Configurar(CofreInventarioUI nuevaUI, bool nuevoEsSlotJugador, int nuevoIndiceSlot)
    {
        cofreInventarioUI = nuevaUI;
        esSlotJugador = nuevoEsSlotJugador;
        indiceSlot = nuevoIndiceSlot;

        BuscarReferencias();
        PrepararBoton();
    }

    public void Refrescar(InventorySlot slot)
    {
        BuscarReferencias();

        if (slotVisual == null)
        {
            return;
        }

        if (slot == null || slot.IsEmpty())
        {
            slotVisual.SetEmpty();
            return;
        }

        slotVisual.SetSlot(slot.itemData, slot.amount);
    }

    public void MarcarSeleccionado(bool seleccionado)
    {
        BuscarReferencias();

        if (slotVisual != null)
        {
            slotVisual.SetSelected(seleccionado);
        }
    }

    private void BuscarReferencias()
    {
        if (slotVisual == null)
        {
            slotVisual = GetComponent<InventorySlotUI>();
        }

        if (boton == null)
        {
            boton = GetComponent<Button>();
        }

        if (boton == null)
        {
            boton = gameObject.AddComponent<Button>();
        }
    }

    private void PrepararBoton()
    {
        if (boton == null)
        {
            return;
        }

        boton.onClick.RemoveListener(AlHacerClick);
        boton.onClick.AddListener(AlHacerClick);
    }

    private void AlHacerClick()
    {
        if (cofreInventarioUI == null)
        {
            return;
        }

        cofreInventarioUI.AlPulsarSlot(esSlotJugador, indiceSlot);
    }
}
