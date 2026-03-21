using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class IntroVideoManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string siguienteEscena = "mapav2";

    void Start()
    {
        videoPlayer.loopPointReached += AlTerminarVideo;
    }

    void Update()
    {
        if (Input.anyKeyDown)
        {
            CargarJuego();
        }
    }

    void AlTerminarVideo(VideoPlayer vp)
    {
        CargarJuego();
    }

    void CargarJuego()
    {
        SceneManager.LoadScene(siguienteEscena);
    }
}