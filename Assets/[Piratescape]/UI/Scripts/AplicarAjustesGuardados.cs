using UnityEngine;
using UnityEngine.Audio;

public class AplicarAjustesGuardados : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string FullscreenKey = "Settings_Fullscreen";
    private const string QualityKey = "Settings_Quality";

    private void Awake()
    {
        AplicarTodo();
    }

    public void AplicarTodo()
    {
        AplicarVolumen("MasterVolume", PlayerPrefs.GetInt("MasterVolume", 10));
        AplicarVolumen("MusicVolume", PlayerPrefs.GetInt("MusicVolume", 10));
        AplicarVolumen("SFXVolume", PlayerPrefs.GetInt("SFXVolume", 10));
        AplicarVolumen("VoicesVolume", PlayerPrefs.GetInt("VoicesVolume", 10));

        bool pantallaCompleta = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
        Screen.fullScreen = pantallaCompleta;

        int calidad = PlayerPrefs.GetInt(QualityKey, 1);
        AplicarCalidad(calidad);
    }

    private void AplicarVolumen(string parametro, int valorEntero)
    {
        if (audioMixer == null)
            return;

        valorEntero = Mathf.Clamp(valorEntero, 0, 10);

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

        audioMixer.SetFloat(parametro, volumenDB);
    }

    private void AplicarCalidad(int calidad)
    {
        if (QualitySettings.names.Length <= 0)
            return;

        calidad = Mathf.Clamp(calidad, 0, 2);

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
}