using System.Collections.Generic;

/// <summary>
/// Proporciona un almacenamiento en memoria para respuestas JSON,
/// evitando solicitudes remotas repetidas durante la sesión de ejecución.
/// </summary>
public class SimpleCache
{
    private readonly Dictionary<string, string> jsonCache =
        new Dictionary<string, string>();

    /// <summary>
    /// Intenta recuperar un JSON previamente almacenado asociado a la clave especificada.
    /// </summary>
    /// <param name="key">Identificador único del recurso (usualmente la URL).</param>
    /// <param name="json">Contenido JSON recuperado si existe.</param>
    /// <returns>True si el valor existe en cache; de lo contrario, false.</returns>
    public bool TryGet(string key, out string json)
    {
        return jsonCache.TryGetValue(key, out json);
    }

    /// <summary>
    /// Almacena un JSON en cache para futuras consultas dentro de la misma sesión.
    /// Ignora valores nulos o vacíos para evitar estados inválidos.
    /// </summary>
    /// <param name="key">Identificador único del recurso (usualmente la URL).</param>
    /// <param name="json">Contenido JSON a almacenar.</param>
    public void Store(string key, string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;
        jsonCache[key] = json;
    }
}