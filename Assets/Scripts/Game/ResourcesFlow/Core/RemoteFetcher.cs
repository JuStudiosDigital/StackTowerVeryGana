using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Responsable de realizar solicitudes HTTP para obtener contenido JSON
/// desde un endpoint remoto mediante métodos GET o POST.
/// </summary>
public class RemoteFetcher
{
    /// <summary>
    /// Ejecuta una solicitud HTTP asíncrona y retorna el JSON recibido.
    /// </summary>
    /// <param name="url">Endpoint de destino para la solicitud.</param>
    /// <param name="postBody">Payload en formato JSON para solicitudes POST. Si es null o vacío, se usa GET.</param>
    /// <param name="onSuccess">Callback invocado con el JSON recibido cuando la solicitud es exitosa.</param>
    /// <param name="onFailure">Callback invocado cuando ocurre un error en la solicitud o la respuesta es inválida.</param>
    /// <returns>Coroutine de Unity que gestiona el ciclo de vida de la solicitud.</returns>
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