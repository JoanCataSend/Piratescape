using UnityEngine;

public class SandFootprintSpawner : MonoBehaviour
{
    [Header("Puntos de apoyo")]
    [SerializeField] private Transform bootPoint;
    [SerializeField] private Transform stickPoint;

    [Header("Prefabs de huella en arena")]
    [SerializeField] private GameObject bootFootprintPrefab;
    [SerializeField] private GameObject stickFootprintPrefab;

    [Header("Detección de superficies")]
    [SerializeField] private LayerMask sandLayer;
    [SerializeField] private LayerMask grassLayer;
    [SerializeField] private float raycastHeight = 0.6f;
    [SerializeField] private float raycastDistance = 2f;

    [Header("Configuración de huellas")]
    [SerializeField] private float distanceBetweenPrints = 0.55f;
    [SerializeField] private float minimumMoveDistance = 0.03f;
    [SerializeField] private float yOffset = 0.025f;
    [SerializeField] private float footprintLifeTime = 12f;

    [Header("Audio")]
    [Tooltip("AudioSource utilizado para reproducir todas las pisadas.")]
    [SerializeField] private AudioSource audioSourcePisadas;

    [Header("Arena - pie normal")]
    [SerializeField] private AudioClip[] sonidosArenaPieNormal;

    [Header("Arena - pata de palo")]
    [SerializeField] private AudioClip[] sonidosArenaPataPalo;

    [Header("Hierba - pie normal")]
    [SerializeField] private AudioClip[] sonidosHierbaPieNormal;

    [Header("Hierba - pata de palo")]
    [SerializeField] private AudioClip[] sonidosHierbaPataPalo;

    [Header("Configuración del sonido")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenPisadas = 0.65f;

    [SerializeField] private float pitchMinimo = 0.95f;
    [SerializeField] private float pitchMaximo = 1.05f;

    private Vector3 lastFramePosition;
    private Vector3 lastFootprintPosition;
    private bool useBootNext = true;

    private int ultimoSonidoArenaPieNormal = -1;
    private int ultimoSonidoArenaPataPalo = -1;
    private int ultimoSonidoHierbaPieNormal = -1;
    private int ultimoSonidoHierbaPataPalo = -1;

    private void Awake()
    {
        PrepararAudioSource();
    }

    private void Start()
    {
        lastFramePosition = transform.position;
        lastFootprintPosition = transform.position;
    }

    private void Update()
    {
        Vector3 movementThisFrame =
            transform.position - lastFramePosition;

        movementThisFrame.y = 0f;
        lastFramePosition = transform.position;

        if (movementThisFrame.magnitude < minimumMoveDistance)
        {
            return;
        }

        Vector3 distanceSinceLastPrint =
            transform.position - lastFootprintPosition;

        distanceSinceLastPrint.y = 0f;

        if (distanceSinceLastPrint.magnitude >= distanceBetweenPrints)
        {
            SpawnNextFootstep();
            lastFootprintPosition = transform.position;
        }
    }

    private void PrepararAudioSource()
    {
        if (audioSourcePisadas == null)
        {
            audioSourcePisadas = GetComponent<AudioSource>();
        }

        if (audioSourcePisadas == null)
        {
            audioSourcePisadas =
                gameObject.AddComponent<AudioSource>();
        }

        audioSourcePisadas.playOnAwake = false;
        audioSourcePisadas.loop = false;
        audioSourcePisadas.spatialBlend = 1f;
        audioSourcePisadas.dopplerLevel = 0f;

        audioSourcePisadas.rolloffMode =
            AudioRolloffMode.Logarithmic;

        audioSourcePisadas.minDistance = 1f;
        audioSourcePisadas.maxDistance = 15f;
    }

    private void SpawnNextFootstep()
    {
        if (useBootNext)
        {
            TrySpawnFootstep(
                bootPoint,
                bootFootprintPrefab,
                true
            );
        }
        else
        {
            TrySpawnFootstep(
                stickPoint,
                stickFootprintPrefab,
                false
            );
        }

        useBootNext = !useBootNext;
    }

    private void TrySpawnFootstep(
        Transform point,
        GameObject footprintPrefab,
        bool esPieNormal)
    {
        if (point == null)
        {
            return;
        }

        Vector3 rayOrigin =
            point.position +
            Vector3.up * raycastHeight;

        LayerMask capasDetectables =
            sandLayer | grassLayer;

        if (!Physics.Raycast(
                rayOrigin,
                Vector3.down,
                out RaycastHit hit,
                raycastDistance,
                capasDetectables,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        int layerSuelo =
            hit.collider.gameObject.layer;

        bool estaEnArena =
            LayerPerteneceAMascara(
                layerSuelo,
                sandLayer
            );

        bool estaEnHierba =
            LayerPerteneceAMascara(
                layerSuelo,
                grassLayer
            );

        if (estaEnArena)
        {
            ReproducirPisadaArena(esPieNormal);

            CrearHuellaArena(
                hit,
                footprintPrefab
            );

            return;
        }

        if (estaEnHierba)
        {
            ReproducirPisadaHierba(esPieNormal);
        }
    }

    private bool LayerPerteneceAMascara(
        int layer,
        LayerMask mascara)
    {
        return (mascara.value & (1 << layer)) != 0;
    }

    private void CrearHuellaArena(
        RaycastHit hit,
        GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        Vector3 spawnPosition =
            hit.point +
            hit.normal * yOffset;

        Vector3 forwardOnSurface =
            Vector3.ProjectOnPlane(
                transform.forward,
                hit.normal
            ).normalized;

        if (forwardOnSurface.sqrMagnitude < 0.001f)
        {
            forwardOnSurface =
                transform.forward;
        }

        Quaternion spawnRotation =
            Quaternion.LookRotation(
                forwardOnSurface,
                hit.normal
            ) *
            Quaternion.Euler(
                90f,
                0f,
                0f
            );

        GameObject footprint = Instantiate(
            prefab,
            spawnPosition,
            spawnRotation
        );

        Destroy(
            footprint,
            footprintLifeTime
        );
    }

    private void ReproducirPisadaArena(
        bool esPieNormal)
    {
        AudioClip clipSeleccionado;

        if (esPieNormal)
        {
            clipSeleccionado =
                ObtenerClipAleatorioSinRepetir(
                    sonidosArenaPieNormal,
                    ref ultimoSonidoArenaPieNormal
                );
        }
        else
        {
            clipSeleccionado =
                ObtenerClipAleatorioSinRepetir(
                    sonidosArenaPataPalo,
                    ref ultimoSonidoArenaPataPalo
                );
        }

        ReproducirClipPisada(
            clipSeleccionado
        );
    }

    private void ReproducirPisadaHierba(
        bool esPieNormal)
    {
        AudioClip clipSeleccionado;

        if (esPieNormal)
        {
            clipSeleccionado =
                ObtenerClipAleatorioSinRepetir(
                    sonidosHierbaPieNormal,
                    ref ultimoSonidoHierbaPieNormal
                );
        }
        else
        {
            clipSeleccionado =
                ObtenerClipAleatorioSinRepetir(
                    sonidosHierbaPataPalo,
                    ref ultimoSonidoHierbaPataPalo
                );
        }

        ReproducirClipPisada(
            clipSeleccionado
        );
    }

    private void ReproducirClipPisada(
        AudioClip clip)
    {
        if (audioSourcePisadas == null ||
            clip == null)
        {
            return;
        }

        float pitchMin =
            Mathf.Min(
                pitchMinimo,
                pitchMaximo
            );

        float pitchMax =
            Mathf.Max(
                pitchMinimo,
                pitchMaximo
            );

        audioSourcePisadas.pitch =
            Random.Range(
                pitchMin,
                pitchMax
            );

        audioSourcePisadas.PlayOneShot(
            clip,
            volumenPisadas
        );
    }

    private AudioClip ObtenerClipAleatorioSinRepetir(
        AudioClip[] clips,
        ref int ultimoIndice)
    {
        if (clips == null ||
            clips.Length == 0)
        {
            return null;
        }

        if (clips.Length == 1)
        {
            ultimoIndice = 0;
            return clips[0];
        }

        int nuevoIndice;
        int intentos = 0;

        do
        {
            nuevoIndice = Random.Range(
                0,
                clips.Length
            );

            intentos++;
        }
        while (
            nuevoIndice == ultimoIndice &&
            intentos < 10
        );

        ultimoIndice = nuevoIndice;

        return clips[nuevoIndice];
    }
}