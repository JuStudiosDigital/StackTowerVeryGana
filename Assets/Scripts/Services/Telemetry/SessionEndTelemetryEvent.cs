using System;

/// <summary>
/// Evento emitido cuando la sesión termina.
/// </summary>
public sealed class SessionEndTelemetryEvent : ITelemetryEvent
{
    public TelemetryEventType EventType => TelemetryEventType.SessionEnd;
    public long Timestamp { get; }

    private readonly string sessionId;
    private readonly float totalPlayTime;

    public SessionEndTelemetryEvent(string sessionId, float totalPlayTime)
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.sessionId = sessionId;
        this.totalPlayTime = totalPlayTime;
    }

    public object ToPayload()
    {
        return new
        {
            type = EventType.ToString(),
            session_id = sessionId,
            total_play_time = totalPlayTime,
            timestamp = Timestamp
        };
    }
}
