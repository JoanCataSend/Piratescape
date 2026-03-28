using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private SistemaSaludJugador sistemaSaludJugador;

    public int CurrentHealth => sistemaSaludJugador != null ? Mathf.RoundToInt(sistemaSaludJugador.SaludActual) : 0;
    public int MaxHealth => sistemaSaludJugador != null ? Mathf.RoundToInt(sistemaSaludJugador.SaludMaxima) : 0;

    private void Awake()
    {
        if (sistemaSaludJugador == null)
        {
            sistemaSaludJugador = FindFirstObjectByType<SistemaSaludJugador>();
        }
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || sistemaSaludJugador == null) return;

        sistemaSaludJugador.AumentarSalud(amount);
        Debug.Log($"Vida curada +{amount}. Vida actual: {CurrentHealth}/{MaxHealth}");
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || sistemaSaludJugador == null) return;

        sistemaSaludJugador.ReducirSaludDirecta(amount);
        Debug.Log($"Daño recibido -{amount}. Vida actual: {CurrentHealth}/{MaxHealth}");
    }
}