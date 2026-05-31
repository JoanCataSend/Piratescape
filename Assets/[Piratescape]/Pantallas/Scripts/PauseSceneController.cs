using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseSceneController : MonoBehaviour
{
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

        SceneManager.LoadScene("MenuPrincipal");
    }

    public void SalirJuego()
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