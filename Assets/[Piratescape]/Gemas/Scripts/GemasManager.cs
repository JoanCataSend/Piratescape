using UnityEngine;

public class GemasManager : MonoBehaviour
{
    [SerializeField] private GameObject[] gemas;

    private void Start()
    {
        ActivarGemas();
    }

    public void ActivarGemas()
    {
        for (int i = 0; i < gemas.Length; i++)
        {
            if (gemas[i] != null)
            {
                gemas[i].SetActive(true);
            }
        }
    }
}