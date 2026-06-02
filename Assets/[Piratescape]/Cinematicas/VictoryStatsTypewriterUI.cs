using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VictoryStatsTypewriterUI : MonoBehaviour
{
    [System.Serializable]
    public class EstadisticaVictoria
    {
        public string nombre;
        public string valor;
    }

    [Header("Referencias")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoEstadisticas;
    [SerializeField] private AudioSource audioSource;

    [Header("Contenido")]
    [SerializeField] private string titulo = "VICTORIA";

    [SerializeField] private List<EstadisticaVictoria> estadisticas = new List<EstadisticaVictoria>()
    {
        new EstadisticaVictoria { nombre = "Puntos", valor = "1000" },
        new EstadisticaVictoria { nombre = "Materiales utilizados", valor = "24" },
        new EstadisticaVictoria { nombre = "Días sobrevividos", valor = "7" },
        new EstadisticaVictoria { nombre = "Desmayos", valor = "1" }
    };

    [Header("Estilo título")]
    [SerializeField] private Color colorTitulo = new Color(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private float tamanoTitulo = 72f;
    [SerializeField] private bool tituloEnMayusculas = true;

    [Header("Estilo estadísticas")]
    [SerializeField] private Color colorEstadisticas = Color.white;
    [SerializeField] private float tamanoEstadisticas = 36f;

    [Header("Typewriter")]
    [SerializeField] private float tiempoEntreCaracteresTitulo = 0.045f;
    [SerializeField] private float tiempoEntreCaracteresEstadisticas = 0.03f;
    [SerializeField] private float pausaDespuesTitulo = 0.35f;
    [SerializeField] private float pausaEntreLineas = 0.12f;
    [SerializeField] private bool reproducirSonidoEnEspacios = false;

    [Header("Sonido 8-bit")]
    [SerializeField] private AudioClip sonidoCaracter;
    [SerializeField] private float volumenSonido = 0.45f;
    [SerializeField] private float frecuenciaBeep = 1300f;
    [SerializeField] private float duracionBeep = 0.025f;

    [Header("Animación extra")]
    [SerializeField] private bool animarEscalaTitulo = true;
    [SerializeField] private float escalaInicialTitulo = 0.75f;
    [SerializeField] private float escalaFinalTitulo = 1f;
    [SerializeField] private float duracionAnimacionEscalaTitulo = 0.25f;

    private Coroutine rutinaActual;

    private void Awake()
    {
        CachearReferencias();

        if (sonidoCaracter == null)
        {
            sonidoCaracter = CrearBeep8Bit();
        }

        LimpiarTextos();
        AplicarEstilosIniciales();
    }

    private void OnEnable()
    {
        MostrarPantalla();
    }

    private void CachearReferencias()
    {
        if (panelRoot == null)
        {
            panelRoot = gameObject;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    private void AplicarEstilosIniciales()
    {
        if (textoTitulo != null)
        {
            textoTitulo.color = colorTitulo;
            textoTitulo.fontSize = tamanoTitulo;
            textoTitulo.alignment = TextAlignmentOptions.Center;
        }

        if (textoEstadisticas != null)
        {
            textoEstadisticas.color = colorEstadisticas;
            textoEstadisticas.fontSize = tamanoEstadisticas;
            textoEstadisticas.alignment = TextAlignmentOptions.Center;
        }
    }

    private void LimpiarTextos()
    {
        if (textoTitulo != null)
        {
            textoTitulo.text = "";
        }

        if (textoEstadisticas != null)
        {
            textoEstadisticas.text = "";
        }
    }

    public void MostrarPantalla()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        AplicarEstilosIniciales();
        LimpiarTextos();

        rutinaActual = StartCoroutine(AnimarPantallaVictoria());
    }

    public void ConfigurarEstadisticasFalsas()
    {
        estadisticas.Clear();

        estadisticas.Add(new EstadisticaVictoria { nombre = "Puntos", valor = "1000" });
        estadisticas.Add(new EstadisticaVictoria { nombre = "Materiales utilizados", valor = "24" });
        estadisticas.Add(new EstadisticaVictoria { nombre = "Días sobrevividos", valor = "7" });
        estadisticas.Add(new EstadisticaVictoria { nombre = "Desmayos", valor = "1" });
    }

    public void ConfigurarEstadisticasReales(
        int puntos,
        int materialesUtilizados,
        int diasSobrevividos,
        int desmayos
    )
    {
        estadisticas.Clear();

        estadisticas.Add(new EstadisticaVictoria { nombre = "Puntos", valor = puntos.ToString() });
        estadisticas.Add(new EstadisticaVictoria { nombre = "Materiales utilizados", valor = materialesUtilizados.ToString() });
        estadisticas.Add(new EstadisticaVictoria { nombre = "Días sobrevividos", valor = diasSobrevividos.ToString() });
        estadisticas.Add(new EstadisticaVictoria { nombre = "Desmayos", valor = desmayos.ToString() });
    }

    public void AnadirEstadistica(string nombre, string valor)
    {
        estadisticas.Add(new EstadisticaVictoria
        {
            nombre = nombre,
            valor = valor
        });
    }

    private IEnumerator AnimarPantallaVictoria()
    {
        if (textoTitulo == null || textoEstadisticas == null)
        {
            yield break;
        }

        string tituloFinal = tituloEnMayusculas ? titulo.ToUpper() : titulo;

        if (animarEscalaTitulo)
        {
            StartCoroutine(AnimarEscalaTitulo());
        }

        yield return EscribirTexto(
            textoTitulo,
            tituloFinal,
            tiempoEntreCaracteresTitulo
        );

        yield return new WaitForSecondsRealtime(pausaDespuesTitulo);

        for (int i = 0; i < estadisticas.Count; i++)
        {
            if (estadisticas[i] == null)
            {
                continue;
            }

            string linea = estadisticas[i].nombre + ": " + estadisticas[i].valor;

            yield return EscribirTexto(
                textoEstadisticas,
                linea,
                tiempoEntreCaracteresEstadisticas
            );

            if (i < estadisticas.Count - 1)
            {
                textoEstadisticas.text += "\n";
                yield return new WaitForSecondsRealtime(pausaEntreLineas);
            }
        }

        rutinaActual = null;
    }

    private IEnumerator EscribirTexto(TMP_Text textoObjetivo, string textoCompleto, float tiempoEntreCaracteres)
    {
        if (textoObjetivo == null || string.IsNullOrEmpty(textoCompleto))
        {
            yield break;
        }

        for (int i = 0; i < textoCompleto.Length; i++)
        {
            char caracter = textoCompleto[i];

            textoObjetivo.text += caracter;

            if (DebeReproducirSonido(caracter))
            {
                ReproducirSonidoCaracter();
            }

            yield return new WaitForSecondsRealtime(tiempoEntreCaracteres);
        }
    }

    private IEnumerator AnimarEscalaTitulo()
    {
        if (textoTitulo == null)
        {
            yield break;
        }

        Transform tituloTransform = textoTitulo.transform;

        float tiempo = 0f;
        tituloTransform.localScale = Vector3.one * escalaInicialTitulo;

        while (tiempo < duracionAnimacionEscalaTitulo)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tiempo / duracionAnimacionEscalaTitulo);
            float tSuave = Mathf.SmoothStep(0f, 1f, t);

            float escala = Mathf.Lerp(escalaInicialTitulo, escalaFinalTitulo, tSuave);
            tituloTransform.localScale = Vector3.one * escala;

            yield return null;
        }

        tituloTransform.localScale = Vector3.one * escalaFinalTitulo;
    }

    private bool DebeReproducirSonido(char caracter)
    {
        if (reproducirSonidoEnEspacios)
        {
            return true;
        }

        return !char.IsWhiteSpace(caracter);
    }

    private void ReproducirSonidoCaracter()
    {
        if (audioSource == null || sonidoCaracter == null)
        {
            return;
        }

        audioSource.PlayOneShot(sonidoCaracter, volumenSonido);
    }

    private AudioClip CrearBeep8Bit()
    {
        int sampleRate = 44100;
        int sampleCount = Mathf.CeilToInt(sampleRate * duracionBeep);

        AudioClip clip = AudioClip.Create(
            "Retro_8Bit_Beep",
            sampleCount,
            1,
            sampleRate,
            false
        );

        float[] samples = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleRate;

            float ondaCuadrada = Mathf.Sin(2f * Mathf.PI * frecuenciaBeep * t) >= 0f ? 1f : -1f;

            float fadeOut = 1f - ((float)i / sampleCount);

            samples[i] = ondaCuadrada * fadeOut * 0.35f;
        }

        clip.SetData(samples, 0);

        return clip;
    }
}