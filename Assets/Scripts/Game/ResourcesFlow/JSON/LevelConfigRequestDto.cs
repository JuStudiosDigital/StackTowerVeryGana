using System;

/// <summary>
/// Payload enviado al servicio en la nube para solicitar
/// la configuración dinámica del nivel.
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
