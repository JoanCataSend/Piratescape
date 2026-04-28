using UnityEngine;

[CreateAssetMenu(fileName = "MonedaItem", menuName = "Gameplay/Items/Moneda Item")]
public sealed class MonedaItemData : ItemData
{
    [Header("Datos de moneda")]
    [SerializeField] private TipoMoneda tipoMoneda;
    [SerializeField] private int valor = 1;

    public TipoMoneda TipoMoneda => tipoMoneda;
    public int Valor => valor;
}