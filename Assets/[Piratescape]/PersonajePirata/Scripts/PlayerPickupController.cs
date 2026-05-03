using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerPickupController : MonoBehaviour
{
    [SerializeField] private MonoBehaviour itemReceiverSource;

    private IItemReceiver itemReceiver;
    private JugadorActivador jugadorActivador;
    private ObjetoRecogibleInteractuable objetoActual;

    private InputDeviceType lastInputDevice = InputDeviceType.KeyboardMouse;

    private enum InputDeviceType
    {
        KeyboardMouse,
        PlayStation,
        Xbox,
        GenericGamepad
    }

    private void Awake()
    {
        itemReceiver = itemReceiverSource as IItemReceiver;

        if (itemReceiver == null)
        {
            PlayerInventory inventory = GetComponent<PlayerInventory>();

            if (inventory != null)
            {
                itemReceiver = inventory;
                itemReceiverSource = inventory;
                Debug.Log("PlayerPickupController: itemReceiver asignado automáticamente a PlayerInventory.", this);
            }
        }

        if (itemReceiver == null)
        {
            Debug.LogError("PlayerPickupController: no se encontró ningún IItemReceiver válido en el jugador.", this);
        }

        jugadorActivador = GetComponent<JugadorActivador>();

        if (jugadorActivador == null)
        {
            Debug.LogError("PlayerPickupController: JugadorActivador not found on player.", this);
        }
    }

    private void Update()
    {
        if (jugadorActivador == null)
        {
            return;
        }

        ActualizarUltimoDispositivoUsado();

        ObjetoRecogibleInteractuable nuevoObjeto = BuscarRecogibleMasCercano();

        if (objetoActual != nuevoObjeto)
        {
            if (objetoActual != null)
            {
                objetoActual.OcultarPrompt();
            }

            objetoActual = nuevoObjeto;
        }

        if (objetoActual != null)
        {
            objetoActual.MostrarPrompt(GetInteractionIcon());

            if (SeHaPulsadoInteraccion())
            {
                TryCollect(objetoActual);
            }
        }
    }

    public bool TryCollect(ICollectible collectible)
    {
        if (collectible == null)
        {
            Debug.LogWarning("PlayerPickupController: collectible is null.");
            return false;
        }

        ObjetoRecogibleInteractuable objetoRecogible = collectible as ObjetoRecogibleInteractuable;

        if (objetoRecogible != null && EsObjetoDelJugador(objetoRecogible))
        {
            objetoRecogible.OcultarPrompt();
            return false;
        }

        if (collectible.ItemData == null)
        {
            Debug.LogWarning("PlayerPickupController: collectible item data is null.");
            return false;
        }

        if (itemReceiver == null)
        {
            Debug.LogWarning("PlayerPickupController: item receiver is not assigned.");
            return false;
        }

        Debug.Log("Intentando recoger: " + collectible.ItemData.DisplayName + " x" + collectible.Amount);

        bool added = itemReceiver.TryAddItem(collectible.ItemData, collectible.Amount);

        Debug.Log("Resultado TryAddItem: " + added);

        if (!added)
        {
            return false;
        }

        collectible.OnCollected();

        if (objetoActual == objetoRecogible)
        {
            objetoActual = null;
        }

        return true;
    }

    private ObjetoRecogibleInteractuable BuscarRecogibleMasCercano()
    {
        ObjetoRecogibleInteractuable[] todos = FindObjectsByType<ObjetoRecogibleInteractuable>(FindObjectsSortMode.None);

        ObjetoRecogibleInteractuable mejor = null;
        float mejorDistancia = float.MaxValue;

        Vector3 posicionJugador = jugadorActivador.Position;

        foreach (ObjetoRecogibleInteractuable objeto in todos)
        {
            if (objeto == null || !objeto.EstaDisponible)
            {
                continue;
            }

            if (EsObjetoDelJugador(objeto))
            {
                objeto.OcultarPrompt();
                continue;
            }

            if (!objeto.EstaEnRango(posicionJugador))
            {
                continue;
            }

            float distancia = Vector3.Distance(objeto.transform.position, posicionJugador);

            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = objeto;
            }
        }

        return mejor;
    }

    private bool EsObjetoDelJugador(ObjetoRecogibleInteractuable objeto)
    {
        if (objeto == null)
        {
            return false;
        }

        return objeto.transform.IsChildOf(transform);
    }

    private bool SeHaPulsadoInteraccion()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            lastInputDevice = InputDeviceType.KeyboardMouse;
            return true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            lastInputDevice = DetectarTipoMando(Gamepad.current);
            return true;
        }

        return false;
    }

    private void ActualizarUltimoDispositivoUsado()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            lastInputDevice = InputDeviceType.KeyboardMouse;
            return;
        }

        if (Mouse.current != null)
        {
            bool mouseUsado =
                Mouse.current.leftButton.wasPressedThisFrame ||
                Mouse.current.rightButton.wasPressedThisFrame ||
                Mouse.current.middleButton.wasPressedThisFrame ||
                Mouse.current.delta.ReadValue() != Vector2.zero ||
                Mouse.current.scroll.ReadValue() != Vector2.zero;

            if (mouseUsado)
            {
                lastInputDevice = InputDeviceType.KeyboardMouse;
                return;
            }
        }

        if (Gamepad.current != null)
        {
            bool mandoUsado =
                Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f ||
                Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f ||
                Gamepad.current.dpad.ReadValue().sqrMagnitude > 0.01f ||
                Gamepad.current.leftTrigger.ReadValue() > 0.1f ||
                Gamepad.current.rightTrigger.ReadValue() > 0.1f ||
                Gamepad.current.buttonSouth.wasPressedThisFrame ||
                Gamepad.current.buttonNorth.wasPressedThisFrame ||
                Gamepad.current.buttonEast.wasPressedThisFrame ||
                Gamepad.current.buttonWest.wasPressedThisFrame ||
                Gamepad.current.startButton.wasPressedThisFrame ||
                Gamepad.current.selectButton.wasPressedThisFrame;

            if (mandoUsado)
            {
                lastInputDevice = DetectarTipoMando(Gamepad.current);
            }
        }
    }

    private InputDeviceType DetectarTipoMando(Gamepad gamepad)
    {
        if (gamepad == null)
        {
            return InputDeviceType.GenericGamepad;
        }

        string displayName = gamepad.displayName != null ? gamepad.displayName.ToLower() : "";
        string name = gamepad.name != null ? gamepad.name.ToLower() : "";
        string manufacturer = gamepad.description.manufacturer != null
            ? gamepad.description.manufacturer.ToLower()
            : "";
        string product = gamepad.description.product != null
            ? gamepad.description.product.ToLower()
            : "";

        string combinedInfo = displayName + " " + name + " " + manufacturer + " " + product;

        Debug.Log("MANDO DETECTADO -> " + combinedInfo);

        if (combinedInfo.Contains("sony") ||
            combinedInfo.Contains("playstation") ||
            combinedInfo.Contains("dualshock") ||
            combinedInfo.Contains("dualsense") ||
            combinedInfo.Contains("wireless controller"))
        {
            return InputDeviceType.PlayStation;
        }

        if (combinedInfo.Contains("xbox") ||
            combinedInfo.Contains("microsoft") ||
            combinedInfo.Contains("xinput"))
        {
            return InputDeviceType.Xbox;
        }

        return InputDeviceType.GenericGamepad;
    }

    private string GetInteractionIcon()
    {
        switch (lastInputDevice)
        {
            case InputDeviceType.PlayStation:
                return "□";

            case InputDeviceType.Xbox:
                return "X";

            case InputDeviceType.GenericGamepad:
                return "X";

            case InputDeviceType.KeyboardMouse:
            default:
                return "E";
        }
    }
}