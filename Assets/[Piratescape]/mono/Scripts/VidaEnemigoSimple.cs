using UnityEngine;

[DisallowMultipleComponent]
public class VidaEnemigoSimple : MonoBehaviour
{
    [Header("Vida")]
    [SerializeField] private int vidaMaxima = 30;
    [SerializeField] private int vidaActual = 30;
    [SerializeField] private bool reiniciarVidaAlActivarse = true;

    [Header("Muerte")]
    [SerializeField] private bool destruirAlMorir = false;
    [SerializeField] private bool desactivarAlMorir = true;
    [SerializeField] private float retardoMuerte = 0.1f;

    [Header("FX opcional")]
    [SerializeField] private ParticleSystem particulasMuerte;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sonidoMuerte;
    [Range(0f, 1f)]
    [SerializeField] private float volumenMuerte = 0.8f;

    [Header("Animator opcional")]
    [SerializeField] private Animator animator;
    [SerializeField] private string triggerDanio = "Hit";
    [SerializeField] private string triggerMuerte = "Die";

    private bool muerto;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        vidaActual = Mathf.Clamp(vidaActual <= 0 ? vidaMaxima : vidaActual, 1, vidaMaxima);
    }

    private void OnEnable()
    {
        if (reiniciarVidaAlActivarse)
        {
            muerto = false;
            vidaActual = Mathf.Max(1, vidaMaxima);
        }
    }

    public void RecibirDanio(int cantidad)
    {
        if (muerto || cantidad <= 0)
        {
            return;
        }

        vidaActual -= cantidad;
        ReproducirTrigger(triggerDanio);

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    public void TakeDamage(int cantidad)
    {
        RecibirDanio(cantidad);
    }

    public void AplicarDanio(int cantidad)
    {
        RecibirDanio(cantidad);
    }

    private void Morir()
    {
        if (muerto)
        {
            return;
        }

        muerto = true;
        ReproducirTrigger(triggerMuerte);

        if (particulasMuerte != null)
        {
            particulasMuerte.transform.SetParent(null, true);
            particulasMuerte.Play(true);
            Destroy(particulasMuerte.gameObject, 3f);
        }

        if (audioSource != null && sonidoMuerte != null)
        {
            audioSource.PlayOneShot(sonidoMuerte, volumenMuerte);
        }

        if (desactivarAlMorir)
        {
            Invoke(nameof(DesactivarObjeto), retardoMuerte);
            return;
        }

        if (destruirAlMorir)
        {
            Destroy(gameObject, retardoMuerte);
        }
    }

    private void DesactivarObjeto()
    {
        gameObject.SetActive(false);
    }

    private void ReproducirTrigger(string nombreTrigger)
    {
        if (animator == null || string.IsNullOrWhiteSpace(nombreTrigger))
        {
            return;
        }

        foreach (AnimatorControllerParameter parametro in animator.parameters)
        {
            if (parametro.type == AnimatorControllerParameterType.Trigger && parametro.name == nombreTrigger)
            {
                animator.SetTrigger(nombreTrigger);
                return;
            }
        }
    }
}
