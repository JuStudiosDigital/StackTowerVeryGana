using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sistema central de telemetría.
/// Acumula, agrupa y envía eventos en momentos seguros.
/// </summary>
public sealed class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    private readonly Queue<ITelemetryEvent> eventQueue =
        new Queue<ITelemetryEvent>();

    private ITelemetrySender telemetrySender;
    private string sessionId;
    private float sessionStartTime;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Inicializa la sesión de telemetría.
    /// </summary>
    public void StartSession(
        string userHash,
        string gameTitle,
        string campaignId,
        float loadTime,
        ITelemetrySender sender)
    {
        telemetrySender = sender;
        sessionId = Guid.NewGuid().ToString();
        sessionStartTime = Time.realtimeSinceStartup;

        EnqueueEvent(new SessionStartTelemetryEvent(
            sessionId,
            userHash,
            gameTitle,
            campaignId,
            loadTime
        ));
    }

    /// <summary>
    /// Registra un resultado de nivel.
    /// </summary>
    public void TrackLevelResult(
        string levelId,
        bool completed,
        float playTime,
        int moves,
        int coinsEarned)
    {
        EnqueueEvent(new LevelResultTelemetryEvent(
            sessionId,
            levelId,
            completed,
            playTime,
            moves,
            coinsEarned
        ));

        Flush();
    }

    /// <summary>
    /// Finaliza la sesión y envía los datos pendientes.
    /// </summary>
    public void EndSession()
    {
        float totalPlayTime =
            Time.realtimeSinceStartup - sessionStartTime;

        EnqueueEvent(new SessionEndTelemetryEvent(
            sessionId,
            totalPlayTime
        ));

        Flush();
    }

    private void EnqueueEvent(ITelemetryEvent telemetryEvent)
    {
        eventQueue.Enqueue(telemetryEvent);
    }

    private void Flush()
    {
        if (eventQueue.Count == 0 || telemetrySender == null)
        {
            return;
        }

        telemetrySender.Send(eventQueue);
        eventQueue.Clear();
    }
}
