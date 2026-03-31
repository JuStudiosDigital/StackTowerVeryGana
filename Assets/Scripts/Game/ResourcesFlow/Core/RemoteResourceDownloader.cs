using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Proporciona utilidades para la descarga de recursos remotos utilizados en runtime.
/// Encapsula la lógica de acceso a red y conversión de datos para texturas y audio,
/// desacoplando la obtención de assets del resto del sistema.
/// </summary>
public class RemoteResourceDownloader
{
    /// <summary>
    /// Descarga una textura desde una URL remota.
    /// </summary>
    /// <param name="url">Dirección del recurso remoto.</param>
    /// <param name="onSuccess">Callback ejecutado al completar correctamente la descarga.</param>
    /// <param name="onFailure">Callback ejecutado en caso de error.</param>
    /// <returns>IEnumerator para ejecución en coroutine.</returns>
    public IEnumerator DownloadTexture(
        string url,
        Action<Texture2D> onSuccess,
        Action onFailure)
    {
        DevLog.Log($"[Downloader][Texture] Start → {url}");

        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            DevLog.Warning($"[Downloader][Texture] Error → {url} | {request.error}");
            onFailure?.Invoke();
            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        if (texture == null)
        {
            DevLog.Warning($"[Downloader][Texture] Null Texture → {url}");
            onFailure?.Invoke();
            yield break;
        }

        DevLog.Log($"[Downloader][Texture] Success → {url}");

        try
        {
            onSuccess?.Invoke(texture);
        }
        catch (Exception e)
        {
            DevLog.Error($"[Downloader][Texture] Callback Error → {e.Message}");
        }
    }

    /// <summary>
    /// Descarga un clip de audio desde una URL remota.
    /// El tipo de audio se infiere automáticamente a partir de la extensión del archivo.
    /// </summary>
    /// <param name="url">Dirección del recurso remoto.</param>
    /// <param name="onSuccess">Callback ejecutado al completar correctamente la descarga.</param>
    /// <param name="onFailure">Callback ejecutado en caso de error.</param>
    /// <returns>IEnumerator para ejecución en coroutine.</returns>
    public IEnumerator DownloadAudio(
        string url,
        Action<AudioClip> onSuccess,
        Action onFailure)
    {
        DevLog.Log($"[Downloader][Audio] Start → {url}");

        AudioType type = ResolveAudioType(url);

        using UnityWebRequest request =
            UnityWebRequestMultimedia.GetAudioClip(url, type);

        request.timeout = 10;

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            DevLog.Warning($"[Downloader][Audio] Error → {url} | {request.error}");
            onFailure?.Invoke();
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        if (clip == null)
        {
            DevLog.Warning($"[Downloader][Audio] Null Clip → {url}");
            onFailure?.Invoke();
            yield break;
        }

        DevLog.Log($"[Downloader][Audio] Success → {url}");

        try
        {
            onSuccess?.Invoke(clip);
        }
        catch (Exception e)
        {
            DevLog.Error($"[Downloader][Audio] Callback Error → {e.Message}");
        }
    }

    /// <summary>
    /// Crea un <see cref="Sprite"/> a partir de una textura previamente descargada.
    /// </summary>
    /// <param name="texture">Textura fuente.</param>
    /// <returns>Sprite generado o null si la textura es inválida.</returns>
    public Sprite CreateSprite(Texture2D texture)
    {
        if (texture == null)
        {
            DevLog.Warning("[Downloader][Sprite] Texture null al crear sprite");
            return null;
        }

        DevLog.Log("[Downloader][Sprite] Creating Sprite");

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    /// <summary>
    /// Determina el tipo de audio esperado en función de la extensión de la URL.
    /// </summary>
    /// <param name="url">URL del recurso.</param>
    /// <returns>Tipo de audio compatible con UnityWebRequest.</returns>
    private AudioType ResolveAudioType(string url)
    {
        string lower = url.ToLower();

        if (lower.EndsWith(".mp3")) return AudioType.MPEG;
        if (lower.EndsWith(".wav")) return AudioType.WAV;
        if (lower.EndsWith(".ogg")) return AudioType.OGGVORBIS;

        DevLog.Warning($"[Downloader][Audio] Unknown format → {url} (fallback OGG)");

        return AudioType.OGGVORBIS;
    }
}