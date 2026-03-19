using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Servicio utilitario responsable de descargar recursos remotos
/// (JSON y assets binarios) de forma segura y tipada,
/// aplicando validaciones defensivas para evitar fallos en tiempo de ejecución.
/// 
/// Ahora soporta:
/// - Descarga JSON vía GET (compatibilidad legacy).
/// - Descarga JSON vía POST con payload (servicio en la nube).
/// - Descarga de assets binarios (Texture2D y AudioClip).
/// 
/// La clase mantiene compatibilidad retroactiva con el flujo existente,
/// permitiendo migración progresiva hacia servicios REST.
/// </summary>
public class RemoteResourceDownloader
{
    #region JSON DOWNLOAD (GET - Legacy Support)

    /// <summary>
    /// Descarga un archivo JSON desde una URL remota usando método GET
    /// y lo deserializa de forma segura al tipo solicitado.
    /// 
    /// Este método mantiene compatibilidad con el sistema anterior
    /// basado en acceso directo a archivos JSON en hosting.
    /// 
    /// Ante cualquier fallo (URL inválida, error de red, JSON vacío
    /// o error de deserialización), se ejecuta el callback de fallo
    /// para permitir aplicar lógica de fallback.
    /// </summary>
    public IEnumerator DownloadJson<T>(
        string url,
        Action<T> onSuccess,
        Action onFailure)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            onFailure?.Invoke();
            yield break;
        }

        using UnityWebRequest request = UnityWebRequest.Get(url);

        yield return request.SendWebRequest();

        yield return HandleJsonResponse(request, url, onSuccess, onFailure);
    }

    #endregion

    #region JSON DOWNLOAD (POST - Cloud Service)

    /// <summary>
    /// Envía un request HTTP POST con un payload serializado en JSON
    /// y deserializa la respuesta al tipo solicitado.
    /// 
    /// Este método está diseñado para integración con servicios REST
    /// donde el backend genera dinámicamente la configuración final.
    /// 
    /// Permite incluir headers personalizados (ej. Authorization).
    /// </summary>
    /// <typeparam name="TResponse">Tipo de respuesta esperado.</typeparam>
    /// <typeparam name="TRequest">Tipo del payload enviado.</typeparam>
    /// <param name="url">Endpoint del servicio.</param>
    /// <param name="requestPayload">Objeto que será serializado a JSON.</param>
    /// <param name="headers">Headers opcionales adicionales.</param>
    /// <param name="onSuccess">Callback ejecutado si la respuesta es válida.</param>
    /// <param name="onFailure">Callback ejecutado ante error.</param>
    public IEnumerator PostJson<TResponse, TRequest>(
        string url,
        TRequest requestPayload,
        System.Collections.Generic.Dictionary<string, string> headers,
        Action<TResponse> onSuccess,
        Action onFailure)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            onFailure?.Invoke();
            yield break;
        }

        string jsonBody = JsonUtility.ToJson(requestPayload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using UnityWebRequest request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        if (headers != null)
        {
            foreach (var pair in headers)
            {
                request.SetRequestHeader(pair.Key, pair.Value);
            }
        }

        yield return request.SendWebRequest();

        yield return HandleJsonResponse(request, url, onSuccess, onFailure);
    }

    #endregion

    #region JSON INTERNAL HANDLING

    /// <summary>
    /// Maneja la validación común y deserialización
    /// de respuestas JSON para GET y POST.
    /// 
    /// Centraliza validaciones para evitar duplicación de lógica.
    /// </summary>
    private IEnumerator HandleJsonResponse<T>(
        UnityWebRequest request,
        string url,
        Action<T> onSuccess,
        Action onFailure)
    {
        if (request.result != UnityWebRequest.Result.Success)
        {
            DevLog.Warning(
                $"[RemoteResourceDownloader][JSON] Error en request: {url}\n" +
                $"Result: {request.result}\n" +
                $"Error: {request.error}"
            );

            onFailure?.Invoke();
            yield break;
        }

        string json = request.downloadHandler.text;

        if (string.IsNullOrWhiteSpace(json))
        {
            onFailure?.Invoke();
            yield break;
        }

        T data;

        try
        {
            data = JsonUtility.FromJson<T>(json);
        }
        catch (Exception exception)
        {
            DevLog.Warning(
                $"[RemoteResourceDownloader][JSON] Error deserializando JSON desde: {url}\n" +
                $"Exception: {exception.Message}"
            );

            onFailure?.Invoke();
            yield break;
        }

        if (data == null)
        {
            DevLog.Warning(
                $"[RemoteResourceDownloader][JSON] JSON incompatible con tipo {typeof(T).Name}: {url}"
            );

            onFailure?.Invoke();
            yield break;
        }

        onSuccess?.Invoke(data);
    }

    #endregion

    #region ASSET DOWNLOAD

    /// <summary>
    /// Descarga un recurso remoto binario tipado.
    /// 
    /// Actualmente soporta únicamente:
    /// - <see cref="Texture2D"/>
    /// - <see cref="AudioClip"/>
    /// 
    /// Cualquier error de red, tipado o contenido inválido
    /// activa el callback de fallo para permitir aplicar fallback.
    /// </summary>
    public IEnumerator DownloadAsset<T>(
        string url,
        Action<T> onSuccess,
        Action onFailure) where T : UnityEngine.Object
    {
        if (typeof(T) != typeof(AudioClip) && typeof(T) != typeof(Texture2D))
        {
            DevLog.Error(
                $"[RemoteResourceDownloader] Tipo no soportado: {typeof(T).Name}. " +
                $"Solo se permiten AudioClip o Texture2D."
            );

            onFailure?.Invoke();
            yield break;
        }

        if (string.IsNullOrWhiteSpace(url))
        {
            DevLog.Warning(
                "[RemoteResourceDownloader][ASSET] URL nula o vacía. Abortando descarga."
            );

            onFailure?.Invoke();
            yield break;
        }

        UnityWebRequest request;

        if (typeof(T) == typeof(AudioClip))
        {
            AudioType audioType = ResolveAudioType(url);
            request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
        }
        else
        {
            request = UnityWebRequestTexture.GetTexture(url);
        }

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            DevLog.Warning(
                $"[RemoteResourceDownloader][ASSET] Error descargando {url}\n" +
                $"Result: {request.result}\n" +
                $"Error: {request.error}\n" +
                $"HandlerError: {request.downloadHandler?.error}"
            );

            onFailure?.Invoke();
            yield break;
        }

        if (typeof(T) == typeof(AudioClip))
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

            if (clip == null || clip.loadState != AudioDataLoadState.Loaded)
            {
                DevLog.Error(
                    $"[RemoteResourceDownloader][ASSET] Audio inválido o no soportado: {url}"
                );

                onFailure?.Invoke();
                yield break;
            }

            onSuccess?.Invoke(clip as T);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);

            if (texture == null)
            {
                DevLog.Error(
                    $"[RemoteResourceDownloader][ASSET] Texture inválida descargada: {url}"
                );

                onFailure?.Invoke();
                yield break;
            }

            onSuccess?.Invoke(texture as T);
        }
    }

    #endregion

    #region AUDIO UTILITIES

    /// <summary>
    /// Determina el tipo de audio a utilizar basándose
    /// en la extensión del archivo remoto.
    /// 
    /// Unity soporta en tiempo de ejecución:
    /// MP3, WAV y OGG Vorbis.
    /// </summary>
    private AudioType ResolveAudioType(string url)
    {
        string lowerUrl = url.ToLowerInvariant();

        if (lowerUrl.EndsWith(".mp3")) return AudioType.MPEG;
        if (lowerUrl.EndsWith(".wav")) return AudioType.WAV;
        if (lowerUrl.EndsWith(".ogg")) return AudioType.OGGVORBIS;

        DevLog.Warning(
            $"[RemoteResourceDownloader] Extensión desconocida, forzando OGG Vorbis: {url}"
        );

        return AudioType.OGGVORBIS;
    }

    #endregion
}
