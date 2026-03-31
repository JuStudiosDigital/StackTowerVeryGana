using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Proveedor centralizado de datos de branding y configuración de gameplay.
/// 
/// Este componente actúa como una fuente única de verdad (Single Source of Truth)
/// para elementos visuales y parámetros configurables del juego.
///
/// Responsabilidades:
/// - Proveer variaciones visuales (sprites, textos, colores)
/// - Exponer parámetros de gameplay configurables sin hardcode
/// 
/// Nota de diseño:
/// Se implementa como singleton para facilitar acceso global controlado,
/// evitando la necesidad de inyección manual en múltiples sistemas.
/// </summary>
public sealed class BrandingManager : MonoBehaviour
{
    /// <summary>
    /// Instancia global del sistema de branding.
    /// </summary>
    public static BrandingManager Instance { get; private set; }

    #region Inspector

    [Header("Branding - Imágenes")]

    [SerializeField]
    private List<Sprite> images = new();

    [Header("Branding - Textos")]

    [SerializeField]
    private List<string> texts = new();

    [Header("Branding - Colores")]

    [SerializeField]
    private List<Color> colors = new();

    [Header("Branding - Gameplay")]

    /// <summary>
    /// Cantidad de contenedores necesarios para generar una moneda.
    /// </summary>
    [SerializeField]
    private int containersPerCoin = 3;

    [Header("Reward Configuration")]

    /// <summary>
    /// Cantidad de monedas otorgadas por acción (lógicas y visuales).
    /// </summary>
    [SerializeField]
    private int coinsPerAction = 2;

    #endregion

    #region Unity

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    #endregion

    #region Public API

    public Sprite GetRandomImage()
    {
        if (images.Count == 0)
            return null;

        return images[Random.Range(0, images.Count)];
    }

    public string GetRandomText()
    {
        if (texts.Count == 0)
            return string.Empty;

        return texts[Random.Range(0, texts.Count)];
    }

    public Color GetRandomColor()
    {
        if (colors.Count == 0)
            return Color.white;

        return colors[Random.Range(0, colors.Count)];
    }

    /// <summary>
    /// Obtiene la cantidad de contenedores requeridos para generar una moneda.
    /// </summary>
    public int GetContainersPerCoin()
    {
        return Mathf.Max(1, containersPerCoin);
    }

    /// <summary>
    /// Cantidad de monedas otorgadas por acción (lógicas y visuales).
    /// </summary>
    public int CoinsPerAction => coinsPerAction;

    #endregion
}