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
    public float NormalizedEnergy => maxEnergy > 0f ? currentEnergy / maxEnergy : 0f;

    private void Awake()
    {
        CachearReferencias();
    }

    private void Start()
    {
        ValidarValoresIniciales();
        NotifyEnergyChanged();
    }

    private void Update()
    {
        if (movimientoPlayer == null)
        {
            return;
        }

        if (!movimientoPlayer.IsActuallySprinting())
        {
            return;
        }

        float energiaAGastar = energyLossPerSecondWhileSprinting * Time.deltaTime;
        TryUseEnergy(energiaAGastar);
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

        currentEnergy = LimitarEnergiaActual(currentEnergy - amount);
        NotifyEnergyChanged();

        return true;
    }

    public void RestoreEnergy(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        currentEnergy = LimitarEnergiaActual(currentEnergy + amount);
        NotifyEnergyChanged();
    }

    public void FillEnergy()
    {
        currentEnergy = maxEnergy;
        NotifyEnergyChanged();
    }

    public void SetEnergy(float amount)
    {
        currentEnergy = LimitarEnergiaActual(amount);
        NotifyEnergyChanged();
    }

    private void CachearReferencias()
    {
        if (movimientoPlayer == null)
        {
            movimientoPlayer = GetComponent<movimientoplayer>();
        }
    }

    private void ValidarValoresIniciales()
    {
        maxEnergy = LimitarEnergiaMaxima(maxEnergy);
        currentEnergy = LimitarEnergiaActual(currentEnergy);
    }

    private float LimitarEnergiaMaxima(float valor)
    {
        return Mathf.Max(0f, valor);
    }

    private float LimitarEnergiaActual(float valor)
    {
        return Mathf.Clamp(valor, 0f, maxEnergy);
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