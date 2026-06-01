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

    [SerializeField] private ArticuloTiendaData articuloPlatano;
    [SerializeField] private ArticuloTiendaData articuloCoco;
    [SerializeField] private ArticuloTiendaData articuloCuerda;
    [SerializeField] private ArticuloTiendaData articuloClavo;
    [SerializeField] private ArticuloTiendaData articuloEspantamonos;

    private bool mostrarPlatano = true;
    private bool mostrarCuerda = true;
    private bool espantamonosComprado;
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
            // Ocultar espantamonos si ya fue comprado
            if (articuloData == articuloEspantamonos && espantamonosComprado)
            {
                continue;
            }

            // Alternar platano y coco
            if (articuloData == articuloPlatano && !mostrarPlatano)
            {
                continue;
            }

            if (articuloData == articuloCoco && mostrarPlatano)
            {
                continue;
            }

            // Alternar cuerda y clavo
            if (articuloData == articuloCuerda && !mostrarCuerda)
            {
                continue;
            }

            if (articuloData == articuloClavo && mostrarCuerda)
            {
                continue;
            }

            Transform puntoAparicion = ObtenerPuntoAparicion(articuloData);

            ArticuloTienda articuloCreado = Instantiate(prefabArticuloTienda, contenedorArticulos);
            articuloCreado.name = "ShopItemUI_" + articuloData.Nombre;
            articuloCreado.Configurar(articuloData, puntoAparicion);
        }

        // Cambiar para la siguiente vez
        mostrarPlatano = !mostrarPlatano;
        mostrarCuerda = !mostrarCuerda;

    }

    public void ReconstruirArticulos()
    {
        CrearArticulos();
    }

    public void MarcarEspantamonosComprado()
    {
        espantamonosComprado = true;
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
