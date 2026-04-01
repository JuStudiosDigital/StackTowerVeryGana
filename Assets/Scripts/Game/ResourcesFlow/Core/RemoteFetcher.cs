using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Encargado de obtener JSON desde distintas fuentes.
/// </summary>
public class RemoteFetcher
{
    public IEnumerator Fetch(
        string url,
        string postBody,
        Action<string> onSuccess,
        Action onFailure)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            DevLog.Error("[RemoteFetcher] URL inválida o vacía");
            onFailure?.Invoke();
            yield break;
        }

        UnityWebRequest request;

        bool isPost = !string.IsNullOrEmpty(postBody);

        DevLog.Log($"[RemoteFetcher] Iniciando request → {(isPost ? "POST" : "GET")} | URL: {url}");

        if (isPost)
        {
            DevLog.Log($"[RemoteFetcher] Payload enviado: {postBody}");
        }

        if (!isPost)
        {
            request = UnityWebRequest.Get(url);
        }
        else
        {
            byte[] body = System.Text.Encoding.UTF8.GetBytes(postBody);

            request = new UnityWebRequest(url, "POST");
            request.uploadHandler = new UploadHandlerRaw(body);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
        }

        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            DevLog.Warning(
                $"[RemoteFetcher] ❌ Error en request\n" +
                $"URL: {url}\n" +
                $"Result: {request.result}\n" +
                $"Error: {request.error}"
            );

            onFailure?.Invoke();
            yield break;
        }

        string json = request.downloadHandler.text;

        if (string.IsNullOrWhiteSpace(json))
        {
            DevLog.Warning($"[RemoteFetcher] ⚠️ JSON vacío recibido desde: {url}");
            onFailure?.Invoke();
            yield break;
        }

        DevLog.Log(
            $"[RemoteFetcher] JSON recibido correctamente\n" +
            $"URL: {url}\n" +
            $"Length: {json.Length}"
        );

        /// (Opcional) Log parcial para debug sin romper consola
        DevLog.Log($"[RemoteFetcher] Preview JSON: {json.ToString()}");

        onSuccess?.Invoke(json);
    }
}