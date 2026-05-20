using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class LimiteAguaNatural : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform agua;

    [Header("Capas")]
    [SerializeField] private LayerMask capaSuelo;

    [Header("Profundidad")]
    [SerializeField] private float profundidadMaxima = 1.2f;

    [Header("Empuje")]
    [SerializeField] private float fuerzaEmpuje = 4f;

    [Header("Raycast")]
    [SerializeField] private float alturaRaycast = 3f;
    [SerializeField] private float distanciaRaycast = 10f;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        Vector3 origen = transform.position + Vector3.up * alturaRaycast;

        if (Physics.Raycast(
            origen,
            Vector3.down,
            out RaycastHit hit,
            distanciaRaycast,
            capaSuelo))
        {
            float alturaSuelo = hit.point.y;

            float profundidad = agua.position.y - alturaSuelo;

            if (profundidad > profundidadMaxima)
            {
                Vector3 direccionSalida =
                    (transform.position - hit.point).normalized;

                direccionSalida.y = 0f;

                controller.Move(
                    direccionSalida *
                    fuerzaEmpuje *
                    Time.deltaTime);
            }
        }
    }
}