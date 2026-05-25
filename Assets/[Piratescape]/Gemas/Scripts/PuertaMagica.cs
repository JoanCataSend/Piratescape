using UnityEngine;

public class PuertaMagica : MonoBehaviour, Interactuable
{
    [Header("Interacción")]
    [SerializeField] private float rango = 3f;

    [SerializeField]
    private string mensajeBloqueada =
        "Necesitas las 3 gemas mágicas";

    [SerializeField]
    private string mensajeDesbloqueada =
        "Pulsa E para abrir la puerta mágica";

    [Header("Gemas necesarias")]
    [SerializeField] private ItemData gemaRosa;
    [SerializeField] private ItemData gemaAmarilla;
    [SerializeField] private ItemData gemaMorada;

    [Header("Cinemática")]
    [SerializeField] private FinalAlternativoController cinematica;

    private PlayerInventory inventario;
    private JugadorActivador jugador;
    private bool cinematicaLanzada;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => true;

    private void Start()
    {
        inventario = FindFirstObjectByType<PlayerInventory>();
        jugador = FindFirstObjectByType<JugadorActivador>();
    }

    private void Update()
    {
        if (jugador == null || cinematicaLanzada)
            return;

        float distancia =
            Vector3.Distance(
                transform.position,
                jugador.Position
            );

        if (distancia <= rango)
        {
            string mensaje =
                TieneLasTresGemas()
                ? mensajeDesbloqueada
                : mensajeBloqueada;

            InteractionUI.Instance?.Show(this, mensaje);
        }
        else
        {
            InteractionUI.Instance?.Hide(this);
        }
    }

    public void Interactuar()
    {
        if (cinematicaLanzada)
            return;

        if (!TieneLasTresGemas())
        {
            Debug.Log("Faltan gemas.");
            return;
        }

        cinematicaLanzada = true;

        InteractionUI.Instance?.Hide(this);

        if (cinematica != null)
        {
            cinematica.IniciarCinematica();
        }
        else
        {
            Debug.LogWarning("No hay cinemática asignada.");
        }
    }

    private bool TieneLasTresGemas()
    {
        if (inventario == null)
            return false;

        return
            inventario.ObtenerCantidad(gemaRosa) > 0 &&
            inventario.ObtenerCantidad(gemaAmarilla) > 0 &&
            inventario.ObtenerCantidad(gemaMorada) > 0;
    }
}