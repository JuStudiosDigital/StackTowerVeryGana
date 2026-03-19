using System;
using System.Collections;

/// <summary>
/// Define el contrato para cualquier proveedor de configuración de nivel.
/// Permite desacoplar la fuente del JSON (URL directa o servicio en la nube).
/// </summary>
public interface ILevelConfigProvider
{
    /// <summary>
    /// Solicita la configuración del nivel de forma asincrónica.
    /// </summary>
    /// <param name="onSuccess">Callback ejecutado con la configuración recibida.</param>
    /// <param name="onFailure">Callback ejecutado ante error.</param>
    IEnumerator GetLevelConfigAsync(
        Action<LevelConfigData> onSuccess,
        Action onFailure);
}
