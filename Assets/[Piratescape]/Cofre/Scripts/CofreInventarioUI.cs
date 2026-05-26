using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class CofreInventarioUI : MonoBehaviour
{
    public static bool HayAlgunaUIAbierta { get; private set; }
    public static int FrameUltimoCierre { get; private set; } = -1;
    public static bool SeHaCerradoUIEsteFrame => FrameUltimoCierre == Time.frameCount;

    [Header("Panel")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject contenidoVisual;
    [SerializeField] private TMP_Text textoMensaje;

    [Header("Inventario del jugador")]
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private Transform contenedorSlotsJugador;
    [SerializeField] private SlotCofreUI[] slotsJugador;

    [Header("Inventario del cofre")]
    [SerializeField] private Transform contenedorSlotsCofre;
    [SerializeField] private SlotCofreUI[] slotsCofre;

    [Header("Pausa")]
    [SerializeField] private bool pausarJuegoAlAbrir = true;

    private InventarioCofre cofreActual;
    private CursorLockMode cursorLockAnterior;
    private bool cursorVisibleAnterior;
    private bool cursorGuardado;
    private float timeScaleAnterior = 1f;
    private bool timeScaleGuardado;

    private int frameApertura = -1;

    public bool EstaAbierto
    {
        get
        {
            return panel != null && panel.activeSelf;
        }
    }

    private void Awake()
    {
        if (inventarioJugador == null)
        {
            inventarioJugador = FindFirstObjectByType<PlayerInventory>();
        }

        PrepararReferenciasAutomaticas();
        ConfigurarSlots();

        if (contenidoVisual != null)
        {
            contenidoVisual.SetActive(true);
        }

        if (panel != null)
        {
            panel.SetActive(false);
        }

        HayAlgunaUIAbierta = false;
    }

    private void OnDisable()
    {
        if (cofreActual != null)
        {
            DesuscribirseEventos();
            cofreActual = null;
        }

        if (HayAlgunaUIAbierta)
        {
            FrameUltimoCierre = Time.frameCount;
        }

        HayAlgunaUIAbierta = false;
        RestaurarPausaSiHaceFalta();
    }

    private void Update()
    {
        if (!EstaAbierto)
        {
            return;
        }

        if (Time.frameCount == frameApertura)
        {
            return;
        }

        if (SeHaPulsadoCerrar())
        {
            Cerrar();
        }
    }

    private bool SeHaPulsadoCerrar()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return true;
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                return true;
            }
        }

        if (Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    public void Abrir(InventarioCofre nuevoCofre)
    {
        if (nuevoCofre == null)
        {
            Debug.LogWarning("CofreInventarioUI: no hay InventarioCofre para abrir.", this);
            return;
        }

        if (EstaAbierto && cofreActual == nuevoCofre)
        {
            RefrescarUI();
            return;
        }

        if (inventarioJugador == null)
        {
            inventarioJugador = FindFirstObjectByType<PlayerInventory>();
        }

        PrepararReferenciasAutomaticas();
        ConfigurarSlots();

        if (panel == null)
        {
            Debug.LogWarning("CofreInventarioUI: falta asignar el PanelCofre.", this);
            return;
        }

        DesuscribirseEventos();
        cofreActual = nuevoCofre;
        SuscribirseEventos();

        if (!EstaAbierto)
        {
            cursorLockAnterior = Cursor.lockState;
            cursorVisibleAnterior = Cursor.visible;
            cursorGuardado = true;

            if (pausarJuegoAlAbrir)
            {
                timeScaleAnterior = Time.timeScale;
                timeScaleGuardado = true;
                Time.timeScale = 0f;
            }
        }

        HayAlgunaUIAbierta = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        frameApertura = Time.frameCount;

        panel.SetActive(true);

        if (contenidoVisual != null)
        {
            contenidoVisual.SetActive(true);
        }

        MostrarMensaje("Click: mover 1 objeto. Shift + click: mover todo el stack. E/ESC: cerrar.");
        RefrescarUI();
    }

    public void Cerrar()
    {
        DesuscribirseEventos();
        cofreActual = null;

        if (panel != null)
        {
            panel.SetActive(false);
        }

        if (HayAlgunaUIAbierta)
        {
            FrameUltimoCierre = Time.frameCount;
        }

        HayAlgunaUIAbierta = false;
        FrameUltimoCierre = Time.frameCount;
        RestaurarPausaSiHaceFalta();

        if (cursorGuardado)
        {
            Cursor.lockState = cursorLockAnterior;
            Cursor.visible = cursorVisibleAnterior;
            cursorGuardado = false;
        }
    }

    public void Alternar(InventarioCofre nuevoCofre)
    {
        if (EstaAbierto)
        {
            Cerrar();
        }
        else
        {
            Abrir(nuevoCofre);
        }
    }

    public void AlPulsarSlot(bool esSlotJugador, int indiceSlot)
    {
        if (!EstaAbierto || cofreActual == null || inventarioJugador == null)
        {
            return;
        }

        if (esSlotJugador)
        {
            GuardarDesdeJugador(indiceSlot);
        }
        else
        {
            SacarDelCofre(indiceSlot);
        }

        RefrescarUI();
    }

    public void RefrescarUI()
    {
        RefrescarSlotsJugador();
        RefrescarSlotsCofre();
    }

    private void GuardarDesdeJugador(int indiceSlot)
    {
        InventorySlot slotJugador = inventarioJugador.GetSlot(indiceSlot);

        if (slotJugador == null || slotJugador.IsEmpty())
        {
            MostrarMensaje("Ese slot del jugador esta vacio.");
            return;
        }

        int cantidadSolicitada = EstaPulsandoShift() ? slotJugador.amount : 1;
        int cantidadGuardada = cofreActual.IntentarAnadirItem(slotJugador.itemData, cantidadSolicitada);

        if (cantidadGuardada <= 0)
        {
            MostrarMensaje("El cofre esta lleno.");
            return;
        }

        slotJugador.amount -= cantidadGuardada;

        if (slotJugador.amount <= 0)
        {
            slotJugador.Clear();
        }

        inventarioJugador.NotifyInventoryChanged();
        MostrarMensaje("Guardado en el cofre: " + cantidadGuardada);
    }

    private void SacarDelCofre(int indiceSlot)
    {
        InventorySlot slotCofre = cofreActual.GetSlot(indiceSlot);

        if (slotCofre == null || slotCofre.IsEmpty())
        {
            MostrarMensaje("Ese slot del cofre esta vacio.");
            return;
        }

        int cantidadSolicitada = EstaPulsandoShift() ? slotCofre.amount : 1;
        int cantidadQueCabe = CalcularCantidadQueCabeEnJugador(slotCofre.itemData, cantidadSolicitada);

        if (cantidadQueCabe <= 0)
        {
            MostrarMensaje("No tienes espacio en el inventario.");
            return;
        }

        bool anadido = inventarioJugador.TryAddItem(slotCofre.itemData, cantidadQueCabe);

        if (!anadido)
        {
            MostrarMensaje("No tienes espacio en el inventario.");
            return;
        }

        cofreActual.QuitarDelSlot(indiceSlot, cantidadQueCabe);
        MostrarMensaje("Sacado del cofre: " + cantidadQueCabe);
    }

    private int CalcularCantidadQueCabeEnJugador(ItemData itemData, int cantidadMaxima)
    {
        for (int cantidad = cantidadMaxima; cantidad >= 1; cantidad--)
        {
            if (inventarioJugador.CanAddItem(itemData, cantidad))
            {
                return cantidad;
            }
        }

        return 0;
    }

    private void RefrescarSlotsJugador()
    {
        if (slotsJugador == null || inventarioJugador == null)
        {
            return;
        }

        for (int i = 0; i < slotsJugador.Length; i++)
        {
            if (slotsJugador[i] == null)
            {
                continue;
            }

            InventorySlot slotJugador = inventarioJugador.GetSlot(i);
            bool tieneItem = slotJugador != null && !slotJugador.IsEmpty();

            slotsJugador[i].Refrescar(slotJugador);
            slotsJugador[i].MarcarSeleccionado(tieneItem && i == inventarioJugador.SelectedSlotIndex);
        }
    }

    private void RefrescarSlotsCofre()
    {
        if (slotsCofre == null)
        {
            return;
        }

        for (int i = 0; i < slotsCofre.Length; i++)
        {
            if (slotsCofre[i] == null)
            {
                continue;
            }

            InventorySlot slot = cofreActual != null ? cofreActual.GetSlot(i) : null;
            slotsCofre[i].Refrescar(slot);
            slotsCofre[i].MarcarSeleccionado(false);
        }
    }

    private void PrepararReferenciasAutomaticas()
    {
        if (panel != null)
        {
            if (contenedorSlotsJugador == null)
            {
                contenedorSlotsJugador = BuscarHijoPorNombre(panel.transform, "SlotsJugador");
            }

            if (contenedorSlotsCofre == null)
            {
                contenedorSlotsCofre = BuscarHijoPorNombre(panel.transform, "SlotsCofre");
            }
        }

        if ((slotsJugador == null || slotsJugador.Length == 0) && contenedorSlotsJugador != null)
        {
            slotsJugador = ObtenerSlotsDelContenedor(contenedorSlotsJugador);
        }

        if ((slotsCofre == null || slotsCofre.Length == 0) && contenedorSlotsCofre != null)
        {
            slotsCofre = ObtenerSlotsDelContenedor(contenedorSlotsCofre);
        }
    }

    private Transform BuscarHijoPorNombre(Transform raiz, string nombre)
    {
        if (raiz == null)
        {
            return null;
        }

        Transform[] hijos = raiz.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < hijos.Length; i++)
        {
            if (hijos[i].name == nombre)
            {
                return hijos[i];
            }
        }

        return null;
    }

    private SlotCofreUI[] ObtenerSlotsDelContenedor(Transform contenedor)
    {
        if (contenedor == null)
        {
            return new SlotCofreUI[0];
        }

        InventorySlotUI[] visuales = contenedor.GetComponentsInChildren<InventorySlotUI>(true);
        SlotCofreUI[] resultado = new SlotCofreUI[visuales.Length];

        for (int i = 0; i < visuales.Length; i++)
        {
            SlotCofreUI slot = visuales[i].GetComponent<SlotCofreUI>();

            if (slot == null)
            {
                slot = visuales[i].gameObject.AddComponent<SlotCofreUI>();
            }

            resultado[i] = slot;
        }

        return resultado;
    }

    private void ConfigurarSlots()
    {
        if (slotsJugador != null)
        {
            for (int i = 0; i < slotsJugador.Length; i++)
            {
                if (slotsJugador[i] != null)
                {
                    slotsJugador[i].Configurar(this, true, i);
                }
            }
        }

        if (slotsCofre != null)
        {
            for (int i = 0; i < slotsCofre.Length; i++)
            {
                if (slotsCofre[i] != null)
                {
                    slotsCofre[i].Configurar(this, false, i);
                }
            }
        }
    }

    private bool EstaPulsandoShift()
    {
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.leftShiftKey.isPressed || Keyboard.current.rightShiftKey.isPressed;
    }

    private void MostrarMensaje(string mensaje)
    {
        if (textoMensaje != null)
        {
            textoMensaje.text = mensaje;
        }
    }

    private void SuscribirseEventos()
    {
        if (inventarioJugador != null)
        {
            inventarioJugador.OnInventoryChanged += RefrescarUI;
        }

        if (cofreActual != null)
        {
            cofreActual.OnInventarioCofreCambiado += RefrescarUI;
        }
    }

    private void DesuscribirseEventos()
    {
        if (inventarioJugador != null)
        {
            inventarioJugador.OnInventoryChanged -= RefrescarUI;
        }

        if (cofreActual != null)
        {
            cofreActual.OnInventarioCofreCambiado -= RefrescarUI;
        }
    }

    private void RestaurarPausaSiHaceFalta()
    {
        if (!timeScaleGuardado)
        {
            return;
        }

        Time.timeScale = timeScaleAnterior;
        timeScaleGuardado = false;
    }
}