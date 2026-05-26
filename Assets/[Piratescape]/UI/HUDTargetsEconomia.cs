using UnityEngine;

public sealed class HUDTargetsEconomia : MonoBehaviour
{
    [Header("Destinos del HUD")]
    [SerializeField] private RectTransform conchas;
    [SerializeField] private RectTransform tulipanes;
    [SerializeField] private RectTransform pinyas;

    public RectTransform GetTarget(TipoMoneda tipoMoneda)
    {
        if (tipoMoneda == TipoMoneda.Concha)
        {
            return conchas;
        }

        if (tipoMoneda == TipoMoneda.Tulipan)
        {
            return tulipanes;
        }

        return pinyas;
    }
}