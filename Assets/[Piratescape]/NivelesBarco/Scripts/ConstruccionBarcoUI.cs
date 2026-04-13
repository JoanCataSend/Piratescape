using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ConstruccionBarcoUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private ConstruccionBarco construccionBarco;
    [SerializeField] private ItemData itemConstruccion;

    [Header("Indicador")]
    [SerializeField] private GameObject raizIndicador;
    [SerializeField] private Image iconoIndicador;
    [SerializeField] private TMP_Text textoIndicador;

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
    }

    private void OnDisable()
    {
        if (construccionBarco == null)
        {
            return;
        }

        construccionBarco.OnConstruccionActualizada -= ManejarConstruccionActualizada;
        construccionBarco.OnConstruccionCompletada -= ManejarConstruccionCompletada;
    }

    public void EstablecerJugadorDentro(bool valor)
    {
        jugadorDentro = valor;
        ActualizarIndicador();
    }

    private void ConfigurarVisualInicial()
    {
        if (iconoIndicador != null && itemConstruccion != null)
        {
            iconoIndicador.sprite = itemConstruccion.Icon;
            iconoIndicador.enabled = itemConstruccion.Icon != null;
        }

        if (raizIndicador != null)
        {
            raizIndicador.SetActive(false);
        }

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
        if (raizIndicador == null || textoIndicador == null || construccionBarco == null || itemConstruccion == null)
        {
            return;
        }

        bool mostrarIndicador = jugadorDentro && !construccionBarco.ConstruccionCompletada;
        raizIndicador.SetActive(mostrarIndicador);

        int cantidadEntregada = construccionBarco.ObtenerCantidadEntregada(itemConstruccion);
        int cantidadNecesaria = construccionBarco.ObtenerCantidadNecesaria(itemConstruccion);

        textoIndicador.text = cantidadEntregada + "/" + cantidadNecesaria;
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