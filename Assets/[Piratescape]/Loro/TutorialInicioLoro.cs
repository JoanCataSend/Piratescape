using System.Collections;
using UnityEngine;

public sealed class TutorialInicioLoro : MonoBehaviour
{
    [SerializeField] private TutorialMisionesLoro tutorialMisionesLoro;
    [SerializeField] private float retrasoInicio = 0.5f;
    [SerializeField] private bool mostrarSoloUnaVez = true;

    private IEnumerator Start()
    {
        if (tutorialMisionesLoro == null)
        {
            tutorialMisionesLoro = FindFirstObjectByType<TutorialMisionesLoro>();
        }

        yield return new WaitForSecondsRealtime(retrasoInicio);

        if (tutorialMisionesLoro == null)
        {
            yield break;
        }

        if (mostrarSoloUnaVez && GestorPartida.Instance != null && GestorPartida.Instance.TutorialCompletado)
        {
            yield break;
        }

        tutorialMisionesLoro.IniciarTutorialAutomatico();
    }
}
