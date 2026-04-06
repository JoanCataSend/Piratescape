using System.Collections;
using TMPro;
using UnityEngine;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [SerializeField] private GameObject prompt;
    [SerializeField] private TMP_Text actionText;

    private Object currentOwner;
    private Coroutine temporaryMessageRoutine;

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

        StopTemporaryRoutineIfNeeded();

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

    public void ShowTemporary(Object owner, string message, float duration)
    {
        if (owner == null)
        {
            return;
        }

        StopTemporaryRoutineIfNeeded();
        temporaryMessageRoutine = StartCoroutine(ShowTemporaryRoutine(owner, message, duration));
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

        StopTemporaryRoutineIfNeeded();

        currentOwner = null;

        if (prompt != null)
        {
            prompt.SetActive(false);
        }
    }

    public void HideImmediate()
    {
        StopTemporaryRoutineIfNeeded();

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

    private IEnumerator ShowTemporaryRoutine(Object owner, string message, float duration)
    {
        currentOwner = owner;

        if (actionText != null)
        {
            actionText.text = message;
        }

        if (prompt != null)
        {
            prompt.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        if (currentOwner == owner)
        {
            currentOwner = null;

            if (prompt != null)
            {
                prompt.SetActive(false);
            }
        }

        temporaryMessageRoutine = null;
    }

    private void StopTemporaryRoutineIfNeeded()
    {
        if (temporaryMessageRoutine == null)
        {
            return;
        }

        StopCoroutine(temporaryMessageRoutine);
        temporaryMessageRoutine = null;
    }
}