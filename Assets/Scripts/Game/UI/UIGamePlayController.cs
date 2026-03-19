using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador principal de la interfaz de usuario durante el gameplay.
/// Centraliza la gestión de popups, botones de interacción,
/// animaciones de transición y navegación fuera de la escena de juego.
/// </summary>
public class UIGamePlayController : MonoBehaviour
{
    #region Serialized References - UI & Managers

    /// <summary>
    /// Gestor centralizado de popups del juego.
    /// Se encarga de abrir, cerrar y bloquear ventanas emergentes.
    /// </summary>
    [SerializeField] public PopupManager popupManager;

    /// <summary>
    /// Popup informativo mostrado durante el gameplay.
    /// </summary>
    [SerializeField] private InfoPopup infoPopUp;

    
    /// <summary>
    /// Popup de alerta mostrado durante el gameplay.
    /// </summary>
    [SerializeField] private AlertPopup alertPopUp;

    /// <summary>
    /// Popup de felicitaciones mostrado al completar el puzzle.
    /// </summary>
    [SerializeField] private CongratsPopup congratsPopup;

    /// <summary>
    /// Gestor de animaciones específicas de la UI durante el gameplay.
    /// Controla transiciones de entrada, salida y estados de carga.
    /// </summary>
    [SerializeField] private UIAnimationManagerGameplay uIAnimationManagerGameplay;

    /// <summary>
    /// Controlador visual de la interfaz de carga.
    /// </summary>
    [SerializeField] private LoadingUIController loadingUI;

    /// <summary>
    /// Nombre de la escena que se carga al salir del gameplay.
    /// </summary>
    [SerializeField] private string sceneName = "MainMenu";

    /// <summary>
    /// Gestor principal de la lógica del gameplay.
    /// Permite comunicar acciones de UI con el estado del juego.
    /// </summary>
    [SerializeField] private GamePlayManager gamePlayManager;

    /// <summary>
    /// Sprite utilizado cuando la imagen guía está visible.
    /// </summary>
    [SerializeField] private Sprite isGuideImageVisibleSprite;

    /// <summary>
    /// Sprite utilizado cuando la imagen guía está oculta.
    /// </summary>
    [SerializeField] private Sprite isGuideImageHiddenSprite;

    /// <summary>
    /// Imagen del botón que controla la visibilidad de la imagen guía.
    /// </summary>
    [SerializeField] private Image guideImageButton;

    /// <summary>
    /// Imagen del botón que controla la visibilidad de la imagen guía.
    /// </summary>
    [SerializeField] private GameObject coinsCounterContainer;

    #endregion

    #region Branding configuration

    /// <summary>
    /// URL del JSON de configuración del nivel.
    /// </summary>
    [SerializeField] private string levelConfigUrl;

    /// <summary>
    /// Nombre de la escena de gameplay actual.
    /// </summary>
    [SerializeField] private string gameplaySceneName = "GameScene";

    #endregion

    #region Unity Lifecycle
    void Start()
    {
        if (!GameManager.Instance.IsAdsEnabled)
        {
            OpenAlert();
            coinsCounterContainer.SetActive(false);
        }
    }
    /// <summary>
    /// Se suscribe a los eventos del gameplay y de los popups
    /// cuando el objeto es habilitado.
    /// </summary>
    private void OnEnable()
    {
        congratsPopup.OnConfirmRequested += OnCongratsConfirmed;

        gamePlayManager.GameplayCompleted += HandleGameplayCompleted;
        gamePlayManager.GameplayEnterRequested += OnGameplayEnterRequested;
        gamePlayManager.GameplayExitRequested += OnGameplayExitRequested;
    }

    /// <summary>
    /// Cancela la suscripción a eventos para evitar referencias inválidas
    /// cuando el objeto es deshabilitado.
    /// </summary>
    private void OnDisable()
    {
        congratsPopup.OnConfirmRequested -= OnCongratsConfirmed;

        gamePlayManager.GameplayCompleted -= HandleGameplayCompleted;
        gamePlayManager.GameplayEnterRequested -= OnGameplayEnterRequested;
        gamePlayManager.GameplayExitRequested -= OnGameplayExitRequested;
    }

    #endregion

    #region Popup Controls

    /// <summary>
    /// Abre el popup informativo del gameplay.
    /// </summary>
    public void OpenInfo()
    {
        popupManager.OpenPopup(infoPopUp);
    }

     /// <summary>
    /// Abre el popup informativo del gameplay.
    /// </summary>
    public void OpenAlert()
    {
        popupManager.OpenPopup(alertPopUp);
    }

    /// <summary>
    /// Abre el popup de felicitaciones.
    /// </summary>
    public void OpenCongrats()
    {
        popupManager.OpenPopup(congratsPopup);
    }

    #endregion

    #region Guide Image Controls

    /// <summary>
    /// Alterna la visibilidad de la imagen guía y actualiza
    /// el sprite del botón según el estado actual.
    /// </summary>
    public void ShowHideGuideImage()
    {
        gamePlayManager.ToggleGuideImage();

        bool isVisible = gamePlayManager.IsGuideImageVisible;

        guideImageButton.sprite = isVisible
            ? isGuideImageVisibleSprite
            : isGuideImageHiddenSprite;
    }

    #endregion

    #region Gameplay Controls

    /// <summary>
    /// Reinicia la partida actual desde el estado inicial.
    /// </summary>
    public void Replay()
    {
        gamePlayManager.RestartGame();
    }

    /// <summary>
    /// Inicia el flujo de salida del gameplay hacia el menú principal.
    /// </summary>
    public void BackToMainMenu()
    {
        gamePlayManager.ExitGameplay(OnGameplayExitCompleted);
    }

    #endregion

    #region Exit Flow & Loading

    /// <summary>
    /// Callback ejecutado cuando finaliza la animación de salida del gameplay.
    /// Inicia la interfaz de carga y el cambio de escena.
    /// </summary>
    private void OnGameplayExitCompleted()
    {
        uIAnimationManagerGameplay.ShowLoading();
        StartCoroutine(RunBackToMenuLoading());
    }

    /// <summary>
    /// Ejecuta la secuencia de carga para regresar al menú principal
    /// utilizando pasos de carga desacoplados.
    /// </summary>
    private IEnumerator RunBackToMenuLoading()
    {
        ILoadingStep[] loadingSteps =
        {
            new SceneLoadingStep(sceneName, 0f)
        };
    
        LoadingContext loadingContext =
            new LoadingContext(loadingUI, loadingSteps.Length);
    
        foreach (ILoadingStep step in loadingSteps)
        {
            yield return step.Execute(loadingContext);
        }
    }

    /// <summary>
    /// Ejecuta el flujo completo de recarga del gameplay:
    /// - Descarga nuevamente la configuración remota
    /// - Precarga los recursos del nivel
    /// - Recarga la escena actual
    /// </summary>
    private IEnumerator RunReloadGameplayFlow()
    {
        ILoadingStep[] loadingSteps =
        {
            new LevelRemoteConfigStep(levelConfigUrl),
            new LevelResourcePreloadStep(),
            new SceneLoadingStep(gameplaySceneName, 0f)
        };

        LoadingContext loadingContext =
            new LoadingContext(loadingUI, loadingSteps.Length);

        foreach (ILoadingStep step in loadingSteps)
        {
            yield return step.Execute(loadingContext);
        }
    }

    /// <summary>
    /// Maneja la confirmación del popup de felicitaciones.
    /// Reinicia el flujo de carga solicitando nueva data
    /// y recargando la escena de gameplay.
    /// </summary>
    private void OnCongratsConfirmed()
    {
        DevLog.Log("UIGamePlayController: Congrats confirmed, reloading gameplay.");
        
        uIAnimationManagerGameplay.ShowLoading();
        StartCoroutine(RunReloadGameplayFlow());
    }

    #endregion

    #region Gameplay Completion & Congrats Flow

    /// <summary>
    /// Configura los datos mostrados en el popup de felicitaciones.
    /// </summary>
    public void SetupCongratsPopup(
        string message,
        string statOne,
        string statTwo,
        string statThree)
    {
        congratsPopup.Setup(message, statOne, statTwo, statThree);
    }

    /// <summary>
    /// Ejecuta el flujo de apertura del popup de felicitaciones,
    /// bloqueando la interacción de fondo.
    /// </summary>
    public void ShowCongratsFlow()
    {
        popupManager.LockOverlay();
        OpenCongrats();
    }

    /// <summary>
    /// Maneja el flujo completo cuando el puzzle es completado.
    /// Prepara la información, oculta el gameplay y muestra el popup.
    /// </summary>
    public void HandleGameplayCompleted(GameResultData result)
    {
        string victoryMessage = ResourceService.Instance.GetText(
            texts => texts.victory_phrase
        );

        SetupCongratsPopup(
            victoryMessage,
            result.time,
            result.totalMoves.ToString(),
            result.coinsCollected.ToString()
        );

        popupManager.LockOverlay();

        gamePlayManager.ExitGameplay(() =>
        {
            OpenCongrats();
        });
    }

    #endregion

    #region Gameplay UI Animations

    /// <summary>
    /// Maneja la solicitud de entrada visual al gameplay.
    /// </summary>
    private void OnGameplayEnterRequested(float duration)
    {
        uIAnimationManagerGameplay.PlayEntryAnimation(duration);
    }

    /// <summary>
    /// Maneja la solicitud de salida visual del gameplay.
    /// </summary>
    private void OnGameplayExitRequested(float duration)
    {
        uIAnimationManagerGameplay.PlayExitAnimation(exitDuration: duration);
    }

    #endregion
}
