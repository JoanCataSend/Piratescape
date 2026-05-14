using UnityEngine;

[CreateAssetMenu(fileName = "TiendaData", menuName = "Gameplay/Tienda/Tienda Data")]
public sealed class TiendaData : ScriptableObject
{
    [Header("Datos de la tienda")]
    [SerializeField] private string nombreTienda;

    [Header("Articulos disponibles")]
    [SerializeField] private ArticuloTiendaData[] articulos;

    public string NombreTienda => nombreTienda;
    public ArticuloTiendaData[] Articulos => articulos;
}