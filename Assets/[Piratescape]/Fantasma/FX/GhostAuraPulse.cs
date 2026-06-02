using UnityEngine;

public sealed class GhostAuraPulse : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform visualAura;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Escala")]
    [SerializeField] private float escalaMin = 1.15f;
    [SerializeField] private float escalaMax = 1.45f;

    [Header("Color / alpha")]
    [SerializeField] private Color colorMin = new Color(0.78f, 0.93f, 1f, 0.22f);
    [SerializeField] private Color colorMax = new Color(0.92f, 0.98f, 1f, 0.48f);

    [Header("Velocidad")]
    [SerializeField] private float velocidadPulso = 2f;

    private Vector3 escalaBase;

    private void Awake()
    {
        if (visualAura == null)
        {
            visualAura = transform;
        }

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        escalaBase = Vector3.one;
    }

    private void OnEnable()
    {
        ActualizarAura(0f);
    }

    private void Update()
    {
        float t = (Mathf.Sin(Time.time * velocidadPulso) + 1f) * 0.5f;
        ActualizarAura(t);
    }

    private void ActualizarAura(float t)
    {
        float escala = Mathf.Lerp(escalaMin, escalaMax, t);
        visualAura.localScale = escalaBase * escala;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(colorMin, colorMax, t);
        }
    }
}