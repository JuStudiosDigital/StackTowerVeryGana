/// <summary>
/// Normaliza una configuración de nivel deserializada desde JSON,
/// garantizando que todos los sub-objetos existan y que sus campos
/// contengan valores seguros por defecto.
/// 
/// Este proceso evita referencias nulas durante el runtime y permite
/// que el sistema de merge y evaluación de políticas funcione de forma
/// predecible incluso ante JSON incompletos o malformados.
/// </summary>
public static class LevelConfigNormalizer
{
    #region Public API

    /// <summary>
    /// Normaliza una instancia de <see cref="LevelConfigData"/> asegurando
    /// que todos los bloques de datos requeridos existan.
    /// 
    /// Si la configuración recibida es nula, se genera una estructura
    /// completamente válida con valores por defecto seguros.
    /// </summary>
    /// <param name="config">
    /// Configuración de nivel deserializada desde JSON remoto o local.
    /// Puede ser nula.
    /// </param>
    /// <returns>
    /// Instancia normalizada de <see cref="LevelConfigData"/> lista para
    /// ser utilizada por el resto del sistema.
    /// </returns>
    public static LevelConfigData Normalize(LevelConfigData config)
    {
        if (config == null)
        {
            return CreateEmpty();
        }

        config.meta ??= new MetaData
        {
            brand_id = string.Empty
        };

        config.branding ??= new BrandingData
        {
            logo_watermark_url = string.Empty,
            colors = new BrandingColors
            {
                primary = "#FFFFFF",
                secondary = "#FFFFFF",
                accent = "#000000"
            }
        };

        config.branding.colors ??= new BrandingColors
        {
            primary = "#FFFFFF",
            secondary = "#FFFFFF",
            accent = "#000000"
        };

        config.game ??= new PuzzleData
        {
            image_url = string.Empty
        };

        config.audio ??= new AudioData
        {
            victory_sound_url = string.Empty,
            coin_sound_url = string.Empty
        };

        config.texts ??= new TextData
        {
            victory_phrase = string.Empty
        };

        config.rewards ??= new RewardData
        {
            coins_per_action = 0,
            coins_on_completion = 0
        };

        return config;
    }

    #endregion

    #region Private Helpers

    /// <summary>
    /// Crea una configuración de nivel completamente vacía y segura,
    /// utilizada como último recurso cuando la configuración original
    /// es nula.
    /// 
    /// Este método garantiza que incluso en escenarios extremos
    /// (fallos críticos de deserialización) el juego no colapse.
    /// </summary>
    /// <returns>
    /// Instancia válida de <see cref="LevelConfigData"/> con todos los
    /// sub-objetos inicializados.
    /// </returns>
    private static LevelConfigData CreateEmpty()
    {
        return Normalize(new LevelConfigData());
    }

    #endregion
}
