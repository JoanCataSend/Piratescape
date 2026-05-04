using UnityEngine;

public class GhostMerchant : MonoBehaviour, Interactuable
{
    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    [Header("Configuración")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;

    [Header("UI Tienda")]
    [SerializeField] private GameObject shopUI;

    [Header("Mensaje")]
    [SerializeField] private string mensaje = "Pulsa E para comerciar";

    private bool activo;
    private IActivador jugador;

    public bool Activo => activo && interactuable;

    private void Start()
    {
        jugador = FindFirstObjectByType<JugadorActivador>();

        if (shopUI != null)
            shopUI.SetActive(false);
    }

    private void Update()
    {
        if (jugador == null)
            return;

        bool nuevoEstado = interactuable && EstaEnRango();

        if (nuevoEstado != activo)
        {
            activo = nuevoEstado;

            if (!activo)
            {
                OcultarPrompt();
                CerrarTienda();
                return;
            }

            if (shopUI == null || !shopUI.activeSelf)
                MostrarPrompt();
        }
    }

    public void Interactuar()
    {
        if (!Activo || shopUI == null)
            return;

        if (shopUI.activeSelf)
            CerrarTienda();
        else
            AbrirTienda();
    }

    private void AbrirTienda()
    {
        shopUI.SetActive(true);
        OcultarPrompt();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CerrarTienda()
    {
        if (shopUI == null)
            return;

        shopUI.SetActive(false);

        if (Activo)
            MostrarPrompt();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool EstaEnRango()
    {
        return Vector3.Distance(transform.position, jugador.Position) <= rango;
    }

    private void MostrarPrompt()
    {
        if (InteractionUI.Instance != null)
            InteractionUI.Instance.Show(this, mensaje);
    }

    private void OcultarPrompt()
    {
        if (InteractionUI.Instance != null)
            InteractionUI.Instance.Hide(this);
    }
}