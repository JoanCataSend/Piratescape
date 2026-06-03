using UnityEngine;

public class RecogibleCircleHighlight : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform player;
    [SerializeField] private SpriteRenderer circleRenderer;

    [Header("Deteccion")]
    [SerializeField] private float distanciaParaMostrar = 2.2f;

    [Header("Animacion")]
    [SerializeField] private float escalaBase = 0.6f;
    [SerializeField] private float intensidadPulso = 0.06f;
    [SerializeField] private float velocidadPulso = 3.5f;

    [Header("Visual")]
    [SerializeField, Range(0f, 1f)] private float alphaMaximo = 0.5f;
    [SerializeField] private float velocidadAparicion = 8f;

    private Vector3 escalaInicial;
    private float alphaActual;

    private void Awake()
    {
        if (circleRenderer != null)
        {
            circleRenderer.enabled = false;
            escalaInicial = circleRenderer.transform.localScale;
        }
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void Update()
    {
        if (player == null || circleRenderer == null)
        {
            return;
        }

        float distancia = Vector3.Distance(transform.position, player.position);
        bool debeMostrarse = distancia <= distanciaParaMostrar;

        ActualizarCirculo(debeMostrarse);
    }

    private void ActualizarCirculo(bool mostrar)
    {
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
}