using UnityEngine;
using Unity.Cinemachine;

public class SensibilidadRatonCinemachine : MonoBehaviour
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

    [Header("Opciones")]
    [SerializeField] private bool invertirY = false;
    [SerializeField] private bool bloquearEnPausa = true;

    private void Awake()
    {
        if (orbitalFollow == null)
        {
            orbitalFollow = GetComponent<CinemachineOrbitalFollow>();
        }
    }

    private void Update()
    {
        if (orbitalFollow == null)
        {
            return;
        }

        if (bloquearEnPausa && Time.timeScale == 0f)
        {
            return;
        }

        float sensibilidad = SettingsMenuController.ObtenerSensibilidadRaton();

        float mouseX = Input.GetAxisRaw("Mouse X");
        float mouseY = Input.GetAxisRaw("Mouse Y");

        orbitalFollow.HorizontalAxis.Value += mouseX * sensibilidad;

        if (invertirY)
        {
            orbitalFollow.VerticalAxis.Value += mouseY * sensibilidad;
        }
        else
        {
            orbitalFollow.VerticalAxis.Value -= mouseY * sensibilidad;
        }
    }
}