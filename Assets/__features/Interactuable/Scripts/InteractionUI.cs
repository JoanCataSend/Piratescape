using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] GameObject prompt;

    void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show()
    {
        prompt.SetActive(true);
    }

    public void Hide()
    {
        prompt.SetActive(false);
    }
}