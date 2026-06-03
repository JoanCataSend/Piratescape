using UnityEngine;
using UnityEngine.UI;

public sealed class VisualizadorBarrasEstado : MonoBehaviour
{
    [Header("Referencias visuales")]
    [SerializeField] private Image rellenoBarraSalud;
    [SerializeField] private Image rellenoBarraEnergia;

    [Header("Preview al seleccionar consumible")]
    [SerializeField] private Image previewBarraSalud;
    [SerializeField] private Image previewBarraEnergia;
    [SerializeField, Range(0f, 1f)] private float alphaMinPreview = 0.25f;
    [SerializeField, Range(0f, 1f)] private float alphaMaxPreview = 0.75f;
    [SerializeField] private float velocidadPulsoPreview = 4f;

    [Header("Salud")]
    [SerializeField] private float saludActual = 100f;
    [SerializeField] private float saludMaxima = 100f;

    [Header("Energia")]
    [SerializeField] private float energiaActual = 100f;
    [SerializeField] private float energiaMaxima = 100f;

    private bool previewSaludActiva;
    private bool previewEnergiaActiva;

    private void Start()
    {
        ValidarValoresIniciales();
        ActualizarBarras();
        OcultarPreview();
    }

    private void Update()
    {
        ActualizarPulsoPreview();
    }

    public void ActualizarBarras()
    {
        ActualizarBarraSalud();
        ActualizarBarraEnergia();
    }

    public void EstablecerSaludActual(float nuevaSaludActual)
    {
        saludActual = LimitarValorActual(nuevaSaludActual, saludMaxima);
        ActualizarBarraSalud();
    }

    public void EstablecerSalud(float nuevaSaludActual, float nuevaSaludMaxima)
    {
        saludMaxima = LimitarValorMaximo(nuevaSaludMaxima);
        saludActual = LimitarValorActual(nuevaSaludActual, saludMaxima);
        ActualizarBarraSalud();
    }

    private void ActualizarBarraSalud()
    {
        ActualizarRellenoBarra(rellenoBarraSalud, saludActual, saludMaxima);
    }

    public void EstablecerEnergiaActual(float nuevaEnergiaActual)
    {
        energiaActual = LimitarValorActual(nuevaEnergiaActual, energiaMaxima);
        ActualizarBarraEnergia();
    }

    public void EstablecerEnergia(float nuevaEnergiaActual, float nuevaEnergiaMaxima)
    {
        energiaMaxima = LimitarValorMaximo(nuevaEnergiaMaxima);
        energiaActual = LimitarValorActual(nuevaEnergiaActual, energiaMaxima);
        ActualizarBarraEnergia();
    }

    private void ActualizarBarraEnergia()
    {
        ActualizarRellenoBarra(rellenoBarraEnergia, energiaActual, energiaMaxima);
    }

    public void MostrarPreviewSalud(float saludPreview)
    {
        if (previewBarraSalud == null)
        {
            return;
        }

        saludPreview = LimitarValorActual(saludPreview, saludMaxima);
        previewBarraSalud.fillAmount = saludMaxima > 0f ? saludPreview / saludMaxima : 0f;

        previewSaludActiva = saludPreview > saludActual;
        previewBarraSalud.enabled = previewSaludActiva;
    }

    public void MostrarPreviewEnergia(float energiaPreview)
    {
        if (previewBarraEnergia == null)
        {
            return;
        }

        energiaPreview = LimitarValorActual(energiaPreview, energiaMaxima);
        previewBarraEnergia.fillAmount = energiaMaxima > 0f ? energiaPreview / energiaMaxima : 0f;

        previewEnergiaActiva = energiaPreview > energiaActual;
        previewBarraEnergia.enabled = previewEnergiaActiva;
    }

    public void OcultarPreview()
    {
        previewSaludActiva = false;
        previewEnergiaActiva = false;

        if (previewBarraSalud != null)
        {
            previewBarraSalud.enabled = false;
            previewBarraSalud.fillAmount = 0f;
        }

        if (previewBarraEnergia != null)
        {
            previewBarraEnergia.enabled = false;
            previewBarraEnergia.fillAmount = 0f;
        }
    }

    private void ActualizarPulsoPreview()
    {
        float pulso = Mathf.Sin(Time.unscaledTime * velocidadPulsoPreview) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(alphaMinPreview, alphaMaxPreview, pulso);

        if (previewBarraSalud != null && previewSaludActiva)
        {
            Color color = previewBarraSalud.color;
            color.a = alpha;
            previewBarraSalud.color = color;
        }

        if (previewBarraEnergia != null && previewEnergiaActiva)
        {
            Color color = previewBarraEnergia.color;
            color.a = alpha;
            previewBarraEnergia.color = color;
        }
    }

    private void ValidarValoresIniciales()
    {
        saludMaxima = LimitarValorMaximo(saludMaxima);
        saludActual = LimitarValorActual(saludActual, saludMaxima);

        energiaMaxima = LimitarValorMaximo(energiaMaxima);
        energiaActual = LimitarValorActual(energiaActual, energiaMaxima);
    }

    private float LimitarValorMaximo(float valorMaximo)
    {
        return Mathf.Max(0f, valorMaximo);
    }

    private float LimitarValorActual(float valorActual, float valorMaximo)
    {
        return Mathf.Clamp(valorActual, 0f, valorMaximo);
    }

    private void ActualizarRellenoBarra(Image barra, float valorActual, float valorMaximo)
    {
        if (barra == null)
        {
            return;
        }

        barra.fillAmount = valorMaximo > 0f ? valorActual / valorMaximo : 0f;
    }
}