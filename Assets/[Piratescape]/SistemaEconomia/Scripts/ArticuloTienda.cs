using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public sealed class ArticuloTienda : MonoBehaviour
{
    [Header("Datos del articulo")]
    [SerializeField] private string nombreArticulo;
    [SerializeField] private GameObject prefabObjeto;
    [SerializeField] private CosteTienda[] costes;

    [Header("Referencias")]
    [SerializeField] private SistemaEconomia sistemaEconomia;
    [SerializeField] private Transform puntoAparicion;
    [SerializeField] private MensajeTienda mensajeTienda;
    [SerializeField] private GhostMerchant vendedorFantasma;

    [Header("Espantamonos")]
    [SerializeField] private bool esEspantamonos;
    [SerializeField] private SistemaEspantamonos sistemaEspantamonos;

    private Button botonComprar;

    private void Awake()
    {
        botonComprar = GetComponent<Button>();
        botonComprar.onClick.AddListener(Comprar);

        if (sistemaEspantamonos == null)
        {
            sistemaEspantamonos = FindFirstObjectByType<SistemaEspantamonos>();
        }

        if (esEspantamonos && sistemaEspantamonos != null)
        {
            sistemaEspantamonos.OnEstadoCambiado += ActualizarVisibilidadEspantamonos;
            ActualizarVisibilidadEspantamonos(sistemaEspantamonos.EstaActivo);
        }
    }

    private void OnDestroy()
    {
        if (botonComprar != null)
        {
            botonComprar.onClick.RemoveListener(Comprar);
        }

        if (esEspantamonos && sistemaEspantamonos != null)
        {
            sistemaEspantamonos.OnEstadoCambiado -= ActualizarVisibilidadEspantamonos;
        }
    }

    private void Comprar()
    {
        Debug.Log("CLICK EN COMPRA: " + nombreArticulo);

        if (!ReferenciasValidas())
        {
            return;
        }

        if (esEspantamonos && sistemaEspantamonos.EstaActivo)
        {
            if (mensajeTienda != null)
            {
                mensajeTienda.MostrarError("El espantamonos ya esta colocado");
            }

            ActualizarVisibilidadEspantamonos(true);
            return;
        }

        if (!PuedeComprar())
        {
            Debug.Log("No tienes dinero suficiente para: " + nombreArticulo);

            if (mensajeTienda != null)
            {
                mensajeTienda.MostrarError("No tienes suficiente dinero");
            }

            return;
        }

        Cobrar();

        if (esEspantamonos)
        {
            bool comprado = sistemaEspantamonos.ComprarEspantamonos(prefabObjeto, puntoAparicion);

            if (!comprado)
            {
                if (mensajeTienda != null)
                {
                    mensajeTienda.MostrarError("No se pudo colocar el espantamonos");
                }

                return;
            }
        }
        else
        {
            Instantiate(prefabObjeto, puntoAparicion.position, puntoAparicion.rotation);
        }

        Debug.Log("Compra realizada: " + nombreArticulo);

        if (vendedorFantasma != null)
        {
            vendedorFantasma.CerrarTiendaDesdeUI();
        }
    }

    private bool ReferenciasValidas()
    {
        if (sistemaEconomia == null)
        {
            Debug.LogError("Falta SistemaEconomia en " + nombreArticulo, this);
            return false;
        }

        if (prefabObjeto == null)
        {
            Debug.LogError("Falta prefabObjeto en " + nombreArticulo, this);
            return false;
        }

        if (puntoAparicion == null)
        {
            Debug.LogError("Falta puntoAparicion en " + nombreArticulo, this);
            return false;
        }

        if (esEspantamonos && sistemaEspantamonos == null)
        {
            Debug.LogError("Falta SistemaEspantamonos en " + nombreArticulo, this);
            return false;
        }

        return true;
    }

    private bool PuedeComprar()
    {
        foreach (CosteTienda coste in costes)
        {
            if (coste == null)
            {
                continue;
            }

            Debug.Log(
                "Comprobando " + nombreArticulo +
                " | Tipo: " + coste.TipoMoneda +
                " | Precio: " + coste.Cantidad +
                " | Conchas actuales: " + sistemaEconomia.Conchas
            );

            if (!sistemaEconomia.TieneMonedasSuficientes(coste.TipoMoneda, coste.Cantidad))
            {
                return false;
            }
        }

        return true;
    }

    private void Cobrar()
    {
        foreach (CosteTienda coste in costes)
        {
            if (coste == null)
            {
                continue;
            }

            sistemaEconomia.GastarMoneda(coste.TipoMoneda, coste.Cantidad);
        }
    }

    private void ActualizarVisibilidadEspantamonos(bool espantamonosActivo)
    {
        if (!esEspantamonos)
        {
            return;
        }

        gameObject.SetActive(!espantamonosActivo);
    }
}