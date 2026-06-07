using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIButtonSounds : MonoBehaviour,
    IPointerEnterHandler,
    IPointerClickHandler
{
    [Header("Referencias")]
    [SerializeField] private Button button;
    [SerializeField] private AudioSource audioSource;

    [Header("Sonidos")]
    [SerializeField] private AudioClip sonidoHover;
    [SerializeField] private AudioClip sonidoClick;

    [Header("Volumen")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenHover = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float volumenClick = 1f;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!BotonDisponible())
        {
            return;
        }

        if (audioSource == null || sonidoHover == null)
        {
            return;
        }

        audioSource.PlayOneShot(sonidoHover, volumenHover);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!BotonDisponible())
        {
            return;
        }

        if (audioSource == null || sonidoClick == null)
        {
            return;
        }

        audioSource.PlayOneShot(sonidoClick, volumenClick);
    }

    private bool BotonDisponible()
    {
        if (button == null)
        {
            return false;
        }

        return button.interactable && button.isActiveAndEnabled;
    }
}