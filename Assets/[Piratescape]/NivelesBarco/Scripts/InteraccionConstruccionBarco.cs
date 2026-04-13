using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InteraccionConstruccionBarco : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ConstruccionBarco construccionBarco;
    [SerializeField] private ConstruccionBarcoUI construccionBarcoUI;

    [Header("Configuracion")]
    [SerializeField] private string accionPrompt = "para ingresar materiales";

    private bool jugadorDentro;
    private InputDeviceType ultimoDispositivoUsado = InputDeviceType.KeyboardMouse;

    private enum InputDeviceType
    {
        KeyboardMouse,
        PlayStation,
        Xbox,
        GenericGamepad
    }

    private void Awake()
    {
        if (construccionBarco == null)
        {
            construccionBarco = GetComponentInParent<ConstruccionBarco>();
        }
    }

    private void Update()
    {
        if (!jugadorDentro)
        {
            return;
        }

        if (construccionBarco == null || construccionBarco.ConstruccionCompletada)
        {
            OcultarPrompt();
            return;
        }

        ActualizarUltimoDispositivoUsado();
        MostrarPrompt();

        if (!SeHaPulsadoInteraccion())
        {
            return;
        }

        construccionBarco.IntentarEntregarUnaUnidad();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        jugadorDentro = true;
        ActualizarUltimoDispositivoUsado();
        MostrarPrompt();

        if (construccionBarcoUI != null)
        {
            construccionBarcoUI.EstablecerJugadorDentro(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        jugadorDentro = false;
        OcultarPrompt();

        if (construccionBarcoUI != null)
        {
            construccionBarcoUI.EstablecerJugadorDentro(false);
        }
    }

    private bool SeHaPulsadoInteraccion()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            ultimoDispositivoUsado = InputDeviceType.KeyboardMouse;
            return true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            ultimoDispositivoUsado = DetectarTipoMando(Gamepad.current);
            return true;
        }

        return false;
    }

    private void ActualizarUltimoDispositivoUsado()
    {
        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            ultimoDispositivoUsado = InputDeviceType.KeyboardMouse;
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
                ultimoDispositivoUsado = InputDeviceType.KeyboardMouse;
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
                ultimoDispositivoUsado = DetectarTipoMando(Gamepad.current);
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

    private string ObtenerIconoInteraccion()
    {
        switch (ultimoDispositivoUsado)
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

    private void MostrarPrompt()
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

        string icono = ObtenerIconoInteraccion();
        InteractionUI.Instance.Show(this, "Pulsa " + icono + " " + accionPrompt);
    }

    private void OcultarPrompt()
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

        InteractionUI.Instance.Hide(this);
    }
}