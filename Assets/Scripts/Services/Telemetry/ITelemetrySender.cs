using System.Collections.Generic;

/// <summary>
/// Define el contrato para el envío de telemetría.
/// Permite cambiar backend sin afectar el juego.
/// </summary>
public interface ITelemetrySender
{
    void Send(IEnumerable<ITelemetryEvent> events);
}
