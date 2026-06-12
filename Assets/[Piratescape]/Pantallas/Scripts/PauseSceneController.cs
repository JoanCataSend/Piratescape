using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class PauseSceneController : MonoBehaviour
{
    [Header("Paneles principales")]
    [SerializeField] private GameObject panelPausaPrincipal;
    [SerializeField] private GameObject panelAjustes;

    [FormerlySerializedAs("panelConfirmarVolverMenu")]
    [SerializeField] private GameObject panelConfirmarSalir;

    [SerializeField] private GameObject panelControles;

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

        if (panelConfirmarSalir != null)
        {
            panelConfirmarSalir.SetActive(false);
        }

        if (panelControles != null)
        {
            panelControles.SetActive(false);
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

    public void GuardarAjustes()
    {
        if (settingsMenuController != null)
        {
            settingsMenuController.Aceptar();
        }
        else if (panelAjustes != null)
        {
            panelAjustes.SetActive(false);
        }

        MostrarPausaPrincipal();
    }

    public void CancelarAjustes()
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

    public void AbrirControles()
    {
        if (panelControles != null)
        {
            panelControles.SetActive(true);
        }
    }

    public void CerrarControles()
    {
        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }
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

        if (panelConfirmarSalir != null)
        {
            panelConfirmarSalir.SetActive(false);
        }

        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }
    }

    public void VolverAlMenu()
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

        SceneManager.LoadScene("MenuPrincipalv2");
    }

    public void PedirSalirJuego()
    {
        if (panelPausaPrincipal != null)
        {
            panelPausaPrincipal.SetActive(false);
        }

        if (panelAjustes != null)
        {
            panelAjustes.SetActive(false);
        }

        if (panelControles != null)
        {
            panelControles.SetActive(false);
        }

        if (panelConfirmarSalir != null)
        {
            panelConfirmarSalir.SetActive(true);
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

    public void SalirJuego()
    {
        PedirSalirJuego();
    }
}