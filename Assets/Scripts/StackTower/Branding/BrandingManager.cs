using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provee datos centralizados de branding para sistemas visuales.
/// Permite desacoplar contenido visual del gameplay.
/// </summary>
public sealed class BrandingManager : MonoBehaviour
{
    /// <summary>
    /// Instancia global para acceso controlado.
    /// </summary>
    public static BrandingManager Instance { get; private set; }

    #region Inspector

    [Header("Branding - Imágenes")]

    [SerializeField]
    [Tooltip("Sprites disponibles para mostrar en elementos de branding.")]
    private List<Sprite> images = new();

    [Header("Branding - Textos")]

    [SerializeField]
    [Tooltip("Mensajes de branding que pueden mostrarse dinámicamente.")]
    private List<string> texts = new();

    [Header("Branding - Colores")]

    [SerializeField]
    [Tooltip("Paleta de colores utilizada para personalizar contenedores.")]
    private List<Color> colors = new();

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

    /// <summary>
    /// Obtiene un sprite aleatorio de branding.
    /// </summary>
    public Sprite GetRandomImage()
    {
        if (images.Count == 0)
            return null;

        return images[Random.Range(0, images.Count)];
    }

    /// <summary>
    /// Obtiene un texto aleatorio de branding.
    /// </summary>
    public string GetRandomText()
    {
        if (texts.Count == 0)
            return string.Empty;

        return texts[Random.Range(0, texts.Count)];
    }

    /// <summary>
    /// Obtiene un color aleatorio de branding.
    /// </summary>
    public Color GetRandomColor()
    {
        if (colors.Count == 0)
            return Color.white;

        return colors[Random.Range(0, colors.Count)];
    }

    #endregion
}