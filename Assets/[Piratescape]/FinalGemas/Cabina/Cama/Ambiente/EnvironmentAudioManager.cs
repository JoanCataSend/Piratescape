using System.Collections;
using UnityEngine;

public class EnvironmentAudioManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambienteBaseSource;
    [SerializeField] private AudioSource sonidosAleatoriosSource;

    [Header("Ambiente base")]
    [SerializeField] private AudioClip ambienteDia;
    [SerializeField] private AudioClip ambienteNoche;

    [Header("Sonidos aleatorios de día")]
    [SerializeField] private AudioClip[] sonidosAleatoriosDia;

    [Header("Sonidos aleatorios de noche")]
    [SerializeField] private AudioClip[] sonidosAleatoriosNoche;

    [Header("Configuración")]
    [SerializeField] private float volumenBaseDia = 0.35f;
    [SerializeField] private float volumenBaseNoche = 0.30f;
    [SerializeField] private float volumenAleatorios = 0.45f;
    [SerializeField] private float duracionFade = 2f;
    [SerializeField] private float tiempoMinimoAleatorio = 18f;
    [SerializeField] private float tiempoMaximoAleatorio = 45f;

    private bool esNocheActual;
    private Coroutine rutinaAleatoria;
    private Coroutine rutinaFade;

    private void Awake()
    {
        if (timeSystem == null)
        {
            timeSystem = FindFirstObjectByType<GameTimeSystem>();
        }

        if (ambienteBaseSource != null)
        {
            ambienteBaseSource.loop = true;
            ambienteBaseSource.playOnAwake = false;
            ambienteBaseSource.spatialBlend = 0f;
        }

        if (sonidosAleatoriosSource != null)
        {
            sonidosAleatoriosSource.loop = false;
            sonidosAleatoriosSource.playOnAwake = false;
            sonidosAleatoriosSource.spatialBlend = 0f;
        }

        if (timeSystem != null)
        {
            esNocheActual = timeSystem.IsNight;
        }

        AudioClip clipInicial = esNocheActual ? ambienteNoche : ambienteDia;
        float volumenInicial = esNocheActual ? volumenBaseNoche : volumenBaseDia;

        CambiarAmbienteBase(clipInicial, volumenInicial, true);
    }

    private void OnEnable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnDayNightChanged += AlCambiarDiaNoche;
        }
    }

    private void OnDisable()
    {
        if (timeSystem != null)
        {
            timeSystem.OnDayNightChanged -= AlCambiarDiaNoche;
        }
    }

    private void Start()
    {
        ReiniciarSonidosAleatorios();
    }

    private void AlCambiarDiaNoche(bool esNoche)
    {
        esNocheActual = esNoche;

        if (esNocheActual)
        {
            CambiarAmbienteBase(ambienteNoche, volumenBaseNoche, false);
        }
        else
        {
            CambiarAmbienteBase(ambienteDia, volumenBaseDia, false);
        }

        ReiniciarSonidosAleatorios();
    }

    private void CambiarAmbienteBase(AudioClip nuevoClip, float volumenObjetivo, bool instantaneo)
    {
        if (ambienteBaseSource == null || nuevoClip == null)
        {
            return;
        }

        if (rutinaFade != null)
        {
            StopCoroutine(rutinaFade);
        }

        if (instantaneo)
        {
            ambienteBaseSource.clip = nuevoClip;
            ambienteBaseSource.volume = volumenObjetivo;
            ambienteBaseSource.Play();
            return;
        }

        rutinaFade = StartCoroutine(FadeCambiarClip(nuevoClip, volumenObjetivo));
    }

    private IEnumerator FadeCambiarClip(AudioClip nuevoClip, float volumenObjetivo)
    {
        float volumenInicio = ambienteBaseSource.volume;

        float t = 0f;

        while (t < duracionFade)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duracionFade);

            ambienteBaseSource.volume = Mathf.Lerp(volumenInicio, 0f, p);

            yield return null;
        }

        ambienteBaseSource.Stop();
        ambienteBaseSource.clip = nuevoClip;
        ambienteBaseSource.volume = 0f;
        ambienteBaseSource.Play();

        t = 0f;

        while (t < duracionFade)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / duracionFade);

            ambienteBaseSource.volume = Mathf.Lerp(0f, volumenObjetivo, p);

            yield return null;
        }

        ambienteBaseSource.volume = volumenObjetivo;
        rutinaFade = null;
    }

    private void ReiniciarSonidosAleatorios()
    {
        if (rutinaAleatoria != null)
        {
            StopCoroutine(rutinaAleatoria);
        }

        rutinaAleatoria = StartCoroutine(RutinaSonidosAleatorios());
    }

    private IEnumerator RutinaSonidosAleatorios()
    {
        while (true)
        {
            float espera = Random.Range(tiempoMinimoAleatorio, tiempoMaximoAleatorio);
            yield return new WaitForSecondsRealtime(espera);

            AudioClip clip = ObtenerClipAleatorioActual();

            if (clip != null && sonidosAleatoriosSource != null)
            {
                sonidosAleatoriosSource.volume = volumenAleatorios;
                sonidosAleatoriosSource.pitch = Random.Range(0.95f, 1.05f);
                sonidosAleatoriosSource.PlayOneShot(clip);
            }
        }
    }

    private AudioClip ObtenerClipAleatorioActual()
    {
        AudioClip[] lista = esNocheActual ? sonidosAleatoriosNoche : sonidosAleatoriosDia;

        if (lista == null || lista.Length == 0)
        {
            return null;
        }

        return lista[Random.Range(0, lista.Length)];
    }
}