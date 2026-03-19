using System;

/// <summary>
/// Evento emitido al finalizar o abandonar un nivel.
/// </summary>
public sealed class LevelResultTelemetryEvent : ITelemetryEvent
{
    public TelemetryEventType EventType => TelemetryEventType.LevelResult;
    public long Timestamp { get; }

    private readonly string sessionId;
    private readonly string levelId;
    private readonly bool completed;
    private readonly float playTime;
    private readonly int moves;
    private readonly int coinsEarned;

    public LevelResultTelemetryEvent(
        string sessionId,
        string levelId,
        bool completed,
        float playTime,
        int moves,
        int coinsEarned)
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.sessionId = sessionId;
        this.levelId = levelId;
        this.completed = completed;
        this.playTime = playTime;
        this.moves = moves;
        this.coinsEarned = coinsEarned;
    }

    public object ToPayload()
    {
        return new
        {
            type = EventType.ToString(),
            session_id = sessionId,
            level_id = levelId,
            completed,
            play_time = playTime,
            moves,
            coins_earned = coinsEarned,
            timestamp = Timestamp
        };
    }
}
