using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private SistemaSaludJugador sistemaSaludJugador;

    public int CurrentHealth => sistemaSaludJugador != null ? Mathf.RoundToInt(sistemaSaludJugador.SaludActual) : 0;
    public int MaxHealth => sistemaSaludJugador != null ? Mathf.RoundToInt(sistemaSaludJugador.SaludMaxima) : 0;
    public bool GodModeActivo => sistemaSaludJugador != null && sistemaSaludJugador.ModoDiosActivo;

    private void Awake()
    {
        BuscarSistemaSaludSiFalta();
    }

    public void Heal(int amount)
    {
        BuscarSistemaSaludSiFalta();

        if (amount <= 0 || sistemaSaludJugador == null)
        {
            return;
        }

        sistemaSaludJugador.AumentarSalud(amount);
        Debug.Log($"Vida curada +{amount}. Vida actual: {CurrentHealth}/{MaxHealth}");
    }

    public void TakeDamage(int amount)
    {
        BuscarSistemaSaludSiFalta();

        if (amount <= 0 || sistemaSaludJugador == null)
        {
            return;
        }

        if (GodModeActivo)
        {
            Debug.Log("GodMode activo: daño ignorado.");
            return;
        }

        sistemaSaludJugador.ReducirSaludDirecta(amount);
        Debug.Log($"Daño recibido -{amount}. Vida actual: {CurrentHealth}/{MaxHealth}");
    }

    public void SetGodMode(bool activo)
    {
        BuscarSistemaSaludSiFalta();

        if (sistemaSaludJugador == null)
        {
            Debug.LogWarning("PlayerHealth: falta referencia a SistemaSaludJugador.");
            return;
        }

        sistemaSaludJugador.EstablecerModoDios(activo);
    }

    private void BuscarSistemaSaludSiFalta()
    {
        if (sistemaSaludJugador == null)
        {
            sistemaSaludJugador = FindFirstObjectByType<SistemaSaludJugador>();
        }
    }
}