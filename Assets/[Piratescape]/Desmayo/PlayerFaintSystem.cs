using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerFaintSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameTimeSystem timeSystem;
    [SerializeField] private SleepFadeUI fadeUI;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private movimientoplayer movimientoPlayer;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private CharacterController playerController;
    [SerializeField] private GameObject faintCarrySequence;
    [SerializeField] private GameObject playerVisualRoot;
    [SerializeField] private NightThreatSystem nightThreatSystem;

    [Header("Aviso antes del desmayo")]
    [SerializeField] private int avisoHour = 2;
    [SerializeField] private int avisoMinute = 30;
    [SerializeField]
    private string mensajeAvisoDesmayo =
        "Me estoy agotando... debería volver pronto al refugio.";

    private int ultimoDiaAvisoDesmayo = -1;

    [Header("Camara Cinemachine")]
    [SerializeField] private CinemachineCamera freeLookCamera;
    [SerializeField] private Transform faintCameraTarget;

    [Header("Punto de reaparicion")]
    [SerializeField] private Transform tentWakePoint;

    [Header("Hora del desmayo")]
    [SerializeField] private int faintHour = 3;
    [SerializeField] private int faintMinute = 0;

    [Header("Hora de despertar tras desmayo")]
    [SerializeField] private int faintWakeHour = 6;
    [SerializeField] private int faintWakeMinute = 0;

    [Header("Caida")]
    [SerializeField] private float fallDuration = 0.55f;
    [SerializeField] private Vector3 fallenRotation = new Vector3(-90f, 0f, 0f);
    [SerializeField] private float fallHeightOffset = 0.3f;

    [Header("Secuencia")]
    [SerializeField] private float blackScreenDuration = 1.0f;
    [SerializeField] private float carryMoveDuration = 2.5f;
    [SerializeField] private float carryEndPause = 0.2f;
    [SerializeField] private Vector3 faintCarryOffset = Vector3.zero;
    [SerializeField] private Vector3 carryArrivalOffset = Vector3.zero;

    private bool isFainting;
    private int lastFaintDay = -1;

    private Transform originalTrackingTarget;
    private Transform originalLookAtTarget;
    private bool originalCustomLookAtTarget;

    private void Awake()
    {
        CachearReferencias();

        if (faintCarrySequence != null)
        {
            faintCarrySequence.SetActive(false);
        }
    }

    private void Update()
    {
        if (isFainting || timeSystem == null)
        {
            return;
        }

        if (YaSeDesmayoHoy())
        {
            return;
        }

        ComprobarAvisoAntesDesmayo();

        if (EsHoraDeDesmayo())
        {
            StartCoroutine(FaintRoutine());
        }
    }

    private void ComprobarAvisoAntesDesmayo()
    {
        if (timeSystem == null)
        {
            return;
        }

        // Ya salió hoy
        if (ultimoDiaAvisoDesmayo == timeSystem.CurrentDay)
        {
            return;
        }

        bool esHoraAviso =
            timeSystem.CurrentHour == avisoHour &&
            timeSystem.CurrentMinute == avisoMinute;

        if (!esHoraAviso)
        {
            return;
        }

        ultimoDiaAvisoDesmayo = timeSystem.CurrentDay;

        if (NightMessageUI.Instance != null)
        {
            NightMessageUI.Instance.ShowMessage(mensajeAvisoDesmayo);
        }
        else
        {
            Debug.Log("Aviso desmayo: " + mensajeAvisoDesmayo);
        }
    }

    private bool EsHoraDeDesmayo()
    {
        return timeSystem.CurrentHour == faintHour &&
               timeSystem.CurrentMinute == faintMinute;
    }

    private bool YaSeDesmayoHoy()
    {
        return timeSystem.CurrentDay == lastFaintDay;
    }

    private IEnumerator FaintRoutine()
    {
        isFainting = true;
        lastFaintDay = timeSystem.CurrentDay;

        bool movimientoOriginalHabilitado = movimientoPlayer != null && movimientoPlayer.enabled;
        bool controllerOriginalHabilitado = playerController != null && playerController.enabled;

        if (movimientoPlayer != null)
        {
            movimientoPlayer.PlayFaintAnimation();
        }
        else
        {
            yield return StartCoroutine(FallDownRoutine());
        }

        yield return new WaitForSeconds(fallDuration);

        if (movimientoPlayer != null)
        {
            movimientoPlayer.enabled = false;
        }

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        Vector3 faintWorldPosition = playerTransform != null
            ? playerTransform.position + faintCarryOffset
            : Vector3.zero;

        if (fadeUI != null)
        {
            yield return fadeUI.FadeOutRoutine();
        }

        if (playerVisualRoot != null)
        {
            playerVisualRoot.SetActive(false);
        }

        if (faintCarrySequence != null)
        {
            faintCarrySequence.transform.position = faintWorldPosition;
            faintCarrySequence.SetActive(true);
        }

        CambiarCamaraAlMono();

        yield return new WaitForSeconds(blackScreenDuration);

        if (fadeUI != null)
        {
            yield return fadeUI.FadeInRoutine();
        }

        yield return StartCoroutine(MoveCarrySequenceToTent());

        yield return new WaitForSeconds(carryEndPause);

        if (playerTransform != null && tentWakePoint != null)
        {
            playerTransform.SetPositionAndRotation(tentWakePoint.position, tentWakePoint.rotation);
        }

        if (timeSystem != null)
        {
            timeSystem.SetTime(faintWakeHour, faintWakeMinute);
        }

        if (playerEnergy != null)
        {
            playerEnergy.SetEnergy(0f);
        }

        if (nightThreatSystem != null)
        {
            nightThreatSystem.ResolveFaintEvent();
        }
        else
        {
            Debug.LogWarning("PlayerFaintSystem: falta referencia a NightThreatSystem", this);
        }

        if (faintCarrySequence != null)
        {
            faintCarrySequence.SetActive(false);
        }

        if (playerVisualRoot != null)
        {
            playerVisualRoot.SetActive(true);
        }

        RestaurarCamara();

        if (playerController != null)
        {
            playerController.enabled = controllerOriginalHabilitado;
        }

        if (movimientoPlayer != null)
        {
            movimientoPlayer.enabled = movimientoOriginalHabilitado;
            movimientoPlayer.RecoverFromFaint();
        }

        isFainting = false;
    }

    private void CambiarCamaraAlMono()
    {
        if (freeLookCamera == null || faintCameraTarget == null)
        {
            return;
        }

        var target = freeLookCamera.Target;

        originalTrackingTarget = target.TrackingTarget;
        originalLookAtTarget = target.LookAtTarget;
        originalCustomLookAtTarget = target.CustomLookAtTarget;

        target.TrackingTarget = faintCameraTarget;
        target.LookAtTarget = faintCameraTarget;
        target.CustomLookAtTarget = true;

        freeLookCamera.Target = target;
    }

    private void RestaurarCamara()
    {
        if (freeLookCamera == null)
        {
            return;
        }

        var target = freeLookCamera.Target;

        target.TrackingTarget = originalTrackingTarget;
        target.LookAtTarget = originalLookAtTarget;
        target.CustomLookAtTarget = originalCustomLookAtTarget;

        freeLookCamera.Target = target;
    }

    private IEnumerator FallDownRoutine()
    {
        if (playerTransform == null)
        {
            yield break;
        }

        Quaternion startRotation = playerTransform.rotation;
        Quaternion targetRotation = Quaternion.Euler(fallenRotation);

        Vector3 startPosition = playerTransform.position;
        Vector3 targetPosition = startPosition + new Vector3(0f, fallHeightOffset, 0f);

        float elapsed = 0f;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fallDuration);

            playerTransform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            playerTransform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        playerTransform.rotation = targetRotation;
        playerTransform.position = targetPosition;
    }

    private IEnumerator MoveCarrySequenceToTent()
    {
        if (faintCarrySequence == null || tentWakePoint == null)
        {
            yield break;
        }

        Vector3 startPosition = faintCarrySequence.transform.position;
        Vector3 targetPosition = tentWakePoint.position + carryArrivalOffset;

        Vector3 direction = targetPosition - startPosition;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            faintCarrySequence.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }

        float elapsed = 0f;

        while (elapsed < carryMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / carryMoveDuration);

            faintCarrySequence.transform.position = Vector3.Lerp(startPosition, targetPosition, t);

            yield return null;
        }

        faintCarrySequence.transform.position = targetPosition;
    }

    private void CachearReferencias()
    {
        if (timeSystem == null)
        {
            timeSystem = FindFirstObjectByType<GameTimeSystem>();
        }

        if (fadeUI == null)
        {
            fadeUI = FindFirstObjectByType<SleepFadeUI>();
        }

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
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

        if (nightThreatSystem == null)
        {
            nightThreatSystem = FindFirstObjectByType<NightThreatSystem>();
        }
    }
}