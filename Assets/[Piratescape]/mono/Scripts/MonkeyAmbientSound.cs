using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MonkeyAmbientSound : MonoBehaviour
{
    [Header("Ambiente")]
    [SerializeField] private AudioClip[] sonidosMovimiento;

    [Header("Chillidos")]
    [SerializeField] private AudioClip[] sonidosGrito;

    [Header("Volumen")]
    [SerializeField] private float volumenMovimiento = 0.15f;
    [SerializeField] private float volumenGrito = 0.5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        StartCoroutine(RutinaMonos());
    }

    private IEnumerator RutinaMonos()
    {
        while (true)
        {
            if (Random.value < 0.7f)
            {
                ReproducirMovimiento();
            }

            if (Random.value < 0.35f)
            {
                yield return new WaitForSeconds(
                    Random.Range(0.5f, 2f));

                ReproducirGrito();
            }

            yield return new WaitForSeconds(
                Random.Range(2f, 5f));
        }
    }

    private void ReproducirMovimiento()
    {
        if (sonidosMovimiento.Length == 0)
            return;

        AudioClip clip =
            sonidosMovimiento[
                Random.Range(0, sonidosMovimiento.Length)];

        audioSource.PlayOneShot(
            clip,
            volumenMovimiento);
    }

    private void ReproducirGrito()
    {
        if (sonidosGrito.Length == 0)
            return;

        AudioClip clip =
            sonidosGrito[
                Random.Range(0, sonidosGrito.Length)];

        audioSource.PlayOneShot(
            clip,
            volumenGrito);
    }
}