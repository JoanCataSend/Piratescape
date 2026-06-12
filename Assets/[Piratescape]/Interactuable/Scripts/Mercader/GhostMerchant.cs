using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

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

    [Header("Sonido tienda")]
    [SerializeField] private AudioSource audioSourceTienda;
    [SerializeField] private AudioMixerGroup outputTienda;
    [SerializeField] private AudioClip sonidoAbrirTienda;
    [SerializeField] private float volumenAbrirTienda = 0.6f;

    [Header("Intro primera vez")]
    [SerializeField] private bool usarIntroPrimeraVez = true;

    [Tooltip("Si está activado, la intro del fantasma saldrá siempre al pulsar E, aunque ya se haya visto antes.")]
    [SerializeField] private bool forzarIntroSiempre = false;

    [SerializeField] private bool abrirTiendaAlTerminarIntro = true;
    [SerializeField] private GhostIntroDialogueUI introDialogueUI;
    [SerializeField] private GhostSpawn ghostSpawn;
    [SerializeField] private GameObject camaraDialogoFantasma;
    [SerializeField] private GameObject camaraJugador;
    [SerializeField] private MonoBehaviour[] componentesJugadorADesactivar;

    [Header("Colocación intro")]
    [SerializeField] private bool colocarFantasmaFrenteAlJugador = true;
    [SerializeField] private float distanciaFantasmaAlJugador = 2f;

    [Header("UI a ocultar durante intro")]
    [SerializeField] private GameObject[] objetosUIAOcultarDuranteIntro;

    [Header("Protección al abrir tienda")]
    [SerializeField] private float tiempoBloqueoCierreAlAbrirTienda = 0.35f;

    [SerializeField] private string clavePlayerPrefsIntro = "GhostIntroVista";

    [Header("Mensaje")]
    [SerializeField] private string mensaje = "Pulsa E para comerciar";

    private bool activo;
    private bool introEnCurso;
    private IActivador jugador;
    private int ultimoFrameInteraccion = -1;
    private bool[] estadosPreviosUIIntro;

    private float bloquearCierreTiendaHasta;
    private int bloquearInteraccionHastaFrame;

    public bool Activo => activo && interactuable && !introEnCurso;

    private void Awake()
    {
        BuscarReferenciasSiFaltan();
        PrepararAudioTienda();
    }

    private void OnEnable()
    {
        BuscarReferenciasSiFaltan();
        activo = false;
        introEnCurso = false;

        if (shopUI != null)
        {
            shopUI.SetActive(false);
        }

        if (camaraDialogoFantasma != null)
        {
            camaraDialogoFantasma.SetActive(false);
        }
    }

    private void OnDisable()
    {
        OcultarPrompt();

        if (!EstaTiendaEnBloqueoDeCierre())
        {
            CerrarTienda(false);
        }

        FinalizarIntroSinAbrirTienda();
    }

    private void Update()
    {
        BuscarReferenciasSiFaltan();

        if (jugador == null || introEnCurso)
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

                if (!EstaTiendaEnBloqueoDeCierre())
                {
                    CerrarTienda(false);
                }
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

        if (Time.frameCount <= bloquearInteraccionHastaFrame)
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

        if (DebeMostrarIntro())
        {
            StartCoroutine(IntroFantasmaRoutine());
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

    private bool DebeMostrarIntro()
    {
        if (!usarIntroPrimeraVez || introDialogueUI == null)
        {
            return false;
        }

        if (forzarIntroSiempre)
        {
            return true;
        }

        return PlayerPrefs.GetInt(clavePlayerPrefsIntro, 0) == 0;
    }

    private IEnumerator IntroFantasmaRoutine()
    {
        introEnCurso = true;

        OcultarPrompt();
        OcultarUIIntro();

        if (ghostSpawn != null)
        {
            ghostSpawn.BloquearMovimiento(true);

            if (jugador != null)
            {
                Transform transformJugador = ObtenerTransformJugador();

                if (colocarFantasmaFrenteAlJugador)
                {
                    ghostSpawn.ColocarFrenteAlJugador(
                        jugador.Position,
                        transformJugador,
                        distanciaFantasmaAlJugador
                    );
                }
                else
                {
                    ghostSpawn.MirarHacia(jugador.Position);
                }
            }
        }

        CambiarEstadoComponentesJugador(false);

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(false);
        }

        if (camaraDialogoFantasma != null)
        {
            camaraDialogoFantasma.SetActive(true);
        }

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        bool dialogoTerminado = false;
        bool quiereComerciar = false;

        introDialogueUI.IniciarDialogo(resultadoComercio =>
        {
            quiereComerciar = resultadoComercio;
            dialogoTerminado = true;
        });

        while (!dialogoTerminado)
        {
            yield return null;
        }

        PlayerPrefs.SetInt(clavePlayerPrefsIntro, 1);
        PlayerPrefs.Save();

        FinalizarIntroVisual();
        RestaurarUIIntro();

        if (quiereComerciar && abrirTiendaAlTerminarIntro)
        {
            yield return null;

            bloquearInteraccionHastaFrame = Time.frameCount + 10;
            bloquearCierreTiendaHasta = Time.unscaledTime + tiempoBloqueoCierreAlAbrirTienda;

            AbrirTienda();
        }
        else
        {
            Time.timeScale = 1f;
            introEnCurso = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Activo)
            {
                MostrarPrompt();
            }
        }
    }

    private Transform ObtenerTransformJugador()
    {
        if (jugador is MonoBehaviour monoBehaviourJugador)
        {
            return monoBehaviourJugador.transform;
        }

        return null;
    }

    private void FinalizarIntroVisual()
    {
        if (camaraDialogoFantasma != null)
        {
            camaraDialogoFantasma.SetActive(false);
        }

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(true);
        }

        if (ghostSpawn != null)
        {
            ghostSpawn.BloquearMovimiento(false);
        }

        CambiarEstadoComponentesJugador(true);

        introEnCurso = false;
    }

    private void FinalizarIntroSinAbrirTienda()
    {
        if (!introEnCurso)
        {
            return;
        }

        FinalizarIntroVisual();
        RestaurarUIIntro();

        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OcultarUIIntro()
    {
        if (objetosUIAOcultarDuranteIntro == null)
        {
            return;
        }

        estadosPreviosUIIntro = new bool[objetosUIAOcultarDuranteIntro.Length];

        for (int i = 0; i < objetosUIAOcultarDuranteIntro.Length; i++)
        {
            if (objetosUIAOcultarDuranteIntro[i] != null)
            {
                estadosPreviosUIIntro[i] = objetosUIAOcultarDuranteIntro[i].activeSelf;
                objetosUIAOcultarDuranteIntro[i].SetActive(false);
            }
        }
    }

    private void RestaurarUIIntro()
    {
        if (objetosUIAOcultarDuranteIntro == null || estadosPreviosUIIntro == null)
        {
            return;
        }

        for (int i = 0; i < objetosUIAOcultarDuranteIntro.Length; i++)
        {
            if (objetosUIAOcultarDuranteIntro[i] != null)
            {
                objetosUIAOcultarDuranteIntro[i].SetActive(estadosPreviosUIIntro[i]);
            }
        }

        estadosPreviosUIIntro = null;
    }

    public void AbrirTienda()
    {
        if (shopUI == null)
        {
            return;
        }

        bloquearInteraccionHastaFrame = Time.frameCount + 10;
        bloquearCierreTiendaHasta = Time.unscaledTime + tiempoBloqueoCierreAlAbrirTienda;

        shopUI.SetActive(true);
        ReproducirSonidoAbrirTienda();
        OcultarPrompt();

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("GhostMerchant: tienda abierta.");
    }

    public void CerrarTienda(bool mostrarPrompt)
    {
        if (EstaTiendaEnBloqueoDeCierre())
        {
            return;
        }

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

    private bool EstaTiendaEnBloqueoDeCierre()
    {
        return Time.unscaledTime < bloquearCierreTiendaHasta;
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

        if (ghostSpawn == null)
        {
            ghostSpawn = GetComponent<GhostSpawn>();
        }

        if (introDialogueUI == null)
        {
            introDialogueUI = FindFirstObjectByType<GhostIntroDialogueUI>(FindObjectsInactive.Include);
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

    private void CambiarEstadoComponentesJugador(bool valor)
    {
        if (componentesJugadorADesactivar == null)
        {
            return;
        }

        for (int i = 0; i < componentesJugadorADesactivar.Length; i++)
        {
            if (componentesJugadorADesactivar[i] != null)
            {
                componentesJugadorADesactivar[i].enabled = valor;
            }
        }
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

    [ContextMenu("Resetear intro fantasma")]
    private void ResetearIntroFantasma()
    {
        PlayerPrefs.DeleteKey(clavePlayerPrefsIntro);
        PlayerPrefs.Save();

        Debug.Log("GhostMerchant: intro del fantasma reseteada.");
    }

    private void PrepararAudioTienda()
    {
        if (audioSourceTienda == null)
        {
            audioSourceTienda = gameObject.AddComponent<AudioSource>();
        }

        audioSourceTienda.playOnAwake = false;
        audioSourceTienda.loop = false;
        audioSourceTienda.spatialBlend = 0f;
        audioSourceTienda.outputAudioMixerGroup = outputTienda;
    }

    private void ReproducirSonidoAbrirTienda()
    {
        if (audioSourceTienda == null || sonidoAbrirTienda == null)
        {
            return;
        }

        audioSourceTienda.pitch = 1f;
        audioSourceTienda.PlayOneShot(sonidoAbrirTienda, volumenAbrirTienda);
    }
}