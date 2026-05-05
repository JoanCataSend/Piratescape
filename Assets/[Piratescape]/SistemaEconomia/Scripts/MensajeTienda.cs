using System.Collections;
using TMPro;
using UnityEngine;

public sealed class MensajeTienda : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private TMP_Text textoMensaje;

    [Header("Configuracion")]
    [SerializeField] private float duracionMensaje = 2f;

    private Coroutine rutinaActual;

    private void Awake()
    {
        Ocultar();
    }

    public void MostrarError(string mensaje)
    {
        Mostrar(mensaje, Color.red);
    }

    public void MostrarConfirmacion(string mensaje)
    {
        Mostrar(mensaje, Color.white);
    }

    private void Mostrar(string mensaje, Color color)
    {
        if (textoMensaje == null)
        {
            return;
        }

        textoMensaje.text = mensaje;
        textoMensaje.color = color;
        textoMensaje.gameObject.SetActive(true);

        if (rutinaActual != null)
        {
            StopCoroutine(rutinaActual);
        }

        rutinaActual = StartCoroutine(OcultarTrasTiempo());
    }

    private IEnumerator OcultarTrasTiempo()
    {
        yield return new WaitForSecondsRealtime(duracionMensaje);
        Ocultar();
    }

    private void Ocultar()
    {
        if (textoMensaje != null)
        {
            textoMensaje.gameObject.SetActive(false);
        }
    }
}