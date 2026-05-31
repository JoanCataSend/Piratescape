using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    [Header("Cursor")]
    [SerializeField] private Texture2D cursorTexture;

    [Header("Punto de clic del cursor")]
    [SerializeField] private Vector2 hotspot = Vector2.zero;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        AplicarCursor();
    }

    private void Start()
    {
        AplicarCursor();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            AplicarCursor();
        }
    }

    private void AplicarCursor()
    {
        if (cursorTexture == null)
        {
            Debug.LogWarning("No hay textura de cursor asignada.");
            return;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Cursor.SetCursor(cursorTexture, hotspot, CursorMode.ForceSoftware);
    }
}