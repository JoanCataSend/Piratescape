using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(20000)]
[DisallowMultipleComponent]
public class ThirdPersonCameraCollision : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Punto que la camara debe mirar/proteger. Si lo dejas vacio, buscara el objeto con tag Player.")]
    [SerializeField] private Transform objetivo;
    [SerializeField] private bool buscarObjetivoPorTag = true;
    [SerializeField] private string tagObjetivo = "Player";
    [Tooltip("Offset desde el jugador hacia el punto de comprobacion. Ejemplo: 0,1.3,0 para pecho/cabeza.")]
    [SerializeField] private Vector3 offsetObjetivo = new Vector3(0f, 1.25f, 0f);

    [Header("Colision de camara")]
    [SerializeField] private bool activarColisionCamara = true;
    [Tooltip("Normalmente interesa siempre, no solo en la cueva: evita que la camara atraviese paredes, rocas, palmeras, montanas, etc.")]
    [SerializeField] private bool activoSiempre = true;
    [Tooltip("Radio del SphereCast. Mas alto = evita mejor atravesar esquinas, pero puede acercar demasiado la camara.")]
    [SerializeField] private float radioCamara = 0.28f;
    [SerializeField] private float margenSeparacion = 0.08f;
    [SerializeField] private float distanciaMinima = 0.45f;
    [SerializeField] private LayerMask capasObstaculos = ~0;
    [SerializeField] private bool ignorarTriggers = true;
    [SerializeField] private bool ignorarCollidersDelJugador = true;

    [Header("Modo compatibilidad")]
    [Tooltip("ON: usa la posicion donde tu script de camara deja la camara este frame como posicion deseada, y solo la corrige si hay pared. Es lo mas compatible.")]
    [SerializeField] private bool usarPosicionActualComoDeseadaCadaFrame = true;
    [Tooltip("Si desactivas el modo anterior, usa este offset local respecto al padre de la camara.")]
    [SerializeField] private Vector3 posicionLocalDeseadaFallback = new Vector3(0f, 2f, -5f);

    [Header("Suavizado")]
    [SerializeField] private bool suavizarCorreccion = false;
    [SerializeField] private float suavizadoAcercar = 30f;
    [SerializeField] private float suavizadoAlejar = 12f;

    [Header("Debug")]
    [SerializeField] private bool mostrarGizmos = true;
    [SerializeField] private bool camaraBloqueadaDebug;
    [SerializeField] private float distanciaCamaraDebug;

    private Vector3 posicionCorregidaAnterior;
    private bool tienePosicionAnterior;
    private readonly RaycastHit[] impactos = new RaycastHit[24];

    private void Awake()
    {
        if (objetivo == null && buscarObjetivoPorTag)
        {
            BuscarObjetivo();
        }

        if (transform.parent != null)
        {
            posicionLocalDeseadaFallback = transform.localPosition;
        }

        posicionCorregidaAnterior = transform.position;
        tienePosicionAnterior = true;
    }

    private void LateUpdate()
    {
        if (!activarColisionCamara || !activoSiempre)
        {
            camaraBloqueadaDebug = false;
            return;
        }

        if (objetivo == null && buscarObjetivoPorTag)
        {
            BuscarObjetivo();
        }

        if (objetivo == null)
        {
            camaraBloqueadaDebug = false;
            return;
        }

        Vector3 origen = objetivo.position + offsetObjetivo;
        Vector3 posicionDeseada = ObtenerPosicionDeseada();
        Vector3 direccion = posicionDeseada - origen;
        float distanciaDeseada = direccion.magnitude;

        if (distanciaDeseada <= 0.001f)
        {
            camaraBloqueadaDebug = false;
            return;
        }

        direccion /= distanciaDeseada;

        bool hayChoque = TryObtenerImpactoValido(origen, direccion, distanciaDeseada, out RaycastHit impacto);
        Vector3 posicionFinal = posicionDeseada;

        if (hayChoque)
        {
            float distanciaCorregida = Mathf.Max(distanciaMinima, impacto.distance - margenSeparacion);
            posicionFinal = origen + direccion * distanciaCorregida;
            camaraBloqueadaDebug = true;
            distanciaCamaraDebug = distanciaCorregida;
        }
        else
        {
            camaraBloqueadaDebug = false;
            distanciaCamaraDebug = distanciaDeseada;
        }

        if (suavizarCorreccion && tienePosicionAnterior)
        {
            float suavizado = hayChoque ? suavizadoAcercar : suavizadoAlejar;
            float factor = 1f - Mathf.Exp(-Mathf.Max(0.1f, suavizado) * Time.deltaTime);
            posicionFinal = Vector3.Lerp(posicionCorregidaAnterior, posicionFinal, factor);
        }

        transform.position = posicionFinal;
        posicionCorregidaAnterior = posicionFinal;
        tienePosicionAnterior = true;
    }

    private Vector3 ObtenerPosicionDeseada()
    {
        if (usarPosicionActualComoDeseadaCadaFrame)
        {
            return transform.position;
        }

        if (transform.parent != null)
        {
            return transform.parent.TransformPoint(posicionLocalDeseadaFallback);
        }

        if (objetivo != null)
        {
            return objetivo.TransformPoint(posicionLocalDeseadaFallback);
        }

        return transform.position;
    }

    private void BuscarObjetivo()
    {
        GameObject go = GameObject.FindGameObjectWithTag(tagObjetivo);
        if (go != null)
        {
            objetivo = go.transform;
        }
    }

    private bool TryObtenerImpactoValido(Vector3 origen, Vector3 direccion, float distancia, out RaycastHit mejorImpacto)
    {
        mejorImpacto = default;

        QueryTriggerInteraction triggerInteraction = ignorarTriggers
            ? QueryTriggerInteraction.Ignore
            : QueryTriggerInteraction.Collide;

        int count = Physics.SphereCastNonAlloc(
            origen,
            Mathf.Max(0.01f, radioCamara),
            direccion,
            impactos,
            distancia + margenSeparacion,
            capasObstaculos,
            triggerInteraction
        );

        if (count <= 0)
        {
            return false;
        }

        Array.Sort(impactos, 0, count, RaycastHitDistanceComparer.Instance);

        for (int i = 0; i < count; i++)
        {
            RaycastHit hit = impactos[i];

            if (hit.collider == null)
            {
                continue;
            }

            if (ignorarCollidersDelJugador && ColliderPerteneceAlObjetivo(hit.collider))
            {
                continue;
            }

            if (hit.distance <= 0.001f)
            {
                continue;
            }

            mejorImpacto = hit;
            return true;
        }

        return false;
    }

    private bool ColliderPerteneceAlObjetivo(Collider col)
    {
        if (col == null || objetivo == null)
        {
            return false;
        }

        Transform t = col.transform;
        return t == objetivo || t.IsChildOf(objetivo) || objetivo.IsChildOf(t);
    }

    private void OnDrawGizmosSelected()
    {
        if (!mostrarGizmos || objetivo == null)
        {
            return;
        }

        Vector3 origen = objetivo.position + offsetObjetivo;
        Vector3 destino = transform.position;

        Gizmos.color = camaraBloqueadaDebug ? Color.red : Color.cyan;
        Gizmos.DrawLine(origen, destino);
        Gizmos.DrawWireSphere(origen, radioCamara);
        Gizmos.DrawWireSphere(destino, radioCamara);
    }

    private sealed class RaycastHitDistanceComparer : IComparer<RaycastHit>
    {
        public static readonly RaycastHitDistanceComparer Instance = new RaycastHitDistanceComparer();

        public int Compare(RaycastHit a, RaycastHit b)
        {
            return a.distance.CompareTo(b.distance);
        }
    }
}
