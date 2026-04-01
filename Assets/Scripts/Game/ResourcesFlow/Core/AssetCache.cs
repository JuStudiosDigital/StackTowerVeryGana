using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cache de assets en memoria basado en URL.
/// </summary>
public class AssetCache
{
    private readonly Dictionary<string, Object> cache =
        new Dictionary<string, Object>();

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

    public void Store(string url, Object asset)
    {
        if (string.IsNullOrWhiteSpace(url) || asset == null)
            return;

        cache[url] = asset;
    }
}