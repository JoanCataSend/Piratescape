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

    [Header("Audio gemas")]
    [SerializeField] private AudioClip[] sonidosRecogidaGema;

    [Header("Audio llave")]
    [SerializeField] private AudioClip[] sonidosRecogidaLlave;

    private int ultimoIndice = -1;
    private int ultimoIndiceGema = -1;
    private int ultimoIndiceLlave = -1;

    public void Reproducir(ItemData itemData, Vector3 posicion)
    {
        AudioClip clip;

        if (EsLlave(itemData))
        {
            clip = ObtenerClipAleatorioSinRepetir(
                sonidosRecogidaLlave,
                ref ultimoIndiceLlave);
        }
        else if (EsGema(itemData))
        {
            clip = ObtenerClipAleatorioSinRepetir(
                sonidosRecogidaGema,
                ref ultimoIndiceGema);
        }
        else
        {
            clip = ObtenerClipAleatorioSinRepetir(
                sonidosRecogida,
                ref ultimoIndice);
        }

        if (clip == null)
        {
            return;
        }

        GameObject objetoAudio = new GameObject("SFX_RecogerRecurso");
        objetoAudio.transform.position = posicion;

        AudioSource source = objetoAudio.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = outputSFX;
        source.volume = volumen;
        source.pitch = Random.Range(pitchMinimo, pitchMaximo);
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 1f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = distanciaMinima;
        source.maxDistance = distanciaMaxima;
        source.dopplerLevel = 0f;

        source.Play();

        Destroy(objetoAudio, clip.length + 0.25f);
    }

    private AudioClip ObtenerClipAleatorioSinRepetir(AudioClip[] clips, ref int ultimoIndice)
    {
        if (clips == null || clips.Length == 0)
        {
            return null;
        }

        if (clips.Length == 1)
        {
            ultimoIndice = 0;
            return clips[0];
        }

        int indice;

        do
        {
            indice = Random.Range(0, clips.Length);
        }
        while (indice == ultimoIndice);

        ultimoIndice = indice;
        return clips[indice];
    }

    private bool EsGema(ItemData itemData)
    {
        if (itemData == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(itemData.ItemId) &&
            itemData.ItemId.ToLower().Contains("gema"))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(itemData.DisplayName) &&
               itemData.DisplayName.ToLower().Contains("gema");
    }

    private bool EsLlave(ItemData itemData)
    {
        if (itemData == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(itemData.ItemId) &&
            itemData.ItemId.ToLower().Contains("llave"))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(itemData.DisplayName) &&
               itemData.DisplayName.ToLower().Contains("llave");
    }
}