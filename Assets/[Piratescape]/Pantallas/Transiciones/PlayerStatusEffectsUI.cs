using UnityEngine;
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

    [Header("Energía baja")]
    [SerializeField] private float energyEffectStartPercent = 0.3f;
    [SerializeField] private float energyMaxAlpha = 0.35f;
    [SerializeField] private bool energyPulse = true;
    [SerializeField] private float energyPulseSpeed = 2f;
    [SerializeField] private float energyPulseStrength = 0.12f;

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
}