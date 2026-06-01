using System.Collections;
using UnityEngine;

public class MonkeyStealCutsceneController : MonoBehaviour
{
    [System.Serializable]
    private class MonoRobo
    {
        public Transform mono;
        public Transform puntoInicial;
    }

    [Header("Monos")]
    [SerializeField] private MonoRobo[] monos;

    [Header("Puntos de la tienda")]
    [SerializeField] private Transform puntoEntradaTienda;
    [SerializeField] private Transform puntoInteriorTienda;

    [Header("Cámaras")]
    [SerializeField] private GameObject camaraJugador;
    [SerializeField] private GameObject camaraCinematica;

    [Header("Tiempos")]
    [SerializeField] private float esperaDentroTienda = 1f;
    [SerializeField] private float esperaFinal = 0.5f;

    [Header("Velocidades")]
    [SerializeField] private float velocidadIrTienda = 2f;
    [SerializeField] private float velocidadSalirCorriendo = 5f;
    [SerializeField] private float velocidadRotacion = 10f;
    [SerializeField] private float distanciaLlegada = 0.08f;

    private void Awake()
    {
        PrepararEstadoInicial();
    }

    private void PrepararEstadoInicial()
    {
        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(false);
        }

        if (monos == null)
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            if (monos[i] != null && monos[i].mono != null)
            {
                monos[i].mono.gameObject.SetActive(false);
            }
        }
    }

    public void PrepararCinematica()
    {
        PrepararMonos();

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(false);
        }

        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(true);
        }
    }

    public IEnumerator ReproducirMovimientoMonos()
    {
        yield return MoverMonosA(puntoEntradaTienda, velocidadIrTienda);
        yield return MoverMonosA(puntoInteriorTienda, velocidadIrTienda);

        yield return new WaitForSeconds(esperaDentroTienda);

        yield return MoverMonosA(puntoEntradaTienda, velocidadIrTienda);
        yield return MoverMonosAPuntosIniciales(velocidadSalirCorriendo);

        yield return new WaitForSeconds(esperaFinal);
    }

    public void FinalizarCinematica()
    {
        if (camaraCinematica != null)
        {
            camaraCinematica.SetActive(false);
        }

        if (camaraJugador != null)
        {
            camaraJugador.SetActive(true);
        }

        if (monos != null)
        {
            for (int i = 0; i < monos.Length; i++)
            {
                if (monos[i] != null && monos[i].mono != null)
                {
                    monos[i].mono.gameObject.SetActive(false);
                }
            }
        }
    }

    private void PrepararMonos()
    {
        if (monos == null)
        {
            return;
        }

        for (int i = 0; i < monos.Length; i++)
        {
            MonoRobo monoRobo = monos[i];

            if (monoRobo == null || monoRobo.mono == null || monoRobo.puntoInicial == null)
            {
                continue;
            }

            monoRobo.mono.position = monoRobo.puntoInicial.position;
            monoRobo.mono.rotation = monoRobo.puntoInicial.rotation;
            monoRobo.mono.gameObject.SetActive(true);
        }
    }

    private IEnumerator MoverMonosA(Transform destino, float velocidad)
    {
        if (destino == null || monos == null)
        {
            yield break;
        }

        bool todosLlegaron = false;

        while (!todosLlegaron)
        {
            todosLlegaron = true;

            for (int i = 0; i < monos.Length; i++)
            {
                MonoRobo monoRobo = monos[i];

                if (monoRobo == null || monoRobo.mono == null)
                {
                    continue;
                }

                Vector3 destinoConOffset = destino.position + ObtenerOffsetMono(i);
                bool llego = MoverMonoHacia(monoRobo.mono, destinoConOffset, velocidad);

                if (!llego)
                {
                    todosLlegaron = false;
                }
            }

            yield return null;
        }
    }

    private IEnumerator MoverMonosAPuntosIniciales(float velocidad)
    {
        if (monos == null)
        {
            yield break;
        }

        bool todosLlegaron = false;

        while (!todosLlegaron)
        {
            todosLlegaron = true;

            for (int i = 0; i < monos.Length; i++)
            {
                MonoRobo monoRobo = monos[i];

                if (monoRobo == null || monoRobo.mono == null || monoRobo.puntoInicial == null)
                {
                    continue;
                }

                bool llego = MoverMonoHacia(monoRobo.mono, monoRobo.puntoInicial.position, velocidad);

                if (!llego)
                {
                    todosLlegaron = false;
                }
            }

            yield return null;
        }
    }

    private bool MoverMonoHacia(Transform mono, Vector3 destino, float velocidad)
    {
        Vector3 posicionActual = mono.position;
        Vector3 direccion = destino - posicionActual;
        direccion.y = 0f;

        if (direccion.magnitude <= distanciaLlegada)
        {
            mono.position = new Vector3(destino.x, mono.position.y, destino.z);
            return true;
        }

        Vector3 direccionNormalizada = direccion.normalized;

        mono.position += direccionNormalizada * velocidad * Time.deltaTime;

        if (direccionNormalizada.sqrMagnitude > 0.001f)
        {
            Quaternion rotacionObjetivo = Quaternion.LookRotation(direccionNormalizada);

            mono.rotation = Quaternion.Slerp(
                mono.rotation,
                rotacionObjetivo,
                Time.deltaTime * velocidadRotacion
            );
        }

        return false;
    }

    private Vector3 ObtenerOffsetMono(int indice)
    {
        float separacion = 0.45f;

        if (indice == 0)
        {
            return Vector3.zero;
        }

        if (indice % 2 == 0)
        {
            return Vector3.right * separacion * indice;
        }

        return Vector3.left * separacion * indice;
    }
}