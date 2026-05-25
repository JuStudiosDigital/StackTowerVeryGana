using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Envía el payload consolidado de telemetría mediante POST REST.
/// </summary>
public sealed class StatsSender : MonoBehaviour
{
    public static StatsSender Instance { get; private set; }

    [SerializeField]
    [Tooltip("Tiempo máximo de espera para el envío HTTP.")]
    private int timeoutSeconds = 10;

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

    public void Send(GameStatsPayload payload)
    {
        if (payload == null)
            return;

        string endpointUrl = GameManager.Instance.TelemetryUrl;

        if (string.IsNullOrWhiteSpace(endpointUrl))
        {
            Debug.LogError("[StatsSender] No hay TelemetryUrl configurado en GameManager.");
            GameEvents.RaiseTechnicalEvent(TelemetryTechnicalEvents.RemoteUrlInvalid);
            return;
        }

        StartCoroutine(SendRoutine(payload, endpointUrl));
    }

    private IEnumerator SendRoutine(GameStatsPayload payload, string endpointUrl)
    {
        string json = payload.ToJson();
        byte[] body = Encoding.UTF8.GetBytes(json);

        using UnityWebRequest request = new UnityWebRequest(endpointUrl, UnityWebRequest.kHttpVerbPOST);

        request.uploadHandler = new UploadHandlerRaw(body);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.timeout = timeoutSeconds;

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[StatsSender] Error enviando telemetría: {request.error}");
            Debug.LogError($"[StatsSender] Payload fallido: {json}");

            GameEvents.RaiseTechnicalEvent(TelemetryTechnicalEvents.RemoteRequestFailed);
        }
    }
}