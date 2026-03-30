using System;
using UnityEngine;

/// <summary>
/// Orquesta el estado y flujo principal del modo Stack Tower.
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
    /// Evento que se dispara cuando se genera una recompensa asociada a una moneda recolectada.
    /// </summary>
    public event Action<Vector3> PieceRewardTriggered;

    #endregion

    #region Game State

    /// <summary>
    /// Define los estados posibles de la mecánica.
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
    /// Cantidad de contenedores colocados correctamente.
    /// </summary>
    private int placedContainers;

    /// <summary>
    /// Indica si la mecánica ya ha finalizado.
    /// </summary>
    private bool hasFinished;

    #endregion

    #region Unity

    /// <summary>
    /// Suscribe los manejadores a los eventos del sistema.
    /// </summary>
    private void OnEnable()
    {
        StackTowerEvents.OnContainerPlaced += HandleContainerPlaced;
        StackTowerEvents.OnCoinCollected += HandleCoinCollected;
        StackTowerEvents.OnGameOver += HandleGameOver;
    }

    /// <summary>
    /// Desuscribe los manejadores de los eventos del sistema.
    /// </summary>
    private void OnDisable()
    {
        StackTowerEvents.OnContainerPlaced -= HandleContainerPlaced;
        StackTowerEvents.OnCoinCollected -= HandleCoinCollected;
        StackTowerEvents.OnGameOver -= HandleGameOver;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Inicializa o reinicia el estado de la mecánica.
    /// </summary>
    public void StartMechanic()
    {
        placedContainers = 0;
        currentState = GameState.Playing;
        hasFinished = false;
    }

    /// <summary>
    /// Obtiene el puntaje actual basado en contenedores colocados.
    /// </summary>
    /// <returns>Puntaje actual.</returns>
    public int GetScore()
    {
        return placedContainers;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Incrementa el contador de contenedores colocados.
    /// </summary>
    /// <param name="_">Contenedor que ha sido colocado.</param>
    private void HandleContainerPlaced(Container _)
    {
        if (IsGameOver) return;

        placedContainers++;
    }

    /// <summary>
    /// Propaga el evento de recolección de moneda.
    /// </summary>
    /// <param name="worldPos">Posición en el mundo donde ocurrió la recolección.</param>
    private void HandleCoinCollected(Vector3 worldPos)
    {
        if (IsGameOver) return;

        PieceRewardTriggered?.Invoke(worldPos);
    }

    /// <summary>
    /// Maneja la transición al estado de fin de juego.
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