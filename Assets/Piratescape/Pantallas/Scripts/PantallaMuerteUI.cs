using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PantallaMuerteUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SistemaSaludJugador sistemaSaludJugador;
    [SerializeField] private GameObject panelMuerte;
    [SerializeField] private GameObject hud;
    [SerializeField] private AudioSource audioMuerte;

    private bool pantallaMostrada;

    private void Start()
    {
        if (panelMuerte != null)
        {
            panelMuerte.SetActive(false);
        }

        if (audioMuerte != null)
        {
            audioMuerte.Stop();
        }
    }

    private void Update()
    {
        if (sistemaSaludJugador == null || panelMuerte == null)
        {
            return;
        }

        if (sistemaSaludJugador.EstaMuerto && !pantallaMostrada)
        {
            MostrarPantallaMuerte();
        }
    }

    private void MostrarPantallaMuerte()
    {
        pantallaMostrada = true;

        if (hud != null)
        {
            hud.SetActive(false);
        }

        panelMuerte.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (audioMuerte != null && !audioMuerte.isPlaying)
        {
            audioMuerte.Play();
        }
    }

    public void ReintentarPartida()
    {
        Debug.Log("REINTENTAR PULSADO");

        if (audioMuerte != null)
        {
            audioMuerte.Stop();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 1f;
        Scene escenaActual = SceneManager.GetActiveScene();
        SceneManager.LoadScene(escenaActual.buildIndex);
    }

    public void SalirDelJuego()
    {
        Debug.Log("SALIR PULSADO");

        if (audioMuerte != null)
        {
            audioMuerte.Stop();
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}