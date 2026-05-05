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

    private void OnEnable()
    {
        jugador = FindFirstObjectByType<JugadorActivador>();
        activo = false;

        if (shopUI != null)
            shopUI.SetActive(false);
    }

    private void OnDisable()
    {
        OcultarPrompt();

        if (shopUI != null && shopUI.activeSelf)
        {
            shopUI.SetActive(false);
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (jugador == null)
        {
            jugador = FindFirstObjectByType<JugadorActivador>();

            if (jugador == null)
                return;
        }

        bool nuevoEstado = interactuable && EstaEnRango();

        if (nuevoEstado != activo)
        {
            activo = nuevoEstado;

            if (!activo)
            {
                OcultarPrompt();
                CerrarTienda(false);
                return;
            }

            if (shopUI == null || !shopUI.activeSelf)
            {
                MostrarPrompt();
            }
        }
    }

    public void Interactuar()
    {
        if (!Activo || shopUI == null)
            return;

        if (shopUI.activeSelf)
            CerrarTienda(true);
        else
            AbrirTienda();
    }

    private void AbrirTienda()
    {
        shopUI.SetActive(true);
        OcultarPrompt();

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void CerrarTienda(bool mostrarPrompt)
    {
        if (shopUI == null)
            return;

        shopUI.SetActive(false);

        Time.timeScale = 1f;

        if (mostrarPrompt && Activo)
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

    public void CerrarTiendaDesdeUI()
    {
        CerrarTienda(true);
    }
}