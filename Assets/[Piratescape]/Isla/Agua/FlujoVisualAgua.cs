using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FlujoVisualAgua : MonoBehaviour
{
    [Header("Material")]
    [SerializeField] private Renderer rendererAgua;
    [SerializeField] private bool crearInstanciaMaterial = true;

    [Header("Movimiento UV")]
    [SerializeField] private bool moverTextura = true;
    [SerializeField] private Vector2 direccionUV = new Vector2(0f, -1f);
    [SerializeField] private float velocidadUV = 0.25f;
    [SerializeField] private string[] propiedadesTextura =
    {
        "_BaseMap",
        "_MainTex",
        "_Normal",
        "_Normal2",
        "_WaterNormal",
        "_TextureSample0",
        "_TextureSample1",
        "_Foam"
    };

    [Header("Shader Graph opcional")]
    [SerializeField] private bool actualizarPropiedadSpeed = true;
    [SerializeField] private string propiedadSpeed = "Speed";
    [SerializeField] private float valorSpeedShader = 0.18f;

    private Material materialInstanciado;
    private Vector2 offsetActual;

    private void Awake()
    {
        PrepararMaterial();
    }

    private void Reset()
    {
        rendererAgua = GetComponent<Renderer>();
    }

    private void OnValidate()
    {
        if (rendererAgua == null)
        {
            rendererAgua = GetComponent<Renderer>();
        }

        if (direccionUV.sqrMagnitude < 0.0001f)
        {
            direccionUV = new Vector2(0f, -1f);
        }
    }

    private void Update()
    {
        if (!moverTextura)
        {
            return;
        }

        PrepararMaterial();

        if (materialInstanciado == null)
        {
            return;
        }

        Vector2 direccionNormalizada = direccionUV.normalized;
        offsetActual += direccionNormalizada * velocidadUV * Time.deltaTime;
        offsetActual.x = Mathf.Repeat(offsetActual.x, 1f);
        offsetActual.y = Mathf.Repeat(offsetActual.y, 1f);

        AplicarOffset(offsetActual);

        if (actualizarPropiedadSpeed && materialInstanciado.HasProperty(propiedadSpeed))
        {
            materialInstanciado.SetFloat(propiedadSpeed, valorSpeedShader);
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

        if (materialInstanciado != null)
        {
            return;
        }

        materialInstanciado = crearInstanciaMaterial ? rendererAgua.material : rendererAgua.sharedMaterial;
    }

    private void AplicarOffset(Vector2 offset)
    {
        if (propiedadesTextura == null)
        {
            return;
        }

        for (int i = 0; i < propiedadesTextura.Length; i++)
        {
            string propiedad = propiedadesTextura[i];

            if (string.IsNullOrWhiteSpace(propiedad))
            {
                continue;
            }

            if (materialInstanciado.HasProperty(propiedad))
            {
                materialInstanciado.SetTextureOffset(propiedad, offset);
            }
        }
    }
}
