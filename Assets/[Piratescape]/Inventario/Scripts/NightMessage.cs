using UnityEngine;
using TMPro;
using System.Collections;

public class NightMessageUI : MonoBehaviour
{
    public static NightMessageUI Instance;

    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private float duration = 3f;

    private void Awake()
    {
        Instance = this;
        messageText.text = "";
    }

    public void ShowMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        messageText.text = message;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        messageText.gameObject.SetActive(false);
    }
}