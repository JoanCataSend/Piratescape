using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactuador : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool mostrarDebugInteraccion = false;

    private Interactuable[] interactuables;
    private IActivador jugador;

    private void Start()
    {
        jugador = GetComponent<IActivador>();

        if (jugador == null)
        {
            jugador = GetComponent<JugadorActivador>();
        }

        RefreshInteractuables();
    }

    private void Update()
    {
        if (HayUIBloqueanteAbierta())
        {
            return;
        }

        if (Time.frameCount == LoroDialogoUI.FrameCierre)
        {
            return;
        }

        bool interactuar = false;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            interactuar = true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            interactuar = true;
        }

        if (!interactuar)
        {
            return;
        }

        RefreshInteractuables();
        Interactuable objetivo = ObtenerInteractuableActivoMasCercano();

        if (objetivo != null)
        {
            if (mostrarDebugInteraccion)
            {
                MonoBehaviour behaviour = objetivo as MonoBehaviour;
                string nombre = behaviour != null ? behaviour.name : objetivo.ToString();
                Debug.Log("Interactuando con: " + nombre, behaviour);
            }

            objetivo.Interactuar();
        }
        else if (mostrarDebugInteraccion)
        {
            Debug.Log("No hay interactuable activo cerca.", this);
        }
    }

    private bool HayUIBloqueanteAbierta()
    {
        return CofreInventarioUI.HayAlgunaUIAbierta
            || LoroDialogoUI.HayAlgunaUIAbierta
            || HistoriaInicioLoroUI.HayAlgunaUIAbierta;
    }

    private void RefreshInteractuables()
    {
        interactuables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<Interactuable>()
            .ToArray();
    }

    private Interactuable ObtenerInteractuableActivoMasCercano()
    {
        if (interactuables == null || interactuables.Length == 0)
        {
            RefreshInteractuables();
        }

        if (interactuables == null || interactuables.Length == 0)
        {
            return null;
        }

        if (jugador == null)
        {
            jugador = GetComponent<IActivador>();

            if (jugador == null)
            {
                jugador = GetComponent<JugadorActivador>();
            }
        }

        Interactuable mejor = null;
        float mejorDistancia = float.MaxValue;

        foreach (Interactuable item in interactuables)
        {
            if (item == null || !item.Activo)
            {
                continue;
            }

            MonoBehaviour behaviour = item as MonoBehaviour;

            if (behaviour == null || !behaviour.isActiveAndEnabled)
            {
                continue;
            }

            if (behaviour.transform.IsChildOf(transform))
            {
                continue;
            }

            if (behaviour.GetComponent<ObjetoRecogibleInteractuable>() != null)
            {
                continue;
            }

            if (jugador == null)
            {
                mejor = item;
                break;
            }

            Vector3 posicionInteraccion = ObtenerPosicionInteraccion(item, behaviour);
            float distancia = Vector3.Distance(posicionInteraccion, jugador.Position);

            // No volvemos a descartar por rango aqui.
            // Cada interactuable ya calcula su propio Activo con su propio punto de interaccion.
            // Esto arregla la tienda: el prompt salia por el InteractionPoint, pero la E se media desde el centro del prefab.
            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = item;
            }
        }

        return mejor;
    }

    private Vector3 ObtenerPosicionInteraccion(Interactuable item, MonoBehaviour behaviour)
    {
        ShelterSleep shelterSleep = item as ShelterSleep;

        if (shelterSleep != null)
        {
            return shelterSleep.PosicionInteraccion;
        }

        return behaviour.transform.position;
    }
}
