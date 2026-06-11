using System.Collections;
using UnityEngine;

public class ZoneRandomAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;

    [SerializeField] private float minDelay = 10f;
    [SerializeField] private float maxDelay = 30f;

    [Range(0f, 1f)]
    [SerializeField] private float probability = 0.75f;

    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    private void OnEnable()
    {
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));

            if (audioSource == null)
                continue;

            if (clips == null || clips.Length == 0)
                continue;

            if (Random.value > probability)
                continue;

            AudioClip clip = clips[Random.Range(0, clips.Length)];

            audioSource.pitch = Random.Range(minPitch, maxPitch);

            audioSource.PlayOneShot(clip);
        }
    }
}