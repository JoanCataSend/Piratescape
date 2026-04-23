using TMPro;
using UnityEngine;

public sealed class UIEconomia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SistemaEconomia sistemaEconomia;
    [SerializeField] private TMP_Text textoConchas;
    [SerializeField] private TMP_Text textoTulipanes;
    [SerializeField] private TMP_Text textoPinyas;

    private void OnEnable()
    {
        if (sistemaEconomia != null)
        {
            sistemaEconomia.OnEconomiaActualizada += ActualizarUI;
            ActualizarUI(
                sistemaEconomia.Conchas,
                sistemaEconomia.Tulipanes,
                sistemaEconomia.Pinyas);
        }
    }

    private void OnDisable()
    {
        if (sistemaEconomia != null)
        {
            sistemaEconomia.OnEconomiaActualizada -= ActualizarUI;
        }
    }

    private void ActualizarUI(int conchas, int tulipanes, int pinyas)
    {
        if (textoConchas != null)
        {
            textoConchas.text = conchas.ToString();
        }

        if (textoTulipanes != null)
        {
            textoTulipanes.text = tulipanes.ToString();
        }

        if (textoPinyas != null)
        {
            textoPinyas.text = pinyas.ToString();
        }
    }
}   