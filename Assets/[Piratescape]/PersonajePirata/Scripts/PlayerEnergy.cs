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

    [Header("Penalizacion por energia vacia")]
    [SerializeField] private float movementMultiplierWhenExhausted = 0.5f;

    public float MaxEnergy => maxEnergy;
    public float CurrentEnergy => currentEnergy;
    public float NormalizedEnergy => maxEnergy > 0f ? currentEnergy / maxEnergy : 0f;
    public bool IsExhausted => currentEnergy <= 0f;

    private void Awake()
    {
        CachearReferencias();
    }

    private void Start()
    {
        ValidarValoresIniciales();
        AplicarEstadoMovimiento();
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
            currentEnergy = 0f;
            AplicarEstadoMovimiento();
            NotifyEnergyChanged();
            return false;
        }

        currentEnergy = LimitarEnergiaActual(currentEnergy - amount);
        AplicarEstadoMovimiento();
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
        AplicarEstadoMovimiento();
        NotifyEnergyChanged();
    }

    public void FillEnergy()
    {
        currentEnergy = maxEnergy;
        AplicarEstadoMovimiento();
        NotifyEnergyChanged();
    }

    public void SetEnergy(float amount)
    {
        currentEnergy = LimitarEnergiaActual(amount);
        AplicarEstadoMovimiento();
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
        movementMultiplierWhenExhausted = Mathf.Clamp(movementMultiplierWhenExhausted, 0.1f, 1f);
    }

    private float LimitarEnergiaMaxima(float valor)
    {
        return Mathf.Max(0f, valor);
    }

    private float LimitarEnergiaActual(float valor)
    {
        return Mathf.Clamp(valor, 0f, maxEnergy);
    }

    private void AplicarEstadoMovimiento()
    {
        if (movimientoPlayer == null)
        {
            return;
        }

        if (IsExhausted)
        {
            movimientoPlayer.SetCanSprint(false);
            movimientoPlayer.SetEnergySpeedMultiplier(movementMultiplierWhenExhausted);
        }
        else
        {
            movimientoPlayer.SetCanSprint(true);
            movimientoPlayer.SetEnergySpeedMultiplier(1f);
        }
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