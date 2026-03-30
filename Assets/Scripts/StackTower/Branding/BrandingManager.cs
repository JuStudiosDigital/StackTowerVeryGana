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

    /// <summary>
    /// Colección de sprites disponibles para branding visual.
    /// Utilizado por sistemas como BrandingDisplay.
    /// </summary>
    [SerializeField]
    private List<Sprite> images = new();

    [Header("Branding - Textos")]

    /// <summary>
    /// Colección de textos dinámicos para branding.
    /// Permite variaciones sin recompilar.
    /// </summary>
    [SerializeField]
    private List<string> texts = new();

    [Header("Branding - Colores")]

    /// <summary>
    /// Paleta de colores utilizada para personalizar contenedores.
    /// </summary>
    [SerializeField]
    private List<Color> colors = new();

    [Header("Branding - Gameplay")]

    /// <summary>
    /// Cantidad de contenedores necesarios para generar una moneda.
    /// 
    /// Nota de diseño:
    /// Este valor se externaliza para permitir ajustes rápidos
    /// (balanceo, campañas, A/B testing) sin modificar código.
    /// </summary>
    [SerializeField]
    private int containersPerCoin = 3;

    #endregion

    #region Unity

    /// <summary>
    /// Inicializa la instancia global.
    /// 
    /// Garantiza unicidad para evitar inconsistencias de configuración
    /// entre múltiples instancias en escena.
    /// </summary>
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

    /// <summary>
    /// Obtiene la cantidad de contenedores requeridos para generar una moneda.
    /// 
    /// Se asegura de devolver un valor válido (>= 1) para evitar bloqueos lógicos.
    /// </summary>
    public int GetContainersPerCoin()
    {
        return Mathf.Max(1, containersPerCoin);
    }

    #endregion
}