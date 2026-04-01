using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDataLoader
{
    private readonly RemoteFetcher fetcher = new RemoteFetcher();
    private readonly SimpleCache cache = new SimpleCache();


    public IEnumerator Load(
        GameDataProvider provider,
        string url,
        string postBody = null)
    {
        DevLog.Log($"[GameDataLoader] === INICIO LOAD ===");
        DevLog.Log($"[GameDataLoader] URL: {url}");
        DevLog.Log($"[GameDataLoader] Modo: {(string.IsNullOrEmpty(postBody) ? "GET" : "POST")}");

        if (provider == null)
        {
            DevLog.Log("[GameDataLoader] ❌ Provider es null");
            yield break;
        }

        /// 🔹 0. Fallback
        DevLog.Log("[GameDataLoader] Inicializando runtime con fallback base");
        provider.Initialize();

        GameRuntimeData runtime = provider.Runtime;

        if (runtime == null)
        {
            DevLog.Log("[GameDataLoader] ❌ Runtime no inicializado");
            yield break;
        }

        /// 🔹 1. CACHE
        DevLog.Log("[GameDataLoader] Verificando cache...");

        if (cache.TryGet(url, out string cachedJson))
        {
            DevLog.Log("[GameDataLoader] ✔ Cache encontrada");

            bool cacheApply = ApplyJson(runtime, cachedJson);

            if (cacheApply)
            {
                DevLog.Log("[GameDataLoader] ✔ Cache válida, cargando assets...");
                GameManager.Instance.ConfigureAds(true);
                AssetLoadResult cacheResult = null;

                yield return LoadAssets(runtime, provider, r => cacheResult = r);
                
                if (cacheResult != null && cacheResult.RequiredFailed > 0)
                {
                    DevLog.Log("[GameDataLoader] ❌ Cache inválida por assets");
                
                    GameManager.Instance.ConfigureAds(false);
                    yield break;
                }

                DevLog.Log("[GameDataLoader] === FIN LOAD (CACHE) ===");
                yield break;
            }

            DevLog.Log("[GameDataLoader] ⚠ Cache inválida, ignorando...");
            GameManager.Instance.ConfigureAds(false);
        }

        /// 🔹 2. FETCH
        DevLog.Log("[GameDataLoader] Solicitando remoto...");

        bool success = false;
        string downloadedJson = null;

        yield return fetcher.Fetch(
            url,
            postBody,
            json =>
            {
                DevLog.Log("[GameDataLoader] ✔ Respuesta recibida");
                success = true;
                downloadedJson = json;
            },
            () =>
            {
                DevLog.Log("[GameDataLoader] ❌ Error en request remoto");
                success = false;
            }
        );

        /// 🔹 3. RESULTADO
        if (!success)
        {
            DevLog.Log("[GameDataLoader] ⚠ Usando fallback (sin remoto)");
            DevLog.Log("[GameDataLoader] === FIN LOAD (FALLBACK) ===");
            GameManager.Instance.ConfigureAds(false);
            yield break;
        }

        /// 🔹 4. APPLY + VALIDACIÓN
        DevLog.Log("[GameDataLoader] Aplicando JSON...");

        bool successApply = ApplyJson(runtime, downloadedJson);

        if (!successApply)
        {
            DevLog.Log("[GameDataLoader] ❌ JSON inválido (game/rewards falló)");
            DevLog.Log("[GameDataLoader] === FIN LOAD (FALLBACK) ===");
            GameManager.Instance.ConfigureAds(false);
            yield break;
        }

        /// 🔹 5. CACHE STORE (solo si es válido)
        DevLog.Log("[GameDataLoader] Guardando en cache");
        cache.Store(url, downloadedJson);
        GameManager.Instance.ConfigureAds(true);

        /// 🔹 6. ASSETS
        AssetLoadResult assetResult = null;

        yield return LoadAssets(runtime, provider, r => assetResult = r);

        if (assetResult != null && assetResult.RequiredFailed > 0)
        {
            DevLog.Log("[GameDataLoader] ❌ Fallaron assets críticos → fallback total");

            provider.Initialize(); 
            GameManager.Instance.ConfigureAds(false);

            DevLog.Log("[GameDataLoader] === FIN LOAD (FALLBACK POR ASSETS) ===");
            yield break;
        }

        DevLog.Log("[GameDataLoader] ✔ Datos y assets cargados correctamente");
        DevLog.Log("[GameDataLoader] === FIN LOAD (REMOTE) ===");
    }

    #region Internal


    private bool ApplyJson(GameRuntimeData runtime, string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            DevLog.Log("[GameDataLoader] JSON vacío");
            return false;
        }

        try
        {
            DevLog.Log("[GameDataLoader] Deserializando JSON...");

            LevelConfigData data =
                JsonUtility.FromJson<LevelConfigData>(json);

            if (data == null)
            {
                DevLog.Log("[GameDataLoader] JSON inválido");
                return false;
            }

            DevLog.Log("[GameDataLoader] ✔ JSON OK");

            bool applied = runtime.Apply(data);

            if (!applied)
            {
                DevLog.Log("[GameDataLoader] ❌ Validación crítica fallida (game/rewards)");
                return false;
            }

            return true;
        }
        catch (Exception e)
        {
            DevLog.Log($"[GameDataLoader] Error JSON: {e.Message}");
            return false;
        }
    }

    private IEnumerator LoadAssets(
        GameRuntimeData runtime,
        GameDataProvider provider,
        Action<AssetLoadResult> onComplete)
    {
        DevLog.Log("[GameDataLoader] Generando requests de assets...");

        List<AssetRequest> requests = runtime.GetAssetRequests();

        if (requests == null || requests.Count == 0)
        {
            DevLog.Log("[GameDataLoader] No hay assets para descargar");
            yield break;
        }

        DevLog.Log($"[GameDataLoader] Descargando {requests.Count} assets...");

        AssetLoader loader = new AssetLoader();

        AssetLoadResult result = null;

        yield return loader.LoadAll(
            requests,
            provider,
            r => result = r, 
            progress =>
            {
                DevLog.Log($"[GameDataLoader] Asset Progress: {progress:P0}");
            }
        );

        DevLog.Log($"[GameDataLoader] Assets result → Success: {result.Success}, Failed: {result.Failed}, RequiredFailed: {result.RequiredFailed}");

        onComplete?.Invoke(result);

        DevLog.Log("[GameDataLoader] ✔ Assets cargados");
    }

    #endregion
}