using System.Collections;

/// <summary>
/// Define un paso de carga independiente y desacoplado.
/// </summary>
public interface ILoadingStep
{
    /// <summary>
    /// Ejecuta el paso de carga.
    /// </summary>
    IEnumerator Execute(LoadingContext context);
}
