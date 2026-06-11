using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public sealed class LoroIdleSound : MonoBehaviour
{
    [Header("Sonidos idle")]
    [SerializeField] private AudioClip[] sonidosIdle;

    [Header("Tiempo entre sonidos")]
    [SerializeField] private float tiempoMinimo = 4f;
    [SerializeField] private float tiempoMaximo = 9f;

    [Header("Volumen")]
    [SerializeField] private float volumen = 0.35f;
    [SerializeField] private float variacionPitch = 0.08f;

    [Header("Sonido 3D")]
    [SerializeField] private float distanciaMinima = 3f;
    [SerializeField] private float distanciaMaxima = 35f;

    [Header("Comportamiento")]
    [SerializeField] private bool silenciarMientrasMenuLoroAbierto = true;

    private AudioSource audioSource;
    private Coroutine rutinaIdle;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        ConfigurarAudioSource();
    }

    private void OnEnable()
    {
        rutinaIdle = StartCoroutine(RutinaIdle());
    }

    private void OnDisable()
    {
        if (rutinaIdle != null)
        {
            StopCoroutine(rutinaIdle);
            rutinaIdle = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    private IEnumerator RutinaIdle()
    {
        while (true)
        {
            float espera = Random.Range(tiempoMinimo, tiempoMaximo);
            yield return new WaitForSeconds(espera);

            if (PuedeReproducirIdle())
            {
                ReproducirIdleAleatorio();
            }
        }
    }

    private bool PuedeReproducirIdle()
    {
        if (sonidosIdle == null || sonidosIdle.Length == 0)
        {
            return false;
        }

        if (!silenciarMientrasMenuLoroAbierto)
        {
            return true;
        }

        return !LoroDialogoUI.HayAlgunaUIAbierta &&
               !HistoriaInicioLoroUI.HayAlgunaUIAbierta;
    }

    private void ReproducirIdleAleatorio()
    {
        AudioClip clip = sonidosIdle[Random.Range(0, sonidosIdle.Length)];

        audioSource.pitch = Random.Range(
            1f - variacionPitch,
            1f + variacionPitch
        );

        audioSource.PlayOneShot(clip, volumen);
    }

    private void ConfigurarAudioSource()
    {
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = distanciaMinima;
        audioSource.maxDistance = distanciaMaxima;
    }
}