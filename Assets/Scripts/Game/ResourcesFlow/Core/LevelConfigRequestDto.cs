using System;

/// <summary>
/// Representa el payload enviado al backend para solicitar
/// la configuración dinámica del nivel en función del contexto de sesión.
/// </summary>
[Serializable]
public struct LevelConfigRequestDto
{
    /// <summary>
    /// Token de sesión que identifica de forma segura la instancia del usuario.
    /// </summary>
    public string sessionToken;

    /// <summary>
    /// Hash anónimo del usuario utilizado para trazabilidad o segmentación.
    /// </summary>
    public string userHash;

    /// <summary>
    /// Indica si el juego debe operar en modo branded,
    /// habilitando la carga de configuración remota.
    /// </summary>
    public bool isBrandedMode;

    /// <summary>
    /// Identificador de la campaña activa, utilizado para personalización.
    /// </summary>
    public string campaignId;

    /// <summary>
    /// Identificador del juego o variante solicitada.
    /// </summary>
    public string gameTitle;
}