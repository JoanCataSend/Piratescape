using System.Collections.Generic;
using UnityEngine;

public class GemasManager : MonoBehaviour
{
    [SerializeField] private GameObject[] gemas;
    [SerializeField] private Transform[] puntosSpawn;

    private void Start()
    {
        ColocarGemasAleatoriamente();
    }

    public void ColocarGemasAleatoriamente()
    {
        List<Transform> puntosDisponibles = new List<Transform>(puntosSpawn);

        for (int i = 0; i < gemas.Length; i++)
        {
            if (gemas[i] == null || puntosDisponibles.Count == 0)
                continue;

            int indice = Random.Range(0, puntosDisponibles.Count);
            Transform puntoElegido = puntosDisponibles[indice];

            gemas[i].transform.position = puntoElegido.position;
            gemas[i].SetActive(true);

            puntosDisponibles.RemoveAt(indice);
        }
    }
}