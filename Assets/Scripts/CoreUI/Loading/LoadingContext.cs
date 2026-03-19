using UnityEngine;

/// <summary>
/// Contexto central del sistema de carga.
/// Traduce el progreso local de cada paso en progreso global
/// y lo comunica a la capa de presentación.
/// </summary>
public class LoadingContext
{
    #region Fields

    private readonly LoadingUIController loadingUI;
    private readonly int totalSteps;

    private int currentStepIndex;
    private float currentProgress;

    #endregion

    #region Constructors

    /// <summary>
    /// Inicializa el contexto de carga con el número total de pasos.
    /// </summary>
    /// <param name="loadingUI">
    /// Controlador responsable de mostrar el progreso visual.
    /// </param>
    /// <param name="totalSteps">
    /// Cantidad total de pasos del flujo de carga.
    /// </param>
    public LoadingContext(LoadingUIController loadingUI, int totalSteps)
    {
        this.loadingUI = loadingUI;
        this.totalSteps = Mathf.Max(1, totalSteps);

        currentStepIndex = 0;
        currentProgress = 0f;

        loadingUI.ResetProgress();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Reporta el progreso local del paso actual (0–1).
    /// El contexto lo convierte automáticamente en progreso global.
    /// </summary>
    /// <param name="localProgress">
    /// Progreso normalizado del paso activo.
    /// </param>
    public void ReportStepProgress(float localProgress)
    {
        localProgress = Mathf.Clamp01(localProgress);

        float stepWeight = 1f / totalSteps;
        float globalProgress =
            (currentStepIndex * stepWeight) +
            (localProgress * stepWeight);

        SetProgress(globalProgress);
    }

    /// <summary>
    /// Notifica al contexto que el paso actual ha finalizado.
    /// </summary>
    public void CompleteStep()
    {
        currentStepIndex = Mathf.Clamp(currentStepIndex + 1, 0, totalSteps);
        SetProgress((float)currentStepIndex / totalSteps);
    }

    #endregion

    #region Private Methods

    private void SetProgress(float progress)
    {
        currentProgress = Mathf.Clamp01(progress);
        loadingUI.SetProgress(currentProgress);
    }

    #endregion
}
