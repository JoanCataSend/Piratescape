using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSceneController : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPausaPrincipal;
    [SerializeField] private GameObject panelAjustes;
    [SerializeField] private GameObject panelConfirmarVolverMenu;

    [Header("Ajustes")]
    [SerializeField] private SettingsMenuController settingsMenuController;

    private void Start()
    {
        MostrarPausaPrincipal();
    }

    public void Continuar()
    {
        MenuPausaUI menuPausa = FindFirstObjectByType<MenuPausaUI>();

        if (menuPausa != null)
        {
            menuPausa.ReanudarJuego();
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            SceneManager.UnloadSceneAsync("PauseScene");
        }
    }

    public void AbrirAjustes()
    {
        if (panelPausaPrincipal != null)
        {
            panelPausaPrincipal.SetActive(false);
        }

        if (panelConfirmarVolverMenu != null)
        {
            panelConfirmarVolverMenu.SetActive(false);
        }

        if (settingsMenuController != null)
        {
            settingsMenuController.AbrirConfiguracion();
        }
        else if (panelAjustes != null)
        {
            panelAjustes.SetActive(true);
        }
    }

    public void CerrarAjustes()
    {
        if (settingsMenuController != null)
        {
            settingsMenuController.Cancelar();
        }
        else if (panelAjustes != null)
        {
            panelAjustes.SetActive(false);
        }

        MostrarPausaPrincipal();
    }

    public void MostrarPausaPrincipal()
    {
        if (panelPausaPrincipal != null)
        {
            panelPausaPrincipal.SetActive(true);
        }

        if (panelAjustes != null)
        {
            panelAjustes.SetActive(false);
        }


        if (panelConfirmarVolverMenu != null)
        {
            panelConfirmarVolverMenu.SetActive(false);
        }
    }

    public void PedirVolverAlMenu()
    {
        if (panelPausaPrincipal != null)
        {
            panelPausaPrincipal.SetActive(false);
        }

        if (panelConfirmarVolverMenu != null)
        {
            panelConfirmarVolverMenu.SetActive(true);
        }
        else
        {
            ConfirmarVolverAlMenu();
        }
    }

    public void CancelarVolverAlMenu()
    {
        MostrarPausaPrincipal();
    }

    public void ConfirmarVolverAlMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        UnityEngine.EventSystems.EventSystem eventSystemActual =
            UnityEngine.EventSystems.EventSystem.current;

        if (eventSystemActual != null)
        {
            Destroy(eventSystemActual.gameObject);
        }

        SceneManager.LoadScene("MenuPrincipal");
    }

    public void PedirSalirJuego()
    {
        if (panelPausaPrincipal != null)
        {
            panelPausaPrincipal.SetActive(false);
        }
        else
        {
            ConfirmarSalirJuego();
        }
    }

    public void CancelarSalirJuego()
    {
        MostrarPausaPrincipal();
    }

    public void ConfirmarSalirJuego()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}