using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class MonoAmistadPlatano : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform jugador;
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private ItemData itemPlatano;
    [SerializeField] private Animator animatorMono;
    [SerializeField] private NavMeshAgent navMeshAgent;

    [Header("Interaccion")]
    [SerializeField] private float rangoInteraccion = 2.5f;
    [SerializeField] private int platanosNecesariosParaAmistad = 2;
    [SerializeField] private string itemIdPlatano = "platano";
    [SerializeField] private string mensajeSinPlatanos = "Necesitas platanos";
    [SerializeField] private string mensajeDarPlatano = "Dar platano";
    [SerializeField] private string mensajeMonoAmigo = "El mono ahora es tu amigo";
    [SerializeField] private float duracionMensajeTemporal = 1.8f;

    [Header("Seguimiento")]
    [SerializeField] private bool seguirAlSerAmigo = true;
    [SerializeField] private float distanciaParar = 1.8f;
    [SerializeField] private float velocidadSinNavMesh = 2.5f;
    [SerializeField] private float velocidadGiro = 8f;
    [SerializeField] private bool mantenerAlturaInicialSinNavMesh = true;

    [Header("Estado")]
    [SerializeField] private int platanosRecibidos;
    [SerializeField] private bool esAmigo;

    [Header("Animacion opcional")]
    [SerializeField] private string triggerRecibirPlatano = "RecibirPlatano";
    [SerializeField] private string triggerAmigo = "Amigo";
    [SerializeField] private string boolSiguiendo = "Siguiendo";

    private float alturaInicial;
    private bool promptMostrado;
    private float tiempoHastaPermitirOcultarPrompt;

    private void Awake()
    {
        alturaInicial = transform.position.y;

        if (jugador == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                jugador = playerObject.transform;
            }
        }

        if (inventarioJugador == null && jugador != null)
        {
            inventarioJugador = jugador.GetComponent<PlayerInventory>();
        }

        if (animatorMono == null)
        {
            animatorMono = GetComponentInChildren<Animator>();
        }

        if (navMeshAgent == null)
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        ConfigurarNavMeshAgent();
    }

    private void OnEnable()
    {
        ConfigurarNavMeshAgent();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
        {
            OcultarPrompt();
            return;
        }

        if (CofreInventarioUI.HayAlgunaUIAbierta)
        {
            OcultarPrompt();
            return;
        }

        if (jugador == null)
        {
            BuscarJugador();
        }

        if (jugador == null)
        {
            return;
        }

        if (esAmigo)
        {
            OcultarPrompt();
            SeguirJugador();
            return;
        }

        GestionarInteraccion();
    }

    private void GestionarInteraccion()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia > rangoInteraccion)
        {
            OcultarPrompt();
            return;
        }

        MostrarPrompt();

        if (SeHaPulsadoInteraccion())
        {
            IntentarDarPlatano();
        }
    }

    private void IntentarDarPlatano()
    {
        if (inventarioJugador == null)
        {
            inventarioJugador = jugador.GetComponent<PlayerInventory>();
        }

        if (inventarioJugador == null)
        {
            MostrarMensajeTemporal("No encuentro el inventario");
            return;
        }

        ItemData platanoEncontrado = ObtenerItemPlatanoDelInventario();

        if (platanoEncontrado == null)
        {
            MostrarMensajeTemporal(mensajeSinPlatanos);
            return;
        }

        bool eliminado = inventarioJugador.RemoveItem(platanoEncontrado, 1);

        if (!eliminado)
        {
            MostrarMensajeTemporal(mensajeSinPlatanos);
            return;
        }

        platanosRecibidos++;
        ReproducirTrigger(triggerRecibirPlatano);

        if (platanosRecibidos >= platanosNecesariosParaAmistad)
        {
            ConvertirEnAmigo();
            return;
        }

        MostrarMensajeTemporal(mensajeDarPlatano + " " + platanosRecibidos + "/" + platanosNecesariosParaAmistad);
    }

    private ItemData ObtenerItemPlatanoDelInventario()
    {
        if (inventarioJugador == null)
        {
            return null;
        }

        if (itemPlatano != null && inventarioJugador.ObtenerCantidad(itemPlatano) > 0)
        {
            return itemPlatano;
        }

        foreach (InventorySlot slot in inventarioJugador.GetSlots())
        {
            if (slot == null || slot.IsEmpty() || slot.itemData == null)
            {
                continue;
            }

            if (EsPlatano(slot.itemData))
            {
                return slot.itemData;
            }
        }

        return null;
    }

    private bool EsPlatano(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        string id = item.ItemId != null ? item.ItemId.ToLowerInvariant() : "";
        string nombreAsset = item.name != null ? item.name.ToLowerInvariant() : "";
        string nombreMostrar = item.DisplayName != null ? item.DisplayName.ToLowerInvariant() : "";
        string idBuscado = itemIdPlatano != null ? itemIdPlatano.ToLowerInvariant() : "platano";

        return id == idBuscado ||
               nombreAsset.Contains(idBuscado) ||
               nombreMostrar.Contains(idBuscado) ||
               nombreAsset.Contains("plátano") ||
               nombreMostrar.Contains("plátano") ||
               nombreAsset.Contains("banana") ||
               nombreMostrar.Contains("banana");
    }

    private void ConvertirEnAmigo()
    {
        esAmigo = true;
        platanosRecibidos = Mathf.Max(platanosRecibidos, platanosNecesariosParaAmistad);

        ReproducirTrigger(triggerAmigo);
        CambiarBool(boolSiguiendo, seguirAlSerAmigo);
        MostrarMensajeTemporal(mensajeMonoAmigo);
        ConfigurarNavMeshAgent();
    }

    private void SeguirJugador()
    {
        if (!seguirAlSerAmigo || jugador == null)
        {
            CambiarBool(boolSiguiendo, false);
            PararNavMeshAgent();
            return;
        }

        float distancia = Vector3.Distance(transform.position, jugador.position);
        bool debeMoverse = distancia > distanciaParar;

        CambiarBool(boolSiguiendo, debeMoverse);

        if (!debeMoverse)
        {
            PararNavMeshAgent();
            MirarHacia(jugador.position);
            return;
        }

        if (navMeshAgent != null && navMeshAgent.enabled && navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.stoppingDistance = distanciaParar;
            navMeshAgent.SetDestination(jugador.position);
            return;
        }

        SeguirSinNavMesh();
    }

    private void SeguirSinNavMesh()
    {
        Vector3 destino = jugador.position;

        if (mantenerAlturaInicialSinNavMesh)
        {
            destino.y = alturaInicial;
        }

        Vector3 direccion = destino - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Vector3 movimiento = direccion.normalized * velocidadSinNavMesh * Time.deltaTime;

        if (movimiento.sqrMagnitude > direccion.sqrMagnitude)
        {
            movimiento = direccion;
        }

        transform.position += movimiento;
        MirarHacia(destino);
    }

    private void MirarHacia(Vector3 objetivo)
    {
        Vector3 direccion = objetivo - transform.position;
        direccion.y = 0f;

        if (direccion.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion rotacionObjetivo = Quaternion.LookRotation(direccion.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotacionObjetivo, Time.deltaTime * velocidadGiro);
    }

    private void ConfigurarNavMeshAgent()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled)
        {
            return;
        }

        navMeshAgent.stoppingDistance = distanciaParar;
        navMeshAgent.updateRotation = true;

        if (navMeshAgent.isOnNavMesh)
        {
            navMeshAgent.isStopped = !esAmigo;
        }
    }

    private void PararNavMeshAgent()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
        {
            return;
        }

        navMeshAgent.isStopped = true;
        navMeshAgent.ResetPath();
    }

    private void MostrarPrompt()
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

        string texto = mensajeDarPlatano + " " + platanosRecibidos + "/" + platanosNecesariosParaAmistad;
        InteractionUI.Instance.Show(this, texto);
        promptMostrado = true;
    }

    private void MostrarMensajeTemporal(string texto)
    {
        if (InteractionUI.Instance == null)
        {
            return;
        }

        InteractionUI.Instance.ShowTemporary(this, texto, duracionMensajeTemporal);
        tiempoHastaPermitirOcultarPrompt = Time.time + duracionMensajeTemporal;
        promptMostrado = true;
    }

    private void OcultarPrompt()
    {
        if (!promptMostrado || InteractionUI.Instance == null)
        {
            return;
        }

        if (Time.time < tiempoHastaPermitirOcultarPrompt)
        {
            return;
        }

        InteractionUI.Instance.Hide(this);
        promptMostrado = false;
    }

    private bool SeHaPulsadoInteraccion()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            return true;
        }

        if (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private void BuscarJugador()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        jugador = playerObject.transform;

        if (inventarioJugador == null)
        {
            inventarioJugador = jugador.GetComponent<PlayerInventory>();
        }
    }

    private void ReproducirTrigger(string nombreParametro)
    {
        if (animatorMono == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return;
        }

        if (TieneParametroAnimator(nombreParametro, AnimatorControllerParameterType.Trigger))
        {
            animatorMono.SetTrigger(nombreParametro);
        }
    }

    private void CambiarBool(string nombreParametro, bool valor)
    {
        if (animatorMono == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return;
        }

        if (TieneParametroAnimator(nombreParametro, AnimatorControllerParameterType.Bool))
        {
            animatorMono.SetBool(nombreParametro, valor);
        }
    }

    private bool TieneParametroAnimator(string nombreParametro, AnimatorControllerParameterType tipo)
    {
        if (animatorMono == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parametro in animatorMono.parameters)
        {
            if (parametro.type == tipo && parametro.name == nombreParametro)
            {
                return true;
            }
        }

        return false;
    }
}
