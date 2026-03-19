using System.Collections;
using UnityEngine;

/// <summary>
/// Paso de carga que introduce un retardo controlado en el flujo de carga.
/// 
/// Este paso es útil para:
/// - Inicialización de SDKs
/// - Esperas silenciosas
/// - Sincronización visual del loading
/// 
/// El progreso reportado es local (0–1) y el contexto se encarga
/// de traducirlo a progreso global.
/// </summary>
public class FakeDelayStep : ILoadingStep
{
    #region Fields

    /// <summary>
    /// Duración total del retardo en segundos.
    /// </summary>
    private readonly float duration;

    #endregion

    #region Constructors

    /// <summary>
    /// Crea una nueva instancia del paso de retardo.
    /// </summary>
    /// <param name="duration">
    /// Tiempo total de espera en segundos.
    /// </param>
    public FakeDelayStep(float duration)
    {
        this.duration = Mathf.Max(0.01f, duration);
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Ejecuta el retardo y reporta el progreso local al contexto de carga.
    /// </summary>
    /// <param name="context">
    /// Contexto de carga encargado de convertir el progreso local
    /// en progreso global.
    /// </param>
    public IEnumerator Execute(LoadingContext context)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float localProgress = elapsed / duration;
            context.ReportStepProgress(localProgress);

            yield return null;
        }

        context.CompleteStep();
    }

    #endregion
}
