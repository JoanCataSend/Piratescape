using System.Collections;
using UnityEngine;

public sealed class MiniMissionTriggerReporter : MonoBehaviour
{
    [Header("Evento")]
    [SerializeField] private string eventoAReportar = "entrar_cueva_con_luz";
    [SerializeField] private bool reportarUnaSolaVez = true;

    [Header("Jugador")]
    [SerializeField] private string tagJugador = "Player";

    [Header("Requisitos opcionales")]
    [SerializeField] private bool requiereItemEnInventario = false;
    [SerializeField] private ItemData itemRequerido;
    [SerializeField] private string itemIdRequerido = "farol";
    [SerializeField] private int cantidadRequerida = 1;

    [Header("FX murcielagos de cueva")]
    [SerializeField] private bool reproducirMurcielagosAlActivar = false;
    [SerializeField] private bool reproducirMurcielagosUnaSolaVez = true;
    [SerializeField] private GameObject prefabMurcielago;
    [SerializeField] private Transform puntoSalidaMurcielagos;
    [SerializeField] private Transform objetivoVueloOpcional;
    [SerializeField] private Vector3 offsetSalidaMurcielagos = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private Vector3 direccionLocalVuelo = Vector3.forward;
    [SerializeField] private int murcielagosMin = 10;
    [SerializeField] private int murcielagosMax = 16;
    [SerializeField] private float radioSpawnMurcielagos = 0.75f;
    [SerializeField] private float distanciaVueloMin = 6f;
    [SerializeField] private float distanciaVueloMax = 11f;
    [SerializeField] private float alturaExtraMin = 1f;
    [SerializeField] private float alturaExtraMax = 3.6f;
    [SerializeField] private float aperturaHorizontal = 2.6f;
    [SerializeField] private float duracionVueloMin = 1.7f;
    [SerializeField] private float duracionVueloMax = 2.7f;
    [SerializeField] private float alturaArco = 0.8f;
    [SerializeField] private float zigzagFuerza = 0.45f;
    [SerializeField] private float zigzagVelocidad = 13f;
    [SerializeField] private bool mirarDireccionMovimiento = true;
    [SerializeField] private bool usarRotacionOriginalPrefabSiNoMiraDireccion = true;
    [SerializeField] private Vector3 correccionRotacionVueloEuler = Vector3.zero;
    [SerializeField] private float escalaMurcielagoMin = 0.65f;
    [SerializeField] private float escalaMurcielagoMax = 1.15f;
    [SerializeField] private bool desactivarCollidersMurcielagos = true;
    [SerializeField] private bool desactivarRigidbodyMurcielagos = true;
    [SerializeField] private bool destruirMurcielagosAlFinal = true;
    [SerializeField] private float tiempoExtraAntesDeDestruir = 0.3f;
    [SerializeField] private string triggerAnimatorMurcielago = "Fly";

    [Header("Depuracion")]
    [SerializeField] private bool mostrarLogs = false;

    private bool yaReportado;
    private bool yaReprodujoMurcielagos;

    private void Reset()
    {
        Collider col = GetComponent<Collider>();

        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (yaReportado && reportarUnaSolaVez)
        {
            return;
        }

        if (!EsJugador(other, out PlayerInventory inventario))
        {
            return;
        }

        if (requiereItemEnInventario && !MiniMissionManager.InventarioTieneItem(inventario, itemRequerido, itemIdRequerido, cantidadRequerida))
        {
            if (mostrarLogs)
            {
                Debug.Log("[MiniMissionTriggerReporter] El jugador entro, pero no tiene el item requerido: " + itemIdRequerido, this);
            }

            return;
        }

        ReportarEventoYReproducirFX();
    }

    public void ReportarManual()
    {
        if (yaReportado && reportarUnaSolaVez)
        {
            return;
        }

        ReportarEventoYReproducirFX();
    }

    [ContextMenu("Debug/Reproducir murcielagos ahora")]
    public void DebugReproducirMurcielagosAhora()
    {
        ReproducirMurcielagos(true);
    }

    [ContextMenu("Debug/Resetear trigger local")]
    public void DebugResetearTriggerLocal()
    {
        yaReportado = false;
        yaReprodujoMurcielagos = false;
    }

    private void ReportarEventoYReproducirFX()
    {
        MiniMissionManager.ReportarEventoGlobal(eventoAReportar);
        yaReportado = true;

        if (reproducirMurcielagosAlActivar)
        {
            ReproducirMurcielagos(false);
        }

        if (mostrarLogs)
        {
            Debug.Log("[MiniMissionTriggerReporter] Evento reportado: " + eventoAReportar, this);
        }
    }

    private void ReproducirMurcielagos(bool ignorarUnaSolaVez)
    {
        if (prefabMurcielago == null)
        {
            if (mostrarLogs)
            {
                Debug.LogWarning("[MiniMissionTriggerReporter] No hay Prefab Murcielago asignado.", this);
            }

            return;
        }

        if (!ignorarUnaSolaVez && reproducirMurcielagosUnaSolaVez && yaReprodujoMurcielagos)
        {
            return;
        }

        yaReprodujoMurcielagos = true;

        Transform origenTransform = puntoSalidaMurcielagos != null ? puntoSalidaMurcielagos : transform;
        Vector3 origen = origenTransform.position + origenTransform.TransformDirection(offsetSalidaMurcielagos);
        Vector3 direccion = ObtenerDireccionVuelo(origenTransform, origen);
        Vector3 derecha = Vector3.Cross(Vector3.up, direccion).normalized;

        if (derecha.sqrMagnitude <= 0.001f)
        {
            derecha = origenTransform.right;
        }

        Vector3 arriba = Vector3.up;
        int cantidad = Random.Range(Mathf.Min(murcielagosMin, murcielagosMax), Mathf.Max(murcielagosMin, murcielagosMax) + 1);

        for (int i = 0; i < cantidad; i++)
        {
            Vector3 offsetSpawn = derecha * Random.Range(-radioSpawnMurcielagos, radioSpawnMurcielagos)
                                + arriba * Random.Range(-radioSpawnMurcielagos * 0.25f, radioSpawnMurcielagos * 0.6f)
                                + direccion * Random.Range(-radioSpawnMurcielagos * 0.25f, radioSpawnMurcielagos * 0.25f);

            Vector3 inicio = origen + offsetSpawn;
            Vector3 destino = inicio
                            + direccion * Random.Range(distanciaVueloMin, distanciaVueloMax)
                            + derecha * Random.Range(-aperturaHorizontal, aperturaHorizontal)
                            + arriba * Random.Range(alturaExtraMin, alturaExtraMax);

            Quaternion rotacionInicial = ObtenerRotacionInicial(inicio, destino);
            GameObject murcielago = Instantiate(prefabMurcielago, inicio, rotacionInicial);
            murcielago.name = prefabMurcielago.name + "_FX_Cueva";

            ConfigurarMurcielagoInstanciado(murcielago);

            float escala = Random.Range(escalaMurcielagoMin, escalaMurcielagoMax);
            murcielago.transform.localScale = Vector3.Scale(murcielago.transform.localScale, Vector3.one * escala);

            float duracion = Random.Range(duracionVueloMin, duracionVueloMax);
            float fase = Random.Range(0f, 100f);
            StartCoroutine(MoverMurcielago(murcielago.transform, inicio, destino, duracion, fase, derecha));
        }
    }

    private Vector3 ObtenerDireccionVuelo(Transform origenTransform, Vector3 origen)
    {
        if (objetivoVueloOpcional != null)
        {
            Vector3 haciaObjetivo = objetivoVueloOpcional.position - origen;

            if (haciaObjetivo.sqrMagnitude > 0.001f)
            {
                return haciaObjetivo.normalized;
            }
        }

        Vector3 direccion = origenTransform.TransformDirection(direccionLocalVuelo);

        if (direccion.sqrMagnitude <= 0.001f)
        {
            direccion = origenTransform.forward;
        }

        direccion.Normalize();
        return direccion;
    }

    private Quaternion ObtenerRotacionInicial(Vector3 inicio, Vector3 destino)
    {
        if (mirarDireccionMovimiento)
        {
            Vector3 direccion = destino - inicio;

            if (direccion.sqrMagnitude > 0.001f)
            {
                return Quaternion.LookRotation(direccion.normalized, Vector3.up) * Quaternion.Euler(correccionRotacionVueloEuler);
            }
        }

        if (usarRotacionOriginalPrefabSiNoMiraDireccion && prefabMurcielago != null)
        {
            return prefabMurcielago.transform.rotation * Quaternion.Euler(correccionRotacionVueloEuler);
        }

        return transform.rotation * Quaternion.Euler(correccionRotacionVueloEuler);
    }

    private void ConfigurarMurcielagoInstanciado(GameObject murcielago)
    {
        if (murcielago == null)
        {
            return;
        }

        if (desactivarCollidersMurcielagos)
        {
            Collider[] colliders = murcielago.GetComponentsInChildren<Collider>(true);

            foreach (Collider col in colliders)
            {
                if (col != null)
                {
                    col.enabled = false;
                }
            }
        }

        if (desactivarRigidbodyMurcielagos)
        {
            Rigidbody[] rigidbodies = murcielago.GetComponentsInChildren<Rigidbody>(true);

            foreach (Rigidbody rb in rigidbodies)
            {
                if (rb == null)
                {
                    continue;
                }

                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.useGravity = false;
                rb.isKinematic = true;
            }
        }

        if (!string.IsNullOrWhiteSpace(triggerAnimatorMurcielago))
        {
            Animator animator = murcielago.GetComponentInChildren<Animator>();

            if (animator != null && TieneTrigger(animator, triggerAnimatorMurcielago))
            {
                animator.SetTrigger(triggerAnimatorMurcielago);
            }
        }
    }

    private IEnumerator MoverMurcielago(Transform murcielago, Vector3 inicio, Vector3 destino, float duracion, float fase, Vector3 derecha)
    {
        if (murcielago == null)
        {
            yield break;
        }

        float tiempo = 0f;
        Vector3 posicionAnterior = inicio;

        while (tiempo < duracion)
        {
            if (murcielago == null)
            {
                yield break;
            }

            tiempo += Time.deltaTime;
            float t = duracion > 0.001f ? Mathf.Clamp01(tiempo / duracion) : 1f;
            float suavizado = Mathf.SmoothStep(0f, 1f, t);
            Vector3 posicionBase = Vector3.Lerp(inicio, destino, suavizado);
            float arco = Mathf.Sin(t * Mathf.PI) * alturaArco;
            float zigzag = Mathf.Sin(fase + t * zigzagVelocidad) * zigzagFuerza;
            Vector3 posicion = posicionBase + Vector3.up * arco + derecha * zigzag;

            murcielago.position = posicion;

            if (mirarDireccionMovimiento)
            {
                Vector3 direccion = posicion - posicionAnterior;

                if (direccion.sqrMagnitude > 0.0001f)
                {
                    Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion.normalized, Vector3.up) * Quaternion.Euler(correccionRotacionVueloEuler);
                    murcielago.rotation = Quaternion.Slerp(murcielago.rotation, rotacionObjetivo, Time.deltaTime * 12f);
                }
            }

            posicionAnterior = posicion;
            yield return null;
        }

        if (destruirMurcielagosAlFinal && murcielago != null)
        {
            Destroy(murcielago.gameObject, tiempoExtraAntesDeDestruir);
        }
    }

    private bool TieneTrigger(Animator animator, string nombreTrigger)
    {
        if (animator == null || string.IsNullOrWhiteSpace(nombreTrigger))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.type == AnimatorControllerParameterType.Trigger && parametro.name == nombreTrigger)
            {
                return true;
            }
        }

        return false;
    }

    private bool EsJugador(Collider other, out PlayerInventory inventario)
    {
        inventario = null;

        if (other == null)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(tagJugador) && other.CompareTag(tagJugador))
        {
            inventario = other.GetComponent<PlayerInventory>();

            if (inventario == null)
            {
                inventario = other.GetComponentInParent<PlayerInventory>();
            }

            return true;
        }

        inventario = other.GetComponentInParent<PlayerInventory>();
        return inventario != null;
    }
}
