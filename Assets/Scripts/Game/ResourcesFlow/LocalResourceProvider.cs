using UnityEngine;

/// <summary>
/// Proveedor estático de recursos locales incluidos en la build.
/// Centraliza la carga de assets desde la carpeta <c>Resources</c>
/// utilizando una convención de rutas basada en el tipo de recurso.
/// </summary>
/// <remarks>
/// Esta clase abstrae el uso directo de <see cref="Resources.Load{T}(string)"/>
/// para evitar rutas hardcodeadas dispersas en el código.
/// Está pensada para recursos pequeños, configuraciones base
/// y assets que deban estar disponibles sin dependencia de red
/// ni sistemas externos de distribución.
/// </remarks>
public static class LocalResourceProvider
{
    #region Constants

    /// <summary>
    /// Ruta base relativa a <c>Resources</c> para recursos gráficos
    /// como <see cref="Sprite"/> y <see cref="Texture2D"/>.
    /// </summary>
    private const string ImagePath = "LevelAssets/Images/";

    /// <summary>
    /// Ruta base relativa a <c>Resources</c> para recursos de audio
    /// como <see cref="AudioClip"/>.
    /// </summary>
    private const string AudioPath = "LevelAssets/Audio/";

    #endregion

    #region Public API

    /// <summary>
    /// Carga un recurso local desde la carpeta <c>Resources</c>
    /// a partir de una clave lógica y el tipo solicitado.
    /// </summary>
    /// <typeparam name="T">
    /// Tipo del recurso a cargar. Debe heredar de <see cref="UnityEngine.Object"/>.
    /// </typeparam>
    /// <param name="key">
    /// Clave lógica del recurso, sin incluir ruta base ni extensión.
    /// </param>
    /// <returns>
    /// Instancia del recurso solicitado si existe;
    /// de lo contrario, <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Este método delega la resolución de la ruta a
    /// <see cref="ResolvePath{T}(string)"/> para mantener
    /// una única fuente de verdad sobre la estructura de carpetas
    /// y evitar duplicación de rutas en el código.
    /// </remarks>
    public static T Load<T>(string key) where T : Object
    {
        string fullPath = ResolvePath<T>(key);
        T asset = Resources.Load<T>(fullPath);

        if (asset == null)
        {
            DevLog.Error(
                $"[LocalResourceProvider] Recurso local no encontrado: {fullPath}"
            );
        }

        return asset;
    }

    #endregion

    #region Path Resolution

    /// <summary>
    /// Resuelve la ruta interna dentro de la carpeta <c>Resources</c>
    /// en función del tipo del recurso solicitado.
    /// </summary>
    /// <typeparam name="T">
    /// Tipo del recurso que se desea cargar.
    /// </typeparam>
    /// <param name="key">
    /// Clave lógica del recurso.
    /// </param>
    /// <returns>
    /// Ruta completa relativa a la carpeta <c>Resources</c>.
    /// </returns>
    /// <remarks>
    /// La resolución se basa explícitamente en tipos conocidos
    /// para evitar el uso de strings mágicos o rutas duplicadas.
    /// Si el tipo no está contemplado, se retorna la clave tal cual,
    /// permitiendo flexibilidad para casos especiales o extensiones futuras.
    /// </remarks>
    private static string ResolvePath<T>(string key)
    {
        if (typeof(T) == typeof(Sprite) || typeof(T) == typeof(Texture2D))
        {
            return ImagePath + key;
        }

        if (typeof(T) == typeof(AudioClip))
        {
            return AudioPath + key;
        }

        return key;
    }

    #endregion
}
