using System;

/// <summary>
/// Evento emitido una única vez al iniciar una sesión de juego.
/// </summary>
public sealed class SessionStartTelemetryEvent : ITelemetryEvent
{
    public TelemetryEventType EventType => TelemetryEventType.SessionStart;
    public long Timestamp { get; }

    private readonly string sessionId;
    private readonly string userHash;
    private readonly string gameTitle;
    private readonly string campaignId;
    private readonly float loadTime;

    public SessionStartTelemetryEvent(
        string sessionId,
        string userHash,
        string gameTitle,
        string campaignId,
        float loadTime)
    {
        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.sessionId = sessionId;
        this.userHash = userHash;
        this.gameTitle = gameTitle;
        this.campaignId = campaignId;
        this.loadTime = loadTime;
    }

    public object ToPayload()
    {
        return new
        {
            type = EventType.ToString(),
            session_id = sessionId,
            user_hash = userHash,
            game_id = gameTitle,
            campaign_id = campaignId,
            load_time = loadTime,
            timestamp = Timestamp
        };
    }
}
