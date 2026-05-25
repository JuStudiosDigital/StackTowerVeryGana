using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Acumula telemetría de carga, sesión, gameplay y eventos técnicos.
/// Persiste entre escenas para medir correctamente cargas iniciadas antes del gameplay.
/// </summary>
public sealed class StatsCollector : MonoBehaviour
{
    public static StatsCollector Instance { get; private set; }

    private bool hasActiveSession;
    private bool isLoadRunning;

    private float loadStartRealtime;
    private float pendingLoadTime;

    private long sessionStartTimestamp;
    private float sessionStartRealtime;

    private int keysEarned;
    private string result;
    private float gameTime;
    private float performance;

    private readonly List<string> technicalEvents = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.OnLoadStarted += HandleLoadStarted;
        GameEvents.OnLoadFinished += HandleLoadFinished;

        GameEvents.OnGameStarted += HandleGameStarted;
        GameEvents.OnGameEnded += HandleGameEnded;

        GameEvents.OnKeysEarned += HandleKeysEarned;
        GameEvents.OnKeysTotalUpdated += HandleKeysTotalUpdated;
        GameEvents.OnGameTimeUpdated += HandleGameTimeUpdated;
        GameEvents.OnPerformanceUpdated += HandlePerformanceUpdated;
        GameEvents.OnBrandResourceUsed += HandleBrandResourceUsed;
        GameEvents.OnTechnicalEvent += HandleTechnicalEvent;
    }

    private void OnDisable()
    {
        GameEvents.OnLoadStarted -= HandleLoadStarted;
        GameEvents.OnLoadFinished -= HandleLoadFinished;

        GameEvents.OnGameStarted -= HandleGameStarted;
        GameEvents.OnGameEnded -= HandleGameEnded;

        GameEvents.OnKeysEarned -= HandleKeysEarned;
        GameEvents.OnKeysTotalUpdated -= HandleKeysTotalUpdated;
        GameEvents.OnGameTimeUpdated -= HandleGameTimeUpdated;
        GameEvents.OnPerformanceUpdated -= HandlePerformanceUpdated;
        GameEvents.OnBrandResourceUsed -= HandleBrandResourceUsed;
        GameEvents.OnTechnicalEvent -= HandleTechnicalEvent;
    }

    private void HandleLoadStarted()
    {
        isLoadRunning = true;
        loadStartRealtime = Time.realtimeSinceStartup;
    }

    private void HandleLoadFinished()
    {
        if (!isLoadRunning)
            return;

        pendingLoadTime = Time.realtimeSinceStartup - loadStartRealtime;
        isLoadRunning = false;
    }

    private void HandleGameStarted()
    {
        sessionStartTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        sessionStartRealtime = Time.realtimeSinceStartup;
        hasActiveSession = true;

        keysEarned = 0;
        result = string.Empty;
        gameTime = 0f;
        performance = 0f;
        technicalEvents.Clear();
    }

    private void HandleGameEnded(GameTelemetryResult telemetryResult)
    {
        if (!hasActiveSession)
            return;

        long endTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        result = ConvertResult(telemetryResult);

        GameStatsPayload payload = BuildPayload(endTimestamp);

        StatsSender.Instance.Send(payload);

        hasActiveSession = false;
        pendingLoadTime = 0f;
    }

    private GameStatsPayload BuildPayload(long endTimestamp)
    {
        GameStatsPayload payload = new GameStatsPayload
        {
            SessionToken = GameManager.Instance.SessionToken,
            UserHash = GameManager.Instance.UserHash,
            IsBrandedMode = GameManager.Instance.IsBrandedMode,
            CampaignId = GameManager.Instance.CampaignId,
            GameTitle = GameManager.Instance.GameID,
            ErrorCount = technicalEvents.Count,
            ErrorTypes = technicalEvents.ToArray(),
            LoadTime = pendingLoadTime
        };

        payload.AddDouble("start_time", sessionStartTimestamp);
        payload.AddDouble("end_time", endTimestamp);
        payload.AddDouble("play_time", Time.realtimeSinceStartup - sessionStartRealtime);
        payload.AddInt("keys_earned", keysEarned);
        payload.AddString("result", result);
        payload.AddDouble("game_time", gameTime);
        payload.AddDouble("performance", performance);

        return payload;
    }

    private void HandleKeysEarned(int amount)
    {
        if (!hasActiveSession)
            return;

        keysEarned += Mathf.Max(0, amount);
    }

    private void HandleKeysTotalUpdated(int total)
    {
    }

    private void HandleGameTimeUpdated(float seconds)
    {
        if (!hasActiveSession)
            return;

        gameTime = Mathf.Max(0f, seconds);
    }

    private void HandlePerformanceUpdated(float value)
    {
        if (!hasActiveSession)
            return;

        performance = Mathf.Max(0f, value);
    }

    private void HandleBrandResourceUsed()
    {
    }

    private void HandleTechnicalEvent(string eventType)
    {
        if (string.IsNullOrWhiteSpace(eventType))
            return;

        technicalEvents.Add(eventType);
    }

    private static string ConvertResult(GameTelemetryResult telemetryResult)
    {
        return telemetryResult switch
        {
            GameTelemetryResult.Win => "win",
            GameTelemetryResult.Lose => "lose",
            GameTelemetryResult.Draw => "draw",
            GameTelemetryResult.Abandoned => "abandoned",
            _ => "unknown"
        };
    }
}