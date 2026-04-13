using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventoryInput : MonoBehaviour
{
    [SerializeField] private PlayerInventory playerInventory;

    private void Awake()
    {
        if (playerInventory == null)
        {
            playerInventory = GetComponent<PlayerInventory>();
        }
    }

    private void Update()
    {
        if (playerInventory == null)
        {
            return;
        }

        if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) playerInventory.SelectSlot(0);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) playerInventory.SelectSlot(1);
            if (Keyboard.current.digit3Key.wasPressedThisFrame) playerInventory.SelectSlot(2);
            if (Keyboard.current.digit4Key.wasPressedThisFrame) playerInventory.SelectSlot(3);
            if (Keyboard.current.digit5Key.wasPressedThisFrame) playerInventory.SelectSlot(4);

            if (Keyboard.current.qKey.wasPressedThisFrame)
            {
                playerInventory.DropSelectedItem();
            }
        }

        if (Gamepad.current != null)
        {
            if (Gamepad.current.leftShoulder.wasPressedThisFrame)
            {
                playerInventory.SelectPreviousSlot();
            }

            if (Gamepad.current.rightShoulder.wasPressedThisFrame)
            {
                playerInventory.SelectNextSlot();
            }

            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                playerInventory.DropSelectedItem();
            }
        }
    }
}