using System;

/// <summary>
/// Representa una solicitud genérica de carga de asset.
/// </summary>
public struct AssetRequest
{
    public string Url;
    public AssetType Type;
    public bool IsRequired;
    public Action<UnityEngine.Object> OnLoaded;

    public AssetRequest(string url, AssetType type, Action<Object> onLoaded, bool isRequired = false)
    {
        Url = url;
        Type = type;
        OnLoaded = onLoaded;
        IsRequired = isRequired;
    }
}

/// <summary>
/// Tipos de assets soportados.
/// </summary>
public enum AssetType
{
    Texture,
    Sprite,
    Audio
}