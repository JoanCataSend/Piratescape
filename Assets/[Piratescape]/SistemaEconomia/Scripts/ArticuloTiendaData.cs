using UnityEngine;

[CreateAssetMenu(fileName = "ArticuloTiendaData", menuName = "Gameplay/Tienda/Articulo Tienda Data")]
public sealed class ArticuloTiendaData : ScriptableObject
{
    [Header("Tipo de articulo")]
    [SerializeField] private TipoArticuloTienda tipoArticulo;

    [Header("Si es item de inventario")]
    [SerializeField] private ItemData itemInventario;
    [SerializeField] private int cantidadItem = 1;

    [Header("Si es objeto de base")]
    [SerializeField] private ObjetoBaseData objetoBase;

    [Header("Precio")]
    [SerializeField] private CosteTienda[] costes;

    [Header("Sobrescribir visual opcional")]
    [SerializeField] private string nombreManual;
    [SerializeField] private Sprite iconoManual;

    public TipoArticuloTienda TipoArticulo => tipoArticulo;
    public ItemData ItemInventario => itemInventario;
    public int CantidadItem => cantidadItem;
    public ObjetoBaseData ObjetoBase => objetoBase;
    public CosteTienda[] Costes => costes;

    public string Nombre
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(nombreManual))
            {
                return nombreManual;
            }

            if (tipoArticulo == TipoArticuloTienda.ItemInventario && itemInventario != null)
            {
                return itemInventario.DisplayName;
            }

            if (tipoArticulo == TipoArticuloTienda.ObjetoBase && objetoBase != null)
            {
                return objetoBase.Nombre;
            }

            return name;
        }
    }

    public Sprite Icono
    {
        get
        {
            if (iconoManual != null)
            {
                return iconoManual;
            }

            if (tipoArticulo == TipoArticuloTienda.ItemInventario && itemInventario != null)
            {
                return itemInventario.Icon;
            }

            if (tipoArticulo == TipoArticuloTienda.ObjetoBase && objetoBase != null)
            {
                return objetoBase.Icono;
            }

            return null;
        }
    }
}