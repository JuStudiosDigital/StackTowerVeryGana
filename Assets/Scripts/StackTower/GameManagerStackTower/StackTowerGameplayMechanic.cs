using System;
using UnityEngine;

/// <summary>
/// Mecánica principal del Stack Tower.
/// 
/// Unifica:
/// - Estado del juego (antes GameManagerStackTower)
/// - Eventos del gameplay
/// - Conteo de score
/// - Comunicación con GamePlayManager
/// </summary>
public sealed class StackTowerGameplayMechanic : MonoBehaviour,
    IGameplayMechanic,
    IPieceRewardSource
{
    #region Events

    public event Action OnMechanicCompleted;
    public event Action<Vector3> PieceRewardTriggered;

    #endregion

    #region Game State

    private enum GameState
    {
        Playing,
        GameOver
    }

    private GameState currentState = GameState.Playing;

    public bool IsGameOver => currentState == GameState.GameOver;

    #endregion

    #region State

    private int placedContainers;
    private bool hasFinished;

    #endregion

    #region Unity

    private void OnEnable()
    {
        StackTowerEvents.OnContainerPlaced += HandleContainerPlaced;
        StackTowerEvents.OnCoinCollected += HandleCoinCollected;
        StackTowerEvents.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        StackTowerEvents.OnContainerPlaced -= HandleContainerPlaced;
        StackTowerEvents.OnCoinCollected -= HandleCoinCollected;
        StackTowerEvents.OnGameOver -= HandleGameOver;
    }

    #endregion

    #region Gameplay

    public void StartMechanic()
    {
        placedContainers = 0;
        currentState = GameState.Playing;
        hasFinished = false;
    }

    public int GetScore()
    {
        return placedContainers;
    }

    #endregion

    #region Handlers

    private void HandleContainerPlaced()
    {
        if (IsGameOver) return;

        placedContainers++;
    }

    private void HandleCoinCollected(Vector3 worldPos)
    {
        if (IsGameOver) return;

        PieceRewardTriggered?.Invoke(worldPos);
    }

    private void HandleGameOver()
    {
        if (IsGameOver || hasFinished) return;

        currentState = GameState.GameOver;
        hasFinished = true;

        OnMechanicCompleted?.Invoke();
    }

    #endregion
}