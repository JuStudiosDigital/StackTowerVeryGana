using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Implementación HTTP simple para envío de telemetría.
/// </summary>
public sealed class HttpTelemetrySender : ITelemetrySender
{
    private readonly string endpointUrl;

    public HttpTelemetrySender(string endpointUrl)
    {
        this.endpointUrl = endpointUrl;
    }

    public void Send(IEnumerable<ITelemetryEvent> events)
    {
        var payload = new
        {
            events = events.Select(e => e.ToPayload())
        };

        string json = JsonUtility.ToJson(payload);
        UnityWebRequest request =
            new UnityWebRequest(endpointUrl, "POST");

        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        request.SendWebRequest();
    }
}
