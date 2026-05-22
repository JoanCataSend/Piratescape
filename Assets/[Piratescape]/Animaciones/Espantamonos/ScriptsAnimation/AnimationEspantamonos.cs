using UnityEngine;

public class AnimationEspantamonos : MonoBehaviour
{
    [SerializeField] private GameObject camaraADesactivar;

    public void DesactivarCamara()
    {
        if (camaraADesactivar != null)
        {
            camaraADesactivar.SetActive(false);
        }
    }

    public void ActivarCamara()
    {
        if (camaraADesactivar != null)
        {
            camaraADesactivar.SetActive(true);
        }
    }
}