using UnityEngine;
using UnityEngine.Audio;

public class PirateExtraAudioController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private movimientoplayer movimientoPlayer;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSourceVoices;
    [SerializeField] private AudioSource audioSourcePlayer;
    [SerializeField] private AudioSource audioSourceObjetos;

    [Header("Outputs Audio Mixer")]
    [SerializeField] private AudioMixerGroup outputVoices;
    [SerializeField] private AudioMixerGroup outputPlayer;
    [SerializeField] private AudioMixerGroup outputObjetos;

    [Header("Cansancio / respiración")]
    [Tooltip("Sonidos tipo uff, respiración, quejido, cansancio.")]
    [SerializeField] private AudioClip[] sonidosCansancio;

    [Range(0f, 1f)]
    [SerializeField] private float volumenCansancio = 0.75f;

    [SerializeField] private float intervaloCansancioMin = 6f;
    [SerializeField] private float intervaloCansancioMax = 8f;
    [SerializeField] private bool reproducirCansancioAlEmpezar = true;

    [Header("Idle parado")]
    [Tooltip("Sonidos cuando el pirata lleva quieto varios segundos. Aquí puedes poner los pedos.")]
    [SerializeField] private AudioClip[] sonidosIdleParado;

    [Range(0f, 1f)]
    [SerializeField] private float volumenIdleParado = 0.8f;

    [Tooltip("Tiempo quieto antes de reproducir el primer sonido.")]
    [SerializeField] private float tiempoQuietoParaIdle = 5f;

    [Tooltip("Intervalo mínimo entre sonidos si sigue quieto.")]
    [SerializeField] private float intervaloIdleMin = 8f;

    [Tooltip("Intervalo máximo entre sonidos si sigue quieto.")]
    [SerializeField] private float intervaloIdleMax = 14f;

    [Tooltip("Velocidad mínima para considerar que el jugador se está moviendo.")]
    [SerializeField] private float velocidadMinimaMovimiento = 0.05f;

    [SerializeField] private bool reproducirIdleSiEstaCansado = false;

    [Header("Muerte / sin vida")]
    [SerializeField] private AudioClip[] sonidosMuerte;

    [Range(0f, 1f)]
    [SerializeField] private float volumenMuerte = 0.9f;

    [Header("Barco nivel 5 completado")]
    [Tooltip("Sonido corto especial al completar el último nivel del barco.")]
    [SerializeField] private AudioClip[] sonidosBarcoNivel5;

    [Range(0f, 1f)]
    [SerializeField] private float volumenBarcoNivel5 = 0.9f;

    [Header("Construcción del barco completada")]
    [Tooltip("Sonido corto al completar un nivel normal del barco.")]
    [SerializeField] private AudioClip[] sonidosConstruccionBarco;

    [Range(0f, 1f)]
    [SerializeField] private float volumenConstruccionBarco = 0.85f;

    [Header("Variación")]
    [SerializeField] private float pitchMinimo = 0.96f;
    [SerializeField] private float pitchMaximo = 1.04f;

    [Header("Configuración 3D")]
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 15f;

    private Vector3 ultimaPosicion;
    private float tiempoCansancio;
    private float tiempoQuieto;
    private float tiempoSiguienteIdle;

    private bool estabaCansado;
    private bool idleYaHaSonado;

    private int ultimoCansancio = -1;
    private int ultimoIdle = -1;
    private int ultimaMuerte = -1;
    private int ultimoBarcoNivel5 = -1;
    private int ultimaConstruccion = -1;

    private void Awake()
    {
        CachearReferencias();
        PrepararAudioSources();

        ultimaPosicion = transform.position;

        ProgramarSiguienteCansancio();
        ProgramarSiguienteIdle();
    }

    private void Update()
    {
        ActualizarSonidoCansancio();
        ActualizarSonidoIdleParado();
    }

    private void CachearReferencias()
    {
        if (movimientoPlayer == null)
        {
            movimientoPlayer = GetComponent<movimientoplayer>();
        }
    }

    private void PrepararAudioSources()
    {
        audioSourceVoices = PrepararAudioSource(
            audioSourceVoices,
            outputVoices
        );

        audioSourcePlayer = PrepararAudioSource(
            audioSourcePlayer,
            outputPlayer
        );

        audioSourceObjetos = PrepararAudioSource(
            audioSourceObjetos,
            outputObjetos
        );
    }

    private AudioSource PrepararAudioSource(
        AudioSource source,
        AudioMixerGroup output)
    {
        if (source == null)
        {
            source = gameObject.AddComponent<AudioSource>();
        }

        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 1f;
        source.dopplerLevel = 0f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;

        if (output != null)
        {
            source.outputAudioMixerGroup = output;
        }

        return source;
    }

    private void ActualizarSonidoCansancio()
    {
        if (movimientoPlayer == null)
        {
            return;
        }

        bool estaCansado =
            movimientoPlayer.IsTired() &&
            !movimientoPlayer.IsFainted();

        if (!estaCansado)
        {
            estabaCansado = false;
            ProgramarSiguienteCansancio();
            return;
        }

        if (!estabaCansado)
        {
            estabaCansado = true;

            if (reproducirCansancioAlEmpezar)
            {
                ReproducirSonidoCansancio();
            }

            ProgramarSiguienteCansancio();
            return;
        }

        tiempoCansancio -= Time.deltaTime;

        if (tiempoCansancio <= 0f)
        {
            ReproducirSonidoCansancio();
            ProgramarSiguienteCansancio();
        }
    }

    private void ActualizarSonidoIdleParado()
    {
        if (movimientoPlayer == null)
        {
            return;
        }

        Vector3 posicionActual = transform.position;

        Vector3 desplazamiento =
            posicionActual - ultimaPosicion;

        desplazamiento.y = 0f;

        float velocidadHorizontal = 0f;

        if (Time.deltaTime > 0f)
        {
            velocidadHorizontal =
                desplazamiento.magnitude / Time.deltaTime;
        }

        ultimaPosicion = posicionActual;

        bool estaMoviendose =
            velocidadHorizontal > velocidadMinimaMovimiento;

        bool puedeSonarIdle =
            !movimientoPlayer.IsFainted() &&
            !movimientoPlayer.IsPickingUp();

        if (!reproducirIdleSiEstaCansado &&
            movimientoPlayer.IsTired())
        {
            puedeSonarIdle = false;
        }

        if (estaMoviendose || !puedeSonarIdle)
        {
            tiempoQuieto = 0f;
            idleYaHaSonado = false;
            ProgramarSiguienteIdle();
            return;
        }

        tiempoQuieto += Time.deltaTime;

        if (tiempoQuieto < tiempoQuietoParaIdle)
        {
            return;
        }

        if (!idleYaHaSonado)
        {
            idleYaHaSonado = true;
            ReproducirSonidoIdleParado();
            ProgramarSiguienteIdle();
            return;
        }

        tiempoSiguienteIdle -= Time.deltaTime;

        if (tiempoSiguienteIdle <= 0f)
        {
            ReproducirSonidoIdleParado();
            ProgramarSiguienteIdle();
        }
    }

    private void ProgramarSiguienteCansancio()
    {
        float minimo =
            Mathf.Min(
                intervaloCansancioMin,
                intervaloCansancioMax
            );

        float maximo =
            Mathf.Max(
                intervaloCansancioMin,
                intervaloCansancioMax
            );

        tiempoCansancio =
            Random.Range(
                minimo,
                maximo
            );
    }

    private void ProgramarSiguienteIdle()
    {
        float minimo =
            Mathf.Min(
                intervaloIdleMin,
                intervaloIdleMax
            );

        float maximo =
            Mathf.Max(
                intervaloIdleMin,
                intervaloIdleMax
            );

        tiempoSiguienteIdle =
            Random.Range(
                minimo,
                maximo
            );
    }

    private void ReproducirSonidoCansancio()
    {
        AudioClip clip =
            ObtenerClipAleatorioSinRepetir(
                sonidosCansancio,
                ref ultimoCansancio
            );

        ReproducirClip(
            audioSourceVoices,
            clip,
            volumenCansancio
        );
    }

    private void ReproducirSonidoIdleParado()
    {
        AudioClip clip =
            ObtenerClipAleatorioSinRepetir(
                sonidosIdleParado,
                ref ultimoIdle
            );

        ReproducirClip(
            audioSourceObjetos,
            clip,
            volumenIdleParado
        );
    }

    public void ReproducirMuerte()
    {
        AudioClip clip =
            ObtenerClipAleatorioSinRepetir(
                sonidosMuerte,
                ref ultimaMuerte
            );

        if (audioSourceVoices != null)
        {
            audioSourceVoices.Stop();
        }

        ReproducirClip(
            audioSourceVoices,
            clip,
            volumenMuerte
        );
    }

    public void ReproducirBarcoNivel5()
    {
        AudioClip clip =
            ObtenerClipAleatorioSinRepetir(
                sonidosBarcoNivel5,
                ref ultimoBarcoNivel5
            );

        ReproducirClip(
            audioSourceVoices,
            clip,
            volumenBarcoNivel5
        );
    }

    public void ReproducirConstruccionBarco()
    {
        AudioClip clip =
            ObtenerClipAleatorioSinRepetir(
                sonidosConstruccionBarco,
                ref ultimaConstruccion
            );

        ReproducirClip(
            audioSourceObjetos,
            clip,
            volumenConstruccionBarco
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (audioSourceVoices != null &&
            outputVoices != null)
        {
            audioSourceVoices.outputAudioMixerGroup =
                outputVoices;
        }

        if (audioSourcePlayer != null &&
            outputPlayer != null)
        {
            audioSourcePlayer.outputAudioMixerGroup =
                outputPlayer;
        }

        if (audioSourceObjetos != null &&
            outputObjetos != null)
        {
            audioSourceObjetos.outputAudioMixerGroup =
                outputObjetos;
        }
    }
#endif
}
