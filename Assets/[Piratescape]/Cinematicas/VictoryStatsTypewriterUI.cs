using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class VictoryStatsTypewriterUI : MonoBehaviour
{
    [System.Serializable]
    public class EstadisticaVictoria
    {
        public string nombre;
        public string valor;
    }

    [Header("Referencias principales")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text textoTitulo;
    [SerializeField] private TMP_Text textoEstadisticas;
    [SerializeField] private TMP_Text textoTituloCreditos;
    [SerializeField] private TMP_Text textoCreditos;
    [SerializeField] private Button botonSiguiente;
    [SerializeField] private Button botonVolverMenu;
    [SerializeField] private AudioSource audioSource;

    [Header("Contenido estadísticas")]
    [SerializeField] private string titulo = "VICTORIA";

    [SerializeField] private List<EstadisticaVictoria> estadisticas = new List<EstadisticaVictoria>()
    {
        new EstadisticaVictoria { nombre = "Puntos", valor = "1000" },
        new EstadisticaVictoria { nombre = "Materiales utilizados", valor = "24" },
        new EstadisticaVictoria { nombre = "Días sobrevividos", valor = "7" },
        new EstadisticaVictoria { nombre = "Desmayos", valor = "1" }
    };

    [Header("Contenido créditos")]
    [SerializeField] private string tituloCreditos = "CRÉDITOS";

    [TextArea(6, 14)]
    [SerializeField] private string textoCreditosCompleto =
        "<size=38><b>Creado por:</b></size>\n" +
        "Alan Guevara Martínez\n" +
        "Matilde Calleja García\n" +
        "Julia Valén De Oliveira\n" +
        "Joan Català Sendra\n" +
        "Sergi Puig Biosca\n\n" +

        "<size=38><b>Arte y diseño:</b></size>\n" +
        "sdasd asdasd\n" +
        "ad asdada\n\n" +

        "<size=38><b>Programación:</b></size>\n" +
        "sad asda dad\n" +
        "as dasd asdasd\n\n" +

        "<size=38><b>Agradecimientos:</b></size>\n" +
        "Profesorado\n" +
        "Compañeros\n" +
        "Familia y amigos\n\n" +

        "Gracias por jugar.";

    [Header("Escena menú")]
    [SerializeField] private string nombreEscenaMenuPrincipal = "MenuPrincipal";

    [Header("Estilo título")]
    [SerializeField] private Color colorTitulo = new Color(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private float tamanoTitulo = 72f;
    [SerializeField] private bool tituloEnMayusculas = true;

    [Header("Estilo estadísticas")]
    [SerializeField] private Color colorEstadisticas = Color.white;
    [SerializeField] private float tamanoEstadisticas = 36f;

    [Header("Estilo créditos")]
    [SerializeField] private Color colorTituloCreditos = new Color(1f, 0.82f, 0.25f, 1f);
    [SerializeField] private float tamanoTituloCreditos = 72f;
    [SerializeField] private Color colorCreditos = Color.white;
    [SerializeField] private float tamanoCreditos = 30f;

    [Header("Typewriter estadísticas")]
    [SerializeField] private float tiempoEntreCaracteresTitulo = 0.045f;
    [SerializeField] private float tiempoEntreCaracteresEstadisticas = 0.03f;
    [SerializeField] private float pausaDespuesTitulo = 0.35f;
    [SerializeField] private float pausaEntreLineas = 0.12f;

    [Header("Créditos tipo Star Wars")]
    [SerializeField] private float posicionInicialCreditosY = -450f;
    [SerializeField] private float posicionFinalCreditosY = 650f;
    [SerializeField] private float duracionScrollCreditos = 12f;
    [SerializeField] private float pausaAntesBotonVolverMenu = 0.8f;
    [SerializeField] private bool desvanecerCreditosAlFinal = true;

    [Range(0f, 1f)]
    [SerializeField] private float inicioDesvanecidoCreditos = 0.75f;

    [Header("Sonido 8-bit")]
    [SerializeField] private AudioClip sonidoCaracter;
    [SerializeField] private float volumenSonido = 0.45f;
    [SerializeField] private float frecuenciaBeep = 1300f;
    [SerializeField] private float duracionBeep = 0.025f;
    [SerializeField] private bool reproducirSonidoEnEspacios = false;

    [Header("Animación extra título")]
    [SerializeField] private bool animarEscalaTitulo = true;
    [SerializeField] private float escalaInicialTitulo = 0.75f;
    [SerializeField] private float escalaFinalTitulo = 1f;
    [SerializeField] private float duracionAnimacionEscalaTitulo = 0.25f;

    private Coroutine rutinaActual;
    private bool creditosMostrados;
    private RectTransform rectCreditos;
    private CanvasGroup canvasGroupCreditos;
    private CanvasGroup canvasGroupTituloCreditos;

    private void Awake()
    {
        CachearReferencias();
        ConfigurarBotones();

        if (sonidoCaracter == null)
        {
            sonidoCaracter = CrearBeep8Bit();
        }

        AplicarEstilosIniciales();
        LimpiarTodo();
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

        if (textoCreditos != null)
        {
            rectCreditos = textoCreditos.GetComponent<RectTransform>();

            canvasGroupCreditos = textoCreditos.GetComponent<CanvasGroup>();

            if (canvasGroupCreditos == null)
            {
                canvasGroupCreditos = textoCreditos.gameObject.AddComponent<CanvasGroup>();
            }

            textoCreditos.richText = true;
        }

        if (textoTituloCreditos != null)
        {
            canvasGroupTituloCreditos = textoTituloCreditos.GetComponent<CanvasGroup>();

            if (canvasGroupTituloCreditos == null)
            {
                canvasGroupTituloCreditos = textoTituloCreditos.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    private void ConfigurarBotones()
    {
        if (botonSiguiente != null)
        {
            botonSiguiente.onClick.RemoveListener(MostrarCreditos);
            botonSiguiente.onClick.AddListener(MostrarCreditos);
        }

        if (botonVolverMenu != null)
        {
            botonVolverMenu.onClick.RemoveListener(VolverAlMenuPrincipal);
            botonVolverMenu.onClick.AddListener(VolverAlMenuPrincipal);
        }
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

        if (textoTituloCreditos != null)
        {
            textoTituloCreditos.color = colorTituloCreditos;
            textoTituloCreditos.fontSize = tamanoTituloCreditos;
            textoTituloCreditos.alignment = TextAlignmentOptions.Center;
        }

        if (textoCreditos != null)
        {
            textoCreditos.color = colorCreditos;
            textoCreditos.fontSize = tamanoCreditos;
            textoCreditos.alignment = TextAlignmentOptions.Center;
            textoCreditos.richText = true;
        }
    }

    private void LimpiarTodo()
    {
        if (textoTitulo != null)
        {
            textoTitulo.text = "";
            textoTitulo.gameObject.SetActive(true);
            textoTitulo.transform.localScale = Vector3.one;
        }

        if (textoEstadisticas != null)
        {
            textoEstadisticas.text = "";
            textoEstadisticas.gameObject.SetActive(true);
        }

        if (textoTituloCreditos != null)
        {
            textoTituloCreditos.text = "";
            textoTituloCreditos.gameObject.SetActive(false);
            textoTituloCreditos.transform.localScale = Vector3.one;
        }

        if (textoCreditos != null)
        {
            textoCreditos.text = "";
            textoCreditos.gameObject.SetActive(false);
        }

        if (canvasGroupTituloCreditos != null)
        {
            canvasGroupTituloCreditos.alpha = 1f;
        }

        if (canvasGroupCreditos != null)
        {
            canvasGroupCreditos.alpha = 1f;
        }

        if (rectCreditos != null)
        {
            Vector2 posicion = rectCreditos.anchoredPosition;
            posicion.y = posicionInicialCreditosY;
            rectCreditos.anchoredPosition = posicion;
        }

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(false);
        }

        if (botonVolverMenu != null)
        {
            botonVolverMenu.gameObject.SetActive(false);
        }

        creditosMostrados = false;
    }

    public void MostrarPantalla()
    {
        DesbloquearCursor();

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        AplicarEstilosIniciales();
        LimpiarTodo();

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
            tiempoEntreCaracteresTitulo,
            true
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
                tiempoEntreCaracteresEstadisticas,
                true
            );

            if (i < estadisticas.Count - 1)
            {
                textoEstadisticas.text += "\n";
                yield return new WaitForSecondsRealtime(pausaEntreLineas);
            }
        }

        DesbloquearCursor();

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(true);
        }

        rutinaActual = null;
    }

    private void MostrarCreditos()
    {
        DesbloquearCursor();

        if (creditosMostrados)
        {
            return;
        }

        creditosMostrados = true;

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        rutinaActual = StartCoroutine(MostrarCreditosRoutine());
    }

    private IEnumerator MostrarCreditosRoutine()
    {
        DesbloquearCursor();

        if (botonSiguiente != null)
        {
            botonSiguiente.gameObject.SetActive(false);
        }

        if (botonVolverMenu != null)
        {
            botonVolverMenu.gameObject.SetActive(false);
        }

        if (textoTitulo != null)
        {
            textoTitulo.gameObject.SetActive(false);
        }

        if (textoEstadisticas != null)
        {
            textoEstadisticas.gameObject.SetActive(false);
        }

        if (textoTituloCreditos != null)
        {
            textoTituloCreditos.text = tituloCreditos;
            textoTituloCreditos.gameObject.SetActive(true);
            textoTituloCreditos.transform.localScale = Vector3.one * escalaInicialTitulo;
            StartCoroutine(AnimarEscalaTexto(textoTituloCreditos.transform));
        }

        if (textoCreditos != null)
        {
            textoCreditos.text = textoCreditosCompleto;
            textoCreditos.gameObject.SetActive(true);
        }

        if (canvasGroupTituloCreditos != null)
        {
            canvasGroupTituloCreditos.alpha = 1f;
        }

        if (canvasGroupCreditos != null)
        {
            canvasGroupCreditos.alpha = 1f;
        }

        if (rectCreditos != null)
        {
            Vector2 posicion = rectCreditos.anchoredPosition;
            posicion.y = posicionInicialCreditosY;
            rectCreditos.anchoredPosition = posicion;
        }

        yield return ScrollCreditosRoutine();

        yield return new WaitForSecondsRealtime(pausaAntesBotonVolverMenu);

        DesbloquearCursor();

        if (botonVolverMenu != null)
        {
            botonVolverMenu.gameObject.SetActive(true);
        }

        rutinaActual = null;
    }

    private IEnumerator ScrollCreditosRoutine()
    {
        if (rectCreditos == null)
        {
            yield break;
        }

        float tiempo = 0f;

        while (tiempo < duracionScrollCreditos)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tiempo / duracionScrollCreditos);
            float tSuave = Mathf.SmoothStep(0f, 1f, t);

            Vector2 posicion = rectCreditos.anchoredPosition;
            posicion.y = Mathf.Lerp(posicionInicialCreditosY, posicionFinalCreditosY, tSuave);
            rectCreditos.anchoredPosition = posicion;

            if (desvanecerCreditosAlFinal)
            {
                if (t >= inicioDesvanecidoCreditos)
                {
                    float tFade = Mathf.InverseLerp(inicioDesvanecidoCreditos, 1f, t);

                    if (canvasGroupCreditos != null)
                    {
                        canvasGroupCreditos.alpha = Mathf.Lerp(1f, 0f, tFade);
                    }

                    if (canvasGroupTituloCreditos != null)
                    {
                        canvasGroupTituloCreditos.alpha = Mathf.Lerp(1f, 0f, tFade);
                    }
                }
                else
                {
                    if (canvasGroupCreditos != null)
                    {
                        canvasGroupCreditos.alpha = 1f;
                    }

                    if (canvasGroupTituloCreditos != null)
                    {
                        canvasGroupTituloCreditos.alpha = 1f;
                    }
                }
            }

            yield return null;
        }

        Vector2 posicionFinal = rectCreditos.anchoredPosition;
        posicionFinal.y = posicionFinalCreditosY;
        rectCreditos.anchoredPosition = posicionFinal;

        if (desvanecerCreditosAlFinal)
        {
            if (canvasGroupCreditos != null)
            {
                canvasGroupCreditos.alpha = 0f;
            }

            if (canvasGroupTituloCreditos != null)
            {
                canvasGroupTituloCreditos.alpha = 0f;
            }
        }
    }

    private IEnumerator EscribirTexto(
        TMP_Text textoObjetivo,
        string textoCompleto,
        float tiempoEntreCaracteres,
        bool reproducirSonido
    )
    {
        if (textoObjetivo == null || string.IsNullOrEmpty(textoCompleto))
        {
            yield break;
        }

        for (int i = 0; i < textoCompleto.Length; i++)
        {
            char caracter = textoCompleto[i];

            textoObjetivo.text += caracter;

            if (reproducirSonido && DebeReproducirSonido(caracter))
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

        yield return AnimarEscalaTexto(textoTitulo.transform);
    }

    private IEnumerator AnimarEscalaTexto(Transform textoTransform)
    {
        if (textoTransform == null)
        {
            yield break;
        }

        float tiempo = 0f;
        textoTransform.localScale = Vector3.one * escalaInicialTitulo;

        while (tiempo < duracionAnimacionEscalaTitulo)
        {
            tiempo += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(tiempo / duracionAnimacionEscalaTitulo);
            float tSuave = Mathf.SmoothStep(0f, 1f, t);

            float escala = Mathf.Lerp(escalaInicialTitulo, escalaFinalTitulo, tSuave);
            textoTransform.localScale = Vector3.one * escala;

            yield return null;
        }

        textoTransform.localScale = Vector3.one * escalaFinalTitulo;
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

    private void DesbloquearCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void VolverAlMenuPrincipal()
    {
        Time.timeScale = 1f;

        DesbloquearCursor();

        SceneManager.LoadScene(nombreEscenaMenuPrincipal);
    }
}