using UnityEngine;

public sealed class RecursoGenerado : MonoBehaviour
{
    private GrupoSpawnRecursos grupoPropietario;
    private PuntoSpawnRecurso puntoPropietario;

    public void Configurar(GrupoSpawnRecursos grupo, PuntoSpawnRecurso punto)
    {
        grupoPropietario = grupo;
        puntoPropietario = punto;
    }

    public void Consumir()
    {
        if (grupoPropietario != null && puntoPropietario != null)
        {
            grupoPropietario.NotificarRecursoConsumido(this, puntoPropietario);
        }

        Destroy(gameObject);
    }
}