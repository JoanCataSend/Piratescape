using UnityEngine;
using TMPro;

public class AjustesGraficos : MonoBehaviour
{
    public TMP_Dropdown dropdownCalidad;

    private void Start()
    {
        // Cargar la calidad guardada
        int calidadGuardada = PlayerPrefs.GetInt("CalidadGrafica", 1);

        dropdownCalidad.value = calidadGuardada;
        dropdownCalidad.RefreshShownValue();

        CambiarCalidad(calidadGuardada);
    }

    public void CambiarCalidad(int valor)
    {
        if (valor == 0)
        {
            // Bajo
            QualitySettings.SetQualityLevel(1);
        }
        else if (valor == 1)
        {
            // Medio
            QualitySettings.SetQualityLevel(2);
        }
        else if (valor == 2)
        {
            // Alto
            QualitySettings.SetQualityLevel(5);
        }

        PlayerPrefs.SetInt("CalidadGrafica", valor);
        PlayerPrefs.Save();
    }
}