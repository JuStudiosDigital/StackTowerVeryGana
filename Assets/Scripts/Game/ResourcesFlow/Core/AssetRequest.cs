using System;

/// <summary>
/// Representa una solicitud inmutable para la carga de un recurso remoto.
/// Encapsula la información necesaria para resolver el asset y aplicar su resultado.
/// </summary>
public struct AssetRequest
{
    /// <summary>
    /// URL absoluta del recurso a descargar.
    /// </summary>
    public string Url;

    /// <summary>
    /// Tipo de asset esperado, utilizado para seleccionar el flujo de carga adecuado.
    /// </summary>
    public AssetType Type;

    /// <summary>
    /// Callback invocado al completarse la carga del asset.
    /// Permite desacoplar la asignación del recurso del sistema de descarga.
    /// </summary>
    public Action<UnityEngine.Object> OnLoaded;

    /// <summary>
    /// Inicializa una nueva solicitud de carga de asset.
    /// </summary>
    /// <param name="url">URL del recurso remoto.</param>
    /// <param name="type">Tipo de asset a cargar.</param>
    /// <param name="onLoaded">Acción a ejecutar cuando el asset esté disponible.</param>
    public AssetRequest(string url, AssetType type, Action<UnityEngine.Object> onLoaded)
    {
        Url = url;
        Type = type;
        OnLoaded = onLoaded;
    }
}

/// <summary>
/// Define los tipos de recursos soportados por el sistema de carga.
/// Determina el método de descarga y procesamiento aplicado.
/// </summary>
public enum AssetType
{
    Texture,
    Sprite,
    Audio
}