using UnityEngine;

public class GhostSpawn : MonoBehaviour
{
    [Header("Puntos de aparición")]
    [SerializeField] private Transform[] puntosAparicion;

    [Header("Primer spawn")]
    [SerializeField] private Transform primerPuntoAparicion;

    [Header("Visual / FX")]
    [SerializeField] private GhostVisualEffect ghostVisualEffect;

    [Header("Movimiento en forma de 8")]
    [SerializeField] private float amplitudX = 0.8f;
    [SerializeField] private float amplitudZ = 0.4f;
    [SerializeField] private float velocidad = 1.2f;

    private int ultimoPunto = -1;
    private Vector3 posicionBase;
    private bool estaDeNoche;
    private bool primeraAparicionRealizada;
    private bool movimientoBloqueado;

    private void Awake()
    {
        if (ghostVisualEffect == null)
        {
            ghostVisualEffect = GetComponent<GhostVisualEffect>();
        }
    }

    public void EmpezarNoche()
    {
        gameObject.SetActive(true);

        AparecerEnPuntoNocturno();

        estaDeNoche = true;
        movimientoBloqueado = false;

        if (ghostVisualEffect != null)
        {
            ghostVisualEffect.PlaySpawn();
        }
    }

    public void EmpezarDia()
    {
        estaDeNoche = false;
        movimientoBloqueado = false;

        if (ghostVisualEffect != null)
        {
            ghostVisualEffect.PlayDespawn();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (!estaDeNoche || movimientoBloqueado)
        {
            return;
        }

        MoverEnFormaDeOcho();
    }

    public void BloquearMovimiento(bool bloquear)
    {
        movimientoBloqueado = bloquear;

        if (bloquear)
        {
            posicionBase = transform.position;
        }
    }

    public void MirarHacia(Vector3 posicionObjetivo)
    {
        Vector3 direccion = posicionObjetivo - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion.normalized);
        transform.rotation = rotacionObjetivo;
    }

    public void ColocarFrenteAlJugador(Vector3 posicionJugador, Transform transformJugador, float distancia)
    {
        Vector3 direccionFrenteJugador;

        if (transformJugador != null)
        {
            direccionFrenteJugador = transformJugador.forward;
            direccionFrenteJugador.y = 0f;
        }
        else
        {
            direccionFrenteJugador = posicionJugador - transform.position;
            direccionFrenteJugador.y = 0f;
        }

        if (direccionFrenteJugador.sqrMagnitude <= 0.001f)
        {
            direccionFrenteJugador = -transform.forward;
            direccionFrenteJugador.y = 0f;
        }

        direccionFrenteJugador.Normalize();

        Vector3 nuevaPosicion = posicionJugador + direccionFrenteJugador * distancia;

        nuevaPosicion.y = transform.position.y;

        transform.position = nuevaPosicion;
        posicionBase = nuevaPosicion;

        MirarHacia(posicionJugador);
    }

    private void AparecerEnPuntoNocturno()
    {
        if (!primeraAparicionRealizada && primerPuntoAparicion != null)
        {
            primeraAparicionRealizada = true;

            transform.position = primerPuntoAparicion.position;
            posicionBase = transform.position;

            return;
        }

        if (puntosAparicion == null || puntosAparicion.Length == 0)
        {
            Debug.LogWarning("No hay puntos asignados al fantasma");
            return;
        }

        int nuevoPunto;

        do
        {
            nuevoPunto = Random.Range(0, puntosAparicion.Length);
        }
        while (nuevoPunto == ultimoPunto && puntosAparicion.Length > 1);

        ultimoPunto = nuevoPunto;

        transform.position = puntosAparicion[nuevoPunto].position;
        posicionBase = transform.position;
    }

    private void MoverEnFormaDeOcho()
    {
        float tiempo = Time.time * velocidad;

        float x = Mathf.Sin(tiempo) * amplitudX;
        float z = Mathf.Sin(tiempo * 2f) * amplitudZ;
        float y = Mathf.Sin(Time.time * 2f) * 0.25f;

        Vector3 nuevaPosicion = posicionBase + new Vector3(x, y, z);

        Vector3 direccion = nuevaPosicion - transform.position;
        direccion.y = 0f;
        direccion = direccion.normalized;

        if (direccion.sqrMagnitude > 0.001f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                rotacionObjetivo,
                Time.deltaTime * 8f
            );
        }

        transform.position = nuevaPosicion;
    }
}