using System;
using UnityEngine;

/// <summary>
/// Canal de eventos del Stack Tower.
/// </summary>
public static class StackTowerEvents
{
    public static event Action OnContainerPlaced;
    public static event Action OnGameOver;

    /// <summary>
    /// Evento cuando se recolecta moneda con posición.
    /// </summary>
    public static event Action<Vector3> OnCoinCollected;

    public static void RaiseContainerPlaced()
    {
        OnContainerPlaced?.Invoke();
    }

    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
    }

    public static void RaiseCoinCollected(Vector3 position)
    {
        OnCoinCollected?.Invoke(position);
    }
}