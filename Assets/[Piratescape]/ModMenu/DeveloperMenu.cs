using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class DeveloperMenu : MonoBehaviour
{
    [Header("Referencias principales")]
    [SerializeField] private GameTimeSystem sistemaTiempo;
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private PlayerHealth saludJugador;
    [SerializeField] private PlayerEnergy energiaJugador;
    [SerializeField] private SistemaEconomia sistemaEconomia;

    [Header("Velocidad del tiempo")]
    [SerializeField] private float segundosRealesPorMinutoJuegoNormal = 1f;

    [Header("Items de inventario")]
    [SerializeField] private ItemData maderaItem;
    [SerializeField] private ItemData platanoItem;
    [SerializeField] private ItemData cocoItem;
    [SerializeField] private ItemData cafeItem;
    [SerializeField] private ItemData piedraItem;
    [SerializeField] private ItemData clavoItem;
    [SerializeField] private ItemData gemaRojaItem;
    [SerializeField] private ItemData gemaAmarillaItem;
    [SerializeField] private ItemData gemaMoradaItem;

    [Header("Opciones iniciales")]
    [SerializeField] private bool mostrarFPSAlIniciar = false;

    private GameObject panelRaiz;
    private RectTransform ventanaRect;
    private TextMeshProUGUI fpsText;

    private bool menuAbierto;
    private bool mostrarFPS;

    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;

    private float fpsTimer;
    private int fpsFrames;
    private float fpsAcumulado;

    private void Awake()
    {
        BuscarReferenciasSiFaltan();
        CrearEventSystemSiHaceFalta();
        CrearUI();

        menuAbierto = false;
        mostrarFPS = mostrarFPSAlIniciar;

        panelRaiz.SetActive(false);
        fpsText.gameObject.SetActive(mostrarFPS);
    }

    private void Update()
    {
        GestionarTeclaF1();
        ActualizarFPS();
    }

    private void GestionarTeclaF1()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
            CambiarEstadoMenu();
        }
    }

    private void CambiarEstadoMenu()
    {
        menuAbierto = !menuAbierto;
        panelRaiz.SetActive(menuAbierto);

        if (menuAbierto)
        {
            ActivarCursorMenu();
        }
        else
        {
            RestaurarCursorAnterior();
        }
    }

    private void ActivarCursorMenu()
    {
        cursorLockAnterior = Cursor.lockState;
        cursorVisibleAnterior = Cursor.visible;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void RestaurarCursorAnterior()
    {
        Cursor.lockState = cursorLockAnterior;
        Cursor.visible = cursorVisibleAnterior;
    }

    private void BuscarReferenciasSiFaltan()
    {
        if (sistemaTiempo == null)
        {
            sistemaTiempo = FindFirstObjectByType<GameTimeSystem>();
        }

        if (inventarioJugador == null)
        {
            inventarioJugador = FindFirstObjectByType<PlayerInventory>();
        }

        if (saludJugador == null)
        {
            saludJugador = FindFirstObjectByType<PlayerHealth>();
        }

        if (energiaJugador == null)
        {
            energiaJugador = FindFirstObjectByType<PlayerEnergy>();
        }

        if (sistemaEconomia == null)
        {
            sistemaEconomia = FindFirstObjectByType<SistemaEconomia>();
        }
    }

    private void CrearEventSystemSiHaceFalta()
    {
        if (FindFirstObjectByType<EventSystem>() != null)
        {
            return;
        }

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private void CrearUI()
    {
        GameObject canvasObject = new GameObject("DeveloperMenuCanvas");
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 9999;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        CrearPanelPrincipal(canvasObject.transform);
        CrearTextoFPS(canvasObject.transform);
    }

    private void CrearPanelPrincipal(Transform padre)
    {
        panelRaiz = new GameObject("DeveloperMenuPanel");
        panelRaiz.transform.SetParent(padre, false);

        RectTransform panelRect = panelRaiz.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image fondoOscuro = panelRaiz.AddComponent<Image>();
        fondoOscuro.color = new Color(0f, 0f, 0f, 0.35f);

        GameObject ventana = new GameObject("Window");
        ventana.transform.SetParent(panelRaiz.transform, false);

        ventanaRect = ventana.AddComponent<RectTransform>();
        ventanaRect.anchorMin = new Vector2(0.5f, 1f);
        ventanaRect.anchorMax = new Vector2(0.5f, 1f);
        ventanaRect.pivot = new Vector2(0.5f, 1f);
        ventanaRect.anchoredPosition = new Vector2(0f, -20f);
        ventanaRect.sizeDelta = new Vector2(760f, 760f);

        Image ventanaImage = ventana.AddComponent<Image>();
        ventanaImage.color = new Color(0.09f, 0.07f, 0.05f, 0.96f);

        VerticalLayoutGroup ventanaLayout = ventana.AddComponent<VerticalLayoutGroup>();
        ventanaLayout.padding = new RectOffset(18, 18, 14, 14);
        ventanaLayout.spacing = 8;
        ventanaLayout.childAlignment = TextAnchor.UpperCenter;
        ventanaLayout.childControlWidth = true;
        ventanaLayout.childControlHeight = true;
        ventanaLayout.childForceExpandWidth = true;
        ventanaLayout.childForceExpandHeight = false;

        CrearTitulo(ventana.transform, "MENÚ DESARROLLADOR");
        CrearSubtitulo(ventana.transform, "F1 para abrir/cerrar | Pulsa una sección para desplegar");

        CrearSeccionTiempo(ventana.transform);
        CrearSeccionJugador(ventana.transform);
        CrearSeccionEconomia(ventana.transform);
        CrearSeccionInventario(ventana.transform);
        CrearSeccionDebugVisual(ventana.transform);
    }

    private void CrearSeccionTiempo(Transform padre)
    {
        GameObject contenido = CrearSeccionDesplegable(padre, "PARTIDA / TIEMPO", true);

        CrearFilaBotones(contenido.transform,
            ("Pausar / Reanudar", TogglePausarTiempo),
            ("Tiempo x1", () => EstablecerVelocidadTiempo(1f)),
            ("Tiempo x5", () => EstablecerVelocidadTiempo(5f)),
            ("Tiempo x20", () => EstablecerVelocidadTiempo(20f))
        );

        CrearFilaBotones(contenido.transform,
            ("Pasar a Día 08:00", PasarADia),
            ("Pasar a Atardecer 18:00", PasarAAtardecer),
            ("Pasar a Noche 22:00", PasarANoche)
        );
    }

    private void CrearSeccionJugador(Transform padre)
    {
        GameObject contenido = CrearSeccionDesplegable(padre, "JUGADOR / SUPERVIVENCIA", false);

        CrearFilaBotones(contenido.transform,
            ("GodMode ON/OFF", ToggleGodMode),
            ("Vida al máximo", VidaAlMaximo),
            ("Energía al máximo", EnergiaAlMaximo)
        );

        CrearFilaBotones(contenido.transform,
            ("Vida baja", VidaBaja),
            ("Energía baja", EnergiaBaja)
        );
    }

    private void CrearSeccionEconomia(Transform padre)
    {
        GameObject contenido = CrearSeccionDesplegable(padre, "ECONOMÍA", false);

        CrearFilaBotones(contenido.transform,
            ("+20 Conchas", () => AnadirMoneda(TipoMoneda.Concha, 20)),
            ("+5 Tulipanes", () => AnadirMoneda(TipoMoneda.Tulipan, 5)),
            ("+3 Pinyas", () => AnadirMoneda(TipoMoneda.Pinya, 3))
        );

        CrearFilaBotones(contenido.transform,
            ("+100 Conchas", () => AnadirMoneda(TipoMoneda.Concha, 100)),
            ("+20 Tulipanes", () => AnadirMoneda(TipoMoneda.Tulipan, 20)),
            ("+20 Pinyas", () => AnadirMoneda(TipoMoneda.Pinya, 20))
        );

        CrearFilaBotones(contenido.transform,
            ("Pack tienda", PackTienda)
        );
    }

    private void CrearSeccionInventario(Transform padre)
    {
        GameObject contenido = CrearSeccionDesplegable(padre, "INVENTARIO", false);

        CrearFilaBotones(contenido.transform,
            ("+20 Madera", () => AnadirItem(maderaItem, 20)),
            ("+5 Plátanos", () => AnadirItem(platanoItem, 5)),
            ("+5 Cocos", () => AnadirItem(cocoItem, 5))
        );

        CrearFilaBotones(contenido.transform,
            ("+5 Cafés", () => AnadirItem(cafeItem, 5)),
            ("+10 Piedras", () => AnadirItem(piedraItem, 10)),
            ("+9 Clavos", () => AnadirItem(clavoItem, 9))
        );

        CrearFilaBotones(contenido.transform,
            ("+1 Gema Roja", () => AnadirItem(gemaRojaItem, 1)),
            ("+1 Gema Amarilla", () => AnadirItem(gemaAmarillaItem, 1)),
            ("+1 Gema Morada", () => AnadirItem(gemaMoradaItem, 1))
        );

        CrearFilaBotones(contenido.transform,
            ("Vaciar inventario", VaciarInventario),
            ("Llenar inventario de prueba", LlenarInventarioDePrueba)
        );
    }

    private void CrearSeccionDebugVisual(Transform padre)
    {
        GameObject contenido = CrearSeccionDesplegable(padre, "DEBUG VISUAL", false);

        CrearFilaBotones(contenido.transform,
            ("Mostrar FPS", ToggleFPS),
            ("Cerrar menú", CambiarEstadoMenu)
        );
    }

    private GameObject CrearSeccionDesplegable(Transform padre, string titulo, bool abiertaPorDefecto)
    {
        GameObject bloque = new GameObject("Seccion_" + titulo);
        bloque.transform.SetParent(padre, false);

        VerticalLayoutGroup bloqueLayout = bloque.AddComponent<VerticalLayoutGroup>();
        bloqueLayout.spacing = 4;
        bloqueLayout.childAlignment = TextAnchor.UpperCenter;
        bloqueLayout.childControlWidth = true;
        bloqueLayout.childControlHeight = true;
        bloqueLayout.childForceExpandWidth = true;
        bloqueLayout.childForceExpandHeight = false;

        ContentSizeFitter bloqueFitter = bloque.AddComponent<ContentSizeFitter>();
        bloqueFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject cabecera = new GameObject("Header_" + titulo);
        cabecera.transform.SetParent(bloque.transform, false);

        Image cabeceraImage = cabecera.AddComponent<Image>();
        cabeceraImage.color = new Color(0.18f, 0.12f, 0.06f, 0.95f);

        Button cabeceraButton = cabecera.AddComponent<Button>();

        ColorBlock colores = cabeceraButton.colors;
        colores.normalColor = new Color(0.18f, 0.12f, 0.06f, 0.95f);
        colores.highlightedColor = new Color(0.34f, 0.20f, 0.08f, 1f);
        colores.pressedColor = new Color(0.12f, 0.07f, 0.03f, 1f);
        colores.selectedColor = new Color(0.34f, 0.20f, 0.08f, 1f);
        cabeceraButton.colors = colores;

        LayoutElement cabeceraLayout = cabecera.AddComponent<LayoutElement>();
        cabeceraLayout.preferredHeight = 36f;

        GameObject textoObject = new GameObject("Text");
        textoObject.transform.SetParent(cabecera.transform, false);

        RectTransform textoRect = textoObject.AddComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(12f, 0f);
        textoRect.offsetMax = new Vector2(-12f, 0f);

        TextMeshProUGUI textoCabecera = textoObject.AddComponent<TextMeshProUGUI>();
        textoCabecera.fontSize = 20f;
        textoCabecera.alignment = TextAlignmentOptions.MidlineLeft;
        textoCabecera.color = new Color(0.95f, 0.72f, 0.35f);

        GameObject contenido = new GameObject("Contenido_" + titulo);
        contenido.transform.SetParent(bloque.transform, false);

        VerticalLayoutGroup contenidoLayout = contenido.AddComponent<VerticalLayoutGroup>();
        contenidoLayout.spacing = 5;
        contenidoLayout.childAlignment = TextAnchor.UpperCenter;
        contenidoLayout.childControlWidth = true;
        contenidoLayout.childControlHeight = true;
        contenidoLayout.childForceExpandWidth = true;
        contenidoLayout.childForceExpandHeight = false;

        ContentSizeFitter contenidoFitter = contenido.AddComponent<ContentSizeFitter>();
        contenidoFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        bool abierta = abiertaPorDefecto;

        contenido.SetActive(abierta);
        textoCabecera.text = abierta ? "▼ " + titulo : "▶ " + titulo;

        cabeceraButton.onClick.AddListener(() =>
        {
            abierta = !abierta;

            contenido.SetActive(abierta);
            textoCabecera.text = abierta ? "▼ " + titulo : "▶ " + titulo;

            ReconstruirLayout();
        });

        return contenido;
    }

    private void ReconstruirLayout()
    {
        if (ventanaRect == null)
        {
            return;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(ventanaRect);
    }

    private void CrearTextoFPS(Transform padre)
    {
        GameObject fpsObject = new GameObject("FPS_Text");
        fpsObject.transform.SetParent(padre, false);

        RectTransform rect = fpsObject.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-20f, -20f);
        rect.sizeDelta = new Vector2(220f, 48f);

        fpsText = fpsObject.AddComponent<TextMeshProUGUI>();
        fpsText.text = "FPS: --";
        fpsText.fontSize = 28f;
        fpsText.alignment = TextAlignmentOptions.Right;
        fpsText.color = Color.white;
    }

    private void CrearTitulo(Transform padre, string texto)
    {
        GameObject objeto = new GameObject("Title");
        objeto.transform.SetParent(padre, false);

        TextMeshProUGUI titulo = objeto.AddComponent<TextMeshProUGUI>();
        titulo.text = texto;
        titulo.fontSize = 30f;
        titulo.alignment = TextAlignmentOptions.Center;
        titulo.color = new Color(1f, 0.82f, 0.35f);

        LayoutElement layout = objeto.AddComponent<LayoutElement>();
        layout.preferredHeight = 40f;
    }

    private void CrearSubtitulo(Transform padre, string texto)
    {
        GameObject objeto = new GameObject("Subtitle");
        objeto.transform.SetParent(padre, false);

        TextMeshProUGUI subtitulo = objeto.AddComponent<TextMeshProUGUI>();
        subtitulo.text = texto;
        subtitulo.fontSize = 14f;
        subtitulo.alignment = TextAlignmentOptions.Center;
        subtitulo.color = new Color(0.9f, 0.85f, 0.75f);

        LayoutElement layout = objeto.AddComponent<LayoutElement>();
        layout.preferredHeight = 24f;
    }

    private void CrearFilaBotones(Transform padre, params BotonDebug[] botones)
    {
        GameObject fila = new GameObject("ButtonRow");
        fila.transform.SetParent(padre, false);

        HorizontalLayoutGroup layout = fila.AddComponent<HorizontalLayoutGroup>();
        layout.spacing = 6;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        LayoutElement filaLayout = fila.AddComponent<LayoutElement>();
        filaLayout.preferredHeight = 40f;

        for (int i = 0; i < botones.Length; i++)
        {
            CrearBoton(fila.transform, botones[i].texto, botones[i].accion);
        }
    }

    private void CrearBoton(Transform padre, string texto, UnityEngine.Events.UnityAction accion)
    {
        GameObject botonObject = new GameObject("Button_" + texto);
        botonObject.transform.SetParent(padre, false);

        Image imagen = botonObject.AddComponent<Image>();
        imagen.color = new Color(0.48f, 0.18f, 0.07f, 1f);

        Button boton = botonObject.AddComponent<Button>();
        boton.onClick.AddListener(accion);

        ColorBlock colores = boton.colors;
        colores.normalColor = new Color(0.48f, 0.18f, 0.07f, 1f);
        colores.highlightedColor = new Color(0.68f, 0.32f, 0.13f, 1f);
        colores.pressedColor = new Color(0.34f, 0.12f, 0.04f, 1f);
        colores.selectedColor = new Color(0.68f, 0.32f, 0.13f, 1f);
        boton.colors = colores;

        GameObject textoObject = new GameObject("Text");
        textoObject.transform.SetParent(botonObject.transform, false);

        RectTransform textoRect = textoObject.AddComponent<RectTransform>();
        textoRect.anchorMin = Vector2.zero;
        textoRect.anchorMax = Vector2.one;
        textoRect.offsetMin = new Vector2(5f, 2f);
        textoRect.offsetMax = new Vector2(-5f, -2f);

        TextMeshProUGUI textoBoton = textoObject.AddComponent<TextMeshProUGUI>();
        textoBoton.text = texto;
        textoBoton.fontSize = 13f;
        textoBoton.alignment = TextAlignmentOptions.Center;
        textoBoton.color = Color.white;
        textoBoton.enableWordWrapping = true;

        LayoutElement layout = botonObject.AddComponent<LayoutElement>();
        layout.minWidth = 90f;
        layout.preferredHeight = 38f;
    }

    private void TogglePausarTiempo()
    {
        if (sistemaTiempo == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a GameTimeSystem.");
            return;
        }

        sistemaTiempo.SetPaused(!sistemaTiempo.IsPaused);
        Debug.Log("DeveloperMenu: tiempo pausado = " + sistemaTiempo.IsPaused);
    }

    private void EstablecerVelocidadTiempo(float multiplicador)
    {
        if (sistemaTiempo == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a GameTimeSystem.");
            return;
        }

        if (multiplicador <= 0f)
        {
            multiplicador = 1f;
        }

        float nuevoValor = segundosRealesPorMinutoJuegoNormal / multiplicador;

        sistemaTiempo.SetTimeScale(nuevoValor);
        sistemaTiempo.ResumeTime();

        Debug.Log("DeveloperMenu: velocidad del tiempo x" + multiplicador);
    }

    private void PasarADia()
    {
        if (sistemaTiempo == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a GameTimeSystem.");
            return;
        }

        sistemaTiempo.SetTime(8, 0);
        Debug.Log("DeveloperMenu: hora cambiada a día 08:00.");
    }

    private void PasarAAtardecer()
    {
        if (sistemaTiempo == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a GameTimeSystem.");
            return;
        }

        sistemaTiempo.SetTime(18, 0);
        Debug.Log("DeveloperMenu: hora cambiada a atardecer 18:00.");
    }

    private void PasarANoche()
    {
        if (sistemaTiempo == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a GameTimeSystem.");
            return;
        }

        sistemaTiempo.SetTime(22, 0);
        Debug.Log("DeveloperMenu: hora cambiada a noche 22:00.");
    }

    private void VidaAlMaximo()
    {
        if (saludJugador == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a PlayerHealth.");
            return;
        }

        int cantidad = saludJugador.MaxHealth - saludJugador.CurrentHealth;

        if (cantidad > 0)
        {
            saludJugador.Heal(cantidad);
        }

        Debug.Log("DeveloperMenu: vida al máximo.");
    }

    private void VidaBaja()
    {
        if (saludJugador == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a PlayerHealth.");
            return;
        }

        int vidaObjetivo = 20;
        int diferencia = saludJugador.CurrentHealth - vidaObjetivo;

        if (diferencia > 0)
        {
            saludJugador.TakeDamage(diferencia);
        }
        else if (diferencia < 0)
        {
            saludJugador.Heal(Mathf.Abs(diferencia));
        }

        Debug.Log("DeveloperMenu: vida baja.");
    }

    private void EnergiaAlMaximo()
    {
        if (energiaJugador == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a PlayerEnergy.");
            return;
        }

        energiaJugador.FillEnergy();
        Debug.Log("DeveloperMenu: energía al máximo.");
    }

    private void EnergiaBaja()
    {
        if (energiaJugador == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a PlayerEnergy.");
            return;
        }

        energiaJugador.SetEnergy(10f);
        Debug.Log("DeveloperMenu: energía baja.");
    }

    private void ToggleGodMode()
    {
        if (saludJugador == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a PlayerHealth.");
            return;
        }

        bool nuevoEstado = !saludJugador.GodModeActivo;

        saludJugador.SetGodMode(nuevoEstado);

        if (nuevoEstado)
        {
            VidaAlMaximo();
        }

        Debug.Log("DeveloperMenu: GodMode = " + nuevoEstado);
    }

    private void PackTienda()
    {
        AnadirMoneda(TipoMoneda.Concha, 100);
        AnadirMoneda(TipoMoneda.Tulipan, 100);
        AnadirMoneda(TipoMoneda.Pinya, 100);

        Debug.Log("DeveloperMenu: pack de tienda añadido.");
    }

    private void AnadirItem(ItemData itemData, int cantidad)
    {
        if (inventarioJugador == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a PlayerInventory.");
            return;
        }

        if (itemData == null)
        {
            Debug.LogWarning("DeveloperMenu: falta asignar un ItemData en el Inspector.");
            return;
        }

        bool anadido = inventarioJugador.TryAddItem(itemData, cantidad);

        if (anadido)
        {
            Debug.Log("DeveloperMenu: añadido " + cantidad + " x " + itemData.DisplayName);
        }
        else
        {
            Debug.LogWarning("DeveloperMenu: no se pudo añadir " + itemData.DisplayName + ". Puede que el inventario esté lleno.");
        }
    }

    private void AnadirMoneda(TipoMoneda tipoMoneda, int cantidad)
    {
        if (sistemaEconomia == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a SistemaEconomia.");
            return;
        }

        sistemaEconomia.AnadirMoneda(tipoMoneda, cantidad);
        Debug.Log("DeveloperMenu: añadida moneda " + tipoMoneda + " x" + cantidad);
    }

    private void VaciarInventario()
    {
        if (inventarioJugador == null)
        {
            Debug.LogWarning("DeveloperMenu: falta referencia a PlayerInventory.");
            return;
        }

        List<InventorySlot> slots = inventarioJugador.GetSlots();

        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null)
            {
                slots[i].Clear();
            }
        }

        inventarioJugador.ForceUpdateUI();
        Debug.Log("DeveloperMenu: inventario vaciado.");
    }

    private void LlenarInventarioDePrueba()
    {
        VaciarInventario();

        AnadirItem(maderaItem, 20);
        AnadirItem(platanoItem, 5);
        AnadirItem(cocoItem, 5);
        AnadirItem(cafeItem, 5);
        AnadirItem(piedraItem, 10);
        AnadirItem(clavoItem, 9);

        Debug.Log("DeveloperMenu: inventario de prueba llenado.");
    }

    private void ToggleFPS()
    {
        mostrarFPS = !mostrarFPS;
        fpsText.gameObject.SetActive(mostrarFPS);
        Debug.Log("DeveloperMenu: mostrar FPS = " + mostrarFPS);
    }

    private void ActualizarFPS()
    {
        if (!mostrarFPS || fpsText == null)
        {
            return;
        }

        fpsFrames++;
        fpsAcumulado += 1f / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= 0.25f)
        {
            float fpsMedio = fpsAcumulado / fpsFrames;
            fpsText.text = "FPS: " + Mathf.RoundToInt(fpsMedio);

            fpsTimer = 0f;
            fpsFrames = 0;
            fpsAcumulado = 0f;
        }
    }

    private struct BotonDebug
    {
        public string texto;
        public UnityEngine.Events.UnityAction accion;

        public BotonDebug(string texto, UnityEngine.Events.UnityAction accion)
        {
            this.texto = texto;
            this.accion = accion;
        }

        public static implicit operator BotonDebug((string texto, UnityEngine.Events.UnityAction accion) datos)
        {
            return new BotonDebug(datos.texto, datos.accion);
        }
    }
}