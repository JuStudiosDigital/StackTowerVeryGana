using System;
using UnityEngine;

/// <summary>
/// Implementa la mecánica principal del modo Stack Tower.
/// Centraliza el estado del juego, el conteo de progreso y la reacción a eventos del sistema,
/// actuando como punto de coordinación entre subsistemas desacoplados.
/// </summary>
public sealed class StackTowerGameplayMechanic : MonoBehaviour,
    IGameplayMechanic,
    IPieceRewardSource
{
    #region Events

    /// <summary>
    /// Evento que se dispara cuando la mecánica ha finalizado.
    /// </summary>
    public event Action OnMechanicCompleted;

    /// <summary>
    /// Evento que se dispara cuando se genera una recompensa asociada a una pieza.
    /// Proporciona la posición en el mundo donde debe representarse.
    /// </summary>
    public event Action<Vector3> PieceRewardTriggered;

    #endregion

    #region Game State

    /// <summary>
    /// Representa los estados posibles de la mecánica de juego.
    /// </summary>
    private enum GameState
    {
        Playing,
        GameOver
    }

    /// <summary>
    /// Estado actual de la mecánica.
    /// </summary>
    private GameState currentState = GameState.Playing;

    /// <summary>
    /// Indica si la mecánica se encuentra en estado de fin de juego.
    /// </summary>
    public bool IsGameOver => currentState == GameState.GameOver;

    #endregion

    #region State

    /// <summary>
    /// Cantidad de contenedores colocados correctamente durante la sesión actual.
    /// </summary>
    private int placedContainers;

    /// <summary>
    /// Indica si la mecánica ya ha finalizado para evitar múltiples ejecuciones del flujo de cierre.
    /// </summary>
    private bool hasFinished;

    #endregion

    #region Unity

    /// <summary>
    /// Suscribe los manejadores a los eventos relevantes del sistema.
    /// </summary>
    private void OnEnable()
    {
        StackTowerEvents.OnContainerPlaced += HandleContainerPlaced;
        StackTowerEvents.OnCoinCollected += HandleCoinCollected;
        StackTowerEvents.OnGameOver += HandleGameOver;
    }

    /// <summary>
    /// Desuscribe los manejadores para prevenir referencias inválidas.
    /// </summary>
    private void OnDisable()
    {
        StackTowerEvents.OnContainerPlaced -= HandleContainerPlaced;
        StackTowerEvents.OnCoinCollected -= HandleCoinCollected;
        StackTowerEvents.OnGameOver -= HandleGameOver;
    }

    #endregion

    #region Gameplay

    /// <summary>
    /// Inicializa o reinicia la mecánica, estableciendo el estado inicial y limpiando contadores.
    /// </summary>
    public void StartMechanic()
    {
        placedContainers = 0;
        currentState = GameState.Playing;
        hasFinished = false;
    }

    /// <summary>
    /// Obtiene el puntaje actual basado en la cantidad de contenedores colocados.
    /// </summary>
    /// <returns>Puntaje actual del jugador.</returns>
    public int GetScore()
    {
        return placedContainers;
    }

    #endregion

    #region Handlers

    /// <summary>
    /// Maneja el evento de colocación de contenedor incrementando el puntaje.
    /// </summary>
    private void HandleContainerPlaced()
    {
        if (IsGameOver) return;

        placedContainers++;
    }

    /// <summary>
    /// Maneja el evento de recolección de moneda y emite una recompensa asociada.
    /// </summary>
    /// <param name="worldPos">Posición en el mundo donde ocurrió la recolección.</param>
    private void HandleCoinCollected(Vector3 worldPos)
    {
        if (IsGameOver) return;

        PieceRewardTriggered?.Invoke(worldPos);
    }

    /// <summary>
    /// Maneja la transición al estado de fin de juego y notifica la finalización de la mecánica.
    /// </summary>
    private void HandleGameOver()
    {
        if (IsGameOver || hasFinished) return;

        currentState = GameState.GameOver;
        hasFinished = true;

        OnMechanicCompleted?.Invoke();
    }

    #endregion
}