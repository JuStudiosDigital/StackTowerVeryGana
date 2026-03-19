using System;
using System.Collections;

/// <summary>
/// Implementación que obtiene la configuración desde una URL directa.
/// Mantiene compatibilidad con el sistema actual.
/// </summary>
public class DirectUrlLevelConfigProvider : ILevelConfigProvider
{
    private readonly string remoteUrl;
    private readonly RemoteResourceDownloader downloader;

    /// <summary>
    /// Inicializa el proveedor con la URL remota.
    /// </summary>
    public DirectUrlLevelConfigProvider(string remoteUrl)
    {
        this.remoteUrl = remoteUrl;
        downloader = new RemoteResourceDownloader();
    }

    /// <summary>
    /// Obtiene la configuración del nivel usando descarga GET tradicional.
    /// </summary>
    public IEnumerator GetLevelConfigAsync(
        Action<LevelConfigData> onSuccess,
        Action onFailure)
    {
        yield return downloader.DownloadJson<LevelConfigData>(
            remoteUrl,
            data =>
            {
                LevelConfigData normalized =
                    LevelConfigNormalizer.Normalize(data);

                onSuccess?.Invoke(normalized);
            },
            onFailure
        );
    }
}
