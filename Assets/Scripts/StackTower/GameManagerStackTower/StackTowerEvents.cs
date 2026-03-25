using System;
using UnityEngine;

/// <summary>
/// Canal centralizado de eventos para el sistema Stack Tower.
/// Permite la comunicación desacoplada entre distintos sistemas del juego.
/// </summary>
public static class StackTowerEvents
{
    #region Events

    /// <summary>
    /// Evento que se dispara cuando un contenedor ha sido colocado correctamente.
    /// </summary>
    public static event Action OnContainerPlaced;

    /// <summary>
    /// Evento que se dispara cuando ocurre una condición de fin de juego.
    /// </summary>
    public static event Action OnGameOver;

    /// <summary>
    /// Evento que se dispara cuando una moneda es recolectada.
    /// Proporciona la posición en el mundo donde ocurrió la recolección.
    /// </summary>
    public static event Action<Vector3> OnCoinCollected;

    #endregion

    #region Event Raisers

    /// <summary>
    /// Invoca el evento de contenedor colocado.
    /// </summary>
    public static void RaiseContainerPlaced()
    {
        OnContainerPlaced?.Invoke();
    }

    /// <summary>
    /// Invoca el evento de fin de juego.
    /// </summary>
    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
    }

    /// <summary>
    /// Invoca el evento de recolección de moneda.
    /// </summary>
    /// <param name="position">Posición en el mundo donde se recolectó la moneda.</param>
    public static void RaiseCoinCollected(Vector3 position)
    {
        OnCoinCollected?.Invoke(position);
    }

    #endregion
}