using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class PlayerStatusEffectsUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] private SistemaSaludJugador sistemaSaludJugador;

    [Header("Overlays")]
    [SerializeField] private Image healthLowOverlay;
    [SerializeField] private Image energyLowOverlay;

    [Header("Ocultar efectos en muerte")]
    [SerializeField] private bool ocultarSiJugadorMuerto = true;
    [SerializeField] private GameObject panelMuerte;

    [Header("Ocultar efectos en pausa")]
    [SerializeField] private bool ocultarSiPausaActiva = true;
    [SerializeField] private GameObject panelPausa;

    [Header("Ocultar efectos en transicion")]
    [SerializeField] private bool ocultarSiTransicionActiva = true;
    [SerializeField] private SleepFadeUI sleepFadeUI;

    [Header("Vida baja")]
    [SerializeField] private float healthEffectStartPercent = 0.4f;
    [SerializeField] private float healthMaxAlpha = 0.55f;
    [SerializeField] private bool healthPulse = true;
    [SerializeField] private float healthPulseSpeed = 4f;
    [SerializeField] private float healthPulseStrength = 0.25f;

    [Header("Sonido vida baja")]
    [SerializeField] private AudioSource audioSourceLatido;
    [SerializeField] private AudioMixerGroup outputLatido;
    [SerializeField] private AudioClip[] sonidosLatido;
    [SerializeField] private float volumenLatido = 0.5f;
    [SerializeField] private float tiempoEntreLatidos = 1.2f;

    [Header("Energía baja")]
    [SerializeField] private float energyEffectStartPercent = 0.3f;
    [SerializeField] private float energyMaxAlpha = 0.35f;
    [SerializeField] private bool energyPulse = true;
    [SerializeField] private float energyPulseSpeed = 2f;
    [SerializeField] private float energyPulseStrength = 0.12f;

    private float tiempoSiguienteLatido;
    private int ultimoLatido = -1;

    private void Awake()
    {
        if (playerEnergy == null)
        {
            playerEnergy = FindFirstObjectByType<PlayerEnergy>();
        }

        if (sistemaSaludJugador == null)
        {
            sistemaSaludJugador = FindFirstObjectByType<SistemaSaludJugador>();
        }

        if (sleepFadeUI == null)
        {
            sleepFadeUI = FindFirstObjectByType<SleepFadeUI>();
        }

        OcultarEfectos();

        PrepararAudioLatido();
    }

    private void Update()
    {
        if (DebeOcultarEfectos())
        {
            OcultarEfectos();
            return;
        }

        ActualizarEfectoVida();
        ActualizarEfectoEnergia();
    }

    private bool DebeOcultarEfectos()
    {
        if (ocultarSiJugadorMuerto &&
            sistemaSaludJugador != null &&
            sistemaSaludJugador.EstaMuerto)
        {
            return true;
        }

        if (ocultarSiJugadorMuerto &&
            panelMuerte != null &&
            panelMuerte.activeSelf)
        {
            return true;
        }

        if (ocultarSiPausaActiva &&
            panelPausa != null &&
            panelPausa.activeSelf)
        {
            return true;
        }

        if (ocultarSiTransicionActiva &&
            sleepFadeUI != null &&
            sleepFadeUI.IsFading)
        {
            return true;
        }

        return false;
    }

    private void OcultarEfectos()
    {
        SetImageAlpha(healthLowOverlay, 0f);
        SetImageAlpha(energyLowOverlay, 0f);
    }

    private void ActualizarEfectoVida()
    {
        if (sistemaSaludJugador == null || healthLowOverlay == null)
        {
            return;
        }

        float saludMaxima = sistemaSaludJugador.SaludMaxima;

        if (saludMaxima <= 0f)
        {
            SetImageAlpha(healthLowOverlay, 0f);
            return;
        }

        float porcentajeSalud = sistemaSaludJugador.SaludActual / saludMaxima;

        float intensidad = CalcularIntensidad(porcentajeSalud, healthEffectStartPercent);
        ActualizarSonidoLatido(intensidad);

        float alpha = intensidad * healthMaxAlpha;

        if (healthPulse && intensidad > 0f)
        {
            float pulso = 1f + Mathf.Sin(Time.time * healthPulseSpeed) * healthPulseStrength;
            alpha *= pulso;
        }

        alpha = Mathf.Clamp01(alpha);

        SetImageAlpha(healthLowOverlay, alpha);
    }

    private void ActualizarEfectoEnergia()
    {
        if (playerEnergy == null || energyLowOverlay == null)
        {
            return;
        }

        float energiaMaxima = playerEnergy.MaxEnergy;

        if (energiaMaxima <= 0f)
        {
            SetImageAlpha(energyLowOverlay, 0f);
            return;
        }

        float porcentajeEnergia = playerEnergy.CurrentEnergy / energiaMaxima;

        float intensidad = CalcularIntensidad(porcentajeEnergia, energyEffectStartPercent);

        float alpha = intensidad * energyMaxAlpha;

        if (energyPulse && intensidad > 0f)
        {
            float pulso = 1f + Mathf.Sin(Time.time * energyPulseSpeed) * energyPulseStrength;
            alpha *= pulso;
        }

        alpha = Mathf.Clamp01(alpha);

        SetImageAlpha(energyLowOverlay, alpha);
    }

    private float CalcularIntensidad(float porcentajeActual, float porcentajeInicio)
    {
        if (porcentajeInicio <= 0f)
        {
            return porcentajeActual <= 0f ? 1f : 0f;
        }

        if (porcentajeActual >= porcentajeInicio)
        {
            return 0f;
        }

        return Mathf.InverseLerp(porcentajeInicio, 0f, porcentajeActual);
    }

    private void SetImageAlpha(Image image, float alpha)
    {
        if (image == null)
        {
            return;
        }

        Color color = image.color;
        color.a = Mathf.Clamp01(alpha);
        image.color = color;
    }

    private void PrepararAudioLatido()
    {
        if (audioSourceLatido == null)
        {
            audioSourceLatido = gameObject.AddComponent<AudioSource>();
        }

        audioSourceLatido.playOnAwake = false;
        audioSourceLatido.loop = false;
        audioSourceLatido.spatialBlend = 0f;
        audioSourceLatido.outputAudioMixerGroup = outputLatido;
    }

    private void ActualizarSonidoLatido(float intensidad)
    {
        if (intensidad <= 0f)
        {
            return;
        }

        if (Time.time < tiempoSiguienteLatido)
        {
            return;
        }

        AudioClip clip = ObtenerLatidoAleatorio();

        if (clip == null || audioSourceLatido == null)
        {
            return;
        }

        audioSourceLatido.PlayOneShot(clip, volumenLatido);

        float espera = Mathf.Lerp(tiempoEntreLatidos, 0.55f, intensidad);
        tiempoSiguienteLatido = Time.time + espera;
    }

    private AudioClip ObtenerLatidoAleatorio()
    {
        if (sonidosLatido == null || sonidosLatido.Length == 0)
        {
            return null;
        }

        if (sonidosLatido.Length == 1)
        {
            ultimoLatido = 0;
            return sonidosLatido[0];
        }

        int indice;

        do
        {
            indice = Random.Range(0, sonidosLatido.Length);
        }
        while (indice == ultimoLatido);

        ultimoLatido = indice;
        return sonidosLatido[indice];
    }
}