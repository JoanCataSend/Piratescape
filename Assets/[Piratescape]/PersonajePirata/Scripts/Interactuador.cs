using System.Linq;
using UnityEngine;

/// <summary>
/// Gestiona la interacción del jugador con los objetos activos,
/// priorizando el interactuable activo más cercano.
/// </summary>
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
        if (Input.GetKeyDown(KeyCode.E))
        {
            Interactuable objetivo = ObtenerInteractuableActivoMasCercano();

            if (objetivo != null)
            {
                objetivo.Interactuar();
            }
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

        if (jugador == null)
        {
            return interactuables.FirstOrDefault(item => item != null && item.Activo);
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

            if (behaviour == null)
            {
                continue;
            }

            float distancia = Vector3.Distance(behaviour.transform.position, jugador.Position);

            if (distancia < mejorDistancia)
            {
                mejorDistancia = distancia;
                mejor = item;
            }
        }

        return mejor;
    }
}