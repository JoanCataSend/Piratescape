using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] private GameObject prompt;
    [SerializeField] private TMP_Text actionText;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show(string message)
    {
        actionText.text = message;
        prompt.SetActive(true);
    }

    public void Hide()
    {
        prompt.SetActive(false);
    }
}