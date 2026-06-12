using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ConstruccionBarcoUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ConstruccionBarco construccionBarco;

    [Header("Indicador general")]
    [SerializeField] private GameObject raizIndicador;

    [Header("Indicador madera")]
    [SerializeField] private GameObject filaMadera;
    [SerializeField] private Image iconoMadera;
    [SerializeField] private TMP_Text textoMadera;
    [SerializeField] private ItemData itemMadera;

    [Header("Indicador clavos")]
    [SerializeField] private GameObject filaClavos;
    [SerializeField] private Image iconoClavos;
    [SerializeField] private TMP_Text textoClavos;
    [SerializeField] private ItemData itemClavos;

    [Header("Indicador cuerda")]
    [SerializeField] private GameObject filaCuerda;
    [SerializeField] private Image iconoCuerda;
    [SerializeField] private TMP_Text textoCuerda;
    [SerializeField] private ItemData itemCuerda;

    [Header("Mensaje final")]
    [SerializeField] private GameObject raizMensajeCompletado;
    [SerializeField] private TMP_Text textoMensajeCompletado;
    [SerializeField] private float duracionMensajeCompletado = 2.5f;

    private bool jugadorDentro;
    private Coroutine rutinaMensajeCompletado;

    private void Awake()
    {
        if (construccionBarco == null)
        {
            construccionBarco = FindFirstObjectByType<ConstruccionBarco>();
        }

        ConfigurarVisualInicial();
        ActualizarIndicador();
    }

    private void OnEnable()
    {
        if (construccionBarco == null)
        {
            return;
        }

        construccionBarco.OnConstruccionActualizada += ManejarConstruccionActualizada;
        construccionBarco.OnConstruccionCompletada += ManejarConstruccionCompletada;
        construccionBarco.OnMensajeConstruccionSolicitado += MostrarMensajeTemporal;
    }

    private void OnDisable()
    {
        if (construccionBarco == null)
        {
            return;
        }

        construccionBarco.OnConstruccionActualizada -= ManejarConstruccionActualizada;
        construccionBarco.OnConstruccionCompletada -= ManejarConstruccionCompletada;
        construccionBarco.OnMensajeConstruccionSolicitado -= MostrarMensajeTemporal;
    }

    public void EstablecerJugadorDentro(bool valor)
    {
        jugadorDentro = valor;
        ActualizarIndicador();
    }

    private void ConfigurarVisualInicial()
    {
        ConfigurarIcono(iconoMadera, itemMadera);
        ConfigurarIcono(iconoClavos, itemClavos);
        ConfigurarIcono(iconoCuerda, itemCuerda);

        if (raizIndicador != null)
        {
            raizIndicador.SetActive(false);
        }

        OcultarFilas();

        if (raizMensajeCompletado != null)
        {
            raizMensajeCompletado.SetActive(false);
        }
        else if (textoMensajeCompletado != null)
        {
            textoMensajeCompletado.gameObject.SetActive(false);
        }
    }

    private void ActualizarIndicador()
    {
        if (raizIndicador == null || construccionBarco == null)
        {
            return;
        }

        bool mostrarIndicador = jugadorDentro && !construccionBarco.ConstruccionCompletada;
        raizIndicador.SetActive(mostrarIndicador);

        if (!mostrarIndicador)
        {
            OcultarFilas();
            return;
        }

        ActualizarFilaMaterial(filaMadera, textoMadera, itemMadera);
        ActualizarFilaMaterial(filaClavos, textoClavos, itemClavos);
        ActualizarFilaMaterial(filaCuerda, textoCuerda, itemCuerda);
    }

    private void ActualizarFilaMaterial(GameObject fila, TMP_Text texto, ItemData item)
    {
        if (fila == null || texto == null || item == null || construccionBarco == null)
        {
            return;
        }

        int cantidadNecesaria = construccionBarco.ObtenerCantidadNecesaria(item);
        bool materialNecesarioEnNivelActual = cantidadNecesaria > 0;

        fila.SetActive(materialNecesarioEnNivelActual);

        if (!materialNecesarioEnNivelActual)
        {
            return;
        }

        int cantidadEntregada = construccionBarco.ObtenerCantidadEntregada(item);
        texto.text = cantidadEntregada + "/" + cantidadNecesaria;
    }

    private void ConfigurarIcono(Image icono, ItemData item)
    {
        if (icono == null || item == null)
        {
            return;
        }

        icono.sprite = item.Icon;
        icono.enabled = item.Icon != null;
    }

    private void OcultarFilas()
    {
        if (filaMadera != null)
        {
            filaMadera.SetActive(false);
        }

        if (filaClavos != null)
        {
            filaClavos.SetActive(false);
        }

        if (filaCuerda != null)
        {
            filaCuerda.SetActive(false);
        }
    }

  

    private void ManejarConstruccionActualizada()
    {
        ActualizarIndicador();
    }

    private void ManejarConstruccionCompletada()
    {
        ActualizarIndicador();
        MostrarMensajeCompletado();
    }

    private void MostrarMensajeCompletado()
    {
        if (raizMensajeCompletado == null && textoMensajeCompletado == null)
        {
            return;
        }

        if (textoMensajeCompletado != null)
        {
            textoMensajeCompletado.text = construccionBarco.UltimoMensajeCompletado;
            textoMensajeCompletado.color = Color.white;
        }

        if (rutinaMensajeCompletado != null)
        {
            StopCoroutine(rutinaMensajeCompletado);
        }

        rutinaMensajeCompletado = StartCoroutine(MostrarMensajeCompletadoRutina());
    }

    private void MostrarMensajeTemporal(string mensaje, Color color)
    {
        if (textoMensajeCompletado != null)
        {
            textoMensajeCompletado.text = mensaje;
            textoMensajeCompletado.color = color;
        }

        if (rutinaMensajeCompletado != null)
        {
            StopCoroutine(rutinaMensajeCompletado);
        }

        rutinaMensajeCompletado = StartCoroutine(MostrarMensajeCompletadoRutina());
    }

    private IEnumerator MostrarMensajeCompletadoRutina()
    {
        if (raizMensajeCompletado != null)
        {
            raizMensajeCompletado.SetActive(true);
        }
        else if (textoMensajeCompletado != null)
        {
            textoMensajeCompletado.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(duracionMensajeCompletado);

        if (raizMensajeCompletado != null)
        {
            raizMensajeCompletado.SetActive(false);
        }
        else if (textoMensajeCompletado != null)
        {
            textoMensajeCompletado.gameObject.SetActive(false);
        }

        rutinaMensajeCompletado = null;
    }
}