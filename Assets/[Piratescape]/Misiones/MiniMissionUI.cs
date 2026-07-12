using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class MiniMissionUI : MonoBehaviour
{
    private enum PosicionPanelMiniMision
    {
        ArribaIzquierda,
        AbajoIzquierda
    }
    [Header("Referencias UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Text tituloText;
    [SerializeField] private Text descripcionText;
    [SerializeField] private Text progresoText;
    [SerializeField] private Text avisoText;

    [Header("Auto UI")]
    [SerializeField] private bool crearUIBasicaSiFalta = true;
    [SerializeField] private PosicionPanelMiniMision posicionPantalla = PosicionPanelMiniMision.AbajoIzquierda;
    [SerializeField] private bool aplicarPosicionAlIniciar = true;
    [SerializeField] private bool aplicarPosicionCadaFrame = true;
    [SerializeField] private bool ponerPanelDirectamenteEnCanvas = true;
    [SerializeField] private Vector2 posicionPanel = new Vector2(24f, 120f);
    [SerializeField] private Vector2 tamanoPanel = new Vector2(410f, 118f);
    [SerializeField] private Color colorPanel = new Color(0f, 0f, 0f, 0.48f);
    [SerializeField] private Color colorTexto = Color.white;

    [Header("Avisos")]
    [SerializeField] private float duracionAvisoCompletada = 2.2f;
    [SerializeField] private bool ocultarAlCompletarTodas = false;

    private Coroutine avisoRoutine;
    private RectTransform panelRectCache;
    private Canvas canvasCache;

    private void Awake()
    {
        if (crearUIBasicaSiFalta && (panelRoot == null || tituloText == null || descripcionText == null || progresoText == null))
        {
            CrearUIBasica();
        }

        PrepararJerarquiaUI();

        if (aplicarPosicionAlIniciar)
        {
            AplicarPosicionPanel();
        }
    }

    private void OnEnable()
    {
        PrepararJerarquiaUI();
        AplicarPosicionPanel();
    }

    private void LateUpdate()
    {
        if (aplicarPosicionCadaFrame)
        {
            PrepararJerarquiaUI();
            AplicarPosicionPanel();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            AplicarPosicionPanel();
        }
    }
#endif

    public void MostrarMision(string titulo, string descripcion, string progreso)
    {
        PrepararJerarquiaUI();
        AplicarPosicionPanel();

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        SetText(tituloText, titulo);
        SetText(descripcionText, descripcion);
        SetText(progresoText, progreso);
    }

    public void MostrarCompletada(string titulo, string texto)
    {
        string mensaje = string.IsNullOrWhiteSpace(texto) ? "Mision completada" : texto;

        if (avisoText != null)
        {
            if (avisoRoutine != null)
            {
                StopCoroutine(avisoRoutine);
            }

            avisoRoutine = StartCoroutine(MostrarAvisoRoutine("✓ " + mensaje));
        }

        if (tituloText != null)
        {
            SetText(tituloText, "✓ " + titulo);
        }
    }

    public void MostrarTodasCompletadas()
    {
        if (ocultarAlCompletarTodas)
        {
            Ocultar();
            return;
        }

        PrepararJerarquiaUI();
        AplicarPosicionPanel();

        if (panelRoot != null)
        {
            panelRoot.SetActive(true);
        }

        SetText(tituloText, "Misiones completadas");
        SetText(descripcionText, "Ya has completado las misiones iniciales.");
        SetText(progresoText, "");
    }

    public void Ocultar()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }
    }

    private IEnumerator MostrarAvisoRoutine(string texto)
    {
        avisoText.gameObject.SetActive(true);
        SetText(avisoText, texto);
        yield return new WaitForSeconds(duracionAvisoCompletada);
        avisoText.gameObject.SetActive(false);
        avisoRoutine = null;
    }

    [ContextMenu("Aplicar posicion abajo izquierda")]
    private void AplicarPosicionAbajoIzquierda()
    {
        posicionPantalla = PosicionPanelMiniMision.AbajoIzquierda;
        posicionPanel = new Vector2(Mathf.Abs(posicionPanel.x), Mathf.Abs(posicionPanel.y));
        AplicarPosicionPanel();
    }

    [ContextMenu("Aplicar posicion arriba izquierda")]
    private void AplicarPosicionArribaIzquierda()
    {
        posicionPantalla = PosicionPanelMiniMision.ArribaIzquierda;
        posicionPanel = new Vector2(Mathf.Abs(posicionPanel.x), -Mathf.Abs(posicionPanel.y));
        AplicarPosicionPanel();
    }

    private void AplicarPosicionPanel()
    {
        if (panelRoot == null)
        {
            return;
        }

        if (panelRectCache == null)
        {
            panelRectCache = panelRoot.GetComponent<RectTransform>();
        }

        AplicarPosicionPanel(panelRectCache);
    }

    private void AplicarPosicionPanel(RectTransform panelRect)
    {
        if (panelRect == null)
        {
            return;
        }

        if (posicionPantalla == PosicionPanelMiniMision.AbajoIzquierda)
        {
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(Mathf.Abs(posicionPanel.x), Mathf.Abs(posicionPanel.y));
        }
        else
        {
            panelRect.anchorMin = new Vector2(0f, 1f);
            panelRect.anchorMax = new Vector2(0f, 1f);
            panelRect.pivot = new Vector2(0f, 1f);
            panelRect.anchoredPosition = new Vector2(Mathf.Abs(posicionPanel.x), -Mathf.Abs(posicionPanel.y));
        }

        panelRect.sizeDelta = tamanoPanel;
    }

    private void CrearUIBasica()
    {
        Canvas canvas = ObtenerCanvas();

        PrepararContenedorEnCanvas(canvas);

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        if (font == null)
        {
            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        panelRoot = new GameObject("Panel_MiniMision", typeof(RectTransform), typeof(Image));
        panelRoot.transform.SetParent(ponerPanelDirectamenteEnCanvas && canvas != null ? canvas.transform : transform, false);

        panelRectCache = panelRoot.GetComponent<RectTransform>();
        AplicarPosicionPanel(panelRectCache);

        Image image = panelRoot.GetComponent<Image>();
        image.color = colorPanel;

        tituloText = CrearTexto("Titulo", panelRoot.transform, font, 20, FontStyle.Bold, new Vector2(16f, -10f), new Vector2(-16f, 28f));
        descripcionText = CrearTexto("Descripcion", panelRoot.transform, font, 16, FontStyle.Normal, new Vector2(16f, -42f), new Vector2(-16f, 56f));
        progresoText = CrearTexto("Progreso", panelRoot.transform, font, 15, FontStyle.Bold, new Vector2(16f, -92f), new Vector2(-16f, 22f));

        avisoText = CrearTexto("AvisoCompletada", panelRoot.transform, font, 17, FontStyle.Bold, new Vector2(16f, -122f), new Vector2(-16f, 28f));
        avisoText.color = new Color(1f, 0.92f, 0.45f, 1f);
        avisoText.gameObject.SetActive(false);
    }


    private void PrepararJerarquiaUI()
    {
        Canvas canvas = ObtenerCanvas();
        PrepararContenedorEnCanvas(canvas);

        if (panelRoot == null)
        {
            return;
        }

        if (ponerPanelDirectamenteEnCanvas && canvas != null && panelRoot.transform.parent != canvas.transform)
        {
            panelRoot.transform.SetParent(canvas.transform, false);
        }

        if (panelRectCache == null)
        {
            panelRectCache = panelRoot.GetComponent<RectTransform>();
        }
    }

    private Canvas ObtenerCanvas()
    {
        if (canvasCache != null)
        {
            return canvasCache;
        }

        canvasCache = GetComponentInParent<Canvas>();

        if (canvasCache == null)
        {
            GameObject canvasGO = new GameObject("Canvas_MiniMisiones", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasCache = canvasGO.GetComponent<Canvas>();
            canvasCache.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
        }

        return canvasCache;
    }

    private void PrepararContenedorEnCanvas(Canvas canvas)
    {
        if (canvas == null)
        {
            return;
        }

        if (transform.parent != canvas.transform)
        {
            transform.SetParent(canvas.transform, false);
        }

        RectTransform contenedorRect = transform as RectTransform;

        if (contenedorRect == null)
        {
            return;
        }

        contenedorRect.anchorMin = Vector2.zero;
        contenedorRect.anchorMax = Vector2.one;
        contenedorRect.pivot = new Vector2(0.5f, 0.5f);
        contenedorRect.anchoredPosition = Vector2.zero;
        contenedorRect.sizeDelta = Vector2.zero;
        contenedorRect.localScale = Vector3.one;
    }

    private Text CrearTexto(string nombre, Transform padre, Font font, int fontSize, FontStyle style, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject go = new GameObject(nombre, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(padre, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        Text text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = colorTexto;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value ?? string.Empty;
        }
    }
}
