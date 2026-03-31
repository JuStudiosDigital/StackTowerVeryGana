using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Representa el estado de datos en tiempo de ejecución del juego.
/// Actúa como una capa mutable sobre <see cref="GameData"/>, permitiendo
/// sobrescribir valores mediante configuración remota sin modificar el fallback.
/// </summary>
public class GameRuntimeData
{
    #region Meta

    /// <summary>
    /// Identificador de la marca activa.
    /// </summary>
    public string BrandId { get; private set; }

    /// <summary>
    /// Identificador de la campaña activa.
    /// </summary>
    public string CampaignId { get; private set; }

    #endregion

    #region Assets

    /// <summary>
    /// Imagen principal del gameplay (ej. puzzle).
    /// </summary>
    public Texture2D PuzzleImage { get; private set; }

    /// <summary>
    /// Logo principal de la marca.
    /// </summary>
    public Texture2D MainLogo { get; private set; }

    /// <summary>
    /// Logo secundario o watermark.
    /// </summary>
    public Texture2D WatermarkLogo { get; private set; }

    /// <summary>
    /// Música de fondo.
    /// </summary>
    public AudioClip Music { get; private set; }

    /// <summary>
    /// Sonido asociado a la obtención de recompensas.
    /// </summary>
    public AudioClip KeyWin { get; private set; }

    /// <summary>
    /// Sonido de victoria.
    /// </summary>
    public AudioClip Victory { get; private set; }

    #endregion

    #region Texts

    /// <summary>
    /// Mensajes mostrados al completar el nivel.
    /// </summary>
    public IReadOnlyList<string> VictoryMessages { get; private set; }

    #endregion

    #region Rewards

    /// <summary>
    /// Cantidad de llaves otorgadas por acción.
    /// </summary>
    public int KeysPerAction { get; private set; }

    /// <summary>
    /// Cantidad de llaves otorgadas al completar el nivel.
    /// </summary>
    public int KeysOnCompletion { get; private set; }

    #endregion

    /// <summary>
    /// Genera las solicitudes de carga de assets basadas en las URLs actuales.
    /// Cada request encapsula la asignación del asset al estado runtime.
    /// </summary>
    public List<AssetRequest> GetAssetRequests()
    {
        DevLog.Log("[GameRuntimeData] Generando lista de AssetRequests.");

        var requests = new List<AssetRequest>();

        void LogRequest(string type, string url)
        {
            DevLog.Log($"[GameRuntimeData] Request agregado → Type: {type} | URL: {url}");
        }

        if (!string.IsNullOrWhiteSpace(PuzzleImageUrl))
        {
            LogRequest("Texture", PuzzleImageUrl);

            requests.Add(new AssetRequest(
                PuzzleImageUrl,
                AssetType.Texture,
                obj => SetPuzzleImage(obj as Texture2D)
            ));
        }

        if (!string.IsNullOrWhiteSpace(MainLogoUrl))
        {
            LogRequest("Texture", MainLogoUrl);

            requests.Add(new AssetRequest(
                MainLogoUrl,
                AssetType.Texture,
                obj => SetMainLogo(obj as Texture2D)
            ));
        }

        if (!string.IsNullOrWhiteSpace(WatermarkLogoUrl))
        {
            LogRequest("Texture", WatermarkLogoUrl);

            requests.Add(new AssetRequest(
                WatermarkLogoUrl,
                AssetType.Texture,
                obj => SetWatermarkLogo(obj as Texture2D)
            ));
        }

        if (!string.IsNullOrWhiteSpace(MusicUrl))
        {
            LogRequest("Audio", MusicUrl);

            requests.Add(new AssetRequest(
                MusicUrl,
                AssetType.Audio,
                obj => SetMusic(obj as AudioClip)
            ));
        }

        if (!string.IsNullOrWhiteSpace(KeyWinUrl))
        {
            LogRequest("Audio", KeyWinUrl);

            requests.Add(new AssetRequest(
                KeyWinUrl,
                AssetType.Audio,
                obj => SetKeyWin(obj as AudioClip)
            ));
        }

        if (!string.IsNullOrWhiteSpace(VictoryUrl))
        {
            LogRequest("Audio", VictoryUrl);

            requests.Add(new AssetRequest(
                VictoryUrl,
                AssetType.Audio,
                obj => SetVictory(obj as AudioClip)
            ));
        }

        DevLog.Log($"[GameRuntimeData] Total requests generados: {requests.Count}");

        return requests;
    }

    #region Init

    /// <summary>
    /// Inicializa el estado runtime utilizando los datos base (fallback).
    /// Este método garantiza que el juego sea funcional incluso sin datos remotos.
    /// </summary>
    public void Initialize(GameData baseData)
    {
        DevLog.Log("[GameRuntimeData] Inicializando con fallback (GameData).");

        BrandId = baseData.BrandId;
        CampaignId = baseData.CampaignId;

        PuzzleImage = baseData.PuzzleImage;
        MainLogo = baseData.MainLogo;
        WatermarkLogo = baseData.WatermarkLogo;

        Music = baseData.Music;
        KeyWin = baseData.KeyWin;
        Victory = baseData.Victory;

        PuzzleImageUrl = baseData.PuzzleImageUrl;
        MainLogoUrl = baseData.MainLogoUrl;
        WatermarkLogoUrl = baseData.WatermarkLogoUrl;

        MusicUrl = baseData.MusicUrl;
        KeyWinUrl = baseData.KeyWinUrl;
        VictoryUrl = baseData.VictoryUrl;

        VictoryMessages = baseData.VictoryMessages;

        KeysPerAction = baseData.KeysPerAction;
        KeysOnCompletion = baseData.KeysOnCompletion;

        DevLog.Log($"[GameRuntimeData] Fallback cargado → BrandId: {BrandId}, CampaignId: {CampaignId}");
    }

    #endregion

    #region Apply Remote

    /// <summary>
    /// Aplica configuración remota al estado runtime.
    /// Requiere que los bloques críticos (game y rewards) sean válidos;
    /// en caso contrario, se descarta la actualización completa.
    /// Los campos opcionales se aplican únicamente si contienen datos válidos.
    /// </summary>
    /// <param name="remote">Datos deserializados desde el backend.</param>
    /// <returns>
    /// true si la configuración es válida y fue aplicada correctamente;
    /// false si falla la validación crítica.
    /// </returns>
    public bool Apply(LevelConfigData remote)
    {
        if (remote == null)
        {
            DevLog.Warning("[GameRuntimeData] Remote NULL");
            return false;
        }

        DevLog.Log("[GameRuntimeData] Aplicando configuración remota...");

        bool hasGame = remote.game != null &&
                       !string.IsNullOrWhiteSpace(remote.game.puzzle_image_url);

        bool hasRewards = remote.rewards != null &&
                          remote.rewards.keys_per_action > 0 &&
                          remote.rewards.keys_on_completion > 0;

        if (!hasGame || !hasRewards)
        {
            DevLog.Warning("[GameRuntimeData] ❌ FALLÓ VALIDACIÓN CRÍTICA (game/rewards)");
            return false;
        }

        DevLog.Log("[GameRuntimeData] ✔ Validación crítica OK");

        PuzzleImageUrl = remote.game.puzzle_image_url;
        DevLog.Log($"[GameRuntimeData] PuzzleImageUrl → {PuzzleImageUrl}");

        KeysPerAction = remote.rewards.keys_per_action;
        KeysOnCompletion = remote.rewards.keys_on_completion;

        DevLog.Log($"[GameRuntimeData] KeysPerAction → {KeysPerAction}");
        DevLog.Log($"[GameRuntimeData] KeysOnCompletion → {KeysOnCompletion}");

        if (!string.IsNullOrWhiteSpace(remote.meta?.brand_id))
            BrandId = remote.meta.brand_id;

        if (!string.IsNullOrWhiteSpace(remote.meta?.campaign_id))
            CampaignId = remote.meta.campaign_id;

        if (remote.texts?.victory_messages != null)
        {
            List<string> validMessages = new List<string>();

            foreach (string msg in remote.texts.victory_messages)
            {
                if (!string.IsNullOrWhiteSpace(msg))
                {
                    validMessages.Add(msg);
                }
            }

            if (validMessages.Count > 0)
            {
                VictoryMessages = validMessages;
                DevLog.Log($"[GameRuntimeData] VictoryMessages válidos → {validMessages.Count}");
            }
            else
            {
                DevLog.Log("[GameRuntimeData] VictoryMessages vacíos → usando fallback");
            }
        }

        if (!string.IsNullOrWhiteSpace(remote.branding?.main_logo_url))
            MainLogoUrl = remote.branding.main_logo_url;

        if (!string.IsNullOrWhiteSpace(remote.branding?.watermark_logo_url))
            WatermarkLogoUrl = remote.branding.watermark_logo_url;

        if (!string.IsNullOrWhiteSpace(remote.audio?.music_url))
            MusicUrl = remote.audio.music_url;

        if (!string.IsNullOrWhiteSpace(remote.audio?.key_win_url))
            KeyWinUrl = remote.audio.key_win_url;

        if (!string.IsNullOrWhiteSpace(remote.audio?.victory_url))
            VictoryUrl = remote.audio.victory_url;

        DevLog.Log("[GameRuntimeData] ✔ Apply remoto completado");

        return true;
    }

    #endregion

    #region Runtime Setters (Assets)

    /// <summary>
    /// Asigna la textura principal del juego desde una carga remota.
    /// </summary>
    public void SetPuzzleImage(Texture2D tex)
    {
        if (tex == null)
        {
            DevLog.Warning("[GameRuntimeData] PuzzleImage recibido NULL");
            return;
        }

        PuzzleImage = tex;
        DevLog.Log("[GameRuntimeData] PuzzleImage actualizado (REMOTE)");
    }

    /// <summary>
    /// Asigna el logo principal desde una carga remota.
    /// </summary>
    public void SetMainLogo(Texture2D tex)
    {
        if (tex == null)
        {
            DevLog.Warning("[GameRuntimeData] MainLogo NULL");
            return;
        }

        MainLogo = tex;
        DevLog.Log("[GameRuntimeData] MainLogo actualizado");
    }

    /// <summary>
    /// Asigna el watermark desde una carga remota.
    /// </summary>
    public void SetWatermarkLogo(Texture2D tex)
    {
        if (tex == null)
        {
            DevLog.Warning("[GameRuntimeData] WatermarkLogo NULL");
            return;
        }

        WatermarkLogo = tex;
        DevLog.Log("[GameRuntimeData] WatermarkLogo actualizado");
    }

    /// <summary>
    /// Asigna la música desde una carga remota.
    /// </summary>
    public void SetMusic(AudioClip clip)
    {
        if (clip == null)
        {
            DevLog.Warning("[GameRuntimeData] Music NULL");
            return;
        }

        Music = clip;
        DevLog.Log("[GameRuntimeData] Music actualizado");
    }

    /// <summary>
    /// Asigna el audio de recompensa desde una carga remota.
    /// </summary>
    public void SetKeyWin(AudioClip clip)
    {
        if (clip == null)
        {
            DevLog.Warning("[GameRuntimeData] KeyWin NULL");
            return;
        }

        KeyWin = clip;
        DevLog.Log("[GameRuntimeData] KeyWin actualizado");
    }

    /// <summary>
    /// Asigna el audio de victoria desde una carga remota.
    /// </summary>
    public void SetVictory(AudioClip clip)
    {
        if (clip == null)
        {
            DevLog.Warning("[GameRuntimeData] Victory NULL");
            return;
        }

        Victory = clip;
        DevLog.Log("[GameRuntimeData] Victory actualizado");
    }

    #endregion

    #region Asset URLs

    /// <summary>
    /// URL de la imagen principal del juego.
    /// </summary>
    public string PuzzleImageUrl { get; private set; }

    /// <summary>
    /// URL del logo principal.
    /// </summary>
    public string MainLogoUrl { get; private set; }

    /// <summary>
    /// URL del watermark.
    /// </summary>
    public string WatermarkLogoUrl { get; private set; }

    /// <summary>
    /// URL de la música.
    /// </summary>
    public string MusicUrl { get; private set; }

    /// <summary>
    /// URL del audio de recompensa.
    /// </summary>
    public string KeyWinUrl { get; private set; }

    /// <summary>
    /// URL del audio de victoria.
    /// </summary>
    public string VictoryUrl { get; private set; }

    #endregion
}