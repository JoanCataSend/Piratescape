using System;
using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    public event Action OnEnergyChanged;

    [Header("Referencias")]
    [SerializeField] private VisualizadorBarrasEstado visualizador;
    [SerializeField] private movimientoplayer movimientoPlayer;

    [Header("Configuracion de energia")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float currentEnergy = 100f;

    [Header("Desgaste al correr")]
    [SerializeField] private float energyLossPerSecondWhileSprinting = 10f;

    public float MaxEnergy => maxEnergy;
    public float CurrentEnergy => currentEnergy;
    public float NormalizedEnergy => maxEnergy > 0 ? currentEnergy / maxEnergy : 0f;

    private void Awake()
    {
        if (movimientoPlayer == null)
        {
            movimientoPlayer = GetComponent<movimientoplayer>();
        }
    }

    private void Start()
    {
        maxEnergy = Mathf.Max(0f, maxEnergy);
        currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);

        NotifyEnergyChanged();
    }

    private void Update()
    {
        if (movimientoPlayer == null)
        {
            return;
        }

        if (movimientoPlayer.IsActuallySprinting())
        {
            float energyToUse = energyLossPerSecondWhileSprinting * Time.deltaTime;
            TryUseEnergy(energyToUse);
        }
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
        if (visualizador != null)
        {
            visualizador.EstablecerEnergia(currentEnergy, maxEnergy);
        }

        OnEnergyChanged?.Invoke();
    }
}