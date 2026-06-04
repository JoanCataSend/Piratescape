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

    private bool activated = false;

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

        if (doorEmergenceAnimation != null)
        {
            doorEmergenceAnimation.PlayEmergence();
        }

        yield return new WaitForSeconds(cinematicDuration);

        if (volverACamaraGameplay)
        {
            if (cinematicCamera != null)
            {
                cinematicCamera.SetActive(false);
            }

            if (gameplayCamera != null)
            {
                gameplayCamera.SetActive(true);
            }
        }
    }
}