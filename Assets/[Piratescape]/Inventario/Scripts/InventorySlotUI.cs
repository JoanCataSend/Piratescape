using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text amountText;

    [Header("Fondo / Seleccion")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private bool mostrarFondoCuandoEstaVacio = true;
    [SerializeField, Range(0f, 1f)] private float alphaFondoNormal = 0.35f;
    [SerializeField, Range(0f, 1f)] private float alphaFondoSeleccionado = 1f;

    private Color colorFondoOriginal = Color.white;
    private bool colorFondoCacheado;

    private void Awake()
    {
        CachearColorFondo();
    }

    public void SetEmpty()
    {
        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }

        if (amountText != null)
        {
            amountText.text = "";
        }

        ActualizarFondo(false);
    }

    public void SetSlot(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            SetEmpty();
            return;
        }

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = itemData.Icon;
        }

        if (amountText != null)
        {
            amountText.text = amount + "/" + InventorySlot.MaxStack;
        }

        ActualizarFondo(false);
    }

    public void SetSelected(bool selected)
    {
        ActualizarFondo(selected);
    }

    private void ActualizarFondo(bool selected)
    {
        if (backgroundImage == null)
        {
            return;
        }

        CachearColorFondo();

        backgroundImage.enabled = mostrarFondoCuandoEstaVacio || selected;

        Color color = colorFondoOriginal;
        color.a = selected ? alphaFondoSeleccionado : alphaFondoNormal;
        backgroundImage.color = color;
    }

    private void CachearColorFondo()
    {
        if (colorFondoCacheado || backgroundImage == null)
        {
            return;
        }

        colorFondoOriginal = backgroundImage.color;
        colorFondoCacheado = true;
    }
}
