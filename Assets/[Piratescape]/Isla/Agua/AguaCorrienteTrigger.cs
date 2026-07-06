using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class AguaCorrienteTrigger : MonoBehaviour
{
    [Header("Jugador")]
    [SerializeField] private bool afectarSoloAlJugador = true;
    [SerializeField] private string tagJugador = "Player";

    [Header("Direccion de la corriente")]
    [Tooltip("Si se deja vacio, usa el transform de este objeto.")]
    [SerializeField] private Transform referenciaDireccionCorriente;
    [Tooltip("Direccion local. Normalmente usa 0,0,-1 o 0,0,1 segun como hayas orientado el plano del rio.")]
    [SerializeField] private Vector3 direccionLocalCorriente = Vector3.back;
    [SerializeField] private bool mantenerComponenteVerticalDeLaPendiente = true;
    [SerializeField] private bool invertirDireccionSiApuntaHaciaArriba = true;

    [Header("Fuerza de corriente")]
    [Tooltip("Empuje principal siguiendo la pendiente del rio.")]
    [SerializeField] private float velocidadCorriente = 1.65f;
    [Tooltip("Ayuda a que al jugador le cueste subir y caiga hacia abajo si deja de avanzar.")]
    [SerializeField] private bool aplicarEmpujeVerticalExtraHaciaAbajo = true;
    [SerializeField] private float velocidadVerticalExtraHaciaAbajo = 0.25f;
    [SerializeField] private bool usarUnscaledTime = false;

    [Header("Rigidbody opcional")]
    [SerializeField] private bool afectarRigidbodySiNoHayCharacterController = false;
    [SerializeField] private float fuerzaRigidbody = 14f;

    [Header("Depuracion")]
    [SerializeField] private bool configurarColliderComoTriggerAutomaticamente = true;
    [SerializeField] private bool mostrarGizmos = true;
    [SerializeField] private float longitudGizmoDireccion = 2f;

    private readonly Dictionary<int, int> ultimaFrameCharacterControllerMovido = new Dictionary<int, int>();
    private Collider triggerCollider;

    private void Awake()
    {
        PrepararCollider();
    }

    private void Reset()
    {
        PrepararCollider();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            PrepararCollider();
        }
    }

    private void PrepararCollider()
    {
        if (triggerCollider == null)
        {
            triggerCollider = GetComponent<Collider>();
        }

        if (triggerCollider != null && configurarColliderComoTriggerAutomaticamente)
        {
            triggerCollider.isTrigger = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other == null)
        {
            return;
        }

        CharacterController characterController = other.GetComponentInParent<CharacterController>();

        if (characterController != null)
        {
            if (!EsJugador(characterController.gameObject, other))
            {
                return;
            }

            MoverCharacterController(characterController);
            return;
        }

        if (afectarRigidbodySiNoHayCharacterController)
        {
            Rigidbody rb = other.attachedRigidbody;

            if (rb != null && EsJugador(rb.gameObject, other))
            {
                Vector3 direccion = ObtenerDireccionCorrienteFinal();
                rb.AddForce(direccion * fuerzaRigidbody, ForceMode.Acceleration);
            }
        }
    }

    private bool EsJugador(GameObject posibleJugador, Collider colliderOrigen)
    {
        if (!afectarSoloAlJugador)
        {
            return true;
        }

        if (posibleJugador != null && posibleJugador.CompareTag(tagJugador))
        {
            return true;
        }

        if (colliderOrigen != null && colliderOrigen.CompareTag(tagJugador))
        {
            return true;
        }

        if (colliderOrigen != null && colliderOrigen.transform.root != null && colliderOrigen.transform.root.CompareTag(tagJugador))
        {
            return true;
        }

        return false;
    }

    private void MoverCharacterController(CharacterController characterController)
    {
        int id = characterController.GetInstanceID();

        if (ultimaFrameCharacterControllerMovido.TryGetValue(id, out int ultimaFrame) && ultimaFrame == Time.frameCount)
        {
            return;
        }

        ultimaFrameCharacterControllerMovido[id] = Time.frameCount;

        float delta = usarUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        Vector3 movimiento = ObtenerDireccionCorrienteFinal() * velocidadCorriente;

        if (aplicarEmpujeVerticalExtraHaciaAbajo)
        {
            movimiento += Vector3.down * velocidadVerticalExtraHaciaAbajo;
        }

        characterController.Move(movimiento * delta);
    }

    private Vector3 ObtenerDireccionCorrienteFinal()
    {
        Transform referencia = referenciaDireccionCorriente != null ? referenciaDireccionCorriente : transform;
        Vector3 direccion = referencia.TransformDirection(direccionLocalCorriente);

        if (!mantenerComponenteVerticalDeLaPendiente)
        {
            direccion.y = 0f;
        }

        if (direccion.sqrMagnitude <= 0.0001f)
        {
            direccion = Vector3.down;
        }

        direccion.Normalize();

        if (invertirDireccionSiApuntaHaciaArriba && direccion.y > 0.01f)
        {
            direccion = -direccion;
        }

        return direccion;
    }

    private void OnDrawGizmosSelected()
    {
        if (!mostrarGizmos)
        {
            return;
        }

        Vector3 direccion = ObtenerDireccionCorrienteFinal();
        Vector3 origen = transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origen, origen + direccion * longitudGizmoDireccion);
        Gizmos.DrawSphere(origen + direccion * longitudGizmoDireccion, 0.08f);
    }
}
