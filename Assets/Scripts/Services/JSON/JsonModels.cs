using System;

#region MetaData

/// <summary>
/// Contiene los metadatos generales del nivel.
/// Esta información suele ser utilizada para identificar el origen
/// o el contexto del contenido recibido desde un backend o servicio remoto.
/// </summary>
[Serializable]
public class MetaData
{
    /// <summary>
    /// Identificador único de la marca o campaña asociada al nivel.
    /// Normalmente utilizado para lógica de branding, analítica o personalización.
    /// </summary>
    public string brand_id;
}

#endregion

#region Branding

/// <summary>
/// Define la información visual de branding asociada al nivel.
/// Incluye recursos gráficos y configuración de colores.
/// </summary>
[Serializable]
public class BrandingData
{
    /// <summary>
    /// URL remota del logo o watermark que debe mostrarse durante el nivel.
    /// Puede ser utilizada para cargar dinámicamente una imagen desde red.
    /// </summary>
    public string logo_watermark_url;

    /// <summary>
    /// Conjunto de colores corporativos utilizados para personalizar la interfaz.
    /// </summary>
    public BrandingColors colors;
}

/// <summary>
/// Define los colores principales utilizados para el branding del nivel.
/// Los valores se esperan en formato hexadecimal (por ejemplo: #FF5500).
/// </summary>
[Serializable]
public class BrandingColors
{
    /// <summary>
    /// Color primario de la marca.
    /// Generalmente usado en elementos principales de UI.
    /// </summary>
    public string primary;

    /// <summary>
    /// Color secundario de la marca.
    /// Usado como complemento visual del color primario.
    /// </summary>
    public string secondary;

    /// <summary>
    /// Color de acento.
    /// Utilizado para resaltar acciones, botones o estados especiales.
    /// </summary>
    public string accent;
}

#endregion

#region Puzzle

/// <summary>
/// Contiene la información necesaria para la configuración del puzzle.
/// </summary>
[Serializable]
public class PuzzleData
{
    /// <summary>
    /// URL remota de la imagen base utilizada para generar el rompecabezas.
    /// Esta imagen suele descargarse dinámicamente en tiempo de ejecución.
    /// </summary>
    public string image_url;
}

#endregion

#region Audio

/// <summary>
/// Define los recursos de audio asociados al nivel.
/// Estos sonidos suelen reproducirse en eventos específicos del gameplay.
/// </summary>
[Serializable]
public class AudioData
{
    /// <summary>
    /// URL del sonido que se reproduce al completar exitosamente el nivel.
    /// </summary>
    public string victory_sound_url;

    /// <summary>
    /// URL del sonido que se reproduce al obtener monedas u otras recompensas.
    /// </summary>
    public string coin_sound_url;
}

#endregion

#region Texts

/// <summary>
/// Contiene los textos dinámicos asociados al nivel.
/// Permite personalizar mensajes sin necesidad de recompilar el juego.
/// </summary>
[Serializable]
public class TextData
{
    /// <summary>
    /// Frase mostrada al completar el nivel.
    /// Puede ser utilizada en pantallas de victoria o feedback al jugador.
    /// </summary>
    public string victory_phrase;
}

#endregion

#region Rewards

/// <summary>
/// Define la configuración de recompensas del nivel.
/// Utilizado por sistemas de economía o progresión del jugador.
/// </summary>
[Serializable]
public class RewardData
{
    /// <summary>
    /// Cantidad de monedas otorgadas por cada acción válida del jugador.
    /// Por ejemplo: mover una ficha, realizar una rotación, etc.
    /// </summary>
    public int coins_per_action;

    /// <summary>
    /// Cantidad de monedas otorgadas al completar el nivel.
    /// Normalmente es una recompensa mayor por finalización.
    /// </summary>
    public int coins_on_completion;
}

#endregion
