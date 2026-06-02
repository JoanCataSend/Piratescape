using UnityEngine;

public sealed class GhostAuraBillboard : MonoBehaviour
{
    private Camera camaraPrincipal;

    private void LateUpdate()
    {
        if (camaraPrincipal == null)
        {
            camaraPrincipal = Camera.main;
        }

        if (camaraPrincipal == null)
        {
            return;
        }

        transform.LookAt(
            transform.position + camaraPrincipal.transform.rotation * Vector3.forward,
            camaraPrincipal.transform.rotation * Vector3.up
        );
    }
}