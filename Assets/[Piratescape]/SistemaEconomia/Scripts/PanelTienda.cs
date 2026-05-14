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

    private void OnEnable()
    {
        CrearArticulos();
    }

    private void CrearArticulos()
    {
        if (tiendaData == null || prefabArticuloTienda == null || contenedorArticulos == null)
        {
            Debug.LogError("PanelTienda: faltan referencias.");
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
            if (articulos[i] == null)
            {
                continue;
            }

            ArticuloTienda articuloCreado = Instantiate(prefabArticuloTienda, contenedorArticulos);
            articuloCreado.Configurar(articulos[i]);
        }
    }

    private void LimpiarContenedor()
    {
        for (int i = contenedorArticulos.childCount - 1; i >= 0; i--)
        {
            Destroy(contenedorArticulos.GetChild(i).gameObject);
        }
    }
}