using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Proveedor centralizado de datos de branding y configuración de gameplay.
/// Actúa como fachada entre GameDataProvider y el juego,
/// permitiendo fallback seguro y sincronización dinámica.
/// </summary>
public sealed class BrandingManager : MonoBehaviour
{
    #region Singleton

    public static BrandingManager Instance { get; private set; }

    #endregion

    #region Events

    public static event Action OnBrandingReady;

    #endregion

    #region Inspector

    [Header("Branding - Imágenes")]

    [SerializeField]
    [Tooltip("Sprites de contenedores usados como fallback si no hay datos remotos.")]
    private List<Sprite> images = new();

    [Header("Branding - Textos")]

    [SerializeField]
    [Tooltip("Textos de branding usados como fallback si no hay datos remotos.")]
    private List<string> texts = new();

    [Header("Branding - Colores")]

    [SerializeField]
    [Tooltip("Colores de contenedores usados como fallback si no hay datos remotos.")]
    private List<Color> colors = new();

    [Header("Branding - Gameplay")]

    [SerializeField]
    [Tooltip("Cantidad de contenedores necesarios por moneda. 0 desactiva completamente el spawn de monedas.")]
    private int containersPerCoin = 0;

    [Header("Reward Configuration")]

    [SerializeField]
    [Tooltip("Cantidad de monedas otorgadas por acción. 0 desactiva recompensa por acción.")]
    private int coinsPerAction = 0;

    #endregion

    #region Private State

    private bool isSyncedWithProvider;

    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        TrySyncWithGameDataProvider();
    }

    #endregion

    #region Sync

    /// <summary>
    /// Sincroniza los datos desde GameDataProvider.
    /// Conserva valores en 0 porque representan configuración válida para desactivar recompensas.
    /// </summary>
    private void TrySyncWithGameDataProvider()
    {
        if (isSyncedWithProvider)
            return;

        if (GameDataProvider.Instance == null || !GameDataProvider.Instance.IsInitialized)
            return;

        var provider = GameDataProvider.Instance;

        var containerSprites = provider.GetContainers();
        if (containerSprites != null && containerSprites.Count > 0)
        {
            images = new List<Sprite>(containerSprites);
        }

        var containerColors = provider.GetContainerColors();
        if (containerColors != null && containerColors.Count > 0)
        {
            colors = new List<Color>(containerColors);
        }

        int containersValue = provider.GetContainersPerKey();
        if (containersValue >= 0)
        {
            containersPerCoin = containersValue;
        }

        int keysPerAction = provider.GetKeysPerAction();
        if (keysPerAction >= 0)
        {
            coinsPerAction = keysPerAction;
        }

        isSyncedWithProvider = true;

        OnBrandingReady?.Invoke();
    }

    #endregion

    #region Public API

    public Sprite GetRandomImage()
    {
        TrySyncWithGameDataProvider();

        if (images == null || images.Count == 0)
            return null;

        return images[UnityEngine.Random.Range(0, images.Count)];
    }

    public string GetRandomText()
    {
        TrySyncWithGameDataProvider();

        if (texts == null || texts.Count == 0)
            return string.Empty;

        return texts[UnityEngine.Random.Range(0, texts.Count)];
    }

    public Color GetRandomColor()
    {
        TrySyncWithGameDataProvider();

        if (colors == null || colors.Count == 0)
            return Color.white;

        return colors[UnityEngine.Random.Range(0, colors.Count)];
    }

    /// <summary>
    /// Cantidad de contenedores requeridos para generar una moneda.
    /// 0 significa que el spawn de monedas está desactivado.
    /// </summary>
    public int GetContainersPerCoin()
    {
        TrySyncWithGameDataProvider();
        return Mathf.Max(0, containersPerCoin);
    }

    /// <summary>
    /// Cantidad de monedas otorgadas por acción.
    /// 0 significa que la recompensa por acción está desactivada.
    /// </summary>
    public int CoinsPerAction
    {
        get
        {
            TrySyncWithGameDataProvider();
            return Mathf.Max(0, coinsPerAction);
        }
    }

    #endregion
}