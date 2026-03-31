using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loader genérico encargado de la descarga y resolución de assets remotos.
/// Maneja cache en memoria y ejecución concurrente de cargas sin acoplarse a lógica de juego.
/// </summary>
public class AssetLoader
{
    private readonly RemoteResourceDownloader downloader = new RemoteResourceDownloader();
    private readonly AssetCache cache = new AssetCache();

    /// <summary>
    /// Ejecuta la carga de múltiples solicitudes de assets en paralelo.
    /// Reporta progreso agregado basado en la cantidad de requests completados.
    /// </summary>
    /// <param name="requests">Lista de solicitudes de assets a procesar.</param>
    /// <param name="runner">Instancia de MonoBehaviour utilizada para ejecutar coroutines.</param>
    /// <param name="onProgress">Callback opcional de progreso normalizado (0–1).</param>
    /// <returns>Coroutine que finaliza cuando todas las cargas han terminado.</returns>
    public IEnumerator LoadAll(
        List<AssetRequest> requests,
        MonoBehaviour runner,
        Action<float> onProgress = null)
    {
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
                    () => CompleteStep(),
                    () =>
                    {
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
    }

    /// <summary>
    /// Resuelve una solicitud individual delegando la carga según el tipo de asset.
    /// </summary>
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

    /// <summary>
    /// Carga una textura desde cache o red y ejecuta el callback asociado.
    /// </summary>
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

    /// <summary>
    /// Carga un clip de audio desde cache o red y ejecuta el callback asociado.
    /// </summary>
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

    /// <summary>
    /// Genera un sprite a partir de una textura, utilizando cache si es posible.
    /// </summary>
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

    /// <summary>
    /// Crea un sprite centrado a partir de una textura válida.
    /// </summary>
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
    /// Ejecuta un callback protegido contra excepciones para evitar interrupciones en el flujo de carga.
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