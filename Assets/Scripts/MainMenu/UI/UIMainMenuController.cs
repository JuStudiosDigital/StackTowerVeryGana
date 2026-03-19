using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador principal del menú inicial del juego.
/// Gestiona la apertura de popups, la selección de nivel
/// y la transición hacia la escena de gameplay con pantalla de carga.
/// </summary>
public class UIMainMenuController : MonoBehaviour
{
    #region Serialized Fields

    /// <summary>
    /// Gestor centralizado de popups del menú principal.
    /// Se encarga de abrir y cerrar ventanas emergentes.
    /// </summary>
    [SerializeField] private PopupManager popupManager;

    /// <summary>
    /// Popup informativo que se muestra al solicitar información desde el menú.
    /// </summary>
    [SerializeField] private PopupBase infoPopUp;

    /// <summary>
    /// Gestor de animaciones del menú principal.
    /// Controla las transiciones visuales al interactuar con los botones.
    /// </summary>
    [SerializeField] private UIAnimationManagerMainMenu uIAnimationManagerMainMenu;

    /// <summary>
    /// Controlador de la interfaz de carga que se muestra
    /// durante la transición a la escena de juego.
    /// </summary>
    [SerializeField] private LoadingUIController loadingUI;

    /// <summary>
    /// Nombre de la escena de gameplay que se cargará al iniciar el juego.
    /// </summary>
    [SerializeField] private string sceneName = "Game";

    #endregion

    /// <summary>>
    /// URL del archivo de configuración remota del nivel.
    [SerializeField] private string levelConfigUrl;

    #region Public Methods

    /// <summary>
    /// Abre el popup de información desde el menú principal.
    /// </summary>
    public void OpenInfo()
    {
        popupManager.OpenPopup(infoPopUp);
    }

    /// <summary>
    /// Inicia el flujo de entrada al gameplay.
    /// Asigna el identificador de nivel seleccionado,
    /// ejecuta la animación del menú y comienza la carga de la escena.
    /// </summary>
    /// <param name="IDButton">
    /// Identificador del botón presionado, utilizado como ID de nivel.
    /// </param>
    public void OpenGamePlay(int IDButton)
    {
        GameManager.Instance.LevelID = IDButton;
        uIAnimationManagerMainMenu.OnMainButtonPressed();
        StartCoroutine(RunOpenSceneLoading());
    }

    #endregion

    #region Private Coroutines

   /// <summary>
    /// Ejecuta el flujo de carga de la escena de gameplay utilizando
    /// una secuencia de pasos de carga desacoplados.
    /// </summary>
    private IEnumerator RunOpenSceneLoading()
    {
        ILoadingStep[] loadingSteps =
        {
            new LevelRemoteConfigStep(levelConfigUrl),
            new LevelResourcePreloadStep(),
            new SceneLoadingStep(sceneName, 0f)
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
