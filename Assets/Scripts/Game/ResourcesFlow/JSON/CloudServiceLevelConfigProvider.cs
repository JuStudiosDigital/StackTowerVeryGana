using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Implementación que obtiene la configuración
/// mediante un servicio REST en la nube.
/// Envía un payload base y recibe el JSON final.
/// </summary>
public class CloudServiceLevelConfigProvider : ILevelConfigProvider
{
    private readonly string serviceUrl;

    /// <summary>
    /// Inicializa el proveedor con la URL del endpoint cloud.
    /// </summary>
    public CloudServiceLevelConfigProvider(string serviceUrl)
    {
        this.serviceUrl = serviceUrl;
    }

    public IEnumerator GetLevelConfigAsync(
        Action<LevelConfigData> onSuccess,
        Action onFailure)
    {
        LevelConfigRequestDto requestDto = new LevelConfigRequestDto
        {
            sessionToken = GameManager.Instance.SessionToken,
            userHash = GameManager.Instance.UserHash,
            isBrandedMode = GameManager.Instance.IsBrandedMode,
            campaignId = GameManager.Instance.CampaignId,
            gameTitle = GameManager.Instance.GameTitle
        };

        string jsonBody = JsonUtility.ToJson(requestDto);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest(serviceUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            DevLog.Warning(
                $"[CloudServiceLevelConfigProvider] Error: {request.error}"
            );

            onFailure?.Invoke();
            yield break;
        }

        string responseJson = request.downloadHandler.text;

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            onFailure?.Invoke();
            yield break;
        }

        LevelConfigData data;

        try
        {
            data = JsonUtility.FromJson<LevelConfigData>(responseJson);
        }
        catch (Exception exception)
        {
            DevLog.Error(
                $"[CloudServiceLevelConfigProvider] Error deserializando: {exception.Message}"
            );

            onFailure?.Invoke();
            yield break;
        }

        if (data == null)
        {
            onFailure?.Invoke();
            yield break;
        }

        onSuccess?.Invoke(LevelConfigNormalizer.Normalize(data));
    }
}
