using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SliderVolumenMixer : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider slider;

    [Header("Texto opcional")]
    [SerializeField] private TMP_Text textoPorcentaje;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string nombreParametroMixer;

    [Header("Sonido de prueba opcional")]
    [SerializeField] private AudioSource audioSourcePrueba;
    [SerializeField] private AudioClip sonidoClick;

    [Header("Guardado")]
    [SerializeField] private string nombreGuardado;

    private int ultimoValor;
    private bool iniciado = false;

    private void Awake()
    {
        if (slider == null)
            slider = GetComponent<Slider>();

        slider.minValue = 0;
        slider.maxValue = 10;
        slider.wholeNumbers = true;

        slider.onValueChanged.AddListener(CuandoCambiaSlider);

        CargarValorGuardado();

        iniciado = true;
    }

    private void CuandoCambiaSlider(float valor)
    {
        int valorEntero = Mathf.RoundToInt(valor);

        if (valorEntero == ultimoValor)
            return;

        ultimoValor = valorEntero;

        AplicarVolumen(valorEntero);
        ActualizarTexto(valorEntero);

        if (iniciado && audioSourcePrueba != null && sonidoClick != null)
        {
            audioSourcePrueba.PlayOneShot(sonidoClick);
        }
    }

    public void CargarValorGuardado()
    {
        int valorGuardado = PlayerPrefs.GetInt(nombreGuardado, 10);

        if (slider == null)
            return;

        slider.SetValueWithoutNotify(valorGuardado);

        ultimoValor = valorGuardado;

        AplicarVolumen(valorGuardado);
        ActualizarTexto(valorGuardado);
    }

    public void GuardarValorActual()
    {
        if (slider == null)
            return;

        int valorActual = Mathf.RoundToInt(slider.value);
        PlayerPrefs.SetInt(nombreGuardado, valorActual);
    }

    private void AplicarVolumen(int valorEntero)
    {
        if (audioMixer == null || string.IsNullOrEmpty(nombreParametroMixer))
            return;

        float volumenNormalizado = valorEntero / 10f;

        float volumenDB;

        if (volumenNormalizado <= 0)
        {
            volumenDB = -80f;
        }
        else
        {
            volumenDB = Mathf.Log10(volumenNormalizado) * 20f;
        }

        audioMixer.SetFloat(nombreParametroMixer, volumenDB);
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