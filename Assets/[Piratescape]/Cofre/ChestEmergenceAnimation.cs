using System.Collections;
using UnityEngine;

public class ChestEmergenceAnimation : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform chest;
    [SerializeField] private ParticleSystem sandBurstParticles;
    [SerializeField] private ParticleSystem sandFallParticles;

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

        // El cofre empieza enterrado
        chest.localPosition = finalLocalPosition + Vector3.up * buriedOffsetY;
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
        // Espera antes de empezar la animación
        yield return new WaitForSeconds(delayBeforeStart);

        if (sandBurstParticles != null)
            sandBurstParticles.Play();

        yield return new WaitForSeconds(0.08f);

        if (sandFallParticles != null)
            sandFallParticles.Play();

        Vector3 startLocalPosition = finalLocalPosition + Vector3.up * buriedOffsetY;
        float elapsed = 0f;

        while (elapsed < riseDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / riseDuration);
            float curveValue = riseCurve.Evaluate(t);

            chest.localPosition = Vector3.LerpUnclamped(startLocalPosition, finalLocalPosition, curveValue);

            yield return null;
        }

        chest.localPosition = finalLocalPosition;

        if (sandFallParticles != null)
        {
            yield return new WaitForSeconds(0.25f);
            sandFallParticles.Stop();
        }
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
}