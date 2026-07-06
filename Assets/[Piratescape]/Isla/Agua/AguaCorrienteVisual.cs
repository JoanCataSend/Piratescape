using UnityEngine;

[DisallowMultipleComponent]
public class AguaCorrienteVisual : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Renderer rendererAgua;

    [Header("Material")]
    [SerializeField] private bool crearInstanciaMaterialEnRuntime = true;
    [SerializeField] private bool aplicarColorTransparenteEnRuntime = false;
    [SerializeField] private Color colorAgua = new Color(0.25f, 0.75f, 1f, 0.45f);

    [Header("Movimiento textura base")]
    [SerializeField] private bool animarTexturaBase = true;
    [SerializeField] private string propiedadTexturaBaseURP = "_BaseMap";
    [SerializeField] private string propiedadTexturaBaseLegacy = "_MainTex";
    [SerializeField] private Vector2 velocidadTexturaBase = new Vector2(0f, -0.45f);

    [Header("Movimiento normal map opcional")]
    [SerializeField] private bool animarNormalMap = true;
    [SerializeField] private string propiedadNormalMap = "_BumpMap";
    [SerializeField] private Vector2 velocidadNormalMap = new Vector2(0.08f, -0.28f);

    [Header("Movimiento espuma opcional")]
    [SerializeField] private bool animarTexturaEspuma = true;
    [SerializeField] private string propiedadTexturaEspuma = "_FoamTex";
    [SerializeField] private Vector2 velocidadTexturaEspuma = new Vector2(0f, -0.75f);

    [Header("Ajustes")]
    [SerializeField] private bool usarUnscaledTime = false;
    [SerializeField] private bool reiniciarOffsetsAlActivar = false;

    private Material materialRuntime;
    private Vector2 offsetBaseURP;
    private Vector2 offsetBaseLegacy;
    private Vector2 offsetNormal;
    private Vector2 offsetEspuma;

    private void Awake()
    {
        PrepararMaterial();
    }

    private void OnEnable()
    {
        PrepararMaterial();

        if (reiniciarOffsetsAlActivar)
        {
            offsetBaseURP = Vector2.zero;
            offsetBaseLegacy = Vector2.zero;
            offsetNormal = Vector2.zero;
            offsetEspuma = Vector2.zero;
        }
    }

    private void Reset()
    {
        rendererAgua = GetComponent<Renderer>();
    }

    private void Update()
    {
        if (materialRuntime == null)
        {
            PrepararMaterial();
        }

        if (materialRuntime == null)
        {
            return;
        }

        float delta = usarUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

        if (animarTexturaBase)
        {
            AnimarOffset(propiedadTexturaBaseURP, velocidadTexturaBase, delta, ref offsetBaseURP);
            AnimarOffset(propiedadTexturaBaseLegacy, velocidadTexturaBase, delta, ref offsetBaseLegacy);
        }

        if (animarNormalMap)
        {
            AnimarOffset(propiedadNormalMap, velocidadNormalMap, delta, ref offsetNormal);
        }

        if (animarTexturaEspuma)
        {
            AnimarOffset(propiedadTexturaEspuma, velocidadTexturaEspuma, delta, ref offsetEspuma);
        }
    }

    private void PrepararMaterial()
    {
        if (rendererAgua == null)
        {
            rendererAgua = GetComponent<Renderer>();
        }

        if (rendererAgua == null)
        {
            return;
        }

        if (Application.isPlaying && crearInstanciaMaterialEnRuntime)
        {
            materialRuntime = rendererAgua.material;
        }
        else
        {
            materialRuntime = rendererAgua.sharedMaterial;
        }

        if (materialRuntime == null)
        {
            return;
        }

        if (aplicarColorTransparenteEnRuntime)
        {
            AplicarColor(materialRuntime, colorAgua);
        }
    }

    private void AnimarOffset(string propiedad, Vector2 velocidad, float delta, ref Vector2 offset)
    {
        if (string.IsNullOrWhiteSpace(propiedad) || !materialRuntime.HasProperty(propiedad))
        {
            return;
        }

        offset += velocidad * delta;
        offset.x = Mathf.Repeat(offset.x, 1f);
        offset.y = Mathf.Repeat(offset.y, 1f);
        materialRuntime.SetTextureOffset(propiedad, offset);
    }

    private void AplicarColor(Material material, Color color)
    {
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }
}
