using System.Collections.Generic;

/// <summary>
/// Cache simple en memoria para evitar descargas repetidas.
/// </summary>
public class SimpleCache
{
    private readonly Dictionary<string, string> jsonCache =
        new Dictionary<string, string>();

    public bool TryGet(string key, out string json)
    {
        return jsonCache.TryGetValue(key, out json);
    }

    public void Store(string key, string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        jsonCache[key] = json;
    }
}