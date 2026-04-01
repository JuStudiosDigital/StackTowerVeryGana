using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GameRuntimeData
{
    #region Meta

    public string BrandId { get; private set; }
    public string CampaignId { get; private set; }

    #endregion

    #region Assets (SPRITE FIRST)

    public List<Sprite> ContainerSprites { get; private set; } = new();
    public Texture2D MainLogo { get; private set; }

    public AudioClip KeyWin { get; private set; }
    public AudioClip Victory { get; private set; }

    #endregion

    #region Game Config

    public List<Color> ContainerColors { get; private set; } = new();
    public int ContainersPerKey { get; private set; }

    #endregion

    #region Texts

    public IReadOnlyList<string> GameOverMessages { get; private set; }

    #endregion

    #region Rewards

    public int KeysPerAction { get; private set; }

    #endregion

    #region URLs

    private List<string> containerImageUrls = new();

    public string MainLogoUrl { get; private set; }
    public string KeyWinUrl { get; private set; }
    public string VictoryUrl { get; private set; }

    #endregion

    #region INIT

    public void Initialize(GameData baseData)
    {
        BrandId = baseData.BrandId;
        CampaignId = baseData.CampaignId;

        MainLogo = baseData.MainLogo;
        KeyWin = baseData.KeyWin;
        Victory = baseData.Victory;

        MainLogoUrl = baseData.MainLogoUrl;
        KeyWinUrl = baseData.KeyWinUrl;
        VictoryUrl = baseData.VictoryUrl;

        ContainerSprites = new List<Sprite>(baseData.ContainerSprites);
        ContainerColors = new List<Color>(baseData.ContainerColors);

        ContainersPerKey = baseData.ContainersPerKey;
        GameOverMessages = baseData.GameOverMessages;

        KeysPerAction = baseData.KeysPerAction;
    }

    #endregion

    #region APPLY

    public bool Apply(LevelConfigData remote)
    {
        if (remote == null)
            return false;

        /// =========================
        /// VALIDACIÓN CRÍTICA
        /// =========================

        bool validGame =
            remote.game != null &&
            remote.game.container_images != null &&
            remote.game.container_images.Count > 0 &&
            remote.game.container_colors != null &&
            remote.game.container_colors.Count > 0 &&
            remote.game.containers_per_key > 0;

        bool validRewards =
            remote.rewards != null &&
            remote.rewards.keys_per_action > 0;

        if (!validGame || !validRewards)
        {
            DevLog.Warning("[Runtime] ❌ Validación crítica fallida");
            return false;
        }

        /// VALIDAR COLORES HEX
        List<Color> parsedColors = new();

        foreach (var hex in remote.game.container_colors)
        {
            if (string.IsNullOrWhiteSpace(hex) || !IsValidHex(hex))
                return false;

            if (ColorUtility.TryParseHtmlString(hex, out Color c))
                parsedColors.Add(c);
            else
                return false;
        }

        /// VALIDAR IMÁGENES
        List<string> urls = new();

        foreach (var img in remote.game.container_images)
        {
            if (img == null || string.IsNullOrWhiteSpace(img.url))
                return false;

            urls.Add(img.url);
        }

        /// =========================
        /// APPLY
        /// =========================

        containerImageUrls = urls;
        ContainerColors = parsedColors;
        ContainersPerKey = remote.game.containers_per_key;
        KeysPerAction = remote.rewards.keys_per_action;

        if (!string.IsNullOrWhiteSpace(remote.meta?.brand_id))
            BrandId = remote.meta.brand_id;

        if (!string.IsNullOrWhiteSpace(remote.meta?.campaign_id))
            CampaignId = remote.meta.campaign_id;

        if (!string.IsNullOrWhiteSpace(remote.branding?.main_logo_url))
            MainLogoUrl = remote.branding.main_logo_url;

        if (!string.IsNullOrWhiteSpace(remote.audio?.key_win_url))
            KeyWinUrl = remote.audio.key_win_url;

        if (!string.IsNullOrWhiteSpace(remote.audio?.victory_url))
            VictoryUrl = remote.audio.victory_url;

        if (remote.texts?.game_over_messages != null &&
            remote.texts.game_over_messages.Count > 0)
        {
            GameOverMessages = remote.texts.game_over_messages;
        }

        return true;
    }

    private bool IsValidHex(string hex)
    {
        return Regex.IsMatch(hex, "^#([0-9A-Fa-f]{6})$");
    }

    #endregion

    #region ASSET REQUESTS

    public List<AssetRequest> GetAssetRequests()
    {
        var requests = new List<AssetRequest>();

        ContainerSprites = new List<Sprite>();

        foreach (var url in containerImageUrls)
        {
            requests.Add(new AssetRequest(
                url,
                AssetType.Sprite,
                obj => ContainerSprites.Add(obj as Sprite),
                isRequired: true
            ));
        }

        if (!string.IsNullOrWhiteSpace(MainLogoUrl))
        {
            requests.Add(new AssetRequest(
                MainLogoUrl,
                AssetType.Texture,
                obj => MainLogo = obj as Texture2D
            ));
        }

        if (!string.IsNullOrWhiteSpace(KeyWinUrl))
        {
            requests.Add(new AssetRequest(
                KeyWinUrl,
                AssetType.Audio,
                obj => KeyWin = obj as AudioClip
            ));
        }

        if (!string.IsNullOrWhiteSpace(VictoryUrl))
        {
            requests.Add(new AssetRequest(
                VictoryUrl,
                AssetType.Audio,
                obj => Victory = obj as AudioClip
            ));
        }

        return requests;
    }

    #endregion
}