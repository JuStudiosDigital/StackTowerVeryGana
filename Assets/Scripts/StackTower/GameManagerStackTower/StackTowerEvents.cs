using System;
using UnityEngine;

/// <summary>
/// Canal centralizado de eventos para el sistema Stack Tower.
/// </summary>
public static class StackTowerEvents
{
    #region Events

    /// <summary>
    /// Evento que se dispara cuando un contenedor ha sido colocado correctamente.
    /// </summary>
    public static event Action<Container> OnContainerPlaced;

    /// <summary>
    /// Evento que se dispara cuando ocurre el fin de la partida.
    /// </summary>
    public static event Action OnGameOver;

    /// <summary>
    /// Evento que se dispara cuando una moneda es recolectada.
    /// </summary>
    public static event Action<Vector3> OnCoinCollected;

    #endregion

    #region Event Raisers

    /// <summary>
    /// Invoca el evento de contenedor colocado.
    /// </summary>
    /// <param name="container">Contenedor que ha sido colocado.</param>
    public static void RaiseContainerPlaced(Container container)
    {
        OnContainerPlaced?.Invoke(container);
    }

    /// <summary>
    /// Invoca el evento de fin de juego y reproduce el audio asociado.
    /// </summary>
    public static void RaiseGameOver()
    {
        OnGameOver?.Invoke();
        GameManager.Instance.AudioManager?.Play(AudioTypeGame.EndStackTower);
    }

    /// <summary>
    /// Invoca el evento de recolección de moneda.
    /// </summary>
    /// <param name="position">Posición en el mundo donde ocurrió la recolección.</param>
    public static void RaiseCoinCollected(Vector3 position)
    {
        OnCoinCollected?.Invoke(position);
    }

    #endregion
}