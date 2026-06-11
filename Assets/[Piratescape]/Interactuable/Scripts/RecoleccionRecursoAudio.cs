using UnityEngine;
using UnityEngine.Audio;

public sealed class RecoleccionRecursoAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip[] sonidosRecogida;
    [SerializeField] private AudioMixerGroup outputSFX;

    [Header("Volumen")]
    [SerializeField] private float volumen = 0.45f;
    [SerializeField] private float pitchMinimo = 0.95f;
    [SerializeField] private float pitchMaximo = 1.05f;

    [Header("Sonido 3D")]
    [SerializeField] private float distanciaMinima = 1f;
    [SerializeField] private float distanciaMaxima = 18f;

    private int ultimoIndice = -1;

    public void Reproducir(Vector3 posicion)
    {
        Debug.Log("Intentando reproducir sonido de recogida en: " + posicion, this);

        AudioClip clip = ObtenerClipAleatorioSinRepetir();

        if (clip == null)
        {
            Debug.LogWarning("No se ha encontrado clip para reproducir.", this);
            return;
        }

        Debug.Log("Clip elegido: " + clip.name, this);

        GameObject objetoAudio = new GameObject("SFX_RecogerRecurso");
        objetoAudio.transform.position = posicion;

        AudioSource source = objetoAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = outputSFX;
        source.volume = volumen;
        source.pitch = Random.Range(pitchMinimo, pitchMaximo);
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = distanciaMinima;
        source.maxDistance = distanciaMaxima;
        source.dopplerLevel = 0f;

        source.Play();

        Destroy(objetoAudio, clip.length + 0.25f);
    }

    private AudioClip ObtenerClipAleatorioSinRepetir()
    {
        if (sonidosRecogida == null || sonidosRecogida.Length == 0)
        {
            return null;
        }

        if (sonidosRecogida.Length == 1)
        {
            ultimoIndice = 0;
            return sonidosRecogida[0];
        }

        int indice;

        do
        {
            indice = Random.Range(0, sonidosRecogida.Length);
        }
        while (indice == ultimoIndice);

        ultimoIndice = indice;
        return sonidosRecogida[indice];
    }
}