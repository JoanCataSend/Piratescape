using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class ChestEmergenceAnimation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform chest;
    [SerializeField] private ParticleSystem sandBurstParticles;
    [SerializeField] private ParticleSystem sandFallParticles;

    [Header("Sonido")]
    [SerializeField] private AudioSource audioSourceEmergencia;
    [SerializeField] private AudioClip sonidoEmergencia;
    [SerializeField] private float volumenEmergencia = 0.8f;
    [SerializeField] private AudioMixerGroup outputEmergencia;

    [Header("Animación del cofre")]
    [SerializeField] private float delayBeforeStart = 5f;
    [SerializeField] private float buriedOffsetY = -1.2f;
    [SerializeField] private float riseDuration = 1.2f;
    [SerializeField] private AnimationCurve riseCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.35f, 0.8f),
        new Keyframe(0.75f, 1.05f),
        new Keyframe(1f, 1f)
    );

    [Header("Opcional")]
    [SerializeField] private bool playOnStart = false;

    private Vector3 finalLocalPosition;
    private bool hasPlayed = false;

    private void Awake()
    {
        if (chest == null)
            chest = transform;

        finalLocalPosition = chest.localPosition;

        // El cofre/puerta empieza enterrado o abajo
        chest.localPosition = finalLocalPosition + Vector3.up * buriedOffsetY;

        // Nos aseguramos de que las partículas NO empiecen al iniciar la escena
        if (sandBurstParticles != null)
            sandBurstParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (sandFallParticles != null)
            sandFallParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        PrepararAudioSourceEmergencia();
    }

    private void Start()
    {
        if (playOnStart)
        {
            PlayEmergence();
        }
    }

    public void PlayEmergence()
    {
        if (hasPlayed) return;
        hasPlayed = true;

        StartCoroutine(EmergenceRoutine());
    }

    private IEnumerator EmergenceRoutine()
    {
        // De momento mantenemos el delay porque lo tienes para pruebas.
        // Las partículas NO empiezan aquí.
        yield return new WaitForSeconds(delayBeforeStart);

        Vector3 startLocalPosition = finalLocalPosition + Vector3.up * buriedOffsetY;
        float elapsed = 0f;

        ReproducirSonidoEmergencia();

        // JUSTO AQUÍ empieza a salir el objeto.
        // Por eso las partículas empiezan aquí también.
        if (sandBurstParticles != null)
            sandBurstParticles.Play();

        if (sandFallParticles != null)
            sandFallParticles.Play();

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);
            float curveValue = riseCurve.Evaluate(t);

            chest.localPosition = Vector3.LerpUnclamped(startLocalPosition, finalLocalPosition, curveValue);

            yield return null;
        }

        chest.localPosition = finalLocalPosition;

        // Cuando el objeto ya está fuera, las partículas dejan de emitir.
        // No las borra de golpe, deja que las que ya existen terminen de caer.
        if (sandBurstParticles != null)
            sandBurstParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);

        if (sandFallParticles != null)
            sandFallParticles.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void ResetChest()
    {
        hasPlayed = false;
        chest.localPosition = finalLocalPosition + Vector3.up * buriedOffsetY;

        if (sandBurstParticles != null)
            sandBurstParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (sandFallParticles != null)
            sandFallParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void PrepararAudioSourceEmergencia()
    {
        if (audioSourceEmergencia == null)
        {
            audioSourceEmergencia = gameObject.AddComponent<AudioSource>();
        }

        audioSourceEmergencia.playOnAwake = false;
        audioSourceEmergencia.loop = false;
        audioSourceEmergencia.spatialBlend = 1f;
        audioSourceEmergencia.minDistance = 4f;
        audioSourceEmergencia.maxDistance = 45f;
        audioSourceEmergencia.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSourceEmergencia.dopplerLevel = 0f;
        audioSourceEmergencia.outputAudioMixerGroup = outputEmergencia;
    }

    private void ReproducirSonidoEmergencia()
    {
        Debug.Log("Intentando reproducir sonido emergencia.", this);

        if (audioSourceEmergencia == null)
        {
            Debug.LogWarning("Falta AudioSource Emergencia.", this);
            return;
        }

        if (sonidoEmergencia == null)
        {
            Debug.LogWarning("Falta Sonido Emergencia.", this);
            return;
        }

        Debug.Log("Sonido emergencia: " + sonidoEmergencia.name, this);

        audioSourceEmergencia.PlayOneShot(sonidoEmergencia, volumenEmergencia);
    }
}