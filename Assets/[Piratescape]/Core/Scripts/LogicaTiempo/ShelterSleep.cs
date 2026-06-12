using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class ShelterSleep : MonoBehaviour, Interactuable
{
    [Header("Datos del refugio")]
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;
    [SerializeField] private NightThreatSystem nightThreatSystem;

    [Header("Punto de interacción opcional")]
    [SerializeField] private Transform interactionPoint;

    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private SleepFadeUI fadeUI;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private movimientoplayer movimientoPlayer;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private Animator playerAnimator;

    [Header("Puntos de sueño")]
    [SerializeField] private Transform sleepPoint;
    [SerializeField] private Transform wakePoint;

    [Header("Reglas de sueño")]
    [SerializeField] private int minSleepHour = 18;
    [SerializeField] private int wakeHour = 8;
    [SerializeField] private int wakeMinute = 0;

    [Header("Recuperación de energía")]
    [SerializeField] private int halfRecoveryFromHour = 22;
    [SerializeField] private float halfRecoveryPercent = 0.5f;
    [SerializeField] private float lateRecoveryPercent = 0.25f;

    [Header("Animación de dormir")]
    [SerializeField] private bool movePlayerToSleepPoint = true;
    [SerializeField] private float moveToSleepDuration = 0.15f;
    [SerializeField] private float delayBeforeFade = 0f;
    [SerializeField] private string sleepTriggerName = "Sleep";

    [Header("Despertar")]
    [SerializeField] private string idleStateName = "Idle";
    

    [Header("Sonido al despertar")]
    [SerializeField] private AudioSource audioSourceDespertar;

    [Tooltip("Grupo del Audio Mixer para el sonido de despertar del pirata.")]
    [SerializeField] private AudioMixerGroup outputDespertar;

    [SerializeField] private AudioClip sonidoDespertar;

    [Range(0f, 1f)]
    [SerializeField] private float volumenDespertar = 0.8f;

    [Tooltip("Retraso SOLO del bostezo. El fade in final empieza inmediatamente.")]
    [Min(0f)]
    [SerializeField] private float retrasoSonidoDespertar = 0f;

    [Header("Cinemática monos al dormir")]
    [SerializeField] private MonkeyStealCutsceneController monkeyStealCutsceneController;
    [SerializeField] private bool reproducirCinematicaMonosSinEspantamonos = true;

    [Tooltip("La cámara de monos se cerrará obligatoriamente al pasar este tiempo.")]
    [Min(0.1f)]
    [SerializeField] private float duracionCinematicaMonos = 15f;

    [Header("Mensajes")]
    [SerializeField] private string sleepPromptMessage = "dormir";
    [SerializeField] private string blockedPromptMessage =
        "No puedes dormir hasta las 18:00";

    [Header("Texto nuevo día")]
    [SerializeField] private NewDayTextUI newDayTextUI;

    [Min(0f)]
    [SerializeField] private float retrasoTextoNuevoDia = 1.5f;

    private bool activo;
    private bool isSleeping;
    private IActivador jugador;

    private Coroutine rutinaSonidoDespertar;

    private InputDeviceType lastInputDevice =
        InputDeviceType.KeyboardMouse;

    private enum InputDeviceType
    {
        KeyboardMouse,
        PlayStation,
        Xbox,
        GenericGamepad
    }

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo =>
        activo && !isSleeping && interactuable;

    public Vector3 PosicionInteraccion =>
        GetInteractionPosition();

    private void Awake()
    {
        CachearReferencias();
        PrepararAudioDespertar();
    }

    private void Start()
    {
        if (jugador == null)
        {
            jugador =
                FindFirstObjectByType<JugadorActivador>();
        }

        if (timeSystem == null)
        {
            Debug.LogWarning(
                $"{nameof(ShelterSleep)}: falta referencia a {nameof(GameTimeSystem)}.",
                this
            );
        }

        if (playerEnergy == null)
        {
            Debug.LogWarning(
                $"{nameof(ShelterSleep)}: falta referencia a {nameof(PlayerEnergy)}.",
                this
            );
        }
    }

    private void Update()
    {
        if (jugador == null)
        {
            return;
        }

        ActualizarUltimoDispositivoUsado();

        bool nuevoEstado =
            interactuable &&
            !isSleeping &&
            EstaJugadorEnRango();

        if (nuevoEstado != activo)
        {
            activo = nuevoEstado;

            if (!activo)
            {
                OcultarPrompt();
                return;
            }

            ActualizarPromptSegunHora();
            return;
        }

        if (activo)
        {
            ActualizarPromptSegunHora();
        }
    }

    public void Interactuar()
    {
        TrySleep();
    }

    public void TrySleep()
    {
        CachearReferencias();

        if (isSleeping || !interactuable)
        {
            return;
        }

        if (timeSystem == null)
        {
            Debug.LogWarning(
                $"{nameof(ShelterSleep)}: falta referencia a {nameof(GameTimeSystem)}.",
                this
            );

            return;
        }

        if (!EstaJugadorEnRango())
        {
            return;
        }

        if (!CanSleepNow())
        {
            MostrarPromptBloqueado();

            Debug.Log(
                $"No puedes dormir hasta las {minSleepHour:00}:00"
            );

            return;
        }

        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        isSleeping = true;
        OcultarPrompt();

        bool movimientoOriginalHabilitado =
            movimientoPlayer != null &&
            movimientoPlayer.enabled;

        bool controllerOriginalHabilitado =
            playerController != null &&
            playerController.enabled;

        if (movimientoPlayer != null)
        {
            movimientoPlayer.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (movePlayerToSleepPoint &&
            playerTransform != null &&
            sleepPoint != null)
        {
            yield return MovePlayerRoutine(
                playerTransform,
                sleepPoint.position,
                sleepPoint.rotation,
                moveToSleepDuration
            );
        }

        if (playerAnimator != null &&
            !string.IsNullOrWhiteSpace(sleepTriggerName))
        {
            playerAnimator.SetTrigger(sleepTriggerName);
        }

        if (delayBeforeFade > 0f)
        {
            yield return new WaitForSecondsRealtime(
                delayBeforeFade
            );
        }

        if (fadeUI != null)
        {
            yield return fadeUI.FadeOutDormirRoutine();
        }

        yield return ReproducirCinematicaMonosSiCorresponde();

        if (nightThreatSystem != null)
        {
            nightThreatSystem.ResolveNightEvent();
        }

        if (retrasoTextoNuevoDia > 0f)
        {
            yield return new WaitForSecondsRealtime(retrasoTextoNuevoDia);
        }

        AplicarRecuperacionEnergia();

        int nuevoDia = -1;

        if (timeSystem != null)
        {
            timeSystem.SleepToNextDay(
                wakeHour,
                wakeMinute
            );

            nuevoDia = timeSystem.CurrentDay;
        }

        if (playerTransform != null &&
            wakePoint != null)
        {
            playerTransform.SetPositionAndRotation(
                wakePoint.position,
                wakePoint.rotation
            );
        }

        if (playerAnimator != null)
        {
            if (!string.IsNullOrWhiteSpace(sleepTriggerName))
            {
                playerAnimator.ResetTrigger(
                    sleepTriggerName
                );
            }

            if (!string.IsNullOrWhiteSpace(idleStateName))
            {
                playerAnimator.Play(
                    idleStateName,
                    0,
                    0f
                );

                playerAnimator.Update(0f);
            }
        }

        if (playerController != null)
        {
            playerController.enabled =
                controllerOriginalHabilitado;
        }

        if (movimientoPlayer != null)
        {
            movimientoPlayer.enabled =
                movimientoOriginalHabilitado;
        }

        LanzarSonidoDespertarConRetraso();

        if (fadeUI != null)
        {
            yield return fadeUI.FadeInRoutine();
        }

        Debug.Log("Voy a mostrar texto nuevo día: " + nuevoDia);

        if (newDayTextUI != null && nuevoDia > 0)
        {
            yield return newDayTextUI.ShowDayRoutine(
                nuevoDia
            );
        }
        else
        {
            Debug.LogWarning("No se puede mostrar texto nuevo día. Referencia: " + newDayTextUI + " Día: " + nuevoDia);
        }

        isSleeping = false;

        activo =
            interactuable &&
            EstaJugadorEnRango();

        if (activo)
        {
            ActualizarPromptSegunHora();
        }
    }

    private IEnumerator ReproducirCinematicaMonosSiCorresponde()
    {
        if (!DebeReproducirCinematicaMonos())
        {
            yield break;
        }

        monkeyStealCutsceneController.PrepararCinematica();

        if (fadeUI != null)
        {
            yield return fadeUI.FadeInRoutine();
        }

        monkeyStealCutsceneController.IniciarMovimientoMonos();

        float tiempo = 0f;

        while (tiempo < duracionCinematicaMonos)
        {
            tiempo += Time.unscaledDeltaTime;
            yield return null;
        }

        if (fadeUI != null)
        {
            yield return fadeUI.FadeOutRoutine();
        }

        monkeyStealCutsceneController.FinalizarCinematica();
    }

    private bool DebeReproducirCinematicaMonos()
    {
        if (!reproducirCinematicaMonosSinEspantamonos)
        {
            return false;
        }

        if (monkeyStealCutsceneController == null)
        {
            return false;
        }

        if (nightThreatSystem == null)
        {
            return false;
        }

        return !nightThreatSystem
            .HayProteccionEspantamonosActiva();
    }

    private void AplicarRecuperacionEnergia()
    {
        if (playerEnergy == null ||
            timeSystem == null)
        {
            return;
        }

        float porcentajeRecuperacion =
            ObtenerPorcentajeRecuperacionSegunHora();

        float energiaObjetivo =
            playerEnergy.MaxEnergy *
            porcentajeRecuperacion;

        if (playerEnergy.CurrentEnergy < energiaObjetivo)
        {
            playerEnergy.SetEnergy(
                energiaObjetivo
            );
        }
    }

    private float ObtenerPorcentajeRecuperacionSegunHora()
    {
        int horaActual = timeSystem.CurrentHour;

        if (horaActual < wakeHour)
        {
            return lateRecoveryPercent;
        }

        if (horaActual >= halfRecoveryFromHour)
        {
            return halfRecoveryPercent;
        }

        return 1f;
    }

    private bool CanSleepNow()
    {
        return timeSystem != null &&
               (
                   timeSystem.CurrentHour >= minSleepHour ||
                   timeSystem.CurrentHour < wakeHour
               );
    }

    private bool EstaJugadorEnRango()
    {
        if (jugador == null)
        {
            return false;
        }

        return Vector3.Distance(
            GetInteractionPosition(),
            jugador.Position
        ) <= rango;
    }

    private Vector3 GetInteractionPosition()
    {
        return interactionPoint != null
            ? interactionPoint.position
            : transform.position;
    }

    private void ActualizarPromptSegunHora()
    {
        if (CanSleepNow())
        {
            MostrarPromptDormir();
        }
        else
        {
            MostrarPromptBloqueado();
        }
    }

    private void MostrarPromptDormir()
    {
        InteractionUI.Instance?.Show(
            this,
            ConstruirMensajeDormir()
        );
    }

    private string ConstruirMensajeDormir()
    {
        string icono = GetInteractionIcon();

        if (string.IsNullOrWhiteSpace(sleepPromptMessage))
        {
            return $"Pulsa {icono} para dormir";
        }

        if (sleepPromptMessage.Contains("{0}"))
        {
            return string.Format(
                sleepPromptMessage,
                icono
            );
        }

        string textoNormalizado =
            sleepPromptMessage.ToLower();

        if (textoNormalizado.Contains("pulsa") ||
            textoNormalizado.Contains("press"))
        {
            return sleepPromptMessage;
        }

        if (textoNormalizado.StartsWith("para "))
        {
            return $"Pulsa {icono} {sleepPromptMessage}";
        }

        return $"Pulsa {icono} para {sleepPromptMessage}";
    }

    private void MostrarPromptBloqueado()
    {
        InteractionUI.Instance?.Show(
            this,
            blockedPromptMessage
        );
    }

    private void OcultarPrompt()
    {
        InteractionUI.Instance?.Hide(this);
    }

    private IEnumerator MovePlayerRoutine(
        Transform target,
        Vector3 destinationPosition,
        Quaternion destinationRotation,
        float duration)
    {
        if (target == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            target.SetPositionAndRotation(
                destinationPosition,
                destinationRotation
            );

            yield break;
        }

        Vector3 startPosition = target.position;
        Quaternion startRotation = target.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(
                elapsed / duration
            );

            target.position = Vector3.Lerp(
                startPosition,
                destinationPosition,
                t
            );

            target.rotation = Quaternion.Slerp(
                startRotation,
                destinationRotation,
                t
            );

            yield return null;
        }

        target.SetPositionAndRotation(
            destinationPosition,
            destinationRotation
        );
    }

    private void PrepararAudioDespertar()
    {
        if (audioSourceDespertar == null)
        {
            audioSourceDespertar = GetComponent<AudioSource>();
        }

        if (audioSourceDespertar == null)
        {
            audioSourceDespertar = gameObject.AddComponent<AudioSource>();
        }

        audioSourceDespertar.playOnAwake = false;
        audioSourceDespertar.loop = false;
        audioSourceDespertar.spatialBlend = 0f;
        audioSourceDespertar.dopplerLevel = 0f;
        audioSourceDespertar.outputAudioMixerGroup = outputDespertar;
    }

    private void LanzarSonidoDespertarConRetraso()
    {
        if (sonidoDespertar == null)
        {
            return;
        }

        if (rutinaSonidoDespertar != null)
        {
            StopCoroutine(rutinaSonidoDespertar);
            rutinaSonidoDespertar = null;
        }

        rutinaSonidoDespertar =
            StartCoroutine(ReproducirSonidoDespertarConRetrasoRoutine());
    }

    private IEnumerator ReproducirSonidoDespertarConRetrasoRoutine()
    {
        if (retrasoSonidoDespertar > 0f)
        {
            yield return new WaitForSecondsRealtime(
                retrasoSonidoDespertar
            );
        }

        ReproducirSonidoDespertar();
        rutinaSonidoDespertar = null;
    }

    private void ReproducirSonidoDespertar()
    {
        if (audioSourceDespertar == null ||
            sonidoDespertar == null)
        {
            return;
        }

        audioSourceDespertar.Stop();
        audioSourceDespertar.pitch = 1f;

        audioSourceDespertar.PlayOneShot(
            sonidoDespertar,
            volumenDespertar
        );
    }

    private void CachearReferencias()
    {
        if (timeSystem == null)
        {
            timeSystem =
                FindFirstObjectByType<GameTimeSystem>();
        }

        if (fadeUI == null)
        {
            fadeUI =
                FindFirstObjectByType<SleepFadeUI>();
        }

        if (nightThreatSystem == null)
        {
            nightThreatSystem =
                FindFirstObjectByType<NightThreatSystem>();
        }

        if (monkeyStealCutsceneController == null)
        {
            monkeyStealCutsceneController =
                FindFirstObjectByType<MonkeyStealCutsceneController>();
        }

        if (playerTransform == null)
        {
            JugadorActivador jugadorActivador =
                FindFirstObjectByType<JugadorActivador>();

            if (jugadorActivador != null)
            {
                playerTransform =
                    jugadorActivador.transform;
            }
        }

        if (movimientoPlayer == null &&
            playerTransform != null)
        {
            movimientoPlayer =
                playerTransform
                    .GetComponent<movimientoplayer>();
        }

        if (playerEnergy == null &&
            playerTransform != null)
        {
            playerEnergy =
                playerTransform
                    .GetComponent<PlayerEnergy>();
        }

        if (playerController == null &&
            playerTransform != null)
        {
            playerController =
                playerTransform
                    .GetComponent<CharacterController>();
        }

        if (playerAnimator == null &&
            playerTransform != null)
        {
            playerAnimator =
                playerTransform
                    .GetComponentInChildren<Animator>();
        }
    }

    private void ActualizarUltimoDispositivoUsado()
    {
        if (Keyboard.current != null &&
            Keyboard.current.anyKey.wasPressedThisFrame)
        {
            lastInputDevice =
                InputDeviceType.KeyboardMouse;

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
                lastInputDevice =
                    InputDeviceType.KeyboardMouse;

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
                lastInputDevice =
                    DetectarTipoMando(
                        Gamepad.current
                    );
            }
        }
    }

    private InputDeviceType DetectarTipoMando(
        Gamepad gamepad)
    {
        if (gamepad == null)
        {
            return InputDeviceType.GenericGamepad;
        }

        string displayName =
            gamepad.displayName?.ToLower() ?? "";

        string name =
            gamepad.name?.ToLower() ?? "";

        string manufacturer =
            gamepad.description.manufacturer?.ToLower() ?? "";

        string product =
            gamepad.description.product?.ToLower() ?? "";

        string combinedInfo =
            displayName + " " +
            name + " " +
            manufacturer + " " +
            product;

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
            case InputDeviceType.GenericGamepad:
                return "X";

            default:
                return "E";
        }
    }

    private void OnDisable()
    {
        if (rutinaSonidoDespertar != null)
        {
            StopCoroutine(rutinaSonidoDespertar);
            rutinaSonidoDespertar = null;
        }

        if (audioSourceDespertar != null)
        {
            audioSourceDespertar.Stop();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            GetInteractionPositionEditor(),
            rango
        );
    }

    private Vector3 GetInteractionPositionEditor()
    {
        return interactionPoint != null
            ? interactionPoint.position
            : transform.position;
    }
#endif
}