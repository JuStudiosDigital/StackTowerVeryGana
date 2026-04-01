using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loader genérico de assets.
/// No conoce lógica de juego.
/// </summary>
public class AssetLoader
{
    private readonly RemoteResourceDownloader downloader = new RemoteResourceDownloader();
    private readonly AssetCache cache = new AssetCache();

    /// <summary>
    /// Carga una lista de requests de assets.
    /// </summary>
    public IEnumerator LoadAll(
        List<AssetRequest> requests,
        MonoBehaviour runner,
        Action<AssetLoadResult> onComplete,
        Action<float> onProgress = null)
    {
        AssetLoadResult result = new AssetLoadResult();
        result.Total = requests.Count;

        if (requests == null || runner == null)
        {
            DevLog.Warning("[AssetLoader] Requests o runner null");
            yield break;
        }

        int total = requests.Count;
        int completed = 0;

        DevLog.Log($"[AssetLoader] Iniciando carga de {total} assets");

        if (total == 0)
        {
            DevLog.Log("[AssetLoader] No hay assets para cargar");
            onProgress?.Invoke(1f);
            yield break;
        }

        void CompleteStep()
        {
            completed++;
            float progress = (float)completed / total;

            DevLog.Log($"[AssetLoader] Progreso: {completed}/{total} ({progress:P0})");
            onProgress?.Invoke(progress);
        }

        foreach (var request in requests)
        {
            DevLog.Log($"[AssetLoader] Encolando: {request.Type} → {request.Url}");

            runner.StartCoroutine(
                Load(request,
                    () =>
                    {
                        result.Success++;
                        CompleteStep();
                    },
                    () =>
                    {
                        result.Failed++;

                        if (request.IsRequired)
                            result.RequiredFailed++;

                        DevLog.Warning($"[AssetLoader] Falló carga: {request.Url}");

                        CompleteStep();
                    })
            );
        }

        while (completed < total)
        {
            yield return null;
        }

        DevLog.Log("[AssetLoader] Carga completa");
        onComplete?.Invoke(result);
    }

    private IEnumerator Load(
        AssetRequest request,
        Action onSuccess,
        Action onFailure)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
        {
            DevLog.Warning("[AssetLoader] URL inválida");
            onFailure?.Invoke();
            yield break;
        }

        DevLog.Log($"[AssetLoader] Cargando {request.Type}: {request.Url}");

        switch (request.Type)
        {
            case AssetType.Texture:
                yield return LoadTexture(request, onSuccess, onFailure);
                break;

            case AssetType.Sprite:
                yield return LoadSprite(request, onSuccess, onFailure);
                break;

            case AssetType.Audio:
                yield return LoadAudio(request, onSuccess, onFailure);
                break;

            default:
                DevLog.Warning($"[AssetLoader] Tipo no soportado: {request.Type}");
                onFailure?.Invoke();
                break;
        }
    }

    private IEnumerator LoadTexture(
        AssetRequest request,
        Action onSuccess,
        Action onFailure)
    {
        if (cache.TryGet(request.Url, out Texture2D cached))
        {
            DevLog.Log($"[AssetLoader] Texture desde cache: {request.Url}");

            SafeInvoke(request.OnLoaded, cached);

            onSuccess?.Invoke();
            yield break;
        }

        DevLog.Log($"[AssetLoader] Descargando texture: {request.Url}");

        yield return downloader.DownloadTexture(
            request.Url,
            texture =>
            {
                DevLog.Log($"[AssetLoader] Texture descargada: {request.Url}");

                cache.Store(request.Url, texture);
                SafeInvoke(request.OnLoaded, texture);

                onSuccess?.Invoke();
            },
            () =>
            {
                DevLog.Warning($"[AssetLoader] Error descargando texture: {request.Url}");
                onFailure?.Invoke();
            }
        );
    }

    private IEnumerator LoadAudio(
        AssetRequest request,
        Action onSuccess,
        Action onFailure)
    {
        if (cache.TryGet(request.Url, out AudioClip cached))
        {
            DevLog.Log($"[AssetLoader] Audio desde cache: {request.Url}");

            SafeInvoke(request.OnLoaded, cached);

            onSuccess?.Invoke();
            yield break;
        }

        DevLog.Log($"[AssetLoader] Descargando audio: {request.Url}");

        yield return downloader.DownloadAudio(
            request.Url,
            clip =>
            {
                DevLog.Log($"[AssetLoader] Audio descargado: {request.Url}");

                cache.Store(request.Url, clip);
                SafeInvoke(request.OnLoaded, clip);

                onSuccess?.Invoke();
            },
            () =>
            {
                DevLog.Warning($"[AssetLoader] Error descargando audio: {request.Url}");
                onFailure?.Invoke();
            }
        );
    }

    private IEnumerator LoadSprite(
        AssetRequest request,
        Action onSuccess,
        Action onFailure)
    {
        string spriteKey = request.Url + "_sprite";

        if (cache.TryGet(spriteKey, out Sprite cachedSprite))
        {
            DevLog.Log($"[AssetLoader] Sprite desde cache: {request.Url}");

            SafeInvoke(request.OnLoaded, cachedSprite);

            onSuccess?.Invoke();
            yield break;
        }

        if (cache.TryGet(request.Url, out Texture2D cachedTexture))
        {
            DevLog.Log($"[AssetLoader] Creando sprite desde texture cacheada: {request.Url}");

            Sprite sprite = CreateSprite(cachedTexture);
            cache.Store(spriteKey, sprite);

            SafeInvoke(request.OnLoaded, sprite);

            onSuccess?.Invoke();
            yield break;
        }

        DevLog.Log($"[AssetLoader] Descargando texture para sprite: {request.Url}");

        yield return downloader.DownloadTexture(
            request.Url,
            texture =>
            {
                DevLog.Log($"[AssetLoader] Texture descargada para sprite: {request.Url}");

                cache.Store(request.Url, texture);

                Sprite sprite = CreateSprite(texture);
                cache.Store(spriteKey, sprite);

                SafeInvoke(request.OnLoaded, sprite);

                onSuccess?.Invoke();
            },
            () =>
            {
                DevLog.Warning($"[AssetLoader] Error descargando sprite: {request.Url}");
                onFailure?.Invoke();
            }
        );
    }

    private Sprite CreateSprite(Texture2D texture)
    {
        if (texture == null)
        {
            DevLog.Warning("[AssetLoader] Texture nula al crear sprite");
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    /// <summary>
    /// Ejecuta callbacks de forma segura.
    /// </summary>
    private void SafeInvoke(Action<UnityEngine.Object> action, UnityEngine.Object obj)
    {
        try
        {
            action?.Invoke(obj);
        }
        catch (Exception e)
        {
            DevLog.Error($"[AssetLoader] Error en OnLoaded: {e.Message}");
        }
    }
}

public class AssetLoadResult
{
    public int Total;
    public int Success;
    public int Failed;
    public int RequiredFailed;
}