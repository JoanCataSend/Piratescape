using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class VictoryCutsceneController : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private Transform barcoFinal;
    [SerializeField] private Transform jugador;

    [Header("Puntos del barco")]
    [SerializeField] private Transform boatStartPoint;
    [SerializeField] private Transform boatEndPoint;
    [SerializeField] private Transform playerBoatPoint;

    [Header("Cámaras de cinemática")]
    [SerializeField] private GameObject camaraMarAIsla;
    [SerializeField] private GameObject camaraIslaAMar;
    [SerializeField] private GameObject camaraJugador;

    [Header("UI cinemática")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private CinematicBarsUI cinematicBars;
    [SerializeField] private GameObject pantallaVictoria;

    [Header("Estadísticas victoria")]
    [SerializeField] private VictoryStatsTypewriterUI victoryStatsTypewriterUI;
    [SerializeField] private bool mostrarEstadisticasDuranteCinematica = true;
    [SerializeField] private float tiempoEsperaMostrarEstadisticas = 6f;

    [Header("Música victoria")]
    [SerializeField] private AudioSource musicaVictoriaSource;
    [SerializeField] private AudioMixerGroup outputMusicaVictoria;
    [SerializeField] private AudioClip musicaVictoria;
    [SerializeField] [Range(0f, 1f)] private float volumenMusicaVictoria = 0.2f;
    [SerializeField] private bool musicaVictoriaEnLoop = true;

    [Header("Postprocesado cinemática")]
    [SerializeField] private GameObject postProcesadoCinematica;

    [Header("Destello final")]
    [SerializeField] private GameObject finalStarSparklePrefab;
    [SerializeField] private Transform finalStarSparklePoint;
    [SerializeField] private bool ocultarBarcoAlFinal = true;
    [SerializeField] private float esperaDespuesDestello = 1.2f;

    [Header("UI a ocultar durante cinemática")]
    [SerializeField] private GameObject[] objetosUIAOcultarDuranteCinematica;

    [Header("Gaviotas")]
    [SerializeField] private GameObject gaviotas;
    [SerializeField] private CinematicSeagullsMovement movimientoGaviotas;

    [Header("Componentes a desactivar del jugador")]
    [SerializeField] private MonoBehaviour[] componentesJugadorADesactivar;
    [SerializeField] private CharacterController characterControllerJugador;

    [Header("Tiempos")]
    [SerializeField] private float esperaAntesDeEmpezar = 2f;
    [SerializeField] private float duracionFadeInicial = 1f;
    [SerializeField] private float tiempoHastaCambioCamara = 5f;
    [SerializeField] private float duracionMovimientoBarco = 13f;
    [SerializeField] private float duracionFadeFinal = 1.5f;

    [Header("Movimiento")]
    [SerializeField] private AnimationCurve curvaMovimiento = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [Header("Estado inicial")]
    [SerializeField] private bool ocultarBarcoAlInicio = true;

    private bool cinematicaEnCurso;
    private bool pantallaVictoriaMostrada;
    private bool[] estadosPreviosUI;

    private void Awake()
    {
        CachearReferencias();
        PrepararEstadoInicial();
    }

    private void CachearReferencias()
    {
        if (victoryStatsTypewriterUI == null && pantallaVictoria != null)
        {
            victoryStatsTypewriterUI = pantallaVictoria.GetComponentInChildren<VictoryStatsTypewriterUI>(true);
        }

        if (musicaVictoriaSource == null)
        {
            musicaVictoriaSource = GetComponent<AudioSource>();
        }

        if (musicaVictoriaSource == null)
        {
            musicaVictoriaSource = gameObject.AddComponent<AudioSource>();
        }

        musicaVictoriaSource.playOnAwake = false;
        musicaVictoriaSource.loop = musicaVictoriaEnLoop;
        musicaVictoriaSource.volume = volumenMusicaVictoria;
        musicaVictoriaSource.outputAudioMixerGroup = outputMusicaVictoria;
    }

    private void PrepararEstadoInicial()
    {
        pantallaVictoriaMostrada = false;

        if (ocultarBarcoAlInicio && barcoFinal != null)
        {
            barcoFinal.gameObject.SetActive(false);
        }

        if (camaraMarAIsla != null)
        {
            camaraMarAIsla.SetActive(false);
        }

        if (camaraIslaAMar != null)
        {
            camaraIslaAMar.SetActive(false);
        }

        if (gaviotas != null)
        {
            gaviotas.SetActive(false);
        }

        if (movimientoGaviotas != null)
        {
            movimientoGaviotas.DetenerMovimiento();
        }

        if (pantallaVictoria != null)
        {
            pantallaVictoria.SetActive(false);
        }

        if (postProcesadoCinematica != null)
        {
            postProcesadoCinematica.SetActive(false);
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }

        DetenerMusicaVictoria();
    }

    public void StartVictoryCutsceneAfterDelay(float delay)
    {
        if (cinematicaEnCurso)
        {
            return;
        }

        StartCoroutine(VictoryCutsceneRoutine(delay));
    }

    public void StartVictoryCutscene()
    {
        StartVictoryCutsceneAfterDelay(esperaAntesDeEmpezar);
    }

    private IEnumerator VictoryCutsceneRoutine(float delay)
    {
        cinematicaEnCurso = true;
        pantallaVictoriaMostrada = false;

        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        ReproducirMusicaVictoria();

        OcultarUIDelJuego();
        PrepararUIInicial();

        yield return Fade(0f, 1f, duracionFadeInicial);

        PrepararEscenaCinematica();

        if (cinematicBars != null)
        {
            cinematicBars.ShowBars();
        }

        ActivarCamaraDirecta(camaraMarAIsla);

        yield return Fade(1f, 0f, duracionFadeInicial);

        if (mostrarEstadisticasDuranteCinematica)
        {
            StartCoroutine(MostrarPantallaVictoriaConRetardo());
        }

        yield return MoverBarcoConCambioDeCamara();

        ReproducirDestelloFinal();

        if (esperaDespuesDestello > 0f)
        {
            yield return new WaitForSeconds(esperaDespuesDestello);
        }

        yield return Fade(0f, 1f, duracionFadeFinal);

        MostrarPantallaVictoria();

        cinematicaEnCurso = false;
    }

    private void ReproducirMusicaVictoria()
    {
        if (musicaVictoriaSource == null || musicaVictoria == null)
        {
            return;
        }

        musicaVictoriaSource.clip = musicaVictoria;
        musicaVictoriaSource.loop = musicaVictoriaEnLoop;
        musicaVictoriaSource.volume = volumenMusicaVictoria;

        if (!musicaVictoriaSource.isPlaying)
        {
            musicaVictoriaSource.Play();
        }
    }

    private void DetenerMusicaVictoria()
    {
        if (musicaVictoriaSource != null && musicaVictoriaSource.isPlaying)
        {
            musicaVictoriaSource.Stop();
        }
    }

    private IEnumerator MostrarPantallaVictoriaConRetardo()
    {
        if (tiempoEsperaMostrarEstadisticas > 0f)
        {
            yield return new WaitForSeconds(tiempoEsperaMostrarEstadisticas);
        }

        MostrarPantallaVictoriaUI();
    }

    private void OcultarUIDelJuego()
    {
        if (objetosUIAOcultarDuranteCinematica == null)
        {
            return;
        }

        estadosPreviosUI = new bool[objetosUIAOcultarDuranteCinematica.Length];

        for (int i = 0; i < objetosUIAOcultarDuranteCinematica.Length; i++)
        {
            if (objetosUIAOcultarDuranteCinematica[i] != null)
            {
                estadosPreviosUI[i] = objetosUIAOcultarDuranteCinematica[i].activeSelf;
                objetosUIAOcultarDuranteCinematica[i].SetActive(false);
            }
        }
    }

    private void PrepararUIInicial()
    {
        if (postProcesadoCinematica != null)
        {
            postProcesadoCinematica.SetActive(true);
        }

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = true;
            fadeCanvasGroup.interactable = false;
        }

        if (pantallaVictoria != null)
        {
            pantallaVictoria.SetActive(false);
        }

        if (gaviotas != null)
        {
            gaviotas.SetActive(false);
        }

        if (movimientoGaviotas != null)
        {
            movimientoGaviotas.DetenerMovimiento();
        }
    }

    private void PrepararEscenaCinematica()
    {
        if (barcoFinal != null)
        {
            barcoFinal.gameObject.SetActive(true);
        }

        if (barcoFinal != null && boatStartPoint != null)
        {
            barcoFinal.position = boatStartPoint.position;
            barcoFinal.rotation = boatStartPoint.rotation;
        }

        DesactivarControlJugador();

        if (jugador != null && playerBoatPoint != null)
        {
            jugador.SetParent(playerBoatPoint);
            jugador.localPosition = Vector3.zero;
            jugador.localRotation = Quaternion.identity;
        }

        DesactivarTodasLasCamarasCinematicas();
    }

    private void DesactivarControlJugador()
    {
        if (componentesJugadorADesactivar != null)
        {
            for (int i = 0; i < componentesJugadorADesactivar.Length; i++)
            {
                if (componentesJugadorADesactivar[i] != null)
                {
                    componentesJugadorADesactivar[i].enabled = false;
                }
            }
        }

        if (characterControllerJugador != null)
        {
            characterControllerJugador.enabled = false;
        }
    }

    private void DesactivarTodasLasCamarasCinematicas()
    {
        if (camaraMarAIsla != null)
        {
            camaraMarAIsla.SetActive(false);
        }

        if (camaraIslaAMar != null)
        {
            camaraIslaAMar.SetActive(false);
        }
    }

    private void ActivarCamaraDirecta(GameObject camaraActiva)
    {
        if (camaraJugador != null)
        {
            camaraJugador.SetActive(false);
        }

        if (camaraMarAIsla != null)
        {
            camaraMarAIsla.SetActive(false);
        }

        if (camaraIslaAMar != null)
        {
            camaraIslaAMar.SetActive(false);
        }

        if (camaraActiva != null)
        {
            camaraActiva.SetActive(true);
        }
    }

    private IEnumerator MoverBarcoConCambioDeCamara()
    {
        if (barcoFinal == null || boatStartPoint == null || boatEndPoint == null)
        {
            yield break;
        }

        Vector3 posicionInicio = boatStartPoint.position;
        Vector3 posicionFinal = boatEndPoint.position;

        Quaternion rotacionInicio = boatStartPoint.rotation;
        Quaternion rotacionFinal = boatEndPoint.rotation;

        float tiempo = 0f;
        bool camaraCambiada = false;

        while (tiempo < duracionMovimientoBarco)
        {
            tiempo += Time.deltaTime;

            if (!camaraCambiada && tiempo >= tiempoHastaCambioCamara)
            {
                camaraCambiada = true;

                ActivarCamaraDirecta(camaraIslaAMar);
                ActivarGaviotas();
            }

            float t = Mathf.Clamp01(tiempo / duracionMovimientoBarco);
            float tCurva = curvaMovimiento != null ? curvaMovimiento.Evaluate(t) : t;

            barcoFinal.position = Vector3.Lerp(posicionInicio, posicionFinal, tCurva);
            barcoFinal.rotation = Quaternion.Slerp(rotacionInicio, rotacionFinal, tCurva);

            yield return null;
        }

        barcoFinal.position = posicionFinal;
        barcoFinal.rotation = rotacionFinal;
    }

    private void ReproducirDestelloFinal()
    {
        Vector3 posicionDestello;

        if (finalStarSparklePoint != null)
        {
            posicionDestello = finalStarSparklePoint.position;
        }
        else if (boatEndPoint != null)
        {
            posicionDestello = boatEndPoint.position;
        }
        else if (barcoFinal != null)
        {
            posicionDestello = barcoFinal.position;
        }
        else
        {
            Debug.LogWarning("VictoryCutsceneController: no hay punto para crear el destello final.");
            return;
        }

        if (finalStarSparklePrefab != null)
        {
            GameObject destello = Instantiate(
                finalStarSparklePrefab,
                posicionDestello,
                Quaternion.identity
            );

            destello.SetActive(true);

            OrientarDestelloACamaraActiva(destello);

            Debug.Log("Destello final creado en: " + posicionDestello);
        }
        else
        {
            Debug.LogWarning("VictoryCutsceneController: falta asignar Final Star Sparkle Prefab.");
        }

        if (ocultarBarcoAlFinal && barcoFinal != null)
        {
            barcoFinal.gameObject.SetActive(false);
        }

        if (movimientoGaviotas != null)
        {
            movimientoGaviotas.DetenerMovimiento();
        }
    }

    private void OrientarDestelloACamaraActiva(GameObject destello)
    {
        if (destello == null)
        {
            return;
        }

        Camera camara = ObtenerCamaraActiva();

        if (camara == null)
        {
            return;
        }

        destello.transform.forward = camara.transform.forward;
    }

    private Camera ObtenerCamaraActiva()
    {
        Camera[] camaras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        for (int i = 0; i < camaras.Length; i++)
        {
            if (camaras[i] != null && camaras[i].gameObject.activeInHierarchy && camaras[i].enabled)
            {
                return camaras[i];
            }
        }

        return Camera.main;
    }

    private void ActivarGaviotas()
    {
        if (gaviotas != null)
        {
            gaviotas.SetActive(true);
        }

        if (movimientoGaviotas != null && barcoFinal != null)
        {
            movimientoGaviotas.IniciarMovimiento(barcoFinal);
        }
    }

    private IEnumerator Fade(float desde, float hasta, float duracion)
    {
        if (fadeCanvasGroup == null)
        {
            yield break;
        }

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.blocksRaycasts = true;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);
            float tSuave = Mathf.SmoothStep(0f, 1f, t);

            fadeCanvasGroup.alpha = Mathf.Lerp(desde, hasta, tSuave);

            yield return null;
        }

        fadeCanvasGroup.alpha = hasta;

        if (Mathf.Approximately(hasta, 0f))
        {
            fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void MostrarPantallaVictoria()
    {
        if (movimientoGaviotas != null)
        {
            movimientoGaviotas.DetenerMovimiento();
        }

        if (camaraMarAIsla != null)
        {
            camaraMarAIsla.SetActive(false);
        }

        if (camaraIslaAMar != null)
        {
            camaraIslaAMar.SetActive(false);
        }

        MostrarPantallaVictoriaUI();
    }

    private void MostrarPantallaVictoriaUI()
    {
        if (pantallaVictoriaMostrada)
        {
            return;
        }

        pantallaVictoriaMostrada = true;

        if (victoryStatsTypewriterUI == null && pantallaVictoria != null)
        {
            victoryStatsTypewriterUI = pantallaVictoria.GetComponentInChildren<VictoryStatsTypewriterUI>(true);
        }

        if (victoryStatsTypewriterUI != null)
        {
            victoryStatsTypewriterUI.ConfigurarEstadisticasFalsas();
        }

        if (pantallaVictoria != null)
        {
            pantallaVictoria.SetActive(true);
        }
    }
}