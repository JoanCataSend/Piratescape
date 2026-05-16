using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class PrecioMonedaUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Image imagenMoneda;
    [SerializeField] private TMP_Text textoCantidad;

    public TMP_Text TextoCantidad => textoCantidad;

    public void Configurar(Sprite icono, int cantidad)
    {
        if (imagenMoneda != null)
        {
            imagenMoneda.sprite = icono;
            imagenMoneda.preserveAspect = true;
        }

        if (textoCantidad != null)
        {
            textoCantidad.text = cantidad.ToString();
        }
    }
}