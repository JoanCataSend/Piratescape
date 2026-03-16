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
        Debug.Log("Continuar");
    }

    public void Configuracion()
    {
        Debug.Log("Configuracion");
    }
}