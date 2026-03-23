using System;

public static class GameManagerStackTower
{
    public static bool IsGameOver { get; private set; }

    public static event Action OnGameOver;

    public static void TriggerGameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        OnGameOver?.Invoke();

    }
}