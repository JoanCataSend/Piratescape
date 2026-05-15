using System;
using TMPro;
using UnityEngine;

public sealed class PanelTienda : MonoBehaviour
{
    [Header("Datos")]
    [SerializeField] private TiendaData tiendaData;

    [Header("Prefab")]
    [SerializeField] private ArticuloTienda prefabArticuloTienda;

    [Header("Contenedor")]
    [SerializeField] private Transform contenedorArticulos;

    [Header("Titulo")]
    [SerializeField] private TMP_Text textoTitulo;

    [Header("Puntos para objetos de base")]
    [SerializeField] private PuntoAparicionObjetoBase[] puntosObjetosBase;

    private bool articulosCreados;

    private void OnEnable()
    {
        CrearArticulos();
    }

    public void CrearArticulos()
    {
        if (!ReferenciasValidas())
        {
            return;
        }

        if (textoTitulo != null)
        {
            textoTitulo.text = tiendaData.NombreTienda;
        }

        LimpiarContenedor();

        ArticuloTiendaData[] articulos = tiendaData.Articulos;

        for (int i = 0; i < articulos.Length; i++)
        {
            ArticuloTiendaData articuloData = articulos[i];

            if (articuloData == null)
            {
                continue;
            }

            Transform puntoAparicion = ObtenerPuntoAparicion(articuloData);

            ArticuloTienda articuloCreado = Instantiate(prefabArticuloTienda, contenedorArticulos);
            articuloCreado.name = "ShopItemUI_" + articuloData.Nombre;
            articuloCreado.Configurar(articuloData, puntoAparicion);
        }

        articulosCreados = true;
    }

    public void ReconstruirArticulos()
    {
        articulosCreados = false;
        CrearArticulos();
    }

    private bool ReferenciasValidas()
    {
        if (tiendaData == null)
        {
            Debug.LogError("PanelTienda: falta TiendaData.", this);
            return false;
        }

        if (prefabArticuloTienda == null)
        {
            Debug.LogError("PanelTienda: falta Prefab Articulo Tienda.", this);
            return false;
        }

        if (contenedorArticulos == null)
        {
            Debug.LogError("PanelTienda: falta Contenedor Articulos.", this);
            return false;
        }

        return true;
    }

    private void LimpiarContenedor()
    {
        for (int i = contenedorArticulos.childCount - 1; i >= 0; i--)
        {
            Transform hijo = contenedorArticulos.GetChild(i);

            if (Application.isPlaying)
            {
                Destroy(hijo.gameObject);
            }
            else
            {
                DestroyImmediate(hijo.gameObject);
            }
        }
    }

    private Transform ObtenerPuntoAparicion(ArticuloTiendaData articuloData)
    {
        if (articuloData == null || articuloData.TipoArticulo != TipoArticuloTienda.ObjetoBase)
        {
            return null;
        }

        ObjetoBaseData objetoBase = articuloData.ObjetoBase;

        if (objetoBase == null || puntosObjetosBase == null)
        {
            return null;
        }

        for (int i = 0; i < puntosObjetosBase.Length; i++)
        {
            PuntoAparicionObjetoBase punto = puntosObjetosBase[i];

            if (punto == null)
            {
                continue;
            }

            if (punto.CoincideCon(objetoBase))
            {
                return punto.PuntoAparicion;
            }
        }

        Debug.LogWarning("PanelTienda: no hay punto de aparicion para " + objetoBase.Nombre, this);
        return null;
    }

    [Serializable]
    private sealed class PuntoAparicionObjetoBase
    {
        [SerializeField] private ObjetoBaseData objetoBase;
        [SerializeField] private Transform puntoAparicion;

        public Transform PuntoAparicion => puntoAparicion;

        public bool CoincideCon(ObjetoBaseData otroObjetoBase)
        {
            if (objetoBase == null || otroObjetoBase == null)
            {
                return false;
            }

            if (objetoBase == otroObjetoBase)
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(objetoBase.IdObjeto) && !string.IsNullOrWhiteSpace(otroObjetoBase.IdObjeto))
            {
                return objetoBase.IdObjeto == otroObjetoBase.IdObjeto;
            }

            return false;
        }
    }
}
