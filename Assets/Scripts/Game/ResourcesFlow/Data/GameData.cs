using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Data/Game Data")]
public class GameData : ScriptableObject
{
    [Header("Meta")]
    private string brandId;
    private string campaignId;

    public string BrandId => brandId;
    public string CampaignId => campaignId;

    [Header("Branding")]
    [SerializeField] private Texture2D mainLogo;
    private string mainLogoUrl;

    public Texture2D MainLogo => mainLogo;
    public string MainLogoUrl => mainLogoUrl;

    [Header("Game")]
    [SerializeField] private List<Sprite> containerSprites;
    [SerializeField] private List<Color> containerColors;
    [SerializeField] private int containersPerKey;

    public List<Sprite> ContainerSprites => containerSprites;
    public List<Color> ContainerColors => containerColors;
    public int ContainersPerKey => containersPerKey;

    [Header("Audio")]
    [SerializeField] private AudioClip keyWin;
    [SerializeField] private AudioClip victory;

    private string keyWinUrl;
    private string victoryUrl;

    public AudioClip KeyWin => keyWin;
    public AudioClip Victory => victory;

    public string KeyWinUrl => keyWinUrl;
    public string VictoryUrl => victoryUrl;

    [Header("Texts")]
    [SerializeField] private List<string> gameOverMessages;

    public IReadOnlyList<string> GameOverMessages => gameOverMessages;

    [Header("Rewards")]
    [SerializeField] private int keysPerAction;

    public int KeysPerAction => keysPerAction;
}