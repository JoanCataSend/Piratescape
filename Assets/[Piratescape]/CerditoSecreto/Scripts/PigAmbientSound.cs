using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PigAmbientSound : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private PigAI pigAI;

    [Header("Sonidos")]
    [SerializeField] private AudioClip[] sonidosCerdo;

    [Header("Distancias")]
    [SerializeField] private float distanciaEmpezar = 18f;
    [SerializeField] private float distanciaParar = 22f;
    [SerializeField] private float distanciaMinima3D = 2f;
    [SerializeField] private float distanciaMaxima3D = 25f;

    [Header("Tiempo normal")]
    [SerializeField] private float tiempoNormalMinimo = 2.2f;
    [SerializeField] private float tiempoNormalMaximo = 4.5f;

    [Header("Tiempo nervioso")]
    [SerializeField] private float tiempoHuidaMinimo = 0.8f;
    [SerializeField] private float tiempoHuidaMaximo = 1.8f;

    [Header("Oinks normales")]
    [SerializeField] private int oinksNormalesMinimos = 1;
    [SerializeField] private int oinksNormalesMaximos = 2;

    [Header("Oinks nerviosos")]
    [SerializeField] private int oinksHuidaMinimos = 2;
    [SerializeField] private int oinksHuidaMaximos = 4;

    [Header("Pausa entre oinks")]
    [SerializeField] private float pausaNormalMinima = 0.2f;
    [SerializeField] private float pausaNormalMaxima = 0.5f;
    [SerializeField] private float pausaHuidaMinima = 0.08f;
    [SerializeField] private float pausaHuidaMaxima = 0.25f;

    [Header("Volumen")]
    [SerializeField] private float volumenNormal = 0.28f;
    [SerializeField] private float volumenHuida = 0.35f;
    [SerializeField] private float variacionPitch = 0.08f;
    [SerializeField] private float extraPitchHuida = 0.08f;

    private AudioSource audioSource;
    private Coroutine rutinaSonido;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (pigAI == null)
        {
            pigAI = GetComponent<PigAI>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 1f;
        audioSource.minDistance = distanciaMinima3D;
        audioSource.maxDistance = distanciaMaxima3D;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    private void Update()
    {
        if (player == null || sonidosCerdo == null || sonidosCerdo.Length == 0)
        {
            return;
        }

        audioSource.minDistance = distanciaMinima3D;
        audioSource.maxDistance = distanciaMaxima3D;

        float distancia = Vector3.Distance(transform.position, player.position);

        if (rutinaSonido == null && distancia <= distanciaEmpezar)
        {
            rutinaSonido = StartCoroutine(RutinaSonidoCerdo());
        }
        else if (rutinaSonido != null && distancia >= distanciaParar)
        {
            StopCoroutine(rutinaSonido);
            rutinaSonido = null;
            audioSource.Stop();
        }
    }

    private IEnumerator RutinaSonidoCerdo()
    {
        while (true)
        {
            bool estaHuyendo = pigAI != null && pigAI.EstaHuyendo;

            int cantidadOinks = estaHuyendo
                ? Random.Range(oinksHuidaMinimos, oinksHuidaMaximos + 1)
                : Random.Range(oinksNormalesMinimos, oinksNormalesMaximos + 1);

            for (int i = 0; i < cantidadOinks; i++)
            {
                ReproducirSonidoAleatorio(estaHuyendo);

                float pausaEntreOinks = estaHuyendo
                    ? Random.Range(pausaHuidaMinima, pausaHuidaMaxima)
                    : Random.Range(pausaNormalMinima, pausaNormalMaxima);

                yield return new WaitForSeconds(pausaEntreOinks);
            }

            float espera = estaHuyendo
                ? Random.Range(tiempoHuidaMinimo, tiempoHuidaMaximo)
                : Random.Range(tiempoNormalMinimo, tiempoNormalMaximo);

            yield return new WaitForSeconds(espera);
        }
    }

    private void ReproducirSonidoAleatorio(bool estaHuyendo)
    {
        int indice = Random.Range(0, sonidosCerdo.Length);

        float pitchBase = estaHuyendo ? 1f + extraPitchHuida : 1f;
        audioSource.pitch = Random.Range(
            pitchBase - variacionPitch,
            pitchBase + variacionPitch
        );

        float volumenActual = estaHuyendo ? volumenHuida : volumenNormal;

        audioSource.PlayOneShot(sonidosCerdo[indice], volumenActual);
    }
}