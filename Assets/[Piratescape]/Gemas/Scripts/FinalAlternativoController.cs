using UnityEngine;
using UnityEngine.Video;

public class FinalAlternativoController : MonoBehaviour
{
    [Header("Vídeo")]
    [SerializeField] private GameObject panelVideo;
    [SerializeField] private VideoPlayer videoPlayer;

    public void IniciarCinematica()
    {
        if (panelVideo != null)
        {
            panelVideo.SetActive(true);
        }

        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }

        Debug.Log("Cinemática iniciada");
    }
}