using System;

/// <summary>
/// Resultado final enviado al sistema de telemetría.
/// </summary>
public enum GameTelemetryResult
{
    Win,
    Lose,
    Draw,
    Abandoned
}

/// <summary>
/// Bus de eventos global para telemetría de gameplay.
/// </summary>
public static class GameEvents
{
    public static event Action OnLoadStarted;
    public static event Action OnLoadFinished;

    public static event Action OnGameStarted;
    public static event Action<GameTelemetryResult> OnGameEnded;

    public static event Action<int> OnKeysEarned;
    public static event Action<int> OnKeysTotalUpdated;
    public static event Action<float> OnGameTimeUpdated;
    public static event Action<float> OnPerformanceUpdated;
    public static event Action OnBrandResourceUsed;
    public static event Action<string> OnTechnicalEvent;

    public static void RaiseLoadStarted() => OnLoadStarted?.Invoke();

    public static void RaiseLoadFinished() => OnLoadFinished?.Invoke();

    public static void RaiseGameStarted() => OnGameStarted?.Invoke();

    public static void RaiseGameEnded(GameTelemetryResult result) => OnGameEnded?.Invoke(result);

    public static void RaiseKeysEarned(int amount) => OnKeysEarned?.Invoke(amount);

    public static void RaiseKeysTotal(int total) => OnKeysTotalUpdated?.Invoke(total);

    public static void RaiseGameTime(float seconds) => OnGameTimeUpdated?.Invoke(seconds);

    public static void RaisePerformance(float value) => OnPerformanceUpdated?.Invoke(value);

    public static void RaiseBrandResourceUsed() => OnBrandResourceUsed?.Invoke();

    public static void RaiseTechnicalEvent(string eventType) => OnTechnicalEvent?.Invoke(eventType);
}