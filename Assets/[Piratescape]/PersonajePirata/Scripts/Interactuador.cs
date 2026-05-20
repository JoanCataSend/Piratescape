using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interactuador : MonoBehaviour
{
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
        if (CofreInventarioUI.SeHaCerradoUIEsteFrame)
        {
            return;
        }

        if (Time.timeScale == 0f)
        {
            return;
        }

        if (CofreInventarioUI.HayAlgunaUIAbierta)
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
            objetivo.Interactuar();
        }
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

        Interactuable mejor = null;
        float mejorDistancia = float.MaxValue;
        Vector3 posicionJugador = jugador != null ? jugador.Position : transform.position;

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

            if (item is ObjetoRecogibleInteractuable)
            {
                continue;
            }

            float distancia = Vector3.Distance(behaviour.transform.position, posicionJugador);

            if (distancia > item.Rango)
            {
                continue;
            }

            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = item;
            }
        }

        return mejor;
    }
}
