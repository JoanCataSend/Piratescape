using UnityEngine;
using UnityEngine.Audio;

public sealed class AnimationSoundEvent : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioMixerGroup output;
    [SerializeField] private AudioClip sonido;
    [SerializeField] private float volumen = 0.7f;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.outputAudioMixerGroup = output;
    }

    public void ReproducirSonido()
    {
        if (audioSource == null || sonido == null)
        {
            return;
        }

        audioSource.PlayOneShot(sonido, volumen);
    }
}