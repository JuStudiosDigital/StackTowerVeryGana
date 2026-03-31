using System.Collections;
using UnityEngine;

/// <summary>
/// Paso de carga encargado de precargar los assets definidos por el runtime.
/// </summary>
public class LevelResourcePreloadStep : ILoadingStep
{
    /// <summary>
    /// Loader persistente para reutilizar cache entre partidas.
    /// </summary>
    private static readonly AssetLoader assetLoader = new AssetLoader();

    public IEnumerator Execute(LoadingContext context)
    {
        DevLog.Log("[LevelResourcePreloadStep] Iniciando precarga de recursos.");
        var provider = GameDataProvider.Instance;

        if (provider == null || !provider.IsInitialized)
        {
            Debug.LogError("[LevelResourcePreloadStep] Runtime no inicializado");
            context.CompleteStep();
            yield break;
        }

        var runtime = provider.Runtime;

        /// Obtener requests dinámicos (CLAVE DEL SISTEMA)
        var requests = runtime.GetAssetRequests();

        /// Si no hay assets, completar inmediatamente
        if (requests == null || requests.Count == 0)
        {
            context.ReportStepProgress(1f);
            context.CompleteStep();
            yield break;
        }

        /// Cargar assets en paralelo
        yield return assetLoader.LoadAll(
            requests,
            provider, // MonoBehaviour para correr coroutines
            progress => context.ReportStepProgress(progress)
        );

        context.CompleteStep();
    }
}