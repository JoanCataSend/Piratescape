using System.Collections;
using UnityEngine;

public sealed class NightTreeClimberMonkeySequence : MonoBehaviour
{
    [Header("Monos escaladores")]
    [SerializeField] private NightTreeClimberMonkey[] monosEscaladores;

    [Header("Tiempos")]
    [SerializeField] private float retrasoInicial = 1.5f;
    [SerializeField] private float tiempoEntreMonos = 3f;
    [SerializeField] private bool repetirMientrasSeaNoche = true;

    private Coroutine rutinaSecuencia;
    private bool estaDeNoche;

    public void EmpezarNoche()
    {
        estaDeNoche = true;

        if (rutinaSecuencia != null)
        {
            StopCoroutine(rutinaSecuencia);
        }

        rutinaSecuencia = StartCoroutine(SecuenciaNocturna());
    }

    public void EmpezarDia()
    {
        estaDeNoche = false;

        if (rutinaSecuencia != null)
        {
            StopCoroutine(rutinaSecuencia);
            rutinaSecuencia = null;
        }

        if (monosEscaladores == null)
        {
            return;
        }

        for (int i = 0; i < monosEscaladores.Length; i++)
        {
            if (monosEscaladores[i] != null)
            {
                monosEscaladores[i].EmpezarDia();
            }
        }
    }

    private IEnumerator SecuenciaNocturna()
    {
        yield return new WaitForSeconds(retrasoInicial);

        do
        {
            if (monosEscaladores == null || monosEscaladores.Length == 0)
            {
                yield break;
            }

            for (int i = 0; i < monosEscaladores.Length; i++)
            {
                if (!estaDeNoche)
                {
                    yield break;
                }

                NightTreeClimberMonkey mono = monosEscaladores[i];

                if (mono == null)
                {
                    continue;
                }

                yield return mono.EjecutarSubidaYBajada();

                if (!estaDeNoche)
                {
                    yield break;
                }

                yield return new WaitForSeconds(tiempoEntreMonos);
            }
        }
        while (repetirMientrasSeaNoche);
    }
}