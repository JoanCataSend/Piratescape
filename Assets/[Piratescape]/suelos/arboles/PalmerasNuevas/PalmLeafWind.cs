using UnityEngine;

public class PalmTreeWind : MonoBehaviour
{
    [Header("Hojas de la palmera")]
    [SerializeField] private Transform[] leaves;

    [Header("Movimiento principal")]
    [SerializeField] private float windStrength = 2f;
    [SerializeField] private float windSpeed = 1f;

    [Header("Movimiento secundario")]
    [SerializeField] private float secondaryStrength = 0.6f;
    [SerializeField] private float secondarySpeed = 2f;

    [Header("Ejes")]
    [SerializeField] private bool rotateX = true;
    [SerializeField] private bool rotateY = false;
    [SerializeField] private bool rotateZ = true;

    [Header("Variación")]
    [SerializeField] private bool randomizeLeaves = true;

    private Quaternion[] initialRotations;
    private float[] randomOffsets;
    private float[] randomStrengths;
    private float[] randomSpeeds;

    private void Awake()
    {
        if (leaves == null || leaves.Length == 0)
        {
            BuscarHojasAutomaticamente();
        }

        initialRotations = new Quaternion[leaves.Length];
        randomOffsets = new float[leaves.Length];
        randomStrengths = new float[leaves.Length];
        randomSpeeds = new float[leaves.Length];

        for (int i = 0; i < leaves.Length; i++)
        {
            if (leaves[i] == null)
            {
                continue;
            }

            initialRotations[i] = leaves[i].localRotation;

            if (randomizeLeaves)
            {
                randomOffsets[i] = Random.Range(0f, 100f);
                randomStrengths[i] = Random.Range(0.75f, 1.25f);
                randomSpeeds[i] = Random.Range(0.85f, 1.15f);
            }
            else
            {
                randomOffsets[i] = i * 0.35f;
                randomStrengths[i] = 1f;
                randomSpeeds[i] = 1f;
            }
        }
    }

    private void Update()
    {
        if (leaves == null)
        {
            return;
        }

        for (int i = 0; i < leaves.Length; i++)
        {
            if (leaves[i] == null)
            {
                continue;
            }

            float time = Time.time + randomOffsets[i];

            float mainWave = Mathf.Sin(time * windSpeed * randomSpeeds[i]) * windStrength * randomStrengths[i];
            float secondaryWave = Mathf.Sin(time * secondarySpeed * randomSpeeds[i]) * secondaryStrength * randomStrengths[i];

            float xRotation = rotateX ? mainWave : 0f;
            float yRotation = rotateY ? secondaryWave * 0.5f : 0f;
            float zRotation = rotateZ ? secondaryWave : 0f;

            Quaternion windRotation = Quaternion.Euler(xRotation, yRotation, zRotation);

            leaves[i].localRotation = initialRotations[i] * windRotation;
        }
    }

    private void BuscarHojasAutomaticamente()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        System.Collections.Generic.List<Transform> hojasEncontradas = new System.Collections.Generic.List<Transform>();

        foreach (Transform child in children)
        {
            if (child == transform)
            {
                continue;
            }

            string nombre = child.name.ToLower();

            if (nombre.Contains("hoja") || nombre.Contains("leaf") || nombre.Contains("palma"))
            {
                hojasEncontradas.Add(child);
            }
        }

        leaves = hojasEncontradas.ToArray();
    }
}