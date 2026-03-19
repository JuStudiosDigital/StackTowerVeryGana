using System;

/// <summary>
/// Contenedor raíz que representa la configuración completa de un nivel,
/// deserializada desde un archivo JSON remoto o local.
/// 
/// Esta clase actúa exclusivamente como un Data Transfer Object (DTO)
/// y no debe contener lógica de negocio.
/// </summary>
[Serializable]
public class LevelConfigData
{
    #region Metadata

    /// <summary>
    /// Información general y de control del nivel.
    /// Usualmente incluye identificadores, versión o datos del proveedor.
    /// </summary>
    public MetaData meta;

    #endregion

    #region Branding

    /// <summary>
    /// Datos visuales y de identidad de marca asociados al nivel,
    /// como colores, logotipos o estilos gráficos.
    /// </summary>
    public BrandingData branding;

    #endregion

    #region Puzzle Configuration

    /// <summary>
    /// Configuración específica del rompecabezas,
    /// incluyendo imagen base, tamaño, dificultad o reglas.
    /// </summary>
    public PuzzleData game;

    #endregion

    #region Audio Configuration

    /// <summary>
    /// Configuración de audio del nivel,
    /// incluyendo sonidos de victoria, derrota o ambientación.
    /// </summary>
    public AudioData audio;

    #endregion

    #region Text Content

    /// <summary>
    /// Textos localizados o configurables del nivel,
    /// como mensajes, títulos o instrucciones.
    /// </summary>
    public TextData texts;

    #endregion

    #region Rewards

    /// <summary>
    /// Información de recompensas asociadas al nivel,
    /// como monedas, puntos, ítems o progreso.
    /// </summary>
    public RewardData rewards;

    #endregion
}
