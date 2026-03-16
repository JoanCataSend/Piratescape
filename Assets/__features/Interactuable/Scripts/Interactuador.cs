using System.Linq;
using UnityEngine;

/// <summary>
/// Gestiona la interacción del jugador con los objetos activos.
/// </summary>
public class Interactuador : MonoBehaviour
{
    private Interactuable[] interactuables;

    private void Start()
    {
        interactuables = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<Interactuable>()
            .ToArray();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            foreach (var item in interactuables)
            {
                if (item.Activo)
                {
                    item.Interactuar();
                    return;
                }
            }
        }
    }
}