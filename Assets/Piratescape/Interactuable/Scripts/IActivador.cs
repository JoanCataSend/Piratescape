using UnityEngine;

/// <summary>
/// Expone la posición del objeto que puede activar interacciones.
/// </summary>
public interface IActivador
{
    Vector3 Position { get; }
}