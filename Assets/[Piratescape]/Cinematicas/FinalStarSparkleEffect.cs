using System.Collections;
using UnityEngine;

public class FinalStarSparkleEffect : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Animación")]
    [SerializeField] private float duracion = 0.8f;
    [SerializeField] private float escalaInicial = 0.1f;
    [SerializeField] private float escalaMaxima = 1.4f;
    [SerializeField] private float escalaFinal = 0.05f;
    [SerializeField] private float velocidadRotacion = 180f;

    [Header("Color")]
    [SerializeField] private Color colorInicial = Color.white;
    [SerializeField] private Color colorFinal = new Color(1f, 1f, 1f, 0f);

    [Header("Cámara")]
    [SerializeField] private bool mirarSiempreACamara = true;

    private Vector3 escalaBase;
    private Camera camaraActual;
    private Coroutine rutinaActual;

    private void Awake()
    {
        escalaBase = transform.localScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        BuscarCamaraActiva();

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        rutinaActual = StartCoroutine(AnimacionDestello());
    }

    private void LateUpdate()
    {
        if (!mirarSiempreACamara)
        {
            return;
        }

        if (camaraActual == null || !camaraActual.gameObject.activeInHierarchy)
        {
            BuscarCamaraActiva();
        }

        if (camaraActual == null)
        {
            return;
        }

        transform.forward = camaraActual.transform.forward;
    }

    private void BuscarCamaraActiva()
    {
        Camera[] camaras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

        for (int i = 0; i < camaras.Length; i++)
        {
            if (camaras[i] != null && camaras[i].gameObject.activeInHierarchy && camaras[i].enabled)
            {
                camaraActual = camaras[i];
                return;
            }
        }

        camaraActual = Camera.main;
    }

    private IEnumerator AnimacionDestello()
    {
        float tiempo = 0f;

        transform.localScale = escalaBase * escalaInicial;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorInicial;
        }

        while (tiempo < duracion)
        {
            tiempo += Time.deltaTime;

            float t = Mathf.Clamp01(tiempo / duracion);

            float escala;

            if (t < 0.45f)
            {
                float tEntrada = t / 0.45f;
                escala = Mathf.Lerp(
                    escalaInicial,
                    escalaMaxima,
                    Mathf.SmoothStep(0f, 1f, tEntrada)
                );
            }
            else
            {
                float tSalida = (t - 0.45f) / 0.55f;
                escala = Mathf.Lerp(
                    escalaMaxima,
                    escalaFinal,
                    Mathf.SmoothStep(0f, 1f, tSalida)
                );
            }

            transform.localScale = escalaBase * escala;
            transform.Rotate(Vector3.forward, velocidadRotacion * Time.deltaTime);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(
                    colorInicial,
                    colorFinal,
                    Mathf.SmoothStep(0f, 1f, t)
                );
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}