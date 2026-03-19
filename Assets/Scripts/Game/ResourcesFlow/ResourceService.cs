using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Servicio central responsable de gestionar el acceso unificado
/// a recursos locales y remotos del juego.
///
/// Orquesta:
/// - Descarga y normalización de configuración remota
/// - Fallback automático a configuración local
/// - Registro, precarga y resolución de recursos
/// - Determinación del modo de ejecución (con o sin anuncios)
///
/// Este servicio persiste entre escenas y actúa como punto único
/// de acceso a recursos durante la ejecución del nivel.
/// </summary>
public class ResourceService : MonoBehaviour
{
    #region Singleton

    /// <summary>
    /// Instancia global del servicio.
    /// </summary>
    public static ResourceService Instance { get; private set; }

    /// <summary>
    /// Inicializa el singleton y garantiza una única instancia persistente.
    /// </summary>
    private void Awake()
    {
        if (Instance != null)
        {
            DevLog.Warning(
                "[ResourceService] Instancia duplicada detectada. Destruyendo objeto."
            );
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Fields

    /// <summary>
    /// Configuración final del nivel luego de aplicar normalización y fallback.
    /// </summary>
    private LevelConfigData currentLevelConfig;

    /// <summary>
    /// Cache en memoria de recursos descargados o cargados localmente.
    /// </summary>
    private readonly ResourceCache resourceCache = new ResourceCache();

    /// <summary>
    /// Registro de URLs remotas disponibles para el nivel actual.
    /// </summary>
    private readonly RemoteResourceRegistry remoteRegistry = new RemoteResourceRegistry();

    /// <summary>
    /// Servicio responsable de descargar recursos remotos de forma segura.
    /// </summary>
    private readonly RemoteResourceDownloader downloader = new RemoteResourceDownloader();

    /// <summary>
    /// Indica si la configuración remota contiene un identificador de marca válido.
    /// </summary>
    private bool hasBrandId;

    /// <summary>
    /// Indica si el nivel se ejecuta sin patrocinadores activos.
    /// Este flag es consumido por otros sistemas (ej. GameManager).
    /// </summary>
    public bool IsNoAdsMode { get; private set; }

    /// <summary>
    /// Indica si falló la descarga de algún recurso crítico remoto.
    /// </summary>
    private bool hasCriticalRemoteFailure;

    #endregion

    #region Public API

    /// <summary>
    /// Carga la configuración del nivel desde una fuente remota,
    /// la combina con fallback local y resuelve el modo de ejecución final.
    /// </summary>
    /// <param name="remoteUrl">
    /// URL del JSON remoto que define la configuración del nivel.
    /// </param>
    public IEnumerator LoadLevelConfigAsync(ILevelConfigProvider configProvider)
    {
        hasCriticalRemoteFailure = false;
        LevelConfigData remoteConfig = null;

        /// Descarga y normalización del JSON remoto
        yield return configProvider.GetLevelConfigAsync(
            data => remoteConfig = data,
            () => remoteConfig = null
        );

        /// Carga de configuración base local (fallback)
        LevelConfigData baseConfig =
            LocalLevelConfigProvider.LoadFallbackConfig();

        /// Merge entre configuración remota y fallback local
        currentLevelConfig =
            LevelConfigMerger.MergeWithFallback(remoteConfig, baseConfig);

        /// Determinar presencia de anunciante
        hasBrandId =
            !string.IsNullOrWhiteSpace(currentLevelConfig.meta.brand_id);

        /// Preparación de registros y cache
        resourceCache.Clear();
        remoteRegistry.Clear();
        RegisterRemoteResources(currentLevelConfig);

        /// Precarga de recursos registrados
        yield return PreloadLevelResources(null);

        
        /// Si hubo fallo crítico remoto, se fuerza fallback total
        if (hasCriticalRemoteFailure)
        {
            UseFullFallback();
            ApplyRuntimeMode(LevelRuntimeMode.FullFallback);
            yield break;
        }

        /// Validación crítica: imagen del puzzle
        bool remotePuzzleCached =
            !string.IsNullOrWhiteSpace(currentLevelConfig.game.image_url) &&
            resourceCache.Has(currentLevelConfig.game.image_url);

        bool localPuzzleCached =
            resourceCache.Has("local::PuzzleImage");

        bool puzzleAvailable =
            remotePuzzleCached || localPuzzleCached;

        if (!puzzleAvailable)
        {
            UseFullFallback();
        }

        /// Evaluación del modo de ejecución final
        LevelRuntimeMode runtimeMode =
            LevelConfigPolicyEvaluator.Evaluate(currentLevelConfig);

        /// Aplicación del modo de ejecución
        ApplyRuntimeMode(runtimeMode);
    }

    /// <summary>
    /// Obtiene un sprite resolviendo primero el recurso remoto
    /// y aplicando fallback local si es necesario.
    /// </summary>
    public Sprite GetSprite(string key)
    {
        string remoteUrl = remoteRegistry.GetUrl(key);

        Texture2D texture = null;

        if (!string.IsNullOrEmpty(remoteUrl))
        {
            texture = resourceCache.Get<Texture2D>(remoteUrl);
        }

        if (texture == null)
        {
            texture = resourceCache.Get<Texture2D>($"local::{key}");
        }

        if (texture == null)
        {
            DevLog.Warning(
                $"[ResourceService] Sprite no encontrado para clave: {key}"
            );
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );
    }

    /// <summary>
    /// Obtiene un AudioClip resolviendo primero el recurso remoto
    /// y aplicando fallback local bajo demanda.
    /// </summary>
    public AudioClip GetAudioClip(string key)
    {
        string remoteUrl = remoteRegistry.GetUrl(key);

        if (!string.IsNullOrEmpty(remoteUrl))
        {
            AudioClip remoteClip = resourceCache.Get<AudioClip>(remoteUrl);
            if (remoteClip != null)
            {
                return remoteClip;
            }
        }

        string localCacheKey = $"local::{key}";
        AudioClip localClip = resourceCache.Get<AudioClip>(localCacheKey);

        if (localClip != null)
        {
            return localClip;
        }

        AudioClip loadedLocal = LocalResourceProvider.Load<AudioClip>(key);

        if (loadedLocal != null)
        {
            resourceCache.Store(localCacheKey, loadedLocal);
        }

        return loadedLocal;
    }

    /// <summary>
    /// Obtiene un texto desde la configuración del nivel.
    /// </summary>
    public string GetText(Func<TextData, string> selector)
    {
        return selector.Invoke(currentLevelConfig.texts);
    }

    /// <summary>
    /// Obtiene una textura usando la URL remota actual como identidad.
    /// </summary>
    public Texture2D GetTexture(string key)
    {
        string remoteUrl = remoteRegistry.GetUrl(key);

        if (!string.IsNullOrEmpty(remoteUrl))
        {
            Texture2D texture = resourceCache.Get<Texture2D>(remoteUrl);
            if (texture != null)
            {
                return texture;
            }
        }

        DevLog.Warning(
            $"[ResourceService] Texture no encontrada para clave: {key}"
        );
        return null;
    }

    /// <summary>
    /// Obtiene un valor de recompensa desde la configuración del nivel.
    /// </summary>
    public int GetReward(Func<RewardData, int> selector)
    {
        return selector.Invoke(currentLevelConfig.rewards);
    }

    #endregion

    #region Resource Loading

    /// <summary>
    /// Precarga todos los recursos registrados para el nivel.
    /// </summary>
    public IEnumerator PreloadLevelResources(Action<float> onProgress)
    {
        var keys = remoteRegistry.GetAllKeys();
        int total = keys.Count;
        int loaded = 0;

        foreach (string key in keys)
        {
            bool isAudio = key.Contains("Sound");

            yield return LoadResourceAsync(
                key,
                isAudio,
                failure =>
                {
                    if (!failure)
                    {
                        return;
                    }

                    /// La imagen del puzzle es siempre crítica
                    if (key == "PuzzleImage")
                    {
                        hasCriticalRemoteFailure = true;
                        return;
                    }
                }
            );

            loaded++;
            onProgress?.Invoke((float)loaded / total);
        }
    }

    /// <summary>
    /// Carga un recurso considerando la URL remota como identidad primaria
    /// y aplicando fallback local en caso de fallo.
    /// </summary>
    private IEnumerator LoadResourceAsync(
        string key,
        bool isAudio,
        Action<bool> onFailure)
    {
        string remoteUrl = remoteRegistry.GetUrl(key);

        if (string.IsNullOrEmpty(remoteUrl))
        {
            LoadLocalFallback(key, isAudio);
            yield break;
        }

        if (isAudio)
        {
            yield return downloader.DownloadAsset<AudioClip>(
                remoteUrl,
                clip => resourceCache.Store(remoteUrl, clip),
                () =>
                {
                    onFailure?.Invoke(true);
                    LoadLocalFallback(key, true);
                }
            );
        }
        else
        {
            yield return downloader.DownloadAsset<Texture2D>(
                remoteUrl,
                texture => resourceCache.Store(remoteUrl, texture),
                () =>
                {
                    onFailure?.Invoke(true);
                    LoadLocalFallback(key, false);
                }
            );
        }
    }

    /// <summary>
    /// Carga un recurso local de respaldo y lo almacena en cache.
    /// </summary>
    private void LoadLocalFallback(string key, bool isAudio)
    {
        if (isAudio)
        {
            AudioClip clip = LocalResourceProvider.Load<AudioClip>(key);
            resourceCache.Store($"local::{key}", clip);
        }
        else
        {
            Texture2D texture = LocalResourceProvider.Load<Texture2D>(key);
            resourceCache.Store($"local::{key}", texture);
        }
    }

    #endregion

    #region Internal Logic

    /// <summary>
    /// Registra internamente las URLs remotas disponibles
    /// para la configuración actual del nivel.
    /// </summary>
    private void RegisterRemoteResources(LevelConfigData config)
    {
        if (config == null)
        {
            DevLog.Warning("[ResourceService] Configuración nula.");
            return;
        }

        /// Imagen del puzzle (recurso obligatorio)
        remoteRegistry.Register("PuzzleImage", config.game.image_url);

        /// Recursos asociados a marca solo si existe brand_id
        if (hasBrandId)
        {
            remoteRegistry.Register(
                "VictorySound",
                config.audio.victory_sound_url
            );
            remoteRegistry.Register(
                "CoinSound",
                config.audio.coin_sound_url
            );
            remoteRegistry.Register(
                "LogoWatermark",
                config.branding.logo_watermark_url
            );
        }
    }

    /// <summary>
    /// Aplica el modo de ejecución del nivel,
    /// configurando publicidad y flags internos.
    /// </summary>
    private void ApplyRuntimeMode(LevelRuntimeMode runtimeMode)
    {
        switch (runtimeMode)
        {
            case LevelRuntimeMode.AdsEnabled:
                IsNoAdsMode = false;
                GameManager.Instance.ConfigureAds(true);
                break;

            case LevelRuntimeMode.NoAdsWithRemoteContent:
            case LevelRuntimeMode.FullFallback:
                IsNoAdsMode = true;
                GameManager.Instance.ConfigureAds(false);
                break;
        }
    }

    /// <summary>
    /// Fuerza el uso completo de la configuración local
    /// y desactiva cualquier contenido publicitario.
    /// </summary>
    private void UseFullFallback()
    {
        currentLevelConfig = LocalLevelConfigProvider.LoadFallbackConfig();
        resourceCache.Clear();
        remoteRegistry.Clear();
        RegisterRemoteResources(currentLevelConfig);
        IsNoAdsMode = true;
        hasBrandId = false;
    }

    #endregion
}
