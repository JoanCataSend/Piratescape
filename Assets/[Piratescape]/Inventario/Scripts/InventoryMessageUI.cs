using System.Collections;
using TMPro;
using UnityEngine;

public sealed class InventoryMessageUI : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float visibleTime = 2f;

    private Coroutine hideCoroutine;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = FindFirstObjectByType<PlayerInventory>();
        }

        if (messageText != null)
        {
            messageText.text = "";
            messageText.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryMessageRequested += ShowMessage;
        }
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryMessageRequested -= ShowMessage;
        }
    }

    private void ShowMessage(string message)
    {
        if (messageText == null)
        {
            return;
        }

        messageText.text = message;
        messageText.enabled = true;

        if (hideCoroutine != null)
        {
            StopCoroutine(hideCoroutine);
        }

        hideCoroutine = StartCoroutine(HideAfterTime());
    }

    private IEnumerator HideAfterTime()
    {
        yield return new WaitForSeconds(visibleTime);

        messageText.enabled = false;
        messageText.text = "";
        hideCoroutine = null;
    }
}