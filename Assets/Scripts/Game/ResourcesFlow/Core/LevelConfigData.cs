using System;
using System.Collections.Generic;

[Serializable]
public class LevelConfigData
{
    public MetaData meta;
    public BrandingData branding;
    public GameDataBlock game;
    public AudioData audio;
    public TextData texts;
    public RewardData rewards;
}

#region Meta

[Serializable]
public class MetaData
{
    public string brand_id;
    public string campaign_id;
}

#endregion

#region Branding

[Serializable]
public class BrandingData
{
    public string main_logo_url;
}

#endregion

#region Game

[Serializable]
public class GameDataBlock
{
    public List<string> container_colors;
    public List<ContainerImage> container_images;
    public int containers_per_key;
}

[Serializable]
public class ContainerImage
{
    public string url;
}

#endregion

#region Audio

[Serializable]
public class AudioData
{
    public string key_win_url;
    public string victory_url;
}

#endregion

#region Texts

[Serializable]
public class TextData
{
    public List<string> game_over_messages;
}

#endregion

#region Rewards

[Serializable]
public class RewardData
{
    public int keys_per_action;
}

#endregion