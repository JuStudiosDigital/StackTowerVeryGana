using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Utilidad para descargar recursos remotos.
/// Soporta Texture2D, AudioClip y Sprite.
/// </summary>
public class RemoteResourceDownloader
{
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
    /// Convierte una textura descargada en Sprite.
    /// </summary>
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