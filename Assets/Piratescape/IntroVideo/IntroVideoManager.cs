using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public sealed class IntroVideoManager : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string siguienteEscena = "Demotest";

    private void Start()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("IntroVideoManager: falta la referencia al VideoPlayer.");
            return;
        }

        videoPlayer.loopPointReached += AlTerminarVideo;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= AlTerminarVideo;
        }
    }

    private void Update()
    {
        if (Input.anyKeyDown)
        {
            CargarJuego();
        }
    }

    private void AlTerminarVideo(VideoPlayer reproductor)
    {
        CargarJuego();
    }

    private void CargarJuego()
    {
        if (string.IsNullOrWhiteSpace(siguienteEscena))
        {
            Debug.LogError("IntroVideoManager: el nombre de la siguiente escena está vacío.");
            return;
        }

        SceneManager.LoadScene(siguienteEscena);
    }
}