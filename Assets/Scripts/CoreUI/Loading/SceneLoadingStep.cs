using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Paso del sistema de carga encargado de realizar la carga asíncrona de una escena.
/// Combina el progreso real de Unity con un progreso simulado para garantizar
/// una duración mínima perceptible del proceso de carga.
/// </summary>
public class SceneLoadingStep : ILoadingStep
{
    #region Fields

    private readonly string sceneName;
    private readonly float fakeDuration;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa un nuevo paso de carga de escena.
    /// </summary>
    /// <param name="sceneName">
    /// Nombre exacto de la escena que se desea cargar.
    /// </param>
    /// <param name="fakeDuration">
    /// Duración mínima simulada del proceso de carga.
    /// </param>
    public SceneLoadingStep(string sceneName, float fakeDuration = 3f)
    {
        this.sceneName = sceneName;
        this.fakeDuration = Mathf.Max(0.01f, fakeDuration);
    }

    #endregion

    #region ILoadingStep Implementation

    /// <summary>
    /// Ejecuta el proceso de carga de la escena de forma asíncrona
    /// y reporta el progreso local al contexto de carga.
    /// </summary>
    /// <param name="context">
    /// Contexto de carga compartido utilizado para reportar el progreso.
    /// </param>
    public IEnumerator Execute(LoadingContext context)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        float elapsedTime = 0f;

        while (elapsedTime < fakeDuration || operation.progress < 0.9f)
        {
            elapsedTime += Time.deltaTime;

            float fakeProgress = Mathf.Clamp01(elapsedTime / fakeDuration);
            float realProgress = Mathf.Clamp01(operation.progress / 0.9f);

            float localProgress = Mathf.Min(fakeProgress, realProgress);

            context.ReportStepProgress(localProgress);

            yield return null;
        }

        context.ReportStepProgress(1f);

        yield return new WaitForSeconds(0.2f);

        operation.allowSceneActivation = true;

        context.CompleteStep();
    }

    #endregion
}
