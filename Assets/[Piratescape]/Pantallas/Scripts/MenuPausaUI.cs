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

    [Header("Tutorial")]
    [SerializeField] private TutorialMisionesLoro tutorialMisionesLoro;

    [Header("Input")]
    [SerializeField] private InputActionReference accionPausa;

    [Header("Audio que SÍ debe sonar en pausa")]
    [SerializeField] private AudioSource[] audiosPermitidosEnPausa;

    private bool estaEnPausa;

    private void Awake()
    {
        if (sistemaSaludJugador == null)
        {
            sistemaSaludJugador = FindFirstObjectByType<SistemaSaludJugador>();
        }

        if (tutorialMisionesLoro == null)
        {
            tutorialMisionesLoro = FindFirstObjectByType<TutorialMisionesLoro>();
        }

        PrepararAudiosPermitidosEnPausa();
    }

    private void OnEnable()
    {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        if (accionPausa != null)
        {
            accionPausa.action.performed += AlPulsarPausa;
            accionPausa.action.Enable();
        }
#endif
    }

    private void OnDisable()
    {
#if !(UNITY_WEBGL && !UNITY_EDITOR)
        if (accionPausa != null)
        {
            accionPausa.action.performed -= AlPulsarPausa;
            accionPausa.action.Disable();
        }
#endif
    }

    private void Start()
    {
        estaEnPausa = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    private void Update()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            IntentarAlternarPausa();
        }
#endif
    }

    private void AlPulsarPausa(InputAction.CallbackContext contexto)
    {
        IntentarAlternarPausa();
    }

    private void IntentarAlternarPausa()
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

        if (hud != null)
        {
            hud.SetActive(false);
        }

        if (tutorialMisionesLoro != null)
        {
            tutorialMisionesLoro.OcultarVisualmentePorPausa();
        }

        CambiarEstadoComponentesJugador(false);

        Time.timeScale = 0f;

        // Pausamos el audio global.
        // Solo seguirán sonando los AudioSource que tengan ignoreListenerPause = true.
        PrepararAudiosPermitidosEnPausa();
        AudioListener.pause = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("PauseScene", LoadSceneMode.Additive);
    }

    public void ReanudarJuego()
    {
        if (!estaEnPausa)
        {
            return;
        }

        estaEnPausa = false;

        if (hud != null && !JugadorEstaMuerto())
        {
            hud.SetActive(true);
        }

        if (tutorialMisionesLoro != null)
        {
            tutorialMisionesLoro.RestaurarVisualmenteDespuesDePausa();
        }

        CambiarEstadoComponentesJugador(true);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.UnloadSceneAsync("PauseScene");
    }

    public void ForzarCerrarPausa()
    {
        estaEnPausa = false;

        if (tutorialMisionesLoro != null)
        {
            tutorialMisionesLoro.RestaurarVisualmenteDespuesDePausa();
        }

        CambiarEstadoComponentesJugador(true);

        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.UnloadSceneAsync("PauseScene");
    }

    public void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        SceneManager.LoadScene("MenuPrincipalv2");
    }

    public void SalirDelJuego()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void PrepararAudiosPermitidosEnPausa()
    {
        if (audiosPermitidosEnPausa == null)
        {
            return;
        }

        for (int i = 0; i < audiosPermitidosEnPausa.Length; i++)
        {
            if (audiosPermitidosEnPausa[i] != null)
            {
                audiosPermitidosEnPausa[i].ignoreListenerPause = true;
            }
        }
    }

    private bool HayOtraUIAbierta()
    {
        return CofreInventarioUI.HayAlgunaUIAbierta
            || LoroDialogoUI.HayAlgunaUIAbierta
            || HistoriaInicioLoroUI.HayAlgunaUIAbierta
            || TutorialMisionesLoro.BloquearMenuPausa
            || ConstruccionBarco.HaySecuenciaMejoraBarcoEnCurso;
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