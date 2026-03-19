using System.Collections;

/// <summary>
/// Paso de carga encargado de inicializar sistemas base de Unity
/// y servicios globales requeridos antes de cargar escenas.
/// </summary>
public class UnityBootstrapStep : ILoadingStep
{
    /// <summary>
    /// Ejecuta la inicialización de sistemas globales del juego.
    /// Reporta progreso local al contexto de carga.
    /// </summary>
    /// <param name="context">
    /// Contexto compartido del sistema de carga.
    /// </param>
    public IEnumerator Execute(LoadingContext context)
    {
        context.ReportStepProgress(0f);

        yield return InitializeGlobalSystems(context);

        context.CompleteStep();
    }

    /// <summary>
    /// Inicializa los sistemas globales requeridos por la aplicación,
    /// tales como SDKs, servicios externos o configuraciones base.
    /// </summary>
    /// <param name="context">
    /// Contexto de carga utilizado para reportar progreso local.
    /// </param>
    private IEnumerator InitializeGlobalSystems(LoadingContext context)
    {
        /// Aquí irían inicializaciones reales:
        /// - SDKs
        /// - Analytics
        /// - Servicios remotos
        /// - Configuración inicial

        yield return null;

        context.ReportStepProgress(1f);
    }
}
