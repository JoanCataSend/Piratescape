using UnityEngine;

/// <summary>
/// Expone la posición del jugador para el sistema de interacción.
/// </summary>
public class JugadorActivador : MonoBehaviour, IActivador
{
    public Vector3 Position => transform.position;
}