using System.Collections;
using UnityEngine;

public sealed class AnzueloPesca : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private Transform visualAnzuelo;
    [SerializeField] private float amplitudFlotacion = 0.08f;
    [SerializeField] private float velocidadFlotacion = 2.2f;
    [SerializeField] private float suavizadoPosicion = 12f;

    [Header("Hundimiento al picar")]
    [SerializeField] private float profundidadHundimiento = 0.45f;
    [SerializeField] private float duracionHundimiento = 0.18f;
    [SerializeField] private AnimationCurve curvaHundimiento = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Particulas")]
    [SerializeField] private ParticleSystem particulasFlotando;
    [SerializeField] private ParticleSystem particulasPicada;

    [Header("Linea de caña opcional")]
    [SerializeField] private LineRenderer lineaCana;
    [SerializeField] private Transform puntoSalidaCana;
    [SerializeField] private bool actualizarLinea = true;

    private Vector3 posicionBaseAgua;
    private bool estaFlotando;
    private bool estaHundido;
    private Coroutine rutinaHundimiento;

    public bool EstaHundido => estaHundido;

    private void Awake()
    {
        if (visualAnzuelo == null)
        {
            visualAnzuelo = transform;
        }

        if (lineaCana == null)
        {
            lineaCana = GetComponent<LineRenderer>();
        }
    }

    private void Update()
    {
        ActualizarFlotacion();
        ActualizarLineaCana();
    }

    public void PrepararParaLanzamiento(Vector3 posicionInicial, Transform nuevoPuntoSalidaCana)
    {
        puntoSalidaCana = nuevoPuntoSalidaCana;
        transform.position = posicionInicial;
        posicionBaseAgua = posicionInicial;
        estaFlotando = false;
        estaHundido = false;

        if (rutinaHundimiento != null)
        {
            StopCoroutine(rutinaHundimiento);
            rutinaHundimiento = null;
        }

        if (particulasFlotando != null)
        {
            particulasFlotando.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (particulasPicada != null)
        {
            particulasPicada.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        PrepararLineaCana();
    }

    public void Inicializar(Vector3 posicionAgua, Transform nuevoPuntoSalidaCana)
    {
        posicionBaseAgua = posicionAgua;
        puntoSalidaCana = nuevoPuntoSalidaCana;
        transform.position = posicionBaseAgua;
        estaFlotando = true;
        estaHundido = false;

        if (rutinaHundimiento != null)
        {
            StopCoroutine(rutinaHundimiento);
            rutinaHundimiento = null;
        }

        if (particulasFlotando != null)
        {
            particulasFlotando.Play(true);
        }

        if (particulasPicada != null)
        {
            particulasPicada.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        PrepararLineaCana();
    }

    public void ActivarPicada()
    {
        if (estaHundido)
        {
            return;
        }

        if (particulasPicada != null)
        {
            particulasPicada.Play(true);
        }

        if (rutinaHundimiento != null)
        {
            StopCoroutine(rutinaHundimiento);
        }

        rutinaHundimiento = StartCoroutine(HundirRoutine());
    }

    public void DetenerVisuales()
    {
        DetenerVisuales(false);
    }

    public void DetenerVisuales(bool mantenerLineaCana)
    {
        estaFlotando = false;

        if (rutinaHundimiento != null)
        {
            StopCoroutine(rutinaHundimiento);
            rutinaHundimiento = null;
        }

        if (particulasFlotando != null)
        {
            particulasFlotando.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (particulasPicada != null)
        {
            particulasPicada.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        if (!mantenerLineaCana && lineaCana != null)
        {
            lineaCana.enabled = false;
        }
    }

    public void ActivarLineaCana(bool activa)
    {
        if (lineaCana == null)
        {
            return;
        }

        lineaCana.enabled = activa && puntoSalidaCana != null;
        ActualizarLineaCana();
    }

    private void ActualizarFlotacion()
    {
        if (!estaFlotando || estaHundido)
        {
            return;
        }

        float offsetY = Mathf.Sin(Time.time * velocidadFlotacion) * amplitudFlotacion;
        Vector3 posicionObjetivo = posicionBaseAgua + Vector3.up * offsetY;

        transform.position = Vector3.Lerp(
            transform.position,
            posicionObjetivo,
            Time.deltaTime * suavizadoPosicion
        );
    }

    private IEnumerator HundirRoutine()
    {
        estaHundido = true;
        estaFlotando = false;

        if (particulasFlotando != null)
        {
            particulasFlotando.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        Vector3 inicio = transform.position;
        Vector3 destino = posicionBaseAgua + Vector3.down * profundidadHundimiento;
        float tiempo = 0f;

        while (tiempo < duracionHundimiento)
        {
            tiempo += Time.deltaTime;
            float t = duracionHundimiento > 0f ? tiempo / duracionHundimiento : 1f;
            float evaluado = curvaHundimiento != null ? curvaHundimiento.Evaluate(t) : t;

            transform.position = Vector3.Lerp(inicio, destino, evaluado);
            yield return null;
        }

        transform.position = destino;
        rutinaHundimiento = null;
    }

    private void PrepararLineaCana()
    {
        if (lineaCana == null)
        {
            return;
        }

        lineaCana.positionCount = 2;
        lineaCana.enabled = puntoSalidaCana != null;
        ActualizarLineaCana();
    }

    private void ActualizarLineaCana()
    {
        if (!actualizarLinea || lineaCana == null || !lineaCana.enabled || puntoSalidaCana == null)
        {
            return;
        }

        lineaCana.SetPosition(0, puntoSalidaCana.position);
        lineaCana.SetPosition(1, transform.position);
    }
}
