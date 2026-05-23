using UnityEngine;

public class SandFootprintSpawner : MonoBehaviour
{
    [Header("Puntos de apoyo")]
    [SerializeField] private Transform bootPoint;
    [SerializeField] private Transform stickPoint;

    [Header("Prefabs de huella")]
    [SerializeField] private GameObject bootFootprintPrefab;
    [SerializeField] private GameObject stickFootprintPrefab;

    [Header("Detección de arena")]
    [SerializeField] private LayerMask sandLayer;
    [SerializeField] private float raycastHeight = 0.6f;
    [SerializeField] private float raycastDistance = 2f;

    [Header("Configuración")]
    [SerializeField] private float distanceBetweenPrints = 0.55f;
    [SerializeField] private float minimumMoveDistance = 0.03f;
    [SerializeField] private float yOffset = 0.025f;
    [SerializeField] private float footprintLifeTime = 12f;

    private Vector3 lastFramePosition;
    private Vector3 lastFootprintPosition;
    private bool useBootNext = true;

    private void Start()
    {
        lastFramePosition = transform.position;
        lastFootprintPosition = transform.position;
    }

    private void Update()
    {
        Vector3 movementThisFrame = transform.position - lastFramePosition;
        movementThisFrame.y = 0f;

        lastFramePosition = transform.position;

        if (movementThisFrame.magnitude < minimumMoveDistance)
        {
            return;
        }

        Vector3 distanceSinceLastPrint = transform.position - lastFootprintPosition;
        distanceSinceLastPrint.y = 0f;

        if (distanceSinceLastPrint.magnitude >= distanceBetweenPrints)
        {
            SpawnNextFootprint();
            lastFootprintPosition = transform.position;
        }
    }

    private void SpawnNextFootprint()
    {
        if (useBootNext)
        {
            TrySpawnFootprint(bootPoint, bootFootprintPrefab);
        }
        else
        {
            TrySpawnFootprint(stickPoint, stickFootprintPrefab);
        }

        useBootNext = !useBootNext;
    }

    private void TrySpawnFootprint(Transform point, GameObject prefab)
    {
        if (point == null || prefab == null)
        {
            return;
        }

        Vector3 rayOrigin = point.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, raycastDistance, sandLayer))
        {
            Vector3 spawnPosition = hit.point + hit.normal * yOffset;

            Vector3 forwardOnSurface = Vector3.ProjectOnPlane(transform.forward, hit.normal).normalized;

            if (forwardOnSurface.sqrMagnitude < 0.001f)
            {
                forwardOnSurface = transform.forward;
            }

            Quaternion spawnRotation = Quaternion.LookRotation(forwardOnSurface, hit.normal) * Quaternion.Euler(90f, 0f, 0f);

            GameObject footprint = Instantiate(prefab, spawnPosition, spawnRotation);

            Destroy(footprint, footprintLifeTime);
        }
    }
}