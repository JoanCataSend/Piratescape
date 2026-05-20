using System.Collections;
using UnityEngine;

public sealed class TutorialInicioLoro : MonoBehaviour
{
    [SerializeField] private LoroDialogoUI loroDialogoUI;
    [SerializeField] private float retrasoInicio = 0.5f;
    [SerializeField] private bool mostrarSoloUnaVez = true;

    private IEnumerator Start()
    {
        if (loroDialogoUI == null)
        {
            loroDialogoUI = FindFirstObjectByType<LoroDialogoUI>();
        }

        yield return new WaitForSecondsRealtime(retrasoInicio);

        if (loroDialogoUI == null)
        {
            yield break;
        }

        if (mostrarSoloUnaVez && GestorPartida.Instance != null && GestorPartida.Instance.TutorialCompletado)
        {
            yield break;
        }

        loroDialogoUI.AbrirTutorialAutomatico();
    }
}
