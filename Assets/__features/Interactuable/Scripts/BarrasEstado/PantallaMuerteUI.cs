using UnityEngine;

public sealed class PantallaMuerteUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SistemaSaludJugador sistemaSaludJugador;
    [SerializeField] private GameObject panelMuerte;

    private void Start()
    {
        if (panelMuerte != null)
        {
            panelMuerte.SetActive(false);
        }
    }

    private void Update()
    {
        if (sistemaSaludJugador == null || panelMuerte == null)
        {
            return;
        }

        if (sistemaSaludJugador.EstaMuerto && !panelMuerte.activeSelf)
        {
            panelMuerte.SetActive(true);
        }
    }
}