using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    public void NuevaPartida()
    {
        SceneManager.LoadScene("IntroVideo");
    }

    public void Continuar()
    {
        SceneManager.LoadScene("Demotest");
        Debug.Log("Continuar");
    }

    public void Configuracion()
    {
        Debug.Log("Configuracion");
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}