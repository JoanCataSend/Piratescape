using UnityEngine;
using UnityEngine.Video;

public class FinalAlternativoController : MonoBehaviour
{
    [Header("Vídeo")]
    [SerializeField] private GameObject panelVideo;
    [SerializeField] private VideoPlayer videoPlayer;

    private bool reproduciendo;

    public void IniciarCinematica()
    {
        if (reproduciendo)
            return;

        reproduciendo = true;

        // Pausar el juego
        Time.timeScale = 0f;

        // Mostrar vídeo
        if (panelVideo != null)
        {
            panelVideo.SetActive(true);
        }

        if (videoPlayer != null)
        {
            videoPlayer.Stop();

            // Evento cuando acaba
            videoPlayer.loopPointReached += FinVideo;

            videoPlayer.Play();
        }

        Debug.Log("Cinemática iniciada");
    }

    private void FinVideo(VideoPlayer vp)
    {
        Debug.Log("Vídeo terminado");

        // Reanudar juego (o quitar si quieres dejarlo congelado)
        Time.timeScale = 1f;

        // Ocultar panel
        if (panelVideo != null)
        {
            panelVideo.SetActive(false);
        }

        reproduciendo = false;
    }
}