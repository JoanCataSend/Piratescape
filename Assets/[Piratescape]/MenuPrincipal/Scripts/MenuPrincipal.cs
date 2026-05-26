using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Escenas")]
    [SerializeField] private string nombreEscenaJuego = "MainDemo";

    [Header("Botones")]
    [SerializeField] private Button botonContinuar;

    private void Awake()
    {
        BuscarBotonContinuarSiFalta();
    }

    private void Start()
    {
        ActualizarBotonContinuar();
    }

    public void NuevaPartida()
    {
        GestorPartida.SolicitarNuevaPartidaAlEntrar();
        SceneManager.LoadScene(nombreEscenaJuego);
    }

    public void Continuar()
    {
        if (!GestorPartida.ExistePartidaGuardadaEnDisco())
        {
            Debug.LogWarning("No hay partida guardada. Se iniciara una partida nueva.");
            GestorPartida.SolicitarNuevaPartidaAlEntrar();
            SceneManager.LoadScene(nombreEscenaJuego);
            return;
        }

        GestorPartida.SolicitarCargarAlEntrar();
        SceneManager.LoadScene(nombreEscenaJuego);
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

    private void ActualizarBotonContinuar()
    {
        if (botonContinuar != null)
        {
            botonContinuar.interactable = GestorPartida.ExistePartidaGuardadaEnDisco();
        }
    }

    private void BuscarBotonContinuarSiFalta()
    {
        if (botonContinuar != null)
        {
            return;
        }

        GameObject objetoContinuar = GameObject.Find("Continuar");

        if (objetoContinuar != null)
        {
            botonContinuar = objetoContinuar.GetComponent<Button>();
        }
    }
}
