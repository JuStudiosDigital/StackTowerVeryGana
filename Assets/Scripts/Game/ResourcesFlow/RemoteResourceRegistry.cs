using System.Collections.Generic;

/// <summary>
/// Registro interno de recursos remotos disponibles para el nivel.
/// 
/// Esta clase actúa como una tabla de resolución entre una clave lógica
/// del juego (por ejemplo: "PuzzleImage", "VictorySound") y la URL remota
/// real desde la cual se debe descargar el recurso.
/// 
/// No descarga ni cachea recursos. Su única responsabilidad es
/// mantener la relación clave → URL para el nivel activo.
/// </summary>
public class RemoteResourceRegistry
{
    #region Fields

    /// <summary>
    /// Diccionario interno que asocia claves lógicas del juego
    /// con URLs remotas válidas.
    /// </summary>
    private readonly Dictionary<string, string> registry =
        new Dictionary<string, string>();

    #endregion

    #region Public API

    /// <summary>
    /// Limpia completamente el registro de recursos remotos.
    /// 
    /// Debe ejecutarse antes de registrar una nueva configuración
    /// de nivel para evitar contaminación de datos entre niveles.
    /// </summary>
    public void Clear()
    {
        registry.Clear();
    }

    /// <summary>
    /// Registra una URL remota asociada a una clave lógica del juego.
    /// 
    /// Si la URL es nula o vacía, el registro se omite silenciosamente,
    /// permitiendo que el sistema de fallback local resuelva el recurso.
    /// </summary>
    /// <param name="key">
    /// Clave lógica del recurso (ej. "PuzzleImage", "VictorySound").
    /// </param>
    /// <param name="url">
    /// URL remota desde la cual se descargará el recurso.
    /// </param>
    public void Register(string key, string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            registry[key] = url;
        }
    }

    /// <summary>
    /// Obtiene la URL remota asociada a una clave lógica.
    /// </summary>
    /// <param name="key">
    /// Clave lógica del recurso.
    /// </param>
    /// <returns>
    /// La URL remota registrada o <c>null</c> si la clave no existe.
    /// </returns>
    public string GetUrl(string key)
    {
        return registry.ContainsKey(key) ? registry[key] : null;
    }

    /// <summary>
    /// Obtiene todas las claves lógicas registradas actualmente.
    /// 
    /// Este método se utiliza normalmente para iterar
    /// sobre los recursos que deben ser precargados.
    /// </summary>
    /// <returns>
    /// Lista de claves lógicas registradas.
    /// </returns>
    public List<string> GetAllKeys()
    {
        return new List<string>(registry.Keys);
    }

    #endregion
}
