using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class PantallaMuerteUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SistemaSaludJugador sistemaSaludJugador;
    [SerializeField] private GameObject panelMuerte;
    [SerializeField] private GameObject hud;
    [SerializeField] private AudioSource audioMuerte;

    [Header("Configuración")]
    [SerializeField] private float retrasoMostrarPantalla = 2.5f;

    private bool pantallaMostrada;
    private bool procesoMuerteIniciado;

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

        if (sistemaSaludJugador.EstaMuerto && !procesoMuerteIniciado)
        {
            procesoMuerteIniciado = true;
            StartCoroutine(MostrarPantallaMuerteConRetraso());
        }
    }

    private IEnumerator MostrarPantallaMuerteConRetraso()
    {
        yield return new WaitForSecondsRealtime(retrasoMostrarPantalla);
        MostrarPantallaMuerte();
    }

    private void MostrarPantallaMuerte()
    {
        if (pantallaMostrada)
        {
            return;
        }

        pantallaMostrada = true;

        if (hud != null)
        {
            hud.SetActive(false);
        }

        if (audioMuerte != null)
        {
            audioMuerte.Play();
        }

        panelMuerte.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
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