using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Proporciona un cache en memoria para assets descargados en tiempo de ejecución,
/// indexados por URL. Evita descargas redundantes y reduce latencia en cargas repetidas.
/// </summary>
public class AssetCache
{
    private readonly Dictionary<string, Object> cache =
        new Dictionary<string, Object>();

    /// <summary>
    /// Intenta obtener un asset del cache tipado por su URL.
    /// </summary>
    /// <typeparam name="T">Tipo esperado del asset.</typeparam>
    /// <param name="url">Identificador único del recurso remoto.</param>
    /// <param name="asset">Asset recuperado si existe y coincide el tipo.</param>
    /// <returns>
    /// True si el asset existe en cache y es del tipo solicitado; de lo contrario, false.
    /// </returns>
    public bool TryGet<T>(string url, out T asset) where T : Object
    {
        if (cache.TryGetValue(url, out Object obj))
        {
            asset = obj as T;
            return asset != null;
        }

        asset = null;
        return false;
    }

    /// <summary>
    /// Almacena un asset en cache utilizando su URL como clave.
    /// </summary>
    /// <param name="url">Identificador único del recurso remoto.</param>
    /// <param name="asset">Instancia del asset a almacenar.</param>
    public void Store(string url, Object asset)
    {
        if (string.IsNullOrWhiteSpace(url) || asset == null)
            return;

        cache[url] = asset;
    }
}