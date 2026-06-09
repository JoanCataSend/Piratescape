using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public sealed class ReproducirCinematicaFinal : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector playableDirector;

    [Header("Inicio automático")]
    [Tooltip("Actívalo para que la cinemática comience al cargar esta escena.")]
    [SerializeField] private bool iniciarAlCargarEscena = true;

    [Tooltip("Pequeña espera para que todos los objetos de la escena se inicialicen.")]
    [SerializeField] private float retrasoInicio = 0.1f;

    private bool cinematicaIniciada;
    private Coroutine rutinaInicio;

    private void Awake()
    {
        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }
    }

    private void Start()
    {
        if (iniciarAlCargarEscena)
        {
            rutinaInicio = StartCoroutine(IniciarAlCargarRoutine());
        }
    }

    private IEnumerator IniciarAlCargarRoutine()
    {
        yield return null;

        if (retrasoInicio > 0f)
        {
            yield return new WaitForSecondsRealtime(retrasoInicio);
        }

        rutinaInicio = null;
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

        playableDirector.time = 0;
        playableDirector.Evaluate();
        playableDirector.Play();
    }

    public void ReiniciarCinematica()
    {
        if (playableDirector == null)
        {
            return;
        }

        cinematicaIniciada = false;

        playableDirector.Stop();
        playableDirector.time = 0;
        playableDirector.Evaluate();

        IniciarCinematica();
    }

    private void OnDisable()
    {
        if (rutinaInicio != null)
        {
            StopCoroutine(rutinaInicio);
            rutinaInicio = null;
        }
    }
}