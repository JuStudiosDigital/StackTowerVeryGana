using System;

/// <summary>
/// Eventos relacionados con monedas.
/// </summary>
public static class CoinEvents
{
    public static event Action OnCoinCollected;

    public static void TriggerCoinCollected()
    {
        OnCoinCollected?.Invoke();
    }
}