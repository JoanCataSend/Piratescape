using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenuController : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Pantalla")]
    [SerializeField] private Toggle fullscreenToggle;

    [Header("Audio")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    private const string FullscreenKey = "Settings_Fullscreen";
    private const string MasterVolumeKey = "Settings_MasterVolume";
    private const string MusicVolumeKey = "Settings_MusicVolume";
    private const string SFXVolumeKey = "Settings_SFXVolume";

    private const string MasterVolumeParameter = "MasterVolume";
    private const string MusicVolumeParameter = "MusicVolume";
    private const string SFXVolumeParameter = "SFXVolume";

    private bool savedFullscreen;
    private float savedMasterVolume;
    private float savedMusicVolume;
    private float savedSFXVolume;

    private void Awake()
    {
        if (settingsPanel == null)
        {
            settingsPanel = gameObject;
        }

        CargarValoresGuardados();
        AplicarValoresGuardados();
        CargarValoresEnUI();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void AbrirConfiguracion()
    {
        CargarValoresGuardados();
        CargarValoresEnUI();

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

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void Aceptar()
    {
        LeerValoresDeUI();
        AplicarValoresGuardados();
        GuardarValores();

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    private void CargarValoresGuardados()
    {
        savedFullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;

        savedMasterVolume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
        savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 1f);
        savedSFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, 1f);

        savedMasterVolume = Mathf.Clamp01(savedMasterVolume);
        savedMusicVolume = Mathf.Clamp01(savedMusicVolume);
        savedSFXVolume = Mathf.Clamp01(savedSFXVolume);
    }

    private void CargarValoresEnUI()
    {
        if (fullscreenToggle != null)
        {
            fullscreenToggle.isOn = savedFullscreen;
        }

        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.value = savedMasterVolume;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = savedMusicVolume;
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.value = savedSFXVolume;
        }
    }

    private void LeerValoresDeUI()
    {
        if (fullscreenToggle != null)
        {
            savedFullscreen = fullscreenToggle.isOn;
        }

        if (masterVolumeSlider != null)
        {
            savedMasterVolume = masterVolumeSlider.value;
        }

        if (musicVolumeSlider != null)
        {
            savedMusicVolume = musicVolumeSlider.value;
        }

        if (sfxVolumeSlider != null)
        {
            savedSFXVolume = sfxVolumeSlider.value;
        }

        savedMasterVolume = Mathf.Clamp01(savedMasterVolume);
        savedMusicVolume = Mathf.Clamp01(savedMusicVolume);
        savedSFXVolume = Mathf.Clamp01(savedSFXVolume);
    }

    private void AplicarValoresGuardados()
    {
        Screen.fullScreen = savedFullscreen;

        AplicarVolumen(MasterVolumeParameter, savedMasterVolume);
        AplicarVolumen(MusicVolumeParameter, savedMusicVolume);
        AplicarVolumen(SFXVolumeParameter, savedSFXVolume);
    }

    private void GuardarValores()
    {
        PlayerPrefs.SetInt(FullscreenKey, savedFullscreen ? 1 : 0);

        PlayerPrefs.SetFloat(MasterVolumeKey, savedMasterVolume);
        PlayerPrefs.SetFloat(MusicVolumeKey, savedMusicVolume);
        PlayerPrefs.SetFloat(SFXVolumeKey, savedSFXVolume);

        PlayerPrefs.Save();
    }

    private void AplicarVolumen(string parameterName, float volume)
    {
        if (audioMixer == null)
        {
            return;
        }

        volume = Mathf.Clamp01(volume);

        float volumeDb;

        if (volume <= 0.0001f)
        {
            volumeDb = -80f;
        }
        else
        {
            volumeDb = Mathf.Log10(volume) * 20f;
        }

        audioMixer.SetFloat(parameterName, volumeDb);
    }
}