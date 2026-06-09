using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public sealed class ReproducirCinematicaFinal : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector playableDirector;

    [Header("Transición")]
    [SerializeField] private SleepFadeUI fadeUI;

    [Tooltip("Espera adicional después del fade de entrada antes de iniciar el Timeline.")]
    [Min(0f)]
    [SerializeField] private float retrasoAntesTimeline = 0.1f;

    [Header("Escena siguiente")]
    [Tooltip("Actívalo si al terminar esta cinemática debe cargarse otra escena.")]
    [SerializeField] private bool cargarEscenaAlTerminar = true;

    [Tooltip("Nombre exacto de la siguiente escena, sin .unity.")]
    [SerializeField] private string nombreEscenaSiguiente = "FinalAlternativo";

    private bool cinematicaIniciada;
    private bool finalProcesandose;
    private Coroutine rutinaPrincipal;

    private void Awake()
    {
        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        if (fadeUI == null)
        {
            fadeUI = FindFirstObjectByType<SleepFadeUI>();
        }

        /*
         * Dejamos la escena completamente cubierta antes
         * de que empiece la cinemática.
         */
        if (fadeUI != null)
        {
            fadeUI.SetClosedImmediate();
        }
    }

    private void OnEnable()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped += AlTerminarTimeline;
        }
    }

    private void Start()
    {
        rutinaPrincipal = StartCoroutine(
            IniciarSecuenciaCompleta()
        );
    }

    private IEnumerator IniciarSecuenciaCompleta()
    {
        // Dejamos que todos los objetos de la escena se inicialicen.
        yield return null;

        if (fadeUI != null)
        {
            yield return fadeUI.FadeInRoutine();
        }

        if (retrasoAntesTimeline > 0f)
        {
            yield return new WaitForSecondsRealtime(
                retrasoAntesTimeline
            );
        }

        rutinaPrincipal = null;
        IniciarCinematica();
    }

    public void IniciarCinematica()
    {
        if (cinematicaIniciada)
        {
            return;
        }

        if (playableDirector == null)
        {
            Debug.LogWarning(
                "ReproducirCinematicaFinal: falta asignar el Playable Director.",
                this
            );

            return;
        }

        cinematicaIniciada = true;
        finalProcesandose = false;

        playableDirector.time = 0;
        playableDirector.Evaluate();
        playableDirector.Play();
    }

    private void AlTerminarTimeline(
        PlayableDirector director
    )
    {
        if (!cinematicaIniciada || finalProcesandose)
        {
            return;
        }

        finalProcesandose = true;
        StartCoroutine(FinalizarSecuencia());
    }

    private IEnumerator FinalizarSecuencia()
    {
        if (fadeUI != null)
        {
            yield return fadeUI.FadeOutRoutine();
        }

        if (!cargarEscenaAlTerminar)
        {
            yield break;
        }

        if (string.IsNullOrWhiteSpace(nombreEscenaSiguiente))
        {
            Debug.LogWarning(
                "ReproducirCinematicaFinal: falta indicar la escena siguiente.",
                this
            );

            yield break;
        }

        SceneManager.LoadScene(nombreEscenaSiguiente);
    }

    public void ReiniciarCinematica()
    {
        if (playableDirector == null)
        {
            return;
        }

        StopAllCoroutines();

        cinematicaIniciada = false;
        finalProcesandose = false;

        playableDirector.Stop();
        playableDirector.time = 0;
        playableDirector.Evaluate();

        rutinaPrincipal = StartCoroutine(
            IniciarSecuenciaCompleta()
        );
    }

    private void OnDisable()
    {
        if (playableDirector != null)
        {
            playableDirector.stopped -= AlTerminarTimeline;
        }

        if (rutinaPrincipal != null)
        {
            StopCoroutine(rutinaPrincipal);
            rutinaPrincipal = null;
        }
    }
}