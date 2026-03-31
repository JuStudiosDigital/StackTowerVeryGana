using System;
using System.Collections.Generic;

/// <summary>
/// Representa la estructura completa del JSON recibido desde backend.
/// Actúa como contenedor de datos para configuración dinámica del juego.
/// </summary>
[Serializable]
public class LevelConfigData
{
    /// <summary>
    /// Información de identificación de la campaña y marca.
    /// </summary>
    public MetaData meta;

    /// <summary>
    /// Configuración visual asociada al anunciante.
    /// </summary>
    public BrandingData branding;

    /// <summary>
    /// Configuración principal del gameplay.
    /// </summary>
    public GameDataBlock game;

    /// <summary>
    /// Configuración de recursos de audio remotos.
    /// </summary>
    public AudioData audio;

    /// <summary>
    /// Contenido textual dinámico mostrado al jugador.
    /// </summary>
    public TextData texts;

    /// <summary>
    /// Parámetros de recompensas del juego.
    /// </summary>
    public RewardData rewards;
}

#region Meta

/// <summary>
/// Contiene identificadores utilizados para segmentación de marca y campaña.
/// </summary>
[Serializable]
public class MetaData
{
    /// <summary>
    /// Identificador único de la marca.
    /// </summary>
    public string brand_id;

    /// <summary>
    /// Identificador único de la campaña.
    /// </summary>
    public string campaign_id;
}

#endregion

#region Branding

/// <summary>
/// Define los recursos visuales utilizados para el branding del juego.
/// </summary>
[Serializable]
public class BrandingData
{
    /// <summary>
    /// URL del logo principal de la marca.
    /// </summary>
    public string main_logo_url;

    /// <summary>
    /// URL del logo tipo watermark.
    /// </summary>
    public string watermark_logo_url;
}

#endregion

#region Game

/// <summary>
/// Contiene la configuración visual principal del gameplay.
/// </summary>
[Serializable]
public class GameDataBlock
{
    /// <summary>
    /// URL de la imagen utilizada en el puzzle o escena principal.
    /// </summary>
    public string puzzle_image_url;
}

#endregion

#region Audio

/// <summary>
/// Define las rutas remotas de los efectos de audio utilizados en el juego.
/// </summary>
[Serializable]
public class AudioData
{
    /// <summary>
    /// URL de la música de fondo.
    /// </summary>
    public string music_url;

    /// <summary>
    /// URL del sonido asociado a la obtención de recompensas.
    /// </summary>
    public string key_win_url;

    /// <summary>
    /// URL del sonido de victoria.
    /// </summary>
    public string victory_url;
}

#endregion

#region Texts

/// <summary>
/// Contiene los textos dinámicos mostrados durante la experiencia de juego.
/// </summary>
[Serializable]
public class TextData
{
    /// <summary>
    /// Lista de mensajes mostrados al completar el nivel.
    /// </summary>
    public List<string> victory_messages;
}

#endregion

#region Rewards

/// <summary>
/// Define los valores de recompensas configurables del juego.
/// </summary>
[Serializable]
public class RewardData
{
    /// <summary>
    /// Cantidad de llaves otorgadas por acción.
    /// </summary>
    public int keys_per_action;

    /// <summary>
    /// Cantidad de llaves otorgadas al completar el nivel.
    /// </summary>
    public int keys_on_completion;
}

#endregion