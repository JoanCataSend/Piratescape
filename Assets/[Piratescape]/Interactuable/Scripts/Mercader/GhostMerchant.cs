using UnityEngine;
using UnityEngine.InputSystem;

public class GhostMerchant : MonoBehaviour, Interactuable
{
    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    [Header("Configuracion")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;

    [Header("UI Tienda")]
    [SerializeField] private GameObject shopUI;

    [Header("Mensaje")]
    [SerializeField] private string mensaje = "Pulsa E para comerciar";

    private bool activo;
    private IActivador jugador;
    private int ultimoFrameInteraccion = -1;

    public bool Activo => activo && interactuable;

    private void Awake()
    {
        BuscarReferenciasSiFaltan();
    }

    private void OnEnable()
    {
        BuscarReferenciasSiFaltan();
        activo = false;

        if (shopUI != null)
        {
            shopUI.SetActive(false);
        }
    }

    private void OnDisable()
    {
        OcultarPrompt();
        CerrarTienda(false);
    }

    private void Update()
    {
        BuscarReferenciasSiFaltan();

        if (jugador == null)
        {
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
            }
            else if (shopUI == null || !shopUI.activeSelf)
            {
                MostrarPrompt();
            }
        }

        if (Activo && SeHaPulsadoInteractuar())
        {
            Interactuar();
        }
    }

    public void Interactuar()
    {
        if (Time.frameCount == ultimoFrameInteraccion)
        {
            return;
        }

        ultimoFrameInteraccion = Time.frameCount;

        BuscarReferenciasSiFaltan();

        if (!Activo)
        {
            Debug.Log("GhostMerchant: no abre porque el jugador no esta en rango.");
            return;
        }

        if (shopUI == null)
        {
            Debug.LogError("GhostMerchant: falta asignar ShopPanel en el campo Shop UI.", this);
            return;
        }

        if (shopUI.activeSelf)
        {
            CerrarTienda(true);
        }
        else
        {
            AbrirTienda();
        }
    }

    public void AbrirTienda()
    {
        if (shopUI == null)
        {
            return;
        }

        shopUI.SetActive(true);
        OcultarPrompt();

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("GhostMerchant: tienda abierta.");
    }

    public void CerrarTienda(bool mostrarPrompt)
    {
        if (shopUI != null)
        {
            shopUI.SetActive(false);
        }

        Time.timeScale = 1f;

        if (mostrarPrompt && Activo)
        {
            MostrarPrompt();
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private bool EstaEnRango()
    {
        if (jugador == null)
        {
            return false;
        }

        return Vector3.Distance(transform.position, jugador.Position) <= rango;
    }

    private bool SeHaPulsadoInteractuar()
    {
        bool tecladoNuevo = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        bool mando = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
        bool tecladoViejo = Input.GetKeyDown(KeyCode.E);

        return tecladoNuevo || mando || tecladoViejo;
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (jugador == null)
        {
            jugador = FindFirstObjectByType<JugadorActivador>();
        }

        if (shopUI == null)
        {
            GameObject encontrado = BuscarGameObjectPorNombreIncluyendoInactivos("ShopPanel");

            if (encontrado != null)
            {
                shopUI = encontrado;
            }
        }
    }

    private GameObject BuscarGameObjectPorNombreIncluyendoInactivos(string nombre)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        for (int i = 0; i < transforms.Length; i++)
        {
            Transform actual = transforms[i];

            if (actual == null || actual.gameObject == null)
            {
                continue;
            }

            if (actual.gameObject.scene.IsValid() && actual.name == nombre)
            {
                return actual.gameObject;
            }
        }

        return null;
    }

    private void MostrarPrompt()
    {
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Show(this, mensaje);
        }
    }

    private void OcultarPrompt()
    {
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide(this);
        }
    }

    public void CerrarTiendaDesdeUI()
    {
        CerrarTienda(true);
    }
}
