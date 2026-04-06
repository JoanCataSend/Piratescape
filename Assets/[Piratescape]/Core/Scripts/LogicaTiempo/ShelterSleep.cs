using System.Collections;
using UnityEngine;

public class ShelterSleep : MonoBehaviour, Interactuable
{
    [Header("Datos del refugio")]
    [SerializeField] private string objectName = "tienda";
    [SerializeField] private float rango = 2.5f;
    [SerializeField] private bool interactuable = true;

    [Header("Punto de interaccion opcional")]
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

    [Header("Recuperacion de energia")]
    [SerializeField] private int halfRecoveryFromHour = 22;
    [SerializeField] private float halfRecoveryPercent = 0.5f;
    [SerializeField] private float lateRecoveryPercent = 0.25f;

    [Header("Animacion provisional")]
    [SerializeField] private bool movePlayerToSleepPoint = true;
    [SerializeField] private float moveToSleepDuration = 0.5f;
    [SerializeField] private float delayBeforeFade = 0.2f;
    [SerializeField] private string sleepTriggerName = "Sleep";

    [Header("Mensajes")]
    [SerializeField] private string sleepPromptMessage = "Pulsa E para dormir";
    [SerializeField] private string blockedPromptMessage = "No puedes dormir hasta las 18:00";

    private bool activo;
    private bool isSleeping;
    private IActivador jugador;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => activo && !isSleeping && interactuable;

    private void Awake()
    {
        CachearReferencias();
    }

    private void Start()
    {
        if (jugador == null)
        {
            jugador = FindFirstObjectByType<JugadorActivador>();
        }

        if (timeSystem == null)
        {
            Debug.LogWarning($"{nameof(ShelterSleep)}: falta referencia a {nameof(GameTimeSystem)}.", this);
        }

        if (playerEnergy == null)
        {
            Debug.LogWarning($"{nameof(ShelterSleep)}: falta referencia a {nameof(PlayerEnergy)}.", this);
        }
    }

    private void Update()
    {
        if (jugador == null)
        {
            return;
        }

        bool nuevoEstado = interactuable &&
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

        if (!activo)
        {
            return;
        }

        ActualizarPromptSegunHora();
    }

    public void Interactuar()
    {
        TrySleep();
    }

    public void TrySleep()
    {
        if (isSleeping || !interactuable)
        {
            return;
        }

        if (timeSystem == null)
        {
            Debug.LogWarning($"{nameof(ShelterSleep)}: falta referencia a {nameof(GameTimeSystem)}.", this);
            return;
        }

        if (!EstaJugadorEnRango())
        {
            return;
        }

        if (!CanSleepNow())
        {
            MostrarPromptBloqueado();
            Debug.Log($"No puedes dormir hasta las {minSleepHour:00}:00");
            return;
        }

        StartCoroutine(SleepRoutine());
    }

    private IEnumerator SleepRoutine()
    {
        isSleeping = true;
        OcultarPrompt();

        bool movimientoOriginalHabilitado = movimientoPlayer != null && movimientoPlayer.enabled;
        bool controllerOriginalHabilitado = playerController != null && playerController.enabled;

        if (movimientoPlayer != null)
        {
            movimientoPlayer.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (movePlayerToSleepPoint && playerTransform != null && sleepPoint != null)
        {
            yield return MovePlayerRoutine(playerTransform, sleepPoint.position, sleepPoint.rotation, moveToSleepDuration);
        }

        if (playerAnimator != null && !string.IsNullOrWhiteSpace(sleepTriggerName))
        {
            playerAnimator.SetTrigger(sleepTriggerName);
        }

        if (delayBeforeFade > 0f)
        {
            yield return new WaitForSeconds(delayBeforeFade);
        }

        if (fadeUI != null)
        {
            yield return fadeUI.FadeOutRoutine();
        }

        AplicarRecuperacionEnergia();
        timeSystem.SleepToNextDay(wakeHour, wakeMinute);

        if (playerTransform != null && wakePoint != null)
        {
            playerTransform.SetPositionAndRotation(wakePoint.position, wakePoint.rotation);
        }

        if (playerController != null)
        {
            playerController.enabled = controllerOriginalHabilitado;
        }

        if (movimientoPlayer != null)
        {
            movimientoPlayer.enabled = movimientoOriginalHabilitado;
        }

        if (fadeUI != null)
        {
            yield return fadeUI.FadeInRoutine();
        }

        isSleeping = false;

        activo = interactuable && EstaJugadorEnRango();

        if (activo)
        {
            ActualizarPromptSegunHora();
        }
    }

    private void AplicarRecuperacionEnergia()
    {
        if (playerEnergy == null || timeSystem == null)
        {
            return;
        }

        float porcentajeRecuperacion = ObtenerPorcentajeRecuperacionSegunHora();
        float energiaObjetivo = playerEnergy.MaxEnergy * porcentajeRecuperacion;

        if (playerEnergy.CurrentEnergy < energiaObjetivo)
        {
            playerEnergy.SetEnergy(energiaObjetivo);
        }
    }

    private float ObtenerPorcentajeRecuperacionSegunHora()
    {
        int horaActual = timeSystem.CurrentHour;

        // De 00:00 a 07:59 -> recupera hasta el 25%
        if (horaActual < wakeHour)
        {
            return lateRecoveryPercent;
        }

        // De 22:00 a 23:59 -> recupera hasta el 50%
        if (horaActual >= halfRecoveryFromHour)
        {
            return halfRecoveryPercent;
        }

        // De 18:00 a 21:59 -> recupera hasta el 100%
        return 1f;
    }

    private bool CanSleepNow()
    {
        return timeSystem != null &&
               (timeSystem.CurrentHour >= minSleepHour || timeSystem.CurrentHour < wakeHour);
    }

    private bool EstaJugadorEnRango()
    {
        if (jugador == null)
        {
            return false;
        }

        Vector3 centro = GetInteractionPosition();
        return Vector3.Distance(centro, jugador.Position) <= rango;
    }

    private Vector3 GetInteractionPosition()
    {
        if (interactionPoint != null)
        {
            return interactionPoint.position;
        }

        return transform.position;
    }

    private void ActualizarPromptSegunHora()
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

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
        if (InteractionUI.Instance == null)
        {
            return;
        }

        InteractionUI.Instance.Show(this, sleepPromptMessage);
    }

    private void MostrarPromptBloqueado()
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

        InteractionUI.Instance.Show(this, blockedPromptMessage);
    }

    private void OcultarPrompt()
    {
        if (InteractionUI.Instance != null)
        {
            InteractionUI.Instance.Hide(this);
        }
    }

    private IEnumerator MovePlayerRoutine(Transform target, Vector3 destinationPosition, Quaternion destinationRotation, float duration)
    {
        if (target == null)
        {
            yield break;
        }

        if (duration <= 0f)
        {
            target.SetPositionAndRotation(destinationPosition, destinationRotation);
            yield break;
        }

        Vector3 startPosition = target.position;
        Quaternion startRotation = target.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            target.position = Vector3.Lerp(startPosition, destinationPosition, t);
            target.rotation = Quaternion.Slerp(startRotation, destinationRotation, t);

            yield return null;
        }

        target.SetPositionAndRotation(destinationPosition, destinationRotation);
    }

    private void CachearReferencias()
    {
        if (playerTransform == null)
        {
            JugadorActivador jugadorActivador = FindFirstObjectByType<JugadorActivador>();

            if (jugadorActivador != null)
            {
                playerTransform = jugadorActivador.transform;
            }
        }

        if (movimientoPlayer == null && playerTransform != null)
        {
            movimientoPlayer = playerTransform.GetComponent<movimientoplayer>();
        }

        if (playerEnergy == null && playerTransform != null)
        {
            playerEnergy = playerTransform.GetComponent<PlayerEnergy>();
        }

        if (playerController == null && playerTransform != null)
        {
            playerController = playerTransform.GetComponent<CharacterController>();
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetInteractionPositionEditor(), rango);
    }

    private Vector3 GetInteractionPositionEditor()
    {
        if (interactionPoint != null)
        {
            return interactionPoint.position;
        }

        return transform.position;
    }
#endif
}