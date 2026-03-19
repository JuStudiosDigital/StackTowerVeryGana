/// <summary>
/// Responsable de combinar una configuración de nivel remota
/// con una configuración base local.
/// 
/// Aplica una política de merge defensiva:
/// - El JSON remoto solo sobrescribe valores válidos.
/// - La configuración local garantiza valores seguros.
/// 
/// Este sistema asume que ambas configuraciones ya han sido
/// normalizadas previamente.
/// </summary>
public static class LevelConfigMerger
{
    #region Public API

    /// <summary>
    /// Combina la configuración remota con la configuración local de fallback.
    /// 
    /// Reglas de combinación:
    /// - Campos string solo se sobrescriben si el remoto no es nulo ni vacío.
    /// - Campos numéricos solo se sobrescriben si el valor remoto es mayor a cero.
    /// - El <c>brand_id</c> remoto siempre sobrescribe al local, incluso si está vacío,
    ///   ya que su ausencia es semánticamente relevante (modo sin anunciantes).
    /// 
    /// El resultado final siempre es una configuración válida y segura.
    /// </summary>
    /// <param name="remote">
    /// Configuración remota descargada desde internet.
    /// Puede ser nula o parcialmente incompleta.
    /// </param>
    /// <param name="fallback">
    /// Configuración local base que actúa como respaldo absoluto.
    /// Nunca debería ser nula.
    /// </param>
    /// <returns>
    /// Configuración de nivel final resultante del merge.
    /// </returns>
    public static LevelConfigData MergeWithFallback(
        LevelConfigData remote,
        LevelConfigData fallback)
    {
        /// Normalización defensiva previa para evitar null references
        remote = LevelConfigNormalizer.Normalize(remote);
        fallback = LevelConfigNormalizer.Normalize(fallback);

        #region Meta

        /// El brand_id remoto siempre tiene prioridad,
        /// incluso si llega vacío (modo sin anunciantes).
        fallback.meta.brand_id =
            remote.meta.brand_id;

        #endregion

        #region Game

        /// La imagen del juego solo se sobrescribe
        /// si el remoto contiene una URL válida.
        fallback.game.image_url =
            string.IsNullOrWhiteSpace(remote.game.image_url)
                ? fallback.game.image_url
                : remote.game.image_url;

        #endregion

        #region Audio

        /// Sonido de victoria
        fallback.audio.victory_sound_url =
            string.IsNullOrWhiteSpace(remote.audio.victory_sound_url)
                ? fallback.audio.victory_sound_url
                : remote.audio.victory_sound_url;

        /// Sonido de moneda
        fallback.audio.coin_sound_url =
            string.IsNullOrWhiteSpace(remote.audio.coin_sound_url)
                ? fallback.audio.coin_sound_url
                : remote.audio.coin_sound_url;

        #endregion

        #region Branding

        /// Logo o watermark de la marca
        fallback.branding.logo_watermark_url =
            string.IsNullOrWhiteSpace(remote.branding.logo_watermark_url)
                ? fallback.branding.logo_watermark_url
                : remote.branding.logo_watermark_url;

        #endregion

        #region Textos

        /// Texto de victoria mostrado al finalizar el nivel
        fallback.texts.victory_phrase =
            string.IsNullOrWhiteSpace(remote.texts.victory_phrase)
                ? fallback.texts.victory_phrase
                : remote.texts.victory_phrase;

        #endregion

        #region Recompensas

        /// Recompensa por acción individual
        fallback.rewards.coins_per_action =
            remote.rewards.coins_per_action > 0
                ? remote.rewards.coins_per_action
                : fallback.rewards.coins_per_action;

        /// Recompensa por completar el nivel
        fallback.rewards.coins_on_completion =
            remote.rewards.coins_on_completion > 0
                ? remote.rewards.coins_on_completion
                : fallback.rewards.coins_on_completion;

        #endregion

        return fallback;
    }

    #endregion
}
