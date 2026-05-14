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