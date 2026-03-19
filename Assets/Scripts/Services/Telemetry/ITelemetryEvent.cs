/// <summary>
/// Contrato base para cualquier evento de telemetría.
/// Garantiza consistencia y serialización.
/// </summary>
public interface ITelemetryEvent
{
    /// <summary>
    /// Tipo del evento.
    /// </summary>
    TelemetryEventType EventType { get; }

    /// <summary>
    /// Momento en que ocurrió el evento (UTC).
    /// </summary>
    long Timestamp { get; }

    /// <summary>
    /// Convierte el evento a un payload serializable.
    /// </summary>
    object ToPayload();
}
