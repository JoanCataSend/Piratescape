using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UISliderSounds : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider slider;

    [Header("Sonido de prueba")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoClick;

    [Header("Texto opcional")]
    [SerializeField] private TMP_Text textoPorcentaje;

    private int ultimoValor = -1;
    private bool iniciado = false;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        slider.minValue = 0;
        slider.maxValue = 10;
        slider.wholeNumbers = true;

        ultimoValor = Mathf.RoundToInt(slider.value);

        ActualizarTexto(ultimoValor);

        slider.onValueChanged.AddListener(CuandoCambiaSlider);

        iniciado = true;
    }

    private void CuandoCambiaSlider(float valor)
    {
        int valorEntero = Mathf.RoundToInt(valor);

        if (valorEntero == ultimoValor)
            return;

        ultimoValor = valorEntero;

        ActualizarTexto(valorEntero);

        float volumen = valorEntero / 10f;

        // Esto cambia el volumen general del juego
        AudioListener.volume = volumen;

        // Esto hace sonar el "cli" como prueba del volumen actual
        if (iniciado && audioSource != null && sonidoClick != null)
        {
            audioSource.PlayOneShot(sonidoClick);
        }

        Debug.Log("Volumen: " + (valorEntero * 10) + "%");
    }

    private void ActualizarTexto(int valorEntero)
    {
        if (textoPorcentaje != null)
        {
            int porcentaje = valorEntero * 10;
            textoPorcentaje.text = porcentaje + "%";
        }
    }
}