using System;

/// <summary>
/// Payload enviado al backend para solicitar configuración dinámica.
/// </summary>
[Serializable]
public struct LevelConfigRequestDto
{
    public string sessionToken;
    public string userHash;
    public bool isBrandedMode;
    public string campaignId;
    public string gameTitle;
}