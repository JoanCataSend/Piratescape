using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GrupoSpawnRecursos : MonoBehaviour
{
    [Header("Configuracion del recurso")]
    [SerializeField] private List<GameObject> prefabsRecurso = new List<GameObject>();
    [SerializeField] private Transform contenedorRecursosGenerados;

    [Header("Configuracion del spawn")]
    [SerializeField] private List<PuntoSpawnRecurso> puntosSpawn = new List<PuntoSpawnRecurso>();
    [SerializeField] private int cantidadInicial = 4;
    [SerializeField] private int cantidadMaximaActiva = 4;
    [SerializeField] private float tiempoReaparicion = 20f;
    [SerializeField] private LayerMask mascaraBloqueo;
    [SerializeField] private float desplazamientoAltura = 0.15f;

    private readonly List<RecursoGenerado> recursosActivos = new List<RecursoGenerado>();
    private PuntoSpawnRecurso ultimoPuntoUsado;

    private void Awake()
    {
        CachearPuntosSpawnHijos();
        AjustarConfiguracion();
    }

    private void Start()
    {
        RellenarHastaObjetivo(cantidadInicial);
    }

    public void NotificarRecursoConsumido(RecursoGenerado recurso, PuntoSpawnRecurso punto)
    {
        recursosActivos.Remove(recurso);

        if (punto != null)
        {
            punto.LimpiarRecursoActual();
        }

        StartCoroutine(ReaparecerTrasTiempo());
    }

    public void ReiniciarRecursosDelGrupo()
    {
        StopAllCoroutines();

        for (int i = 0; i < recursosActivos.Count; i++)
        {
            if (recursosActivos[i] != null)
            {
                Destroy(recursosActivos[i].gameObject);
            }
        }

        recursosActivos.Clear();

        for (int i = 0; i < puntosSpawn.Count; i++)
        {
            if (puntosSpawn[i] != null)
            {
                puntosSpawn[i].LimpiarRecursoActual();
            }
        }

        ultimoPuntoUsado = null;

        RellenarHastaObjetivo(cantidadMaximaActiva);
    }

    public void RepoblarRecursosFaltantes()
    {
        LimpiarReferenciasDestruidas();
        RellenarHastaObjetivo(cantidadMaximaActiva);
    }

    private IEnumerator ReaparecerTrasTiempo()
    {
        yield return new WaitForSeconds(tiempoReaparicion);

        LimpiarReferenciasDestruidas();

        if (recursosActivos.Count >= cantidadMaximaActiva)
        {
            yield break;
        }

        GenerarUno();
    }

    private void RellenarHastaObjetivo(int cantidadObjetivo)
    {
        LimpiarReferenciasDestruidas();

        int objetivoSeguro = Mathf.Min(cantidadObjetivo, cantidadMaximaActiva);
        int cantidadFaltante = objetivoSeguro - recursosActivos.Count;

        for (int i = 0; i < cantidadFaltante; i++)
        {
            GenerarUno();
        }
    }

    private void GenerarUno()
    {
        PuntoSpawnRecurso puntoElegido = ObtenerPuntoDisponible();

        if (puntoElegido == null)
        {
            return;
        }

        GameObject prefabElegido = ObtenerPrefabAleatorio();

        if (prefabElegido == null)
        {
            return;
        }

        Vector3 posicionSpawn = puntoElegido.Posicion + Vector3.up * desplazamientoAltura;

        GameObject instancia = Instantiate(
            prefabElegido,
            posicionSpawn,
            prefabElegido.transform.rotation,
            contenedorRecursosGenerados
        );

        RecursoGenerado recursoGenerado = instancia.GetComponent<RecursoGenerado>();

        if (recursoGenerado == null)
        {
            recursoGenerado = instancia.AddComponent<RecursoGenerado>();
        }

        recursoGenerado.Configurar(this, puntoElegido);

        puntoElegido.AsignarRecursoActual(recursoGenerado);
        recursosActivos.Add(recursoGenerado);
        ultimoPuntoUsado = puntoElegido;
    }

    private PuntoSpawnRecurso ObtenerPuntoDisponible()
    {
        List<PuntoSpawnRecurso> puntosValidos = new List<PuntoSpawnRecurso>();

        for (int i = 0; i < puntosSpawn.Count; i++)
        {
            PuntoSpawnRecurso puntoActual = puntosSpawn[i];

            if (puntoActual == null || puntoActual.EstaOcupado || EstaBloqueado(puntoActual))
            {
                continue;
            }

            puntosValidos.Add(puntoActual);
        }

        if (puntosValidos.Count == 0)
        {
            return null;
        }

        if (puntosValidos.Count > 1 && ultimoPuntoUsado != null)
        {
            puntosValidos.Remove(ultimoPuntoUsado);
        }

        if (puntosValidos.Count == 0)
        {
            return null;
        }

        return puntosValidos[Random.Range(0, puntosValidos.Count)];
    }

    private bool EstaBloqueado(PuntoSpawnRecurso punto)
    {
        return Physics.CheckSphere(
            punto.Posicion,
            punto.RadioComprobacion,
            mascaraBloqueo,
            QueryTriggerInteraction.Ignore);
    }

    private GameObject ObtenerPrefabAleatorio()
    {
        if (prefabsRecurso.Count == 0)
        {
            return null;
        }

        return prefabsRecurso[Random.Range(0, prefabsRecurso.Count)];
    }

    private void LimpiarReferenciasDestruidas()
    {
        recursosActivos.RemoveAll(recurso => recurso == null);
    }

    private void CachearPuntosSpawnHijos()
    {
        if (puntosSpawn.Count > 0)
        {
            return;
        }

        PuntoSpawnRecurso[] puntosEncontrados = GetComponentsInChildren<PuntoSpawnRecurso>(true);

        for (int i = 0; i < puntosEncontrados.Length; i++)
        {
            puntosSpawn.Add(puntosEncontrados[i]);
        }
    }

    private void AjustarConfiguracion()
    {
        cantidadInicial = Mathf.Max(0, cantidadInicial);
        cantidadMaximaActiva = Mathf.Max(1, cantidadMaximaActiva);
        cantidadInicial = Mathf.Min(cantidadInicial, cantidadMaximaActiva);
    }
}