using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RecogibleCircleHighlight : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer circleRenderer;

    [Header("Outline blanco")]
    [SerializeField] private bool crearOutlineAutomatico = true;
    [SerializeField] private Material materialOutline;
    [SerializeField] private GameObject[] objetosOutline;

    [Header("Deteccion")]
    [SerializeField] private float distanciaParaMostrar = 2.2f;

    [Header("Animacion circulo")]
    [SerializeField] private float escalaBase = 1f;
    [SerializeField] private float intensidadPulso = 0.06f;
    [SerializeField] private float velocidadPulso = 3.5f;

    [Header("Visual circulo")]
    [SerializeField, Range(0f, 1f)] private float alphaMaximo = 0.5f;
    [SerializeField] private float velocidadAparicion = 8f;

    [Header("Mano / Inventario")]
    [SerializeField] private bool ocultarSiEstaEnLaMano = true;

    private readonly List<GameObject> outlinesGenerados = new List<GameObject>();

    private Vector3 escalaInicial;
    private float alphaActual;
    private bool outlineActivo;

    private void Awake()
    {
        if (circleRenderer != null)
        {
            circleRenderer.enabled = false;
            escalaInicial = circleRenderer.transform.localScale;
        }

        if (crearOutlineAutomatico)
        {
            CrearOutlinesAutomaticos();
        }

        ActivarOutline(false);
    }

    private void Start()
    {
        BuscarPlayerSiHaceFalta();
    }

    private void Update()
    {
        BuscarPlayerSiHaceFalta();

        if (player == null)
        {
            ApagarTodo();
            return;
        }

        if (ocultarSiEstaEnLaMano && transform.IsChildOf(player))
        {
            ApagarTodo();
            return;
        }

        float distancia = Vector3.Distance(transform.position, player.position);
        bool debeMostrarse = distancia <= distanciaParaMostrar;

        ActualizarCirculo(debeMostrarse);
        ActivarOutline(debeMostrarse);
    }

    private void BuscarPlayerSiHaceFalta()
    {
        if (player != null)
        {
            return;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void CrearOutlinesAutomaticos()
    {
        if (materialOutline == null)
        {
            return;
        }

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            MeshRenderer rendererOriginal = renderers[i];

            if (rendererOriginal == null)
            {
                continue;
            }

            if (rendererOriginal.gameObject.name.Contains("Outline"))
            {
                continue;
            }

            MeshFilter meshFilterOriginal = rendererOriginal.GetComponent<MeshFilter>();

            if (meshFilterOriginal == null || meshFilterOriginal.sharedMesh == null)
            {
                continue;
            }

            CrearOutlineParaRenderer(rendererOriginal, meshFilterOriginal);
        }
    }

    private void CrearOutlineParaRenderer(MeshRenderer rendererOriginal, MeshFilter meshFilterOriginal)
    {
        GameObject outlineObj = new GameObject(rendererOriginal.gameObject.name + "_OutlineAuto");

        outlineObj.layer = rendererOriginal.gameObject.layer;

        if (rendererOriginal.transform == transform)
        {
            outlineObj.transform.SetParent(transform);
            outlineObj.transform.localPosition = Vector3.zero;
            outlineObj.transform.localRotation = Quaternion.identity;
            outlineObj.transform.localScale = Vector3.one;
        }
        else
        {
            outlineObj.transform.SetParent(rendererOriginal.transform.parent);
            outlineObj.transform.localPosition = rendererOriginal.transform.localPosition;
            outlineObj.transform.localRotation = rendererOriginal.transform.localRotation;
            outlineObj.transform.localScale = rendererOriginal.transform.localScale;
        }

        MeshFilter outlineMeshFilter = outlineObj.AddComponent<MeshFilter>();
        outlineMeshFilter.sharedMesh = meshFilterOriginal.sharedMesh;

        MeshRenderer outlineRenderer = outlineObj.AddComponent<MeshRenderer>();

        int cantidadMateriales = Mathf.Max(1, rendererOriginal.sharedMaterials.Length);
        Material[] materiales = new Material[cantidadMateriales];

        for (int i = 0; i < materiales.Length; i++)
        {
            materiales[i] = materialOutline;
        }

        outlineRenderer.sharedMaterials = materiales;
        outlineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        outlineRenderer.receiveShadows = false;
        outlineRenderer.enabled = true;

        outlineObj.SetActive(false);
        outlinesGenerados.Add(outlineObj);
    }

    private void ActualizarCirculo(bool mostrar)
    {
        if (circleRenderer == null)
        {
            return;
        }

        if (!circleRenderer.enabled && mostrar)
        {
            circleRenderer.enabled = true;
        }

        float alphaObjetivo = mostrar ? alphaMaximo : 0f;
        alphaActual = Mathf.Lerp(alphaActual, alphaObjetivo, Time.deltaTime * velocidadAparicion);

        Color color = circleRenderer.color;
        color.a = alphaActual;
        circleRenderer.color = color;

        float pulso = Mathf.Sin(Time.time * velocidadPulso) * intensidadPulso;
        float escalaFinal = escalaBase + pulso;

        circleRenderer.transform.localScale = escalaInicial * escalaFinal;

        if (!mostrar && alphaActual <= 0.02f)
        {
            circleRenderer.enabled = false;
        }
    }

    private void ActivarOutline(bool activar)
    {
        if (outlineActivo == activar)
        {
            return;
        }

        outlineActivo = activar;

        for (int i = 0; i < objetosOutline.Length; i++)
        {
            if (objetosOutline[i] != null)
            {
                objetosOutline[i].SetActive(activar);
            }
        }

        for (int i = 0; i < outlinesGenerados.Count; i++)
        {
            if (outlinesGenerados[i] != null)
            {
                outlinesGenerados[i].SetActive(activar);
            }
        }
    }

    private void ApagarTodo()
    {
        alphaActual = 0f;

        if (circleRenderer != null)
        {
            Color color = circleRenderer.color;
            color.a = 0f;
            circleRenderer.color = color;
            circleRenderer.enabled = false;
        }

        ActivarOutline(false);
    }
}