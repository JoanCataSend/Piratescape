using System.Collections;
using UnityEngine;

public class PuertaMagica : MonoBehaviour, Interactuable
{
    [Header("Interacción")]
    [SerializeField] private float rango = 3f;
    [SerializeField] private float distanciaParaUsarObjetos = 1f;

    [SerializeField]
    private string mensajeBloqueada =
        "Entrega las 3 gemas mágicas";

    [SerializeField]
    private string mensajeEntregaGemas =
        "Selecciona una gema y pulsa E para entregarla";

    [SerializeField]
    private string mensajeNecesitaLlave =
        "Selecciona la llave y pulsa E para abrir";

    [SerializeField]
    private string mensajeDesbloqueada =
        "Pulsa E para abrir la puerta mágica";

    [Header("Gemas necesarias")]
    [SerializeField] private ItemData gemaRosa;
    [SerializeField] private ItemData gemaAmarilla;
    [SerializeField] private ItemData gemaMorada;

    [Header("Llave necesaria")]
    [SerializeField] private ItemData llavePuerta;

    [Header("Cambio de material")]
    [SerializeField] private Renderer rendererParteRoja;
    [SerializeField] private int indiceMaterial = 0;
    [SerializeField] private Material materialVerde;

    [Header("Animación del mecanismo")]
    [SerializeField] private Transform objetoARotarConLlave;
    [SerializeField] private float gradosRotacionZ = 90f;
    [SerializeField] private float duracionRotacion = 1.5f;
    [SerializeField] private AnimationCurve curvaRotacion = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(1f, 1f)
    );

    [Header("Cinemática")]
    [SerializeField] private FinalAlternativoController cinematica;
    [SerializeField] private float esperaAntesDeCinematica = 0.3f;

    [Header("Sonido portal")]
    [SerializeField] private AudioSource audioSourcePortal;
    [SerializeField] private AudioClip sonidoPortal;
    [SerializeField] private float volumenPortal = 0.45f;
    private bool puertaAbierta;

    private PlayerInventory inventario;
    private JugadorActivador jugador;

    private bool gemaRosaEntregada;
    private bool gemaAmarillaEntregada;
    private bool gemaMoradaEntregada;

    private bool puertaActivadaConGemas;
    private bool cinematicaLanzada;
    private bool animacionLlaveEnCurso;

    public float Rango
    {
        get => rango;
        set => rango = value;
    }

    public bool Activo => true;

    private void Start()
    {
        inventario = FindFirstObjectByType<PlayerInventory>();
        jugador = FindFirstObjectByType<JugadorActivador>();
    }

    private void Update()
    {
        if (jugador == null || cinematicaLanzada)
        {
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.Position);

        if (distancia <= rango)
        {
            InteractionUI.Instance?.Show(this, ObtenerMensajeActual());
        }
        else
        {
            InteractionUI.Instance?.Hide(this);
        }
    }

    public void Interactuar()
    {
        if (cinematicaLanzada || animacionLlaveEnCurso)
        {
            return;
        }

        if (!JugadorEstaCercaParaUsarObjetos())
        {
            Debug.Log("Estás demasiado lejos de la puerta.");
            return;
        }

        if (!puertaActivadaConGemas)
        {
            IntentarEntregarGemaSeleccionada();
            return;
        }

        IntentarAbrirConLlaveSeleccionada();
    }

    private bool JugadorEstaCercaParaUsarObjetos()
    {
        if (jugador == null)
        {
            return false;
        }

        float distancia = Vector3.Distance(transform.position, jugador.Position);
        return distancia <= distanciaParaUsarObjetos;
    }

    private void IntentarEntregarGemaSeleccionada()
    {
        if (inventario == null)
        {
            Debug.LogWarning("No hay inventario asignado o encontrado.");
            return;
        }

        ItemData itemSeleccionado = ObtenerItemSeleccionado();

        if (itemSeleccionado == null)
        {
            Debug.Log("No hay ningún objeto seleccionado.");
            return;
        }

        if (itemSeleccionado == gemaRosa)
        {
            EntregarGema(ref gemaRosaEntregada, gemaRosa, "Gema rosa");
            return;
        }

        if (itemSeleccionado == gemaAmarilla)
        {
            EntregarGema(ref gemaAmarillaEntregada, gemaAmarilla, "Gema amarilla");
            return;
        }

        if (itemSeleccionado == gemaMorada)
        {
            EntregarGema(ref gemaMoradaEntregada, gemaMorada, "Gema morada");
            return;
        }

        Debug.Log("El objeto seleccionado no es una gema válida para esta puerta.");
    }

    private void EntregarGema(ref bool gemaEntregada, ItemData gema, string nombreGema)
    {
        if (gemaEntregada)
        {
            Debug.Log(nombreGema + " ya estaba entregada.");
            return;
        }

        if (gema == null)
        {
            Debug.LogWarning("Falta asignar una gema en la puerta.");
            return;
        }

        bool eliminada = inventario.RemoveItem(gema, 1);

        if (!eliminada)
        {
            Debug.LogWarning("No se ha podido eliminar " + nombreGema + " del inventario.");
            return;
        }

        gemaEntregada = true;

        Debug.Log(nombreGema + " entregada.");

        if (TodasLasGemasEntregadas())
        {
            ActivarPuertaConGemas();
        }
    }

    private void ActivarPuertaConGemas()
    {
        puertaActivadaConGemas = true;
        CambiarMaterialAVerde();

        Debug.Log("Las 3 gemas han sido entregadas. Puerta activada.");
    }

    private void IntentarAbrirConLlaveSeleccionada()
    {
        if (!TieneLlaveSeleccionada())
        {
            Debug.Log("Necesitas tener la llave seleccionada.");
            return;
        }

        bool llaveEliminada = inventario.RemoveItem(llavePuerta, 1);

        if (!llaveEliminada)
        {
            Debug.LogWarning("No se ha podido eliminar la llave del inventario.");
            return;
        }

        StartCoroutine(AbrirPuertaConLlaveRoutine());
    }

    private IEnumerator AbrirPuertaConLlaveRoutine()
    {
        animacionLlaveEnCurso = true;
        InteractionUI.Instance?.Hide(this);

        if (objetoARotarConLlave != null)
        {
            Quaternion rotacionInicial = objetoARotarConLlave.localRotation;
            Quaternion rotacionFinal = rotacionInicial * Quaternion.Euler(0f, 0f, gradosRotacionZ);

            float tiempo = 0f;

            while (tiempo < duracionRotacion)
            {
                tiempo += Time.deltaTime;
                float t = Mathf.Clamp01(tiempo / duracionRotacion);
                float valorCurva = curvaRotacion.Evaluate(t);

                objetoARotarConLlave.localRotation =
                    Quaternion.Lerp(rotacionInicial, rotacionFinal, valorCurva);

                yield return null;
            }

            objetoARotarConLlave.localRotation = rotacionFinal;
        }

        puertaAbierta = true;
        ReproducirSonidoPortal();

        animacionLlaveEnCurso = false;
    }

    private string ObtenerMensajeActual()
    {
        if (!puertaActivadaConGemas)
        {
            return mensajeEntregaGemas;
        }

        return TieneLlaveSeleccionada()
            ? mensajeDesbloqueada
            : mensajeNecesitaLlave;
    }

    private bool TodasLasGemasEntregadas()
    {
        return gemaRosaEntregada && gemaAmarillaEntregada && gemaMoradaEntregada;
    }

    private bool TieneLlaveSeleccionada()
    {
        ItemData itemSeleccionado = ObtenerItemSeleccionado();

        if (itemSeleccionado == null || llavePuerta == null)
        {
            return false;
        }

        return itemSeleccionado == llavePuerta;
    }

    private ItemData ObtenerItemSeleccionado()
    {
        if (inventario == null)
        {
            return null;
        }

        InventorySlot slotSeleccionado = inventario.GetSlot(inventario.SelectedSlotIndex);

        if (slotSeleccionado == null || slotSeleccionado.IsEmpty() || slotSeleccionado.itemData == null)
        {
            return null;
        }

        return slotSeleccionado.itemData;
    }

    private void CambiarMaterialAVerde()
    {
        if (rendererParteRoja == null || materialVerde == null)
        {
            Debug.LogWarning("Falta asignar el renderer de la parte roja o el material verde.", this);
            return;
        }

        Material[] materiales = rendererParteRoja.materials;

        if (indiceMaterial < 0 || indiceMaterial >= materiales.Length)
        {
            Debug.LogWarning("El índice de material no es válido en la puerta.", this);
            return;
        }

        materiales[indiceMaterial] = materialVerde;
        rendererParteRoja.materials = materiales;
    }

    private void ReproducirSonidoPortal()
    {
        if (audioSourcePortal == null || sonidoPortal == null)
        {
            return;
        }

        audioSourcePortal.clip = sonidoPortal;
        audioSourcePortal.volume = volumenPortal;
        audioSourcePortal.loop = true;
        audioSourcePortal.spatialBlend = 1f;
        audioSourcePortal.minDistance = 4f;
        audioSourcePortal.maxDistance = 45f;
        audioSourcePortal.rolloffMode = AudioRolloffMode.Linear;

        audioSourcePortal.Play();
    }

    private void PararSonidoPortal()
    {
        if (audioSourcePortal != null)
        {
            audioSourcePortal.Stop();
        }
    }

    public void DetenerPortalPorCinematica()
    {
        puertaAbierta = false;
        cinematicaLanzada = true;
        PararSonidoPortal();
    }

}