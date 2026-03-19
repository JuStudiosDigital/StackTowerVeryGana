using UnityEngine;

/// <summary>
/// Proveedor estático de configuración local del nivel.
/// Actúa como fallback absoluto cuando no existe conectividad,
/// el JSON remoto falla o el sistema entra en modo de recuperación total.
/// </summary>
/// <remarks>
/// Este proveedor garantiza que el juego siempre pueda iniciar un nivel
/// con datos válidos, incluso ante fallos críticos de red o assets.
/// </remarks>
public static class LocalLevelConfigProvider
{
    #region Constants

    /// <summary>
    /// Ruta dentro de la carpeta Resources donde se almacena
    /// el archivo JSON de configuración por defecto del nivel.
    /// </summary>
    private const string DefaultConfigPath = "LevelConfigs/default_level_config";

    #endregion

    #region Public API

    /// <summary>
    /// Carga la configuración local por defecto del nivel desde Resources.
    /// </summary>
    /// <remarks>
    /// Este método representa el fallback principal cuando:
    /// - No hay conectividad.
    /// - Falla la descarga del JSON remoto.
    /// - El JSON remoto es inválido o incompleto.
    /// 
    /// Siempre devuelve una instancia válida de <see cref="LevelConfigData"/>.
    /// </remarks>
    /// <returns>
    /// Configuración del nivel deserializada desde el JSON local
    /// o una configuración segura hardcodeada en caso de error crítico.
    /// </returns>
    public static LevelConfigData LoadFallbackConfig()
    {
        TextAsset jsonAsset = Resources.Load<TextAsset>(DefaultConfigPath);

        if (jsonAsset == null)
        {
            DevLog.Error(
                "No se encontró el JSON local de configuración del nivel. " +
                "Verifica que exista en Resources/LevelConfigs/default_level_config.json"
            );

            return CreateHardcodedSafeFallback();
        }

        return JsonUtility.FromJson<LevelConfigData>(jsonAsset.text);
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Crea una configuración de nivel mínima y completamente segura.
    /// </summary>
    /// <remarks>
    /// Este método actúa como último recurso de defensa.
    /// Se utiliza únicamente cuando:
    /// - El JSON remoto falla.
    /// - El JSON local no existe o fue eliminado.
    /// 
    /// Su objetivo es evitar crashes y permitir que el juego
    /// continúe ejecutándose en un estado funcional básico.
    /// </remarks>
    /// <returns>
    /// Instancia de <see cref="LevelConfigData"/> con valores
    /// predeterminados seguros y coherentes.
    /// </returns>
    private static LevelConfigData CreateHardcodedSafeFallback()
    {
        return new LevelConfigData
        {
            meta = new MetaData
            {
                brand_id = string.Empty
            },
            branding = new BrandingData
            {
                logo_watermark_url = string.Empty,
                colors = new BrandingColors
                {
                    primary = "#FFFFFF",
                    secondary = "#FFFFFF",
                    accent = "#000000"
                }
            },
            game = new PuzzleData
            {
                image_url = string.Empty
            },
            audio = new AudioData
            {
                victory_sound_url = string.Empty,
                coin_sound_url = string.Empty
            },
            texts = new TextData
            {
                victory_phrase = "¡Nivel completado!"
            },
            rewards = new RewardData
            {
                coins_per_action = 1,
                coins_on_completion = 10
            }
        };
    }

    #endregion
}
