using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cache en memoria de recursos utilizados por el juego.
/// 
/// Este componente actúa como almacenamiento temporal para recursos
/// descargados remotamente o cargados desde assets locales.
/// 
/// La identidad de cada recurso se define por una clave string,
/// que puede representar:
/// - Una URL remota (contenido dinámico)
/// - Una clave local normalizada (ej: "local::PuzzleImage")
/// 
/// No realiza lógica de fallback ni validaciones de negocio.
/// Su única responsabilidad es almacenar y entregar referencias
/// de recursos ya cargados en memoria.
/// </summary>
public class ResourceCache
{
    #region Fields

    /// <summary>
    /// Diccionario interno que almacena los recursos en memoria
    /// indexados por su clave de identidad.
    /// </summary>
    private readonly Dictionary<string, Object> cacheByKey =
        new Dictionary<string, Object>();

    #endregion

    #region Public API

    /// <summary>
    /// Verifica si existe un recurso cacheado exactamente
    /// bajo la clave proporcionada.
    /// 
    /// No evalúa el tipo del recurso, únicamente su existencia.
    /// </summary>
    /// <param name="key">
    /// Clave única del recurso (URL remota o clave local normalizada).
    /// </param>
    /// <returns>
    /// <c>true</c> si el recurso existe en cache;
    /// <c>false</c> en caso contrario.
    /// </returns>
    public bool Has(string key)
    {
        return cacheByKey.ContainsKey(key);
    }

    /// <summary>
    /// Almacena un recurso en el cache usando su clave de identidad.
    /// 
    /// Si la clave ya existe, el recurso anterior es sobrescrito.
    /// Los recursos nulos son ignorados de forma segura.
    /// </summary>
    /// <param name="key">
    /// Clave única del recurso (URL remota o clave local normalizada).
    /// </param>
    /// <param name="asset">
    /// Recurso ya cargado en memoria.
    /// </param>
    public void Store(string key, Object asset)
    {
        if (asset == null)
        {
            return;
        }

        cacheByKey[key] = asset;
    }

    /// <summary>
    /// Obtiene un recurso cacheado a partir de su clave
    /// y lo retorna tipado.
    /// 
    /// Si la clave no existe o el tipo no coincide,
    /// el método retorna <c>null</c>.
    /// </summary>
    /// <typeparam name="T">
    /// Tipo esperado del recurso (Texture2D, AudioClip, etc.).
    /// </typeparam>
    /// <param name="key">
    /// Clave única del recurso.
    /// </param>
    /// <returns>
    /// Recurso cacheado del tipo solicitado,
    /// o <c>null</c> si no existe o no coincide el tipo.
    /// </returns>
    public T Get<T>(string key) where T : Object
    {
        return cacheByKey.TryGetValue(key, out Object asset)
            ? asset as T
            : null;
    }

    /// <summary>
    /// Limpia completamente el cache de recursos.
    /// 
    /// Este método elimina todas las referencias almacenadas,
    /// permitiendo que Unity libere la memoria asociada
    /// cuando sea apropiado.
    /// 
    /// Debe utilizarse al cambiar de nivel o
    /// al forzar un fallback completo.
    /// </summary>
    public void Clear()
    {
        cacheByKey.Clear();
    }

    #endregion
}
