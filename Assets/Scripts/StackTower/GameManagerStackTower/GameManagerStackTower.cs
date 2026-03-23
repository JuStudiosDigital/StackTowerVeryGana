using UnityEngine;
using System;

/// <summary>
/// Gestiona el estado global del juego.
/// </summary>
public class GameManagerStackTower : MonoBehaviour
{
    public static GameManagerStackTower Instance { get; private set; }

    /// <summary>
    /// Indica si el juego terminó.
    /// </summary>
    public static bool IsGameOver => Instance != null && Instance.currentState == GameState.GameOver;

    /// <summary>
    /// Evento cuando el juego termina.
    /// </summary>
    public static event Action OnGameOver;

    private enum GameState
    {
        Playing,
        GameOver
    }

    private GameState currentState = GameState.Playing;

    private void Awake()
    {
        // Singleton seguro
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Dispara el Game Over.
    /// </summary>
    public static void TriggerGameOver()
    {
        if (Instance == null) return;

        Instance.SetGameOver();
    }

    private void SetGameOver()
    {
        if (currentState == GameState.GameOver) return;

        currentState = GameState.GameOver;

        OnGameOver?.Invoke();
    }

    /// <summary>
    /// Reinicia el estado del juego.
    /// </summary>
    public void RestartGame()
    {
        currentState = GameState.Playing;

        // Opcional: resetear tiempo si usas pausas
        Time.timeScale = 1f;
    }
}