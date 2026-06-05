using System.Collections;
using UnityEngine;

public class GemDoorEmergenceTrigger : MonoBehaviour
{
    [Header("Inventario")]
    [SerializeField] private PlayerInventory playerInventory;

    [Header("Gemas necesarias")]
    [SerializeField] private ItemData gema1;
    [SerializeField] private ItemData gema2;
    [SerializeField] private ItemData gema3;

    [Header("Animación de la puerta/cofre")]
    [SerializeField] private ChestEmergenceAnimation doorEmergenceAnimation;

    [Header("Cámaras")]
    [SerializeField] private GameObject gameplayCamera;
    [SerializeField] private GameObject cinematicCamera;

    [Header("Tiempos")]
    [SerializeField] private float cinematicDuration = 6f;
    [SerializeField] private bool volverACamaraGameplay = true;

    [Header("Temblor de cámara")]
    [SerializeField] private bool activarTemblorCamara = true;
    [SerializeField] private float duracionTemblor = 5f;
    [SerializeField] private float intensidadTemblor = 0.08f;
    [SerializeField] private float velocidadTemblor = 25f;

    private bool activated = false;

    private Coroutine cameraShakeCoroutine;

    private void Awake()
    {
        if (cinematicCamera != null)
        {
            cinematicCamera.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += CheckGems;
        }
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= CheckGems;
        }
    }

    private void Start()
    {
        CheckGems();
    }

    private void CheckGems()
    {
        if (activated)
        {
            return;
        }

        if (playerInventory == null || gema1 == null || gema2 == null || gema3 == null)
        {
            return;
        }

        bool tieneGema1 = playerInventory.ObtenerCantidad(gema1) > 0;
        bool tieneGema2 = playerInventory.ObtenerCantidad(gema2) > 0;
        bool tieneGema3 = playerInventory.ObtenerCantidad(gema3) > 0;

        if (!tieneGema1 || !tieneGema2 || !tieneGema3)
        {
            return;
        }

        activated = true;
        StartCoroutine(PlayDoorCinematicRoutine());
    }

    private IEnumerator PlayDoorCinematicRoutine()
    {
        if (gameplayCamera != null)
        {
            gameplayCamera.SetActive(false);
        }

        if (cinematicCamera != null)
        {
            cinematicCamera.SetActive(true);
        }

        if (activarTemblorCamara && cinematicCamera != null)
        {
            cameraShakeCoroutine = StartCoroutine(ShakeCameraRoutine(cinematicCamera.transform));
        }

        if (doorEmergenceAnimation != null)
        {
            doorEmergenceAnimation.PlayEmergence();
        }

        yield return new WaitForSeconds(cinematicDuration);

        if (cameraShakeCoroutine != null)
        {
            StopCoroutine(cameraShakeCoroutine);
            cameraShakeCoroutine = null;
        }

        if (volverACamaraGameplay)
        {
            if (cinematicCamera != null)
            {
                cinematicCamera.transform.localPosition = Vector3.zero;
                cinematicCamera.SetActive(false);
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.SetActive(true);
            }
        }
    }

    private IEnumerator ShakeCameraRoutine(Transform cameraTransform)
    {
        Vector3 posicionOriginal = cameraTransform.localPosition;

        float tiempo = 0f;

        while (tiempo < duracionTemblor)
        {
            tiempo += Time.deltaTime;

            float fuerzaActual = Mathf.Lerp(intensidadTemblor, 0f, tiempo / duracionTemblor);

            float x = Mathf.PerlinNoise(Time.time * velocidadTemblor, 0f) * 2f - 1f;
            float y = Mathf.PerlinNoise(0f, Time.time * velocidadTemblor) * 2f - 1f;

            Vector3 desplazamiento = new Vector3(x, y, 0f) * fuerzaActual;

            cameraTransform.localPosition = posicionOriginal + desplazamiento;

            yield return null;
        }

        cameraTransform.localPosition = posicionOriginal;
    }
}