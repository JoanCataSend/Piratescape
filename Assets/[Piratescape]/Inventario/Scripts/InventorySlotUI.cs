using System.Collections;
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

    [Header("Animacion al consumir")]
    [SerializeField] private bool animarAlBajarCantidad = true;

    [Tooltip("Normalmente el RectTransform del slot entero.")]
    [SerializeField] private RectTransform elementoAAnimar;

    [Tooltip("Normalmente el RectTransform del icono del objeto.")]
    [SerializeField] private RectTransform iconoAAnimar;

    [SerializeField] private float escalaReboteSlot = 1.02f;
    [SerializeField] private float escalaReboteIcono = 1.55f;
    [SerializeField] private float duracionEntrada = 0.16f;
    [SerializeField] private float duracionMantenerGrande = 0.22f;
    [SerializeField] private float duracionSalida = 0.20f;
    [SerializeField] private float gradosGiroIcono = 8f;
    [SerializeField] private Color colorFlashTexto = Color.yellow;
    [SerializeField] private Color colorFlashIcono = Color.white;

    private Color colorFondoOriginal = Color.white;
    private bool colorFondoCacheado;

    private ItemData itemAnterior;
    private int cantidadAnterior;
    private bool tieneDatoAnterior;

    private Vector3 escalaOriginalSlot;
    private Vector3 escalaOriginalIcono;
    private Quaternion rotacionOriginalIcono;
    private Color colorTextoOriginal = Color.white;
    private Color colorIconoOriginal = Color.white;

    private Coroutine rutinaAnimacion;
    private bool tieneItemActual;

    private void Awake()
    {
        CachearColorFondo();

        if (elementoAAnimar == null)
        {
            elementoAAnimar = GetComponent<RectTransform>();
        }

        if (iconoAAnimar == null && iconImage != null)
        {
            iconoAAnimar = iconImage.rectTransform;
        }

        if (elementoAAnimar != null)
        {
            escalaOriginalSlot = elementoAAnimar.localScale;
        }

        if (iconoAAnimar != null)
        {
            escalaOriginalIcono = iconoAAnimar.localScale;
            rotacionOriginalIcono = iconoAAnimar.localRotation;
        }

        if (amountText != null)
        {
            colorTextoOriginal = amountText.color;
        }

        if (iconImage != null)
        {
            colorIconoOriginal = iconImage.color;
        }
    }

    private void OnEnable()
    {
        RestaurarAnimacion(false);
    }

    public void SetEmpty()
    {
        bool debeAnimarConsumo = animarAlBajarCantidad &&
                                 tieneDatoAnterior &&
                                 itemAnterior != null &&
                                 cantidadAnterior > 0;

        tieneItemActual = false;

        if (amountText != null)
        {
            amountText.text = "";
        }

        ActualizarFondo(false);

        if (debeAnimarConsumo)
        {
            LanzarAnimacionConsumo(true);
        }
        else
        {
            VaciarIcono();
        }

        itemAnterior = null;
        cantidadAnterior = 0;
        tieneDatoAnterior = true;
    }

    public void SetSlot(ItemData itemData, int amount)
    {
        if (itemData == null || amount <= 0)
        {
            SetEmpty();
            return;
        }

        bool debeAnimarConsumo = animarAlBajarCantidad &&
                                 tieneDatoAnterior &&
                                 itemAnterior == itemData &&
                                 amount < cantidadAnterior;

        tieneItemActual = true;

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = itemData.Icon;
        }

        if (amountText != null)
        {
            int maxStack = EsGema(itemData) ? 1 : InventorySlot.DefaultMaxStack;
            amountText.text = amount + "/" + maxStack;
        }

        ActualizarFondo(false);

        if (debeAnimarConsumo)
        {
            LanzarAnimacionConsumo(false);
        }

        itemAnterior = itemData;
        cantidadAnterior = amount;
        tieneDatoAnterior = true;
    }

    public void SetSelected(bool selected)
    {
        ActualizarFondo(selected);
    }

    private void LanzarAnimacionConsumo(bool vaciarAlTerminar)
    {
        if (iconImage == null || iconoAAnimar == null)
        {
            if (vaciarAlTerminar)
            {
                VaciarIcono();
            }

            return;
        }

        if (rutinaAnimacion != null)
        {
            StopCoroutine(rutinaAnimacion);
        }

        rutinaAnimacion = StartCoroutine(AnimacionConsumo(vaciarAlTerminar));
    }

    private IEnumerator AnimacionConsumo(bool vaciarAlTerminar)
    {
        RestaurarAnimacion(false);

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.transform.SetAsLastSibling();
            iconImage.color = colorFlashIcono;
        }

        if (amountText != null)
        {
            amountText.color = colorFlashTexto;
        }

        Vector3 escalaGrandeSlot = escalaOriginalSlot * escalaReboteSlot;
        Vector3 escalaGrandeIcono = escalaOriginalIcono * escalaReboteIcono;

        float tiempo = 0f;

        while (tiempo < duracionEntrada)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracionEntrada;
            float suave = Mathf.Sin(t * Mathf.PI * 0.5f);

            if (elementoAAnimar != null)
            {
                elementoAAnimar.localScale = Vector3.Lerp(escalaOriginalSlot, escalaGrandeSlot, suave);
            }

            if (iconoAAnimar != null)
            {
                iconoAAnimar.localScale = Vector3.Lerp(escalaOriginalIcono, escalaGrandeIcono, suave);
                iconoAAnimar.localRotation = rotacionOriginalIcono * Quaternion.Euler(0f, 0f, gradosGiroIcono);
            }

            yield return null;
        }

        tiempo = 0f;

        while (tiempo < duracionMantenerGrande)
        {
            tiempo += Time.unscaledDeltaTime;

            float movimiento = Mathf.Sin(tiempo * 24f);
            float giro = movimiento * gradosGiroIcono;

            if (iconoAAnimar != null)
            {
                iconoAAnimar.localScale = escalaGrandeIcono;
                iconoAAnimar.localRotation = rotacionOriginalIcono * Quaternion.Euler(0f, 0f, giro);
            }

            yield return null;
        }

        tiempo = 0f;

        while (tiempo < duracionSalida)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracionSalida;
            float suave = 1f - Mathf.Pow(1f - t, 2f);

            if (elementoAAnimar != null)
            {
                elementoAAnimar.localScale = Vector3.Lerp(escalaGrandeSlot, escalaOriginalSlot, suave);
            }

            if (iconoAAnimar != null)
            {
                iconoAAnimar.localScale = Vector3.Lerp(escalaGrandeIcono, escalaOriginalIcono, suave);
                iconoAAnimar.localRotation = Quaternion.Lerp(
                    rotacionOriginalIcono * Quaternion.Euler(0f, 0f, -gradosGiroIcono),
                    rotacionOriginalIcono,
                    suave
                );
            }

            if (amountText != null)
            {
                amountText.color = Color.Lerp(colorFlashTexto, colorTextoOriginal, suave);
            }

            if (iconImage != null)
            {
                iconImage.color = Color.Lerp(colorFlashIcono, colorIconoOriginal, suave);
            }

            yield return null;
        }

        RestaurarAnimacion(vaciarAlTerminar);

        rutinaAnimacion = null;
    }

    private void RestaurarAnimacion(bool vaciarIcono)
    {
        if (elementoAAnimar != null)
        {
            elementoAAnimar.localScale = escalaOriginalSlot;
        }

        if (iconoAAnimar != null)
        {
            iconoAAnimar.localScale = escalaOriginalIcono;
            iconoAAnimar.localRotation = rotacionOriginalIcono;
        }

        if (amountText != null)
        {
            amountText.color = colorTextoOriginal;
        }

        if (iconImage != null)
        {
            iconImage.color = colorIconoOriginal;
        }

        if (vaciarIcono)
        {
            VaciarIcono();
        }
    }

    private void VaciarIcono()
    {
        if (iconImage != null)
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
            iconImage.color = colorIconoOriginal;
        }
    }

    private void ActualizarFondo(bool selected)
    {
        if (backgroundImage == null)
        {
            return;
        }

        CachearColorFondo();

        bool debeMostrarFondo = tieneItemActual && selected;

        backgroundImage.enabled = debeMostrarFondo;

        if (!debeMostrarFondo)
        {
            return;
        }

        Color color = colorFondoOriginal;
        color.a = alphaFondoSeleccionado;
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

    private bool EsGema(ItemData item)
    {
        if (item == null)
            return false;

        return item.name.ToLower().Contains("gema");
    }
}