using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Contenedor de datos base del juego utilizado como fallback local.
/// Define valores iniciales para branding, gameplay, audio y recompensas,
/// que pueden ser sobrescritos en tiempo de ejecución por datos remotos.
/// </summary>
[CreateAssetMenu(menuName = "Game/Data/Game Data")]
public class GameData : ScriptableObject
{
    #region Meta

    [Header("Meta")]

    private string brandId;
    private string campaignId;

    /// <summary>
    /// Identificador de la marca asociada al juego.
    /// </summary>
    public string BrandId => brandId;

    /// <summary>
    /// Identificador de la campaña activa.
    /// </summary>
    public string CampaignId => campaignId;

    #endregion

    #region Branding

    [Header("Branding")]

    [SerializeField] private Texture2D mainLogo;
    [SerializeField] private Texture2D watermarkLogo;

    private string mainLogoUrl;
    private string watermarkLogoUrl;

    /// <summary>
    /// Logo principal utilizado en el juego.
    /// </summary>
    public Texture2D MainLogo => mainLogo;

    /// <summary>
    /// Logo secundario o marca de agua.
    /// </summary>
    public Texture2D WatermarkLogo => watermarkLogo;

    /// <summary>
    /// URL remota del logo principal.
    /// </summary>
    public string MainLogoUrl => mainLogoUrl;

    /// <summary>
    /// URL remota del logo de marca de agua.
    /// </summary>
    public string WatermarkLogoUrl => watermarkLogoUrl;

    #endregion

    #region Game

    [Header("Game")]

    [SerializeField] private Texture2D puzzleImage;
    private string puzzleImageUrl;

    /// <summary>
    /// Imagen principal utilizada en la mecánica del juego.
    /// </summary>
    public Texture2D PuzzleImage => puzzleImage;

    /// <summary>
    /// URL remota de la imagen del juego.
    /// </summary>
    public string PuzzleImageUrl => puzzleImageUrl;

    #endregion

    #region Audio

    [Header("Audio")]

    [SerializeField] private AudioClip music;
    [SerializeField] private AudioClip keyWin;
    [SerializeField] private AudioClip victory;

    private string musicUrl;
    private string keyWinUrl;
    private string victoryUrl;

    /// <summary>
    /// Música de fondo del juego.
    /// </summary>
    public AudioClip Music => music;

    /// <summary>
    /// Sonido reproducido al obtener una recompensa parcial.
    /// </summary>
    public AudioClip KeyWin => keyWin;

    /// <summary>
    /// Sonido reproducido al completar el nivel.
    /// </summary>
    public AudioClip Victory => victory;

    /// <summary>
    /// URL remota de la música de fondo.
    /// </summary>
    public string MusicUrl => musicUrl;

    /// <summary>
    /// URL remota del sonido de recompensa parcial.
    /// </summary>
    public string KeyWinUrl => keyWinUrl;

    /// <summary>
    /// URL remota del sonido de victoria.
    /// </summary>
    public string VictoryUrl => victoryUrl;

    #endregion

    #region Texts

    [Header("Texts")]

    [SerializeField] private List<string> victoryMessages;

    /// <summary>
    /// Mensajes mostrados al completar el nivel.
    /// </summary>
    public IReadOnlyList<string> VictoryMessages => victoryMessages;

    #endregion

    #region Rewards

    [Header("Rewards")]

    private int keysPerAction;
    private int keysOnCompletion;

    /// <summary>
    /// Cantidad de llaves otorgadas por acción durante el juego.
    /// </summary>
    public int KeysPerAction => keysPerAction;

    /// <summary>
    /// Cantidad de llaves otorgadas al completar el nivel.
    /// </summary>
    public int KeysOnCompletion => keysOnCompletion;

    #endregion
}