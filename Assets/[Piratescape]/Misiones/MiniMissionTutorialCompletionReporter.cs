using System.Collections;
using UnityEngine;

public sealed class MiniMissionTutorialCompletionReporter : MonoBehaviour
{
    [Header("Tutorial principal")]
    [SerializeField] private bool completarAlActivarse = false;
    [SerializeField] private float retardoCompletarAlActivarse = 0f;
    [SerializeField] private bool mostrarLogs = false;

    private bool yaReportado;

    private void OnEnable()
    {
        if (completarAlActivarse)
        {
            if (retardoCompletarAlActivarse > 0f)
            {
                StartCoroutine(CompletarConRetardo());
            }
            else
            {
                CompletarTutorialPrincipal();
            }
        }
    }

    public void CompletarTutorialPrincipal()
    {
        if (yaReportado)
        {
            return;
        }

        yaReportado = true;

        if (MiniMissionManager.Instance != null)
        {
            MiniMissionManager.Instance.CompletarTutorialPrincipal();
        }
        else
        {
            MiniMissionManager.CompletarTutorialPrincipalGlobal();
        }

        if (mostrarLogs)
        {
            Debug.Log("[MiniMissionTutorialCompletionReporter] Tutorial principal completado. Se activan las minimisiones.", this);
        }
    }

    [ContextMenu("Debug/Completar tutorial principal")]
    public void DebugCompletarTutorialPrincipal()
    {
        yaReportado = false;
        CompletarTutorialPrincipal();
    }

    private IEnumerator CompletarConRetardo()
    {
        yield return new WaitForSeconds(retardoCompletarAlActivarse);
        CompletarTutorialPrincipal();
    }
}
