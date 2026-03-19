using System.Collections;
using UnityEngine;

/// <summary>
/// Orquestador del flujo de inicialización de la aplicación.
/// Se encarga de definir el orden y la ejecución de los pasos de carga
/// sin contener lógica específica de inicialización ni de interfaz de usuario.
/// </summary>
/// <remarks>
/// Este componente actúa como coordinador de alto nivel.
/// Cada paso de carga es desacoplado y reutilizable mediante la interfaz <see cref="ILoadingStep"/>.
/// </remarks>
public class InitManager : MonoBehaviour
{
    #region Serialized Fields - UI

    /// <summary>
    /// Controlador de la interfaz de usuario de carga.
    /// Se utiliza para mostrar el progreso y estado del proceso de inicialización.
    /// </summary>
    [Header("UI")]
    [SerializeField] private LoadingUIController loadingUI;

    #endregion

    #region Serialized Fields - Scenes

    /// <summary>
    /// Nombre de la escena principal que se cargará
    /// una vez finalizado el proceso de inicialización.
    /// </summary>
    [Header("Escenas")]
    [SerializeField] private string menuSceneName = "MainMenu";

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Método de Unity llamado al iniciar el objeto.
    /// Dispara el flujo completo de inicialización del juego.
    /// </summary>
    private void Start()
    {
        StartCoroutine(RunInitialization());
    }

    #endregion

    #region Initialization Flow

    /// <summary>
    /// Ejecuta el flujo completo de inicialización del juego
    /// utilizando una secuencia de pasos de carga desacoplados.
    /// </summary>
    /// <remarks>
    /// Cada paso implementa <see cref="ILoadingStep"/> y se ejecuta
    /// de forma secuencial, permitiendo escalabilidad y mantenimiento.
    /// </remarks>
    private IEnumerator RunInitialization()
    {
        ILoadingStep[] loadingSteps =
        {
            new UnityBootstrapStep(),
            new SceneLoadingStep(menuSceneName, 0.2f)
        };
    
        LoadingContext loadingContext =
            new LoadingContext(loadingUI, loadingSteps.Length);
    
        foreach (ILoadingStep step in loadingSteps)
        {
            yield return step.Execute(loadingContext);
        }
    }
    #endregion
}
