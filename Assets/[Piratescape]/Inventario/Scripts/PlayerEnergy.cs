using System;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    public event Action OnEnergyChanged;

    [Header("Configuracion de energia")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;

    public float MaxEnergy => maxEnergy;
    public float CurrentEnergy => currentEnergy;
    public float NormalizedEnergy => maxEnergy > 0 ? currentEnergy / maxEnergy : 0f;

    private void Start()
    {
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
        NotifyEnergyChanged();
    }

    public bool HasEnoughEnergy(float amount)
    {
        if (amount <= 0f)
        {
            return true;
        }

        return currentEnergy >= amount;
    }

    public bool TryUseEnergy(float amount)
    {
        if (amount <= 0f)
        {
            return true;
        }

        if (!HasEnoughEnergy(amount))
        {
            return false;
        }

        currentEnergy -= amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        NotifyEnergyChanged();
        return true;
    }

    public void RestoreEnergy(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentEnergy += amount;
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        NotifyEnergyChanged();
    }

    public void FillEnergy()
    {
        currentEnergy = maxEnergy;
        NotifyEnergyChanged();
    }

    public void SetEnergy(float amount)
    {
        currentEnergy = Mathf.Clamp(amount, 0f, maxEnergy);
        NotifyEnergyChanged();
    }

    private void NotifyEnergyChanged()
    {
        OnEnergyChanged?.Invoke();
    }
}