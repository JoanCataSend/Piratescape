using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    private movimientoplayer movimiento;

    private void Awake()
    {
        movimiento = GetComponentInParent<movimientoplayer>();
    }

    public void EndPickUpAnimation()
    {
        if (movimiento != null)
        {
            movimiento.EndPickUpAnimation();
        }
    }
}