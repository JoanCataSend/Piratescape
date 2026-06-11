using UnityEngine;
using UnityEngine.Audio;

public sealed class PlayerJumpAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [Tooltip("Fuente utilizada para la voz del pirata.")]
    [SerializeField] private AudioSource audioSourceVoz;

    [Tooltip("Fuente utilizada para el sonido de viento del salto.")]
    [SerializeField] private AudioSource audioSourceWhoosh;

    [Tooltip("Fuente utilizada para el aterrizaje.")]
    [SerializeField] private AudioSource audioSourceAterrizaje;

    [Header("Outputs Audio Mixer")]
    [Tooltip("Output para voces del pirata: woop, esfuerzo, gruñido, etc.")]
    [SerializeField] private AudioMixerGroup outputVoices;

    [Tooltip("Output para sonidos del jugador durante el salto.")]
    [SerializeField] private AudioMixerGroup outputPlayerSalto;

    [Tooltip("Output para sonidos de aterrizaje del jugador.")]
    [SerializeField] private AudioMixerGroup outputPlayerAterrizaje;

    [Header("Voz al saltar")]
    [Tooltip("Sonidos tipo woop, esfuerzo, gruñido, etc.")]
    [SerializeField] private AudioClip[] sonidosVozSalto;

    [Range(0f, 1f)]
    [SerializeField] private float volumenVoz = 0.7f;

    [Header("Whoosh al saltar")]
    [Tooltip("Sonidos de aire o viento al impulsarse.")]
    [SerializeField] private AudioClip[] sonidosWhoosh;

    [Range(0f, 1f)]
    [SerializeField] private float volumenWhoosh = 0.55f;

    [Header("Aterrizaje en arena")]
    [Tooltip("Sonidos pff al caer sobre la arena.")]
    [SerializeField] private AudioClip[] sonidosAterrizajeArena;

    [Header("Aterrizaje en hierba")]
    [Tooltip("Sonidos pff al caer sobre hierba.")]
    [SerializeField] private AudioClip[] sonidosAterrizajeHierba;

    [Range(0f, 1f)]
    [SerializeField] private float volumenAterrizaje = 0.7f;

    [Header("Detección del suelo")]
    [Tooltip("La misma capa que utilizas para la arena.")]
    [SerializeField] private LayerMask sandLayer;

    [Tooltip("En tu proyecto puedes asignar aquí la capa MiniMap.")]
    [SerializeField] private LayerMask grassLayer;

    [SerializeField] private float alturaRaycast = 0.3f;
    [SerializeField] private float distanciaRaycast = 1.8f;

    [Header("Variación")]
    [SerializeField] private float pitchMinimo = 0.96f;
    [SerializeField] private float pitchMaximo = 1.04f;

    private int ultimoIndiceVoz = -1;
    private int ultimoIndiceWhoosh = -1;
    private int ultimoIndiceArena = -1;
    private int ultimoIndiceHierba = -1;

    private void Awake()
    {
        PrepararAudioSources();
    }

    public void ReproducirInicioSalto()
    {
        ReproducirVozSalto();
        ReproducirWhoosh();
    }

    public void ReproducirAterrizaje()
    {
        Vector3 origen =
            transform.position +
            Vector3.up * alturaRaycast;

        LayerMask capasDetectables =
            sandLayer | grassLayer;

        if (!Physics.Raycast(
                origen,
                Vector3.down,
                out RaycastHit hit,
                distanciaRaycast,
                capasDetectables,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        int layerSuelo =
            hit.collider.gameObject.layer;

        if (LayerPerteneceAMascara(
                layerSuelo,
                sandLayer))
        {
            AudioClip clipArena =
                ObtenerClipAleatorioSinRepetir(
                    sonidosAterrizajeArena,
                    ref ultimoIndiceArena
                );

            ReproducirClip(
                audioSourceAterrizaje,
                clipArena,
                volumenAterrizaje
            );

            return;
        }

        if (LayerPerteneceAMascara(
                layerSuelo,
                grassLayer))
        {
            AudioClip clipHierba =
                ObtenerClipAleatorioSinRepetir(
                    sonidosAterrizajeHierba,
                    ref ultimoIndiceHierba
                );

            ReproducirClip(
                audioSourceAterrizaje,
                clipHierba,
                volumenAterrizaje
            );
        }
    }

    private void ReproducirVozSalto()
    {
        AudioClip clip =
            ObtenerClipAleatorioSinRepetir(
                sonidosVozSalto,
                ref ultimoIndiceVoz
            );

        ReproducirClip(
            audioSourceVoz,
            clip,
            volumenVoz
        );
    }

    private void ReproducirWhoosh()
    {
        AudioClip clip =
            ObtenerClipAleatorioSinRepetir(
                sonidosWhoosh,
                ref ultimoIndiceWhoosh
            );

        ReproducirClip(
            audioSourceWhoosh,
            clip,
            volumenWhoosh
        );
    }

    private void ReproducirClip(
        AudioSource source,
        AudioClip clip,
        float volumen)
    {
        if (source == null || clip == null)
        {
            return;
        }

        float pitchMenor =
            Mathf.Min(
                pitchMinimo,
                pitchMaximo
            );

        float pitchMayor =
            Mathf.Max(
                pitchMinimo,
                pitchMaximo
            );

        source.pitch =
            Random.Range(
                pitchMenor,
                pitchMayor
            );

        source.PlayOneShot(
            clip,
            volumen
        );
    }

    private AudioClip ObtenerClipAleatorioSinRepetir(
        AudioClip[] clips,
        ref int ultimoIndice)
    {
        if (clips == null ||
            clips.Length == 0)
        {
            return null;
        }

        if (clips.Length == 1)
        {
            ultimoIndice = 0;
            return clips[0];
        }

        int nuevoIndice;
        int intentos = 0;

        do
        {
            nuevoIndice =
                Random.Range(
                    0,
                    clips.Length
                );

            intentos++;
        }
        while (
            nuevoIndice == ultimoIndice &&
            intentos < 10
        );

        ultimoIndice = nuevoIndice;

        return clips[nuevoIndice];
    }

    private bool LayerPerteneceAMascara(
        int layer,
        LayerMask mascara)
    {
        return (
            mascara.value &
            (1 << layer)
        ) != 0;
    }

    private void PrepararAudioSources()
    {
        if (audioSourceVoz == null)
        {
            audioSourceVoz =
                CrearAudioSource();
        }

        if (audioSourceWhoosh == null)
        {
            audioSourceWhoosh =
                CrearAudioSource();
        }

        if (audioSourceAterrizaje == null)
        {
            audioSourceAterrizaje =
                CrearAudioSource();
        }

        ConfigurarAudioSource(audioSourceVoz);
        ConfigurarAudioSource(audioSourceWhoosh);
        ConfigurarAudioSource(audioSourceAterrizaje);

        audioSourceVoz.outputAudioMixerGroup = outputVoices;
        audioSourceWhoosh.outputAudioMixerGroup = outputPlayerSalto;
        audioSourceAterrizaje.outputAudioMixerGroup = outputPlayerAterrizaje;
    }

    private AudioSource CrearAudioSource()
    {
        return gameObject.AddComponent<AudioSource>();
    }

    private void ConfigurarAudioSource(
        AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 1f;
        source.dopplerLevel = 0f;

        source.rolloffMode =
            AudioRolloffMode.Logarithmic;

        source.minDistance = 1f;
        source.maxDistance = 15f;
    }
}