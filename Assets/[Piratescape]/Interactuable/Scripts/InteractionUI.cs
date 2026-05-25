using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [Header("Burbuja de interaccion")]
    [SerializeField] private GameObject prompt;
    [SerializeField] private TMP_Text actionText;

    [Header("Animacion")]
    [SerializeField] private bool usarAnimacion = true;
    [SerializeField] private float escalaOculta = 0.75f;
    [SerializeField] private float escalaVisible = 1f;
    [SerializeField] private float velocidadAnimacion = 12f;

    private Object currentOwner;
    private Coroutine temporaryMessageRoutine;

    private Transform promptTransform;
    private Vector3 escalaBase;
    private Vector3 targetScale;
    private bool visible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (prompt != null)
        {
            promptTransform = prompt.transform;
            escalaBase = promptTransform.localScale;
        }
        else
        {
            escalaBase = Vector3.one;
        }

        targetScale = escalaBase * escalaOculta;

        HideImmediate();
    }

    private void LateUpdate()
    {
        if (!usarAnimacion || promptTransform == null)
        {
            return;
        }

        promptTransform.localScale = Vector3.Lerp(
            promptTransform.localScale,
            targetScale,
            Time.deltaTime * velocidadAnimacion
        );

        if (!visible && prompt.activeSelf && Vector3.Distance(promptTransform.localScale, targetScale) < 0.001f)
        {
            prompt.SetActive(false);
        }
    }

    public void Show(Object owner, string message)
    {
        if (owner == null)
        {
            return;
        }

        StopTemporaryRoutineIfNeeded();

        currentOwner = owner;

        if (actionText != null)
        {
            actionText.text = FormatearMensaje(message);
        }

        MostrarBurbuja();
    }

    public void ShowTemporary(Object owner, string message, float duration)
    {
        if (owner == null)
        {
            return;
        }

        StopTemporaryRoutineIfNeeded();
        temporaryMessageRoutine = StartCoroutine(ShowTemporaryRoutine(owner, message, duration));
    }

    public void Hide(Object owner)
    {
        if (owner == null)
        {
            return;
        }

        if (currentOwner != owner)
        {
            return;
        }

        StopTemporaryRoutineIfNeeded();

        currentOwner = null;
        OcultarBurbuja();
    }

    public void HideImmediate()
    {
        StopTemporaryRoutineIfNeeded();

        currentOwner = null;
        visible = false;

        if (promptTransform != null)
        {
            promptTransform.localScale = escalaBase * escalaOculta;
        }

        targetScale = escalaBase * escalaOculta;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    public bool IsOwnedBy(Object owner)
    {
        return currentOwner == owner;
    }

    private IEnumerator ShowTemporaryRoutine(Object owner, string message, float duration)
    {
        currentOwner = owner;

        if (actionText != null)
        {
            actionText.text = FormatearMensaje(message);
        }

        MostrarBurbuja();

        yield return new WaitForSeconds(duration);

        if (currentOwner == owner)
        {
            currentOwner = null;
            OcultarBurbuja();
        }

        temporaryMessageRoutine = null;
    }

    private void MostrarBurbuja()
    {
        visible = true;

        if (prompt != null && !prompt.activeSelf)
        {
            prompt.SetActive(true);
        }

        if (promptTransform != null)
        {
            if (usarAnimacion)
            {
                promptTransform.localScale = escalaBase * escalaOculta;
                targetScale = escalaBase * escalaVisible;
            }
            else
            {
                promptTransform.localScale = escalaBase * escalaVisible;
            }
        }
    }

    private void OcultarBurbuja()
    {
        visible = false;
        targetScale = escalaBase * escalaOculta;

        if (!usarAnimacion && prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    private void StopTemporaryRoutineIfNeeded()
    {
        if (temporaryMessageRoutine == null)
        {
            return;
        }

        StopCoroutine(temporaryMessageRoutine);
        temporaryMessageRoutine = null;
    }

    private string FormatearMensaje(string mensajeOriginal)
    {
        if (string.IsNullOrWhiteSpace(mensajeOriginal))
        {
            return "";
        }

        string mensaje = mensajeOriginal;

        mensaje = mensaje.Replace("Pulsa ", "");
        mensaje = mensaje.Replace(" para recoger ", " ");

        // Si viene "E Concha", quitamos la tecla.
        if (mensaje.StartsWith("E "))
        {
            mensaje = mensaje.Substring(2);
        }

        if (mensaje.StartsWith("▢ "))
        {
            mensaje = mensaje.Substring(2);
        }

        return mensaje;
    }
}