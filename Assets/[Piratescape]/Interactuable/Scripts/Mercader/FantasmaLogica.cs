using UnityEngine;

public class FantasmaLogica : MonoBehaviour
{
    [Header("Aparición")]
    [SerializeField] private Transform[] puntosAparicion;
    [SerializeField] private GameObject visualFantasma;

    [Header("Tiempo")]
    [SerializeField] private bool esDeNoche;
    [SerializeField] private int numeroNoche;

    [Header("Movimiento en forma de 8")]
    [SerializeField] private float amplitudX = 0.8f;
    [SerializeField] private float amplitudZ = 0.4f;
    [SerializeField] private float velocidad = 1.2f;

    [Header("Items de la tienda")]
    [SerializeField] private GameObject itemPlatano;
    [SerializeField] private GameObject itemCoco;
    [SerializeField] private GameObject itemCuerda;
    [SerializeField] private GameObject itemClavos;
    [SerializeField] private GameObject itemEspantamonos;

    private Vector3 posicionBase;
    private bool espantamonosComprado;

    private void Start()
    {
        if (visualFantasma == null)
            visualFantasma = gameObject;

        Desaparecer();
    }

    private void Update()
    {
        if (!esDeNoche)
            return;

        MovimientoEnOcho();
    }

    public void EmpezarNoche()
    {
        esDeNoche = true;
        numeroNoche++;

        AparecerEnPuntoAleatorio();
        ActualizarItemsTienda();

        visualFantasma.SetActive(true);
    }

    public void EmpezarDia()
    {
        esDeNoche = false;
        Desaparecer();
    }

    private void AparecerEnPuntoAleatorio()
    {
        if (puntosAparicion == null || puntosAparicion.Length == 0)
        {
            Debug.LogWarning("No hay puntos de aparición asignados al fantasma.");
            return;
        }

        int indice = Random.Range(0, puntosAparicion.Length);
        transform.position = puntosAparicion[indice].position;

        posicionBase = transform.position;
    }

    private void ActualizarItemsTienda()
    {
        bool nochePar = numeroNoche % 2 == 0;

        if (itemPlatano != null)
            itemPlatano.SetActive(!nochePar);

        if (itemCoco != null)
            itemCoco.SetActive(nochePar);

        if (itemCuerda != null)
            itemCuerda.SetActive(!nochePar);

        if (itemClavos != null)
            itemClavos.SetActive(nochePar);

        if (itemEspantamonos != null)
            itemEspantamonos.SetActive(!espantamonosComprado);
    }

    public void ComprarEspantamonos()
    {
        espantamonosComprado = true;

        if (itemEspantamonos != null)
            itemEspantamonos.SetActive(false);
    }

    private void Desaparecer()
    {
        if (visualFantasma != null)
            visualFantasma.SetActive(false);
    }

    private void MovimientoEnOcho()
    {
        float tiempo = Time.time * velocidad;

        float x = Mathf.Sin(tiempo) * amplitudX;
        float z = Mathf.Sin(tiempo * 2f) * amplitudZ;

        transform.position = posicionBase + new Vector3(x, 0f, z);
    }
}