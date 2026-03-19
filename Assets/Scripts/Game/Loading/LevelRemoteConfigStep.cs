using System.Collections;
using UnityEngine;

/// <summary>
/// Paso de carga encargado de obtener la configuración remota del nivel.
/// Si la descarga falla, el sistema continúa utilizando configuración local.
/// </summary>
public class LevelRemoteConfigStep : ILoadingStep
{
    #region Fields

    private readonly string configUrl;

    #endregion

    #region Constructors

    /// <summary>
    /// Inicializa el paso con la URL del JSON remoto del nivel.
    /// </summary>
    /// <param name="configUrl">
    /// URL desde la cual se intentará descargar la configuración del nivel.
    /// </param>
    public LevelRemoteConfigStep(string configUrl)
    {
        this.configUrl = configUrl;
    }

    #endregion

    #region ILoadingStep Implementation

    /// <summary>
    /// Ejecuta la descarga del JSON remoto del nivel y reporta
    /// el progreso local de forma normalizada.
    /// </summary>
    /// <param name="context">
    /// Contexto compartido del sistema de carga.
    /// </param>
    public IEnumerator Execute(LoadingContext context)
    {
        context.ReportStepProgress(0f);

        ILevelConfigProvider provider;

        #if UNITY_EDITOR
        /// En editor, se puede usar un proveedor directo de URL para facilitar pruebas.
        provider = new DirectUrlLevelConfigProvider(configUrl);
        #else
        /// En builds, se recomienda usar un servicio cloud que envíe un payload base
        /// con datos de sesión y reciba la configuración final, permitiendo personalización
        provider = new CloudServiceLevelConfigProvider(configUrl);
        #endif
        
        IEnumerator loadRoutine =
            ResourceService.Instance.LoadLevelConfigAsync(provider);

        while (loadRoutine.MoveNext())
        {
            context.ReportStepProgress(0.5f);
            yield return loadRoutine.Current;
        }

        context.ReportStepProgress(1f);
        context.CompleteStep();
    }

    #endregion
}
