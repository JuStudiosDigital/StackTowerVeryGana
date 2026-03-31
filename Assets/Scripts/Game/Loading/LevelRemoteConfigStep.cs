using System.Collections;
using UnityEngine;

/// <summary>
/// Paso de carga encargado de obtener la configuración remota del nivel.
/// Si falla, el juego continúa con fallback local.
/// </summary>
public class LevelRemoteConfigStep : ILoadingStep
{
    #region Fields

    private readonly string configUrl;
    private readonly string postBody;

    #endregion

    #region Constructor

    public LevelRemoteConfigStep(string configUrl, string postBody = null)
    {
        this.configUrl = configUrl;
        this.postBody = postBody;
    }

    #endregion

    #region Execute

    public IEnumerator Execute(LoadingContext context)
    {
        DevLog.Log($"[LevelRemoteConfigStep] Iniciando carga remota desde: {configUrl}");
        context.ReportStepProgress(0f);

        var provider = GameDataProvider.Instance;

        if (provider == null)
        {
            Debug.LogError("[LevelRemoteConfigStep] GameDataProvider no encontrado");
            context.CompleteStep();
            yield break;
        }

        var loader = new GameDataLoader();

        yield return loader.Load(provider, configUrl, postBody);

        context.ReportStepProgress(1f);
        context.CompleteStep();
    }

    #endregion
}