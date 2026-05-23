using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public sealed class MenuPausaUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private GameObject hud;
    [SerializeField] private MonoBehaviour[] componentesADesactivarAlPausar;

    [Header("Estado del jugador")]
    [SerializeField] private SistemaSaludJugador sistemaSaludJugador;

    [Header("Input")]
    [SerializeField] private InputActionReference accionPausa;

    private bool estaEnPausa;

    private void Awake()
    {
        if (sistemaSaludJugador == null)
        {
            sistemaSaludJugador = FindFirstObjectByType<SistemaSaludJugador>();
        }
    }

    private void OnEnable()
    {
        if (accionPausa != null)
        {
            accionPausa.action.performed += AlPulsarPausa;
            accionPausa.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (accionPausa != null)
        {
            accionPausa.action.performed -= AlPulsarPausa;
            accionPausa.action.Disable();
        }
    }

    private void Start()
    {
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        estaEnPausa = false;
    }

    private void AlPulsarPausa(InputAction.CallbackContext contexto)
    {
        if (HayOtraUIAbierta())
        {
            return;
        }

        if (JugadorEstaMuerto())
        {
            if (estaEnPausa)
            {
                ForzarCerrarPausa();
            }

            return;
        }

        if (estaEnPausa)
        {
            ReanudarJuego();
        }
        else
        {
            PausarJuego();
        }
    }

    public void PausarJuego()
    {
        if (estaEnPausa || JugadorEstaMuerto() || HayOtraUIAbierta())
        {
            return;
        }

        estaEnPausa = true;

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        if (hud != null)
        {
            hud.SetActive(false);
        }

        CambiarEstadoComponentesJugador(false);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ReanudarJuego()
    {
        if (!estaEnPausa)
        {
            return;
        }

        estaEnPausa = false;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        if (hud != null && !JugadorEstaMuerto())
        {
            hud.SetActive(true);
        }

        CambiarEstadoComponentesJugador(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ForzarCerrarPausa()
    {
        estaEnPausa = false;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        CambiarEstadoComponentesJugador(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MenuPrincipal");
    }

    public void SalirDelJuego()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Application.Quit();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }

    private bool HayOtraUIAbierta()
    {
        return CofreInventarioUI.HayAlgunaUIAbierta
            || LoroDialogoUI.HayAlgunaUIAbierta
            || HistoriaInicioLoroUI.HayAlgunaUIAbierta
            || TutorialMisionesLoro.BloquearMenuPausa;
    }

    private bool JugadorEstaMuerto()
    {
        return sistemaSaludJugador != null && sistemaSaludJugador.EstaMuerto;
    }

    private void CambiarEstadoComponentesJugador(bool valor)
    {
        if (componentesADesactivarAlPausar == null)
        {
            return;
        }

        for (int i = 0; i < componentesADesactivarAlPausar.Length; i++)
        {
            if (componentesADesactivarAlPausar[i] != null)
            {
                componentesADesactivarAlPausar[i].enabled = valor;
            }
        }
    }
}
