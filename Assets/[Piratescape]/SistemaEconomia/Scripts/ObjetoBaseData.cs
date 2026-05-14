using UnityEngine;

[CreateAssetMenu(fileName = "ObjetoBaseData", menuName = "Gameplay/Base/Objeto Base Data")]
public sealed class ObjetoBaseData : ScriptableObject
{
    [Header("Identificacion")]
    [SerializeField] private string idObjeto;
    [SerializeField] private string nombre;
    [SerializeField] private Sprite icono;

    [Header("Prefab")]
    [SerializeField] private GameObject prefab;

    [Header("Comportamiento")]
    [SerializeField] private TipoObjetoBase tipoObjetoBase = TipoObjetoBase.Normal;
    [SerializeField] private bool permitirVariasUnidades = false;
    [SerializeField] private bool ocultarEnTiendaMientrasEsteColocado = true;

    [Header("Mensajes")]
    [SerializeField] private string mensajeCompra = "Objeto colocado en base";
    [SerializeField] private string mensajeYaColocado = "Este objeto ya esta colocado en la base";

    public string IdObjeto => idObjeto;
    public string Nombre => nombre;
    public Sprite Icono => icono;
    public GameObject Prefab => prefab;
    public TipoObjetoBase TipoObjetoBase => tipoObjetoBase;
    public bool PermitirVariasUnidades => permitirVariasUnidades;
    public bool OcultarEnTiendaMientrasEsteColocado => ocultarEnTiendaMientrasEsteColocado;
    public string MensajeCompra => mensajeCompra;
    public string MensajeYaColocado => mensajeYaColocado;
}