using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        Debug.Log("Vida actual: " + currentHealth);
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentHealth -= amount;

        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        Debug.Log("Vida actual: " + currentHealth);
    }
}