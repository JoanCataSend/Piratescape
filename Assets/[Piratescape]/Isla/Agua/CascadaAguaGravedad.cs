using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class CascadaAguaGravedad : MonoBehaviour
{
    [Header("Configuracion automatica")]
    [SerializeField] private bool configurarAutomaticamente = true;
    [SerializeField] private bool aplicarCambiosEnEditor = true;

    [Header("Material")]
    [SerializeField] private Material materialParticulasAgua;

    [Header("Salida de agua")]
    [SerializeField] private float radioSalida = 0.25f;
    [SerializeField] private float cantidadAgua = 180f;
    [SerializeField] private float vidaParticulas = 1.6f;
    [SerializeField] private float velocidadInicialCaida = 4.5f;
    [SerializeField] private float gravedad = 1.4f;
    [SerializeField] private float tamanoParticula = 0.12f;
    [SerializeField] private float variacionTamano = 0.06f;
    [SerializeField] private Color colorAgua = new Color(0.55f, 0.9f, 1f, 0.72f);

    [Header("Aspecto de chorro")]
    [SerializeField] private float estiramientoParticulas = 3.5f;
    [SerializeField] private float anchuraChorro = 0.45f;
    [SerializeField] private float turbulencia = 0.45f;

    [Header("Colision y salpicaduras")]
    [SerializeField] private bool usarColision = true;
    [SerializeField] private LayerMask capasColision = ~0;
    [SerializeField] private ParticleSystem salpicaduraAlChocar;
    [SerializeField] private int maxSalpicadurasPorChoque = 4;
    [SerializeField] private int particulasPorSalpicadura = 8;
    [SerializeField] private float tiempoMinimoEntreSalpicaduras = 0.08f;

    private ParticleSystem sistemaParticulas;
    private ParticleSystemRenderer renderParticulas;
    private readonly List<ParticleCollisionEvent> eventosColision = new List<ParticleCollisionEvent>();
    private float ultimoTiempoSalpicadura = -999f;

    private void Awake()
    {
        ObtenerComponentes();

        if (configurarAutomaticamente)
        {
            ConfigurarSistema();
        }
    }

    private void Reset()
    {
        ObtenerComponentes();
        ConfigurarSistema();
    }

    private void OnValidate()
    {
        radioSalida = Mathf.Max(0.01f, radioSalida);
        cantidadAgua = Mathf.Max(0f, cantidadAgua);
        vidaParticulas = Mathf.Max(0.1f, vidaParticulas);
        velocidadInicialCaida = Mathf.Max(0f, velocidadInicialCaida);
        gravedad = Mathf.Max(0f, gravedad);
        tamanoParticula = Mathf.Max(0.01f, tamanoParticula);
        variacionTamano = Mathf.Max(0f, variacionTamano);
        estiramientoParticulas = Mathf.Max(0.1f, estiramientoParticulas);
        anchuraChorro = Mathf.Max(0f, anchuraChorro);
        turbulencia = Mathf.Max(0f, turbulencia);
        maxSalpicadurasPorChoque = Mathf.Max(0, maxSalpicadurasPorChoque);
        particulasPorSalpicadura = Mathf.Max(1, particulasPorSalpicadura);
        tiempoMinimoEntreSalpicaduras = Mathf.Max(0f, tiempoMinimoEntreSalpicaduras);

        if (!Application.isPlaying && aplicarCambiosEnEditor && configurarAutomaticamente)
        {
            ObtenerComponentes();
            ConfigurarSistema();
        }
    }

    private void ObtenerComponentes()
    {
        if (sistemaParticulas == null)
        {
            sistemaParticulas = GetComponent<ParticleSystem>();
        }

        if (renderParticulas == null)
        {
            renderParticulas = GetComponent<ParticleSystemRenderer>();
        }
    }

    [ContextMenu("Configurar Cascada")]
    public void ConfigurarSistema()
    {
        ObtenerComponentes();

        var main = sistemaParticulas.main;
        main.loop = true;
        main.playOnAwake = true;
        main.duration = 5f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.startLifetime = vidaParticulas;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(tamanoParticula, tamanoParticula + variacionTamano);
        main.startColor = colorAgua;
        main.gravityModifier = gravedad;
        main.maxParticles = Mathf.CeilToInt(cantidadAgua * vidaParticulas * 2f);

        var emission = sistemaParticulas.emission;
        emission.enabled = true;
        emission.rateOverTime = cantidadAgua;

        var shape = sistemaParticulas.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radioSalida;
        shape.radiusThickness = 1f;
        shape.arc = 360f;
        shape.randomDirectionAmount = anchuraChorro;

        var velocity = sistemaParticulas.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.x = new ParticleSystem.MinMaxCurve(-turbulencia, turbulencia);
        velocity.y = new ParticleSystem.MinMaxCurve(-velocidadInicialCaida * 1.15f, -velocidadInicialCaida * 0.85f);
        velocity.z = new ParticleSystem.MinMaxCurve(-turbulencia, turbulencia);

        var noise = sistemaParticulas.noise;
        noise.enabled = turbulencia > 0.01f;
        noise.strength = turbulencia;
        noise.frequency = 1.2f;
        noise.scrollSpeed = 0.5f;
        noise.damping = true;

        var collision = sistemaParticulas.collision;
        collision.enabled = usarColision;
        collision.type = ParticleSystemCollisionType.World;
        collision.mode = ParticleSystemCollisionMode.Collision3D;
        collision.collidesWith = capasColision;
        collision.dampen = 0.25f;
        collision.bounce = 0.03f;
        collision.lifetimeLoss = 0.25f;
        collision.sendCollisionMessages = salpicaduraAlChocar != null;

        if (renderParticulas != null)
        {
            renderParticulas.renderMode = ParticleSystemRenderMode.Stretch;
            renderParticulas.lengthScale = estiramientoParticulas;
            renderParticulas.velocityScale = 0.35f;
            renderParticulas.cameraVelocityScale = 0f;

            if (materialParticulasAgua != null)
            {
                renderParticulas.sharedMaterial = materialParticulasAgua;
            }
        }

        if (Application.isPlaying && !sistemaParticulas.isPlaying)
        {
            sistemaParticulas.Play();
        }
    }

    private void OnParticleCollision(GameObject other)
    {
        if (salpicaduraAlChocar == null)
        {
            return;
        }

        if (Time.time - ultimoTiempoSalpicadura < tiempoMinimoEntreSalpicaduras)
        {
            return;
        }

        int cantidadEventos = ParticlePhysicsExtensions.GetCollisionEvents(sistemaParticulas, other, eventosColision);
        int cantidadASacar = Mathf.Min(cantidadEventos, maxSalpicadurasPorChoque);

        for (int i = 0; i < cantidadASacar; i++)
        {
            ParticleCollisionEvent evento = eventosColision[i];
            salpicaduraAlChocar.transform.position = evento.intersection;
            salpicaduraAlChocar.transform.rotation = Quaternion.LookRotation(evento.normal);
            salpicaduraAlChocar.Emit(particulasPorSalpicadura);
        }

        if (cantidadASacar > 0)
        {
            ultimoTiempoSalpicadura = Time.time;
        }
    }
}
