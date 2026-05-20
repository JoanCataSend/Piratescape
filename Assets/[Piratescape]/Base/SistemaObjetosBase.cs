using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class SistemaObjetosBase : MonoBehaviour
{
    public static SistemaObjetosBase Instance { get; private set; }

    [Header("Punto por defecto")]
    [SerializeField] private Transform puntoAparicionDefecto;

    private readonly List<ObjetoBaseColocado> objetosColocados = new List<ObjetoBaseColocado>();

    public event Action OnObjetosBaseActualizados;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public bool EstaColocado(ObjetoBaseData objetoBase)
    {
        if (objetoBase == null)
        {
            return false;
        }

        LimpiarReferenciasNulas();

        for (int i = 0; i < objetosColocados.Count; i++)
        {
            if (EsMismoObjeto(objetosColocados[i].ObjetoBase, objetoBase))
            {
                return true;
            }
        }

        return false;
    }

    public bool DebeOcultarseEnTienda(ObjetoBaseData objetoBase)
    {
        if (objetoBase == null)
        {
            return false;
        }

        return objetoBase.OcultarEnTiendaMientrasEsteColocado && EstaColocado(objetoBase);
    }

    public bool ColocarObjeto(ObjetoBaseData objetoBase, Transform puntoAparicion)
    {
        if (objetoBase == null)
        {
            Debug.LogWarning("SistemaObjetosBase: falta ObjetoBaseData.");
            return false;
        }

        if (objetoBase.Prefab == null)
        {
            Debug.LogWarning("SistemaObjetosBase: falta prefab en " + objetoBase.Nombre);
            return false;
        }

        if (!objetoBase.PermitirVariasUnidades && EstaColocado(objetoBase))
        {
            return false;
        }

        Transform puntoFinal = puntoAparicion;

        if (puntoFinal == null)
        {
            puntoFinal = puntoAparicionDefecto;
        }

        if (puntoFinal == null)
        {
            Debug.LogWarning("SistemaObjetosBase: falta punto de aparicion.");
            return false;
        }

        GameObject objetoCreado = Instantiate(
            objetoBase.Prefab,
            puntoFinal.position,
            puntoFinal.rotation
        );

        objetosColocados.Add(new ObjetoBaseColocado(objetoBase, objetoCreado));

        OnObjetosBaseActualizados?.Invoke();

        return true;
    }

    public List<DatosObjetoBase> CrearDatosGuardado()
    {
        LimpiarReferenciasNulas();

        List<DatosObjetoBase> datos = new List<DatosObjetoBase>();

        for (int i = 0; i < objetosColocados.Count; i++)
        {
            ObjetoBaseColocado colocado = objetosColocados[i];

            if (colocado == null || colocado.ObjetoBase == null || colocado.ObjetoCreado == null)
            {
                continue;
            }

            DatosObjetoBase dato = new DatosObjetoBase();
            dato.idObjeto = colocado.ObjetoBase.IdObjeto;
            dato.posicion = colocado.ObjetoCreado.transform.position;
            dato.rotacionEuler = colocado.ObjetoCreado.transform.eulerAngles;

            InventarioCofre inventarioCofre = colocado.ObjetoCreado.GetComponentInChildren<InventarioCofre>(true);
            dato.inventarioCofre = InventarioGuardadoUtil.CrearDesdeCofre(inventarioCofre);

            datos.Add(dato);
        }

        return datos;
    }

    public void CargarDatosGuardado(List<DatosObjetoBase> datos, BaseObjectDatabase baseObjectDatabase, ItemDatabase itemDatabase)
    {
        DestruirObjetosColocadosActuales();

        if (datos == null || baseObjectDatabase == null)
        {
            OnObjetosBaseActualizados?.Invoke();
            return;
        }

        for (int i = 0; i < datos.Count; i++)
        {
            DatosObjetoBase dato = datos[i];

            if (dato == null || string.IsNullOrWhiteSpace(dato.idObjeto))
            {
                continue;
            }

            ObjetoBaseData objetoBase = baseObjectDatabase.BuscarPorId(dato.idObjeto);

            if (objetoBase == null || objetoBase.Prefab == null)
            {
                continue;
            }

            GameObject objetoCreado = Instantiate(
                objetoBase.Prefab,
                dato.posicion,
                Quaternion.Euler(dato.rotacionEuler)
            );

            InventarioCofre inventarioCofre = objetoCreado.GetComponentInChildren<InventarioCofre>(true);
            InventarioGuardadoUtil.CargarEnCofre(inventarioCofre, dato.inventarioCofre, itemDatabase);

            objetosColocados.Add(new ObjetoBaseColocado(objetoBase, objetoCreado));
        }

        OnObjetosBaseActualizados?.Invoke();
    }

    private bool EsMismoObjeto(ObjetoBaseData a, ObjetoBaseData b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a == b)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(a.IdObjeto) && !string.IsNullOrWhiteSpace(b.IdObjeto))
        {
            return a.IdObjeto == b.IdObjeto;
        }

        return false;
    }

    private void LimpiarReferenciasNulas()
    {
        for (int i = objetosColocados.Count - 1; i >= 0; i--)
        {
            if (objetosColocados[i].ObjetoCreado == null)
            {
                objetosColocados.RemoveAt(i);
            }
        }
    }

    private void DestruirObjetosColocadosActuales()
    {
        for (int i = objetosColocados.Count - 1; i >= 0; i--)
        {
            if (objetosColocados[i] != null && objetosColocados[i].ObjetoCreado != null)
            {
                Destroy(objetosColocados[i].ObjetoCreado);
            }
        }

        objetosColocados.Clear();
    }

    private sealed class ObjetoBaseColocado
    {
        public ObjetoBaseData ObjetoBase { get; }
        public GameObject ObjetoCreado { get; }

        public ObjetoBaseColocado(ObjetoBaseData objetoBase, GameObject objetoCreado)
        {
            ObjetoBase = objetoBase;
            ObjetoCreado = objetoCreado;
        }
    }
}
