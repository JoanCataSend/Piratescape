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
    [SerializeField] private AudioMixerGroup grupoSalidaSonidoPrueba;

    [Header("Guardado")]
    [SerializeField] private string nombreGuardado;

    private int ultimoValor;
    private bool iniciado = false;

    private void Awake()
    {
        if (slider == null)
        {
            slider = GetComponent<Slider>();
        }

        if (slider == null)
        {
            Debug.LogError("No hay Slider asignado en " + gameObject.name);
            return;
        }

        if (audioSourcePrueba != null)
        {
            audioSourcePrueba.ignoreListenerPause = true;
        }

        slider.minValue = 0;
        slider.maxValue = 10;
        slider.wholeNumbers = true;

        slider.onValueChanged.RemoveListener(CuandoCambiaSlider);
        slider.onValueChanged.AddListener(CuandoCambiaSlider);

        CargarValorGuardado();

        iniciado = true;
    }

    private void CuandoCambiaSlider(float valor)
    {
        int valorEntero = Mathf.RoundToInt(valor);

        if (valorEntero == ultimoValor)
        {
            return;
        }

        ultimoValor = valorEntero;

        AplicarVolumen(valorEntero);
        ActualizarTexto(valorEntero);

        if (iniciado)
        {
            ReproducirSonidoSlider();
        }
    }

    private void ReproducirSonidoSlider()
    {
        if (audioSourcePrueba == null)
        {
            Debug.LogWarning("No hay AudioSource asignado para el sonido del slider en " + gameObject.name);
            return;
        }

        if (sonidoClick == null)
        {
            Debug.LogWarning("No hay AudioClip asignado para el sonido del slider en " + gameObject.name);
            return;
        }

        audioSourcePrueba.ignoreListenerPause = true;

        if (grupoSalidaSonidoPrueba != null)
        {
            audioSourcePrueba.outputAudioMixerGroup = grupoSalidaSonidoPrueba;
        }

        audioSourcePrueba.PlayOneShot(sonidoClick);
    }

    public void CargarValorGuardado()
    {
        if (slider == null)
        {
            return;
        }

        int valorGuardado = PlayerPrefs.GetInt(nombreGuardado, 10);

        slider.SetValueWithoutNotify(valorGuardado);

        ultimoValor = valorGuardado;

        AplicarVolumen(valorGuardado);
        ActualizarTexto(valorGuardado);
    }

    public void GuardarValorActual()
    {
        if (slider == null)
        {
            return;
        }

        int valorActual = Mathf.RoundToInt(slider.value);

        PlayerPrefs.SetInt(nombreGuardado, valorActual);
        PlayerPrefs.Save();
    }

    private void AplicarVolumen(int valorEntero)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning("Falta AudioMixer en " + gameObject.name);
            return;
        }

        if (string.IsNullOrEmpty(nombreParametroMixer))
        {
            Debug.LogWarning("Falta el nombre del parámetro del mixer en " + gameObject.name);
            return;
        }

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

        bool aplicado = audioMixer.SetFloat(nombreParametroMixer, volumenDB);

        if (!aplicado)
        {
            Debug.LogWarning("No existe el parámetro del mixer: " + nombreParametroMixer);
        }
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