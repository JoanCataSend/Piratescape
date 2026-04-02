using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] private GameObject prompt;
    [SerializeField] private TMP_Text actionText;

    private Object currentOwner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideImmediate();
    }

    public void Show(Object owner, string message)
    {
        if (owner == null)
        {
            return;
        }

        currentOwner = owner;

        if (actionText != null)
        {
            actionText.text = message;
        }

        if (prompt != null)
        {
            prompt.SetActive(true);
        }
    }

    public void Hide(Object owner)
    {
        if (owner == null)
        {
            return;
        }

        if (currentOwner != owner)
        {
            return;
        }

        currentOwner = null;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    public void HideImmediate()
    {
        currentOwner = null;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    public bool IsOwnedBy(Object owner)
    {
        return currentOwner == owner;
    }
}