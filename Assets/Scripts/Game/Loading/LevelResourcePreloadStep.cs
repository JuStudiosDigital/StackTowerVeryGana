using System.Collections;

/// <summary>
/// Paso de carga encargado de precargar todos los recursos del nivel.
/// Garantiza que el gameplay no comience sin assets disponibles.
/// </summary>
public class LevelResourcePreloadStep : ILoadingStep
{
    /// <summary>
    /// Ejecuta la precarga de recursos del nivel y reporta
    /// el progreso local al contexto de carga.
    /// </summary>
    /// <param name="context">
    /// Contexto compartido del sistema de carga.
    /// </param>
    public IEnumerator Execute(LoadingContext context)
    {
        yield return ResourceService.Instance.PreloadLevelResources(
            progress => context.ReportStepProgress(progress)
        );

        context.CompleteStep();
    }
}
