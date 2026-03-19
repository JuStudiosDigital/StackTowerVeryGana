/// <summary>
/// Evalúa el modo de ejecución final de un nivel a partir de una
/// configuración previamente normalizada y combinada con fallback.
/// 
/// Esta clase no valida disponibilidad real de recursos (descargas),
/// únicamente toma decisiones lógicas basadas en los datos resultantes
/// del proceso de normalización y merge.
/// </summary>
public static class LevelConfigPolicyEvaluator
{
    #region Public API

    /// <summary>
    /// Determina el modo de ejecución del nivel según:
    /// - La existencia de configuración válida.
    /// - La presencia del recurso mínimo obligatorio (imagen del puzzle).
    /// - La existencia o ausencia de un identificador de marca (brand_id).
    /// </summary>
    /// <param name="config">
    /// Configuración final del nivel, ya normalizada y combinada
    /// con valores de fallback locales.
    /// </param>
    /// <returns>
    /// El modo de ejecución que debe aplicar el sistema en runtime.
    /// </returns>
    public static LevelRuntimeMode Evaluate(LevelConfigData config)
    {
        /// Defensa extrema:
        /// Este caso solo debería ocurrir si hubo un fallo crítico
        /// previo en la carga o normalización de la configuración.
        if (config == null)
        {
            return LevelRuntimeMode.FullFallback;
        }

        /// La imagen del puzzle es el único recurso estrictamente
        /// obligatorio para que el nivel pueda ejecutarse.
        /// Si no existe (incluso tras el merge), se fuerza fallback total.
        if (string.IsNullOrWhiteSpace(config.game?.image_url))
        {
            return LevelRuntimeMode.FullFallback;
        }

        /// Si existe un identificador de marca válido, el nivel
        /// se considera apto para ejecutarse con publicidad activa.
        if (!string.IsNullOrWhiteSpace(config.meta?.brand_id))
        {
            return LevelRuntimeMode.AdsEnabled;
        }

        /// Si no existe brand_id pero sí contenido remoto útil
        /// (ej. imagen del puzzle), el nivel se ejecuta sin anuncios.
        return LevelRuntimeMode.NoAdsWithRemoteContent;
    }

    #endregion
}

/// <summary>
/// Representa el modo de ejecución del nivel en tiempo de ejecución,
/// determinando si se habilita publicidad y qué tipo de contenido
/// se utiliza para la experiencia de juego.
/// </summary>
public enum LevelRuntimeMode
{
    /// <summary>
    /// El nivel se ejecuta con contenido publicitario activo,
    /// utilizando recursos remotos asociados a una marca válida.
    /// </summary>
    AdsEnabled,

    /// <summary>
    /// El nivel se ejecuta sin anuncios, pero utilizando
    /// contenido remoto válido no asociado a patrocinadores.
    /// </summary>
    NoAdsWithRemoteContent,

    /// <summary>
    /// El nivel utiliza exclusivamente recursos locales integrados
    /// en el juego debido a un fallo crítico del contenido remoto.
    /// </summary>
    FullFallback
}
