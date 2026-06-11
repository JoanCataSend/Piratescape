using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Paneles externos opcionales")]
    [SerializeField] private GameObject panelAOcultarAlAbrir;
    [SerializeField] private GameObject panelAMostrarAlCerrar;

    [Header("Panel controles")]
    [SerializeField] private GameObject panelControles;

    [Header("Pantalla")]
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private AudioSource audioSourceUI;
    [SerializeField] private AudioClip sonidoToggle;

private bool iniciado = false;

    [Header("Audio")]
    [SerializeField] private SliderVolumenMixer masterVolumeSlider;
    [SerializeField] private SliderVolumenMixer musicVolumeSlider;
    [SerializeField] private SliderVolumenMixer sfxVolumeSlider;
    [SerializeField] private SliderVolumenMixer voicesVolumeSlider;

    [Header("Sensibilidad del ratón")]
    [SerializeField] private Slider mouseSensitivitySlider;
    [SerializeField] private TMP_Text mouseSensitivityText;
    [SerializeField] private float sensibilidadPorDefecto = 5f;

    private const string FullscreenKey = "Settings_Fullscreen";
    private const string QualityKey = "Settings_Quality";
    private const string MouseSensitivityKey = "Settings_MouseSensitivity";

    private bool savedFullscreen;
    private int savedQuality;
    private float savedMouseSensitivity;

    private void Awake()
    {
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
        }

        ConfigurarDropdownCalidad();
        ConfigurarSliderSensibilidad();
        ConfigurarTogglePantallaCompleta();

        CargarValoresGuardados();
        AplicarValoresGuardados();
        CargarValoresEnUI();

        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }

        iniciado = true;
    }

    public void AbrirConfiguracion()
    {
        CargarValoresGuardados();
        AplicarValoresGuardados();
        CargarValoresEnUI();

        if (panelAOcultarAlAbrir != null)
        {
            panelAOcultarAlAbrir.SetActive(false);
        }

        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void Cancelar()
    {
        CargarValoresGuardados();
        AplicarValoresGuardados();
        CargarValoresEnUI();
        CerrarPanel();
    }

    public void Aceptar()
    {
        LeerValoresDeUI();

        GuardarValores();
        AplicarValoresGuardados();

        CerrarPanel();
    }

    public void AbrirControles()
    {
        if (panelControles != null)
        {
            panelControles.SetActive(true);
        }
    }

    public void CerrarControles()
    {
        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }
    }

    private void CerrarPanel()
    {
        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (panelAMostrarAlCerrar != null)
        {
            panelAMostrarAlCerrar.SetActive(true);
        }
    }

    private void ConfigurarDropdownCalidad()
    {
        if (qualityDropdown == null)
            return;

        qualityDropdown.ClearOptions();

        qualityDropdown.AddOptions(new System.Collections.Generic.List<string>
        {
            "Bajo",
            "Medio",
            "Alto"
        });
    }

    private void ConfigurarSliderSensibilidad()
    {
        if (mouseSensitivitySlider == null)
            return;

        mouseSensitivitySlider.minValue = 1;
        mouseSensitivitySlider.maxValue = 10;
        mouseSensitivitySlider.wholeNumbers = true;

        mouseSensitivitySlider.onValueChanged.AddListener(CuandoCambiaSensibilidad);
    }

    private void CargarValoresGuardados()
    {
        savedFullscreen = PlayerPrefs.GetInt(FullscreenKey, 1) == 1;
        savedQuality = PlayerPrefs.GetInt(QualityKey, 1);
        savedMouseSensitivity = PlayerPrefs.GetFloat(MouseSensitivityKey, sensibilidadPorDefecto);

        savedQuality = Mathf.Clamp(savedQuality, 0, 2);
        savedMouseSensitivity = Mathf.Clamp(savedMouseSensitivity, 1f, 10f);
    }

    private void CargarValoresEnUI()
    {
        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(savedFullscreen);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.value = savedQuality;
            qualityDropdown.RefreshShownValue();
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.CargarValorGuardado();
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.CargarValorGuardado();
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.CargarValorGuardado();
        }

        if (voicesVolumeSlider != null)
        {
            voicesVolumeSlider.CargarValorGuardado();
        }

        if (mouseSensitivitySlider != null)
        {
            mouseSensitivitySlider.SetValueWithoutNotify(savedMouseSensitivity);
        }

        ActualizarTextoSensibilidad(savedMouseSensitivity);
    }

    private void LeerValoresDeUI()
    {
        if (fullscreenToggle != null)
        {
            savedFullscreen = fullscreenToggle.isOn;
        }

        if (qualityDropdown != null)
        {
            savedQuality = qualityDropdown.value;
        }

        if (mouseSensitivitySlider != null)
        {
            savedMouseSensitivity = mouseSensitivitySlider.value;
        }

        savedQuality = Mathf.Clamp(savedQuality, 0, 2);
        savedMouseSensitivity = Mathf.Clamp(savedMouseSensitivity, 1f, 10f);
    }

    private void AplicarValoresGuardados()
    {
        AplicarPantallaCompleta(savedFullscreen);
        AplicarCalidad(savedQuality);
    }

    private void AplicarCalidad(int calidad)
    {
        if (QualitySettings.names.Length <= 0)
        {
            return;
        }

        int nivelUnity = 2;

        if (calidad == 0)
        {
            nivelUnity = 1;
        }
        else if (calidad == 1)
        {
            nivelUnity = 2;
        }
        else if (calidad == 2)
        {
            nivelUnity = 5;
        }

        nivelUnity = Mathf.Clamp(nivelUnity, 0, QualitySettings.names.Length - 1);
        QualitySettings.SetQualityLevel(nivelUnity, true);
    }

    private void GuardarValores()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.GuardarValorActual();
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.GuardarValorActual();
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.GuardarValorActual();
        }

        if (voicesVolumeSlider != null)
        {
            voicesVolumeSlider.GuardarValorActual();
        }

        PlayerPrefs.SetInt(FullscreenKey, savedFullscreen ? 1 : 0);
        PlayerPrefs.SetInt(QualityKey, savedQuality);
        PlayerPrefs.SetFloat(MouseSensitivityKey, savedMouseSensitivity);

        PlayerPrefs.Save();
    }

    private void CuandoCambiaSensibilidad(float valor)
    {
        ActualizarTextoSensibilidad(valor);
    }

    private void ActualizarTextoSensibilidad(float valor)
    {
        if (mouseSensitivityText != null)
        {
            int porcentaje = Mathf.RoundToInt(valor * 10f);
            mouseSensitivityText.text = porcentaje + "%";
        }
    }

    public static float ObtenerSensibilidadRaton()
    {
        return PlayerPrefs.GetFloat(MouseSensitivityKey, 5f);
    }

    private void ConfigurarTogglePantallaCompleta()
    {
        if (fullscreenToggle == null)
            return;

        fullscreenToggle.onValueChanged.RemoveListener(CuandoCambiaPantallaCompleta);
        fullscreenToggle.onValueChanged.AddListener(CuandoCambiaPantallaCompleta);
    }

    private void CuandoCambiaPantallaCompleta(bool estaActivado)
    {
        savedFullscreen = estaActivado;

        AplicarPantallaCompleta(estaActivado);

        if (iniciado)
        {
            ReproducirSonidoUI();
        }
    }

    private void AplicarPantallaCompleta(bool pantallaCompleta)
    {
        if (pantallaCompleta)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.Windowed;
            Screen.fullScreen = false;
        }
    }

    private void ReproducirSonidoUI()
    {
        if (audioSourceUI != null && sonidoToggle != null)
        {
            audioSourceUI.PlayOneShot(sonidoToggle);
        }
    }
}