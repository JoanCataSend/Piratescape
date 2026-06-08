using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class TriggerEscenaFinal : MonoBehaviour
{
    [Header("Escena final")]
    [Tooltip("Nombre exacto de la escena donde está la cinemática final.")]
    [SerializeField] private string nombreEscenaFinal = "EscenaFinal";

    [Header("Configuración")]
    [SerializeField] private bool activarSoloUnaVez = true;

    private bool escenaCargandose;

    private void OnTriggerEnter(Collider other)
    {
        if (escenaCargandose)
        {
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(nombreEscenaFinal))
        {
            Debug.LogWarning(
                "TriggerEscenaFinal: falta escribir el nombre de la escena final.",
                this
            );

            return;
        }

        escenaCargandose = true;

        SceneManager.LoadScene(nombreEscenaFinal);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!activarSoloUnaVez &&
            other.CompareTag("Player"))
        {
            escenaCargandose = false;
        }
    }
}