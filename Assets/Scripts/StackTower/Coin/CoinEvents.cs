using System;

/// <summary>
/// Canal de eventos para notificar la recolección de monedas.
/// </summary>
public static class CoinEvents
{
    #region Events

    /// <summary>
    /// Evento que se dispara cuando una moneda es recolectada.
    /// </summary>
    public static event Action OnCoinCollected;

    #endregion

    #region Public API

    /// <summary>
    /// Invoca el evento de recolección de moneda.
    /// </summary>
    public static void TriggerCoinCollected()
    {
        OnCoinCollected?.Invoke();
    }

    #endregion
}