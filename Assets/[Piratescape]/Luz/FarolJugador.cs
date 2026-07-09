using System.Collections.Generic;
using UnityEngine;

public class FarolJugador : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerInventory inventarioJugador;
    [SerializeField] private Animator animatorJugador;
    [SerializeField] private Transform puntoManoFarol;
    [SerializeField] private Camera camaraPrincipal;

    [Header("Item requerido")]
    [SerializeField] private bool requerirFarolSeleccionado = true;
    [SerializeField] private ItemData itemFarol;
    [SerializeField] private string itemIdFarol = "farol_pirata";
    [SerializeField] private bool permitirCoincidenciaPorNombre = true;

    [Header("Prefab visual en la mano")]
    [SerializeField] private GameObject prefabFarol;
    [SerializeField] private bool crearInstanciaFarolAutomaticamente = true;
    [SerializeField] private bool mantenerInstanciaAunqueNoEsteSeleccionado = true;
    [SerializeField] private bool ocultarFarolAlDeseleccionar = true;
    [SerializeField] private bool usarRotacionOriginalPrefab = true;
    [SerializeField] private bool usarEscalaOriginalPrefab = true;
    [SerializeField] private Vector3 posicionLocalFarol = new Vector3(0.04f, 0.02f, 0.08f);
    [SerializeField] private Vector3 rotacionExtraLocalFarol = Vector3.zero;
    [SerializeField] private Vector3 escalaLocalFarol = Vector3.one;
    [SerializeField] private bool aplicarTransformLocalCadaFrame = true;

    [Header("Luz siempre encendida al equipar")]
    [SerializeField] private Light farolLight;
    [SerializeField] private bool buscarLightEnPrefab = true;
    [SerializeField] private bool usarSiempreLightDeLaInstancia = true;
    [SerializeField] private bool activarTodasLasLucesDelFarol = true;
    [SerializeField] private bool crearLightSiFalta = true;
    [SerializeField] private string nombreLightAuto = "Luz_Farol";
    [SerializeField] private Vector3 posicionLocalLight = new Vector3(0f, 0.06f, 0.12f);
    [SerializeField] private Vector3 rotacionLocalLight = new Vector3(8f, 0f, 0f);
    [SerializeField] private LightType tipoLuz = LightType.Spot;
    [SerializeField] private Color colorFarol = new Color(1f, 0.72f, 0.38f);
    [SerializeField] private float intensidadFarol = 2.8f;
    [SerializeField] private float rangoFarol = 12f;
    [SerializeField] private float anguloFarol = 62f;
    [SerializeField] private float sombrasFuerza = 0.45f;
    [SerializeField] private LightShadows tipoSombras = LightShadows.Soft;

    [Header("Direccion de la luz")]
    [SerializeField] private bool usarDireccionCamaraPrincipal = false;
    [SerializeField] private bool buscarCamaraPrincipalSiFalta = true;
    [SerializeField] private float suavizadoRotacionCamara = 14f;

    [Header("Animacion jugador")]
    [SerializeField] private bool usarAnimacionFarol = true;
    [SerializeField] private string boolTieneFarol = "TieneFarol";
    [SerializeField] private string triggerEquiparFarol = "EquiparFarol";
    [SerializeField] private string triggerGuardarFarol = "GuardarFarol";

    [Header("Parpadeo suave")]
    [SerializeField] private bool usarParpadeoSuave = true;
    [SerializeField] private float fuerzaParpadeo = 0.12f;
    [SerializeField] private float velocidadParpadeo = 5f;

    [Header("Audio opcional")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoEquipar;
    [SerializeField] private AudioClip sonidoGuardar;
    [Range(0f, 1f)]
    [SerializeField] private float volumenAudio = 0.75f;

    [Header("Debug")]
    [SerializeField] private bool farolSeleccionado;
    [SerializeField] private bool mostrarLogs;

    private GameObject instanciaFarol;
    private readonly List<Light> lucesFarol = new List<Light>();
    private float intensidadBase;
    private float semillaParpadeo;
    private bool estadoSeleccionAnterior;

    public bool FarolSeleccionado => farolSeleccionado;

    // Se conserva por compatibilidad con scripts que pudieran consultarlo.
    // En esta versión el farol está encendido siempre que está seleccionado.
    public bool FarolEncendido => farolSeleccionado;

    private void Awake()
    {
        CachearReferencias();
        PrepararAudio();
        PrepararPuntoManoSiFalta();
        PrepararFarolVisualSiHaceFalta();

        semillaParpadeo = Random.Range(0f, 1000f);
        intensidadBase = intensidadFarol;

        ActualizarEstadoSeleccion(true);
    }

    private void Update()
    {
        ActualizarEstadoSeleccion(false);
        ActualizarDireccionFarol();
        ActualizarParpadeo();
    }

    private void LateUpdate()
    {
        if (aplicarTransformLocalCadaFrame && instanciaFarol != null && instanciaFarol.activeSelf)
        {
            AplicarTransformLocalFarol();
        }
    }

    public void ForzarActualizarFarol()
    {
        ActualizarEstadoSeleccion(true);
    }

    private void CachearReferencias()
    {
        if (inventarioJugador == null)
        {
            inventarioJugador = GetComponent<PlayerInventory>();
        }

        if (inventarioJugador == null)
        {
            inventarioJugador = GetComponentInChildren<PlayerInventory>(true);
        }

        if (animatorJugador == null)
        {
            animatorJugador = GetComponentInChildren<Animator>(true);
        }

        if (camaraPrincipal == null && buscarCamaraPrincipalSiFalta)
        {
            camaraPrincipal = Camera.main;
        }
    }

    private void PrepararAudio()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
        }
    }

    private void PrepararPuntoManoSiFalta()
    {
        if (puntoManoFarol != null)
        {
            return;
        }

        GameObject punto = new GameObject("PuntoManoFarol_Auto");
        punto.transform.SetParent(transform, false);
        punto.transform.localPosition = new Vector3(0.35f, 1.15f, 0.45f);
        punto.transform.localRotation = Quaternion.identity;
        puntoManoFarol = punto.transform;
    }

    private void PrepararFarolVisualSiHaceFalta()
    {
        if (!crearInstanciaFarolAutomaticamente)
        {
            if (farolLight != null)
            {
                ConfigurarLight(farolLight);
                ActivarLuz(false);
            }
            return;
        }

        if (instanciaFarol != null)
        {
            PrepararLightDesdeInstancia();
            return;
        }

        if (prefabFarol != null)
        {
            instanciaFarol = Instantiate(prefabFarol, puntoManoFarol);
            instanciaFarol.name = prefabFarol.name + "_EnMano";
            AplicarTransformLocalFarol();
            PrepararLightDesdeInstancia();
            instanciaFarol.SetActive(false);
            return;
        }

        GameObject farolAuto = new GameObject("Farol_Pirata_Visual_Auto");
        farolAuto.transform.SetParent(puntoManoFarol, false);
        instanciaFarol = farolAuto;
        AplicarTransformLocalFarol();
        PrepararLightDesdeInstancia();
        instanciaFarol.SetActive(false);
    }

    private void PrepararLightDesdeInstancia()
    {
        lucesFarol.Clear();

        if (instanciaFarol == null)
        {
            if (farolLight != null)
            {
                lucesFarol.Add(farolLight);
                ConfigurarLight(farolLight);
                ActivarLuz(false);
            }

            return;
        }

        Light[] lucesEnInstancia = instanciaFarol.GetComponentsInChildren<Light>(true);
        Light luzPrincipalInstancia = lucesEnInstancia != null && lucesEnInstancia.Length > 0 ? lucesEnInstancia[0] : null;

        // IMPORTANTE:
        // Si en el Inspector se arrastra la Light del prefab original, esa Light no ilumina la escena.
        // Por defecto preferimos la Light que pertenece a la instancia real que está en la mano.
        bool lightAsignadaNoPerteneceAInstancia = farolLight != null &&
                                                  farolLight.transform != instanciaFarol.transform &&
                                                  !farolLight.transform.IsChildOf(instanciaFarol.transform);

        if (usarSiempreLightDeLaInstancia && luzPrincipalInstancia != null)
        {
            farolLight = luzPrincipalInstancia;
        }
        else if (farolLight == null && buscarLightEnPrefab && luzPrincipalInstancia != null)
        {
            farolLight = luzPrincipalInstancia;
        }
        else if (lightAsignadaNoPerteneceAInstancia && buscarLightEnPrefab && luzPrincipalInstancia != null)
        {
            farolLight = luzPrincipalInstancia;
        }

        if (farolLight == null && crearLightSiFalta)
        {
            GameObject luzGO = new GameObject(nombreLightAuto);
            luzGO.transform.SetParent(instanciaFarol.transform, false);
            luzGO.transform.localPosition = posicionLocalLight;
            luzGO.transform.localRotation = Quaternion.Euler(rotacionLocalLight);
            farolLight = luzGO.AddComponent<Light>();
        }

        if (activarTodasLasLucesDelFarol)
        {
            lucesEnInstancia = instanciaFarol.GetComponentsInChildren<Light>(true);

            foreach (Light luz in lucesEnInstancia)
            {
                if (luz == null || lucesFarol.Contains(luz))
                {
                    continue;
                }

                lucesFarol.Add(luz);
            }
        }

        if (farolLight != null && !lucesFarol.Contains(farolLight))
        {
            lucesFarol.Insert(0, farolLight);
        }

        if (lucesFarol.Count == 0 && farolLight != null)
        {
            lucesFarol.Add(farolLight);
        }

        foreach (Light luz in lucesFarol)
        {
            ConfigurarLight(luz);
        }

        ActivarLuz(false);
    }

    private void AplicarTransformLocalFarol()
    {
        if (instanciaFarol == null)
        {
            return;
        }

        Transform t = instanciaFarol.transform;
        t.localPosition = posicionLocalFarol;

        Quaternion rotacion = Quaternion.identity;

        if (usarRotacionOriginalPrefab && prefabFarol != null)
        {
            rotacion = prefabFarol.transform.localRotation;
        }

        rotacion *= Quaternion.Euler(rotacionExtraLocalFarol);
        t.localRotation = rotacion;

        if (!usarEscalaOriginalPrefab || prefabFarol == null)
        {
            t.localScale = escalaLocalFarol;
        }
        else
        {
            t.localScale = Vector3.Scale(prefabFarol.transform.localScale, escalaLocalFarol);
        }
    }

    private void ConfigurarLight(Light luz)
    {
        if (luz == null)
        {
            return;
        }

        luz.type = tipoLuz;
        luz.color = colorFarol;
        luz.intensity = intensidadFarol;
        luz.range = rangoFarol;
        luz.spotAngle = anguloFarol;
        luz.shadows = tipoSombras;
        luz.shadowStrength = sombrasFuerza;
    }

    private void ActualizarEstadoSeleccion(bool forzar)
    {
        farolSeleccionado = !requerirFarolSeleccionado || EstaSeleccionadoElFarol();

        if (!forzar && farolSeleccionado == estadoSeleccionAnterior)
        {
            return;
        }

        estadoSeleccionAnterior = farolSeleccionado;

        if (farolSeleccionado)
        {
            AlSeleccionarFarol();
        }
        else
        {
            AlDeseleccionarFarol();
        }
    }

    private bool EstaSeleccionadoElFarol()
    {
        if (inventarioJugador == null)
        {
            return false;
        }

        InventorySlot slot = inventarioJugador.GetSlot(inventarioJugador.SelectedSlotIndex);

        if (slot == null || slot.IsEmpty() || slot.itemData == null)
        {
            return false;
        }

        return EsItemFarol(slot.itemData);
    }

    private bool EsItemFarol(ItemData item)
    {
        if (item == null)
        {
            return false;
        }

        if (itemFarol != null && item == itemFarol)
        {
            return true;
        }

        string id = item.ItemId != null ? item.ItemId.ToLowerInvariant() : string.Empty;
        string idBuscado = itemIdFarol != null ? itemIdFarol.ToLowerInvariant() : "farol_pirata";

        if (!string.IsNullOrWhiteSpace(idBuscado) && id == idBuscado)
        {
            return true;
        }

        if (!permitirCoincidenciaPorNombre)
        {
            return false;
        }

        string nombreAsset = item.name != null ? item.name.ToLowerInvariant() : string.Empty;
        string nombreMostrar = item.DisplayName != null ? item.DisplayName.ToLowerInvariant() : string.Empty;

        return nombreAsset.Contains("farol") ||
               nombreMostrar.Contains("farol") ||
               nombreAsset.Contains("linterna") ||
               nombreMostrar.Contains("linterna") ||
               nombreAsset.Contains("lantern") ||
               nombreMostrar.Contains("lantern");
    }

    private void AlSeleccionarFarol()
    {
        if (instanciaFarol == null)
        {
            PrepararFarolVisualSiHaceFalta();
        }

        if (instanciaFarol != null)
        {
            instanciaFarol.SetActive(true);
            AplicarTransformLocalFarol();
        }

        ActivarLuz(true);
        CambiarBoolAnimator(boolTieneFarol, true);
        ReproducirTriggerAnimator(triggerEquiparFarol);
        ReproducirAudio(sonidoEquipar);

        if (mostrarLogs)
        {
            Debug.Log("[FarolJugador] Farol seleccionado: visual activo, luz activa y animacion de farol activada.", this);
        }
    }

    private void AlDeseleccionarFarol()
    {
        ActivarLuz(false);

        if (instanciaFarol != null && ocultarFarolAlDeseleccionar)
        {
            if (mantenerInstanciaAunqueNoEsteSeleccionado)
            {
                instanciaFarol.SetActive(false);
            }
            else
            {
                Destroy(instanciaFarol);
                instanciaFarol = null;
                farolLight = null;
            }
        }

        CambiarBoolAnimator(boolTieneFarol, false);
        ReproducirTriggerAnimator(triggerGuardarFarol);
        ReproducirAudio(sonidoGuardar);

        if (mostrarLogs)
        {
            Debug.Log("[FarolJugador] Farol deseleccionado: visual oculto, luz apagada y animacion de farol desactivada.", this);
        }
    }

    private void ActivarLuz(bool activa)
    {
        if ((lucesFarol == null || lucesFarol.Count == 0) && farolLight == null)
        {
            return;
        }

        if (farolLight != null && (lucesFarol == null || !lucesFarol.Contains(farolLight)))
        {
            lucesFarol.Add(farolLight);
        }

        for (int i = lucesFarol.Count - 1; i >= 0; i--)
        {
            Light luz = lucesFarol[i];

            if (luz == null)
            {
                lucesFarol.RemoveAt(i);
                continue;
            }

            luz.enabled = activa;
            luz.intensity = activa ? intensidadBase : 0f;
        }
    }

    private void ActualizarDireccionFarol()
    {
        if (!farolSeleccionado || !usarDireccionCamaraPrincipal || farolLight == null)
        {
            return;
        }

        if (camaraPrincipal == null && buscarCamaraPrincipalSiFalta)
        {
            camaraPrincipal = Camera.main;
        }

        if (camaraPrincipal == null)
        {
            return;
        }

        Quaternion rotacionObjetivo = Quaternion.LookRotation(camaraPrincipal.transform.forward, Vector3.up);

        foreach (Light luz in lucesFarol)
        {
            if (luz == null)
            {
                continue;
            }

            luz.transform.rotation = Quaternion.Slerp(luz.transform.rotation, rotacionObjetivo, Time.deltaTime * suavizadoRotacionCamara);
        }
    }

    private void ActualizarParpadeo()
    {
        if (!usarParpadeoSuave || !farolSeleccionado || lucesFarol == null || lucesFarol.Count == 0)
        {
            return;
        }

        float ruido = Mathf.PerlinNoise(semillaParpadeo, Time.time * velocidadParpadeo);
        float factor = 1f + (ruido - 0.5f) * fuerzaParpadeo;

        foreach (Light luz in lucesFarol)
        {
            if (luz == null || !luz.enabled)
            {
                continue;
            }

            luz.intensity = intensidadBase * factor;
        }
    }

    private void CambiarBoolAnimator(string nombreParametro, bool valor)
    {
        if (!usarAnimacionFarol || animatorJugador == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return;
        }

        if (TieneParametroAnimator(nombreParametro, AnimatorControllerParameterType.Bool))
        {
            animatorJugador.SetBool(nombreParametro, valor);
        }
    }

    private void ReproducirTriggerAnimator(string nombreParametro)
    {
        if (!usarAnimacionFarol || animatorJugador == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return;
        }

        if (TieneParametroAnimator(nombreParametro, AnimatorControllerParameterType.Trigger))
        {
            animatorJugador.SetTrigger(nombreParametro);
        }
    }

    private bool TieneParametroAnimator(string nombreParametro, AnimatorControllerParameterType tipo)
    {
        if (animatorJugador == null || string.IsNullOrWhiteSpace(nombreParametro))
        {
            return false;
        }

        foreach (AnimatorControllerParameter parametro in animatorJugador.parameters)
        {
            if (parametro.type == tipo && parametro.name == nombreParametro)
            {
                return true;
            }
        }

        return false;
    }

    private void ReproducirAudio(AudioClip clip)
    {
        if (audioSource == null || clip == null)
        {
            return;
        }

        audioSource.PlayOneShot(clip, volumenAudio);
    }

    [ContextMenu("Debug/Rebuscar luz del farol en la instancia")]
    private void DebugRebuscarLuzInstancia()
    {
        PrepararLightDesdeInstancia();
        ActivarLuz(farolSeleccionado);

        if (mostrarLogs)
        {
            Debug.Log($"[FarolJugador] Luces encontradas en farol: {lucesFarol.Count}. Light principal: {(farolLight != null ? farolLight.name : "ninguna")}", this);
        }
    }

    [ContextMenu("Debug/Probar luz del farol ahora")]
    private void DebugProbarLuzFarolAhora()
    {
        if (instanciaFarol == null)
        {
            PrepararFarolVisualSiHaceFalta();
        }

        if (instanciaFarol != null)
        {
            instanciaFarol.SetActive(true);
        }

        PrepararLightDesdeInstancia();
        ConfigurarLight(farolLight);
        ActivarLuz(true);
    }

    private void OnValidate()
    {
        intensidadFarol = Mathf.Max(0f, intensidadFarol);
        rangoFarol = Mathf.Max(0f, rangoFarol);
        anguloFarol = Mathf.Clamp(anguloFarol, 1f, 179f);
        sombrasFuerza = Mathf.Clamp01(sombrasFuerza);
        fuerzaParpadeo = Mathf.Clamp01(fuerzaParpadeo);
        velocidadParpadeo = Mathf.Max(0f, velocidadParpadeo);

        intensidadBase = intensidadFarol;

        if (farolLight != null)
        {
            ConfigurarLight(farolLight);
            farolLight.enabled = farolSeleccionado;
            farolLight.intensity = farolSeleccionado ? intensidadBase : 0f;
        }

        if (lucesFarol != null)
        {
            foreach (Light luz in lucesFarol)
            {
                if (luz == null)
                {
                    continue;
                }

                ConfigurarLight(luz);
                luz.enabled = farolSeleccionado;
                luz.intensity = farolSeleccionado ? intensidadBase : 0f;
            }
        }
    }
}
