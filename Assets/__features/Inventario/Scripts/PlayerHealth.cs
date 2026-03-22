using UnityEngine;
using UnityEngine.UI;
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;
    [SerializeField] private Slider healthBar;
    [SerializeField] private VisualizadorBarrasEstado visualizadorBarras;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void Heal(int amount)
    {
        if (amount <= 0) return;

        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;

        // Actualiza la barra de vida visual
        if (visualizadorBarras != null)
            visualizadorBarras.EstablecerSaludActual(currentHealth);

        Debug.Log("Vida actual: " + currentHealth);
    }

    
    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;

        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        // Actualiza barra visual
        if (visualizadorBarras != null)
            visualizadorBarras.EstablecerSaludActual(currentHealth);

        Debug.Log("Vida actual: " + currentHealth);
    }
    void Start()
    {
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
    }
    void UpdateHealthBar()
    {
        healthBar.value = currentHealth;
    }
}