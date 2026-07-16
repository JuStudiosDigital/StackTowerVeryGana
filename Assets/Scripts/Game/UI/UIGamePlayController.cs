using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    /// Popup de promociones mostrado después del popup de felicitaciones.
    /// </summary>
    [SerializeField] private RewardPopup rewardPopup;

    /// <summary>
    /// Popup de alerta mostrado durante el gameplay.
    /// </summary>
    [SerializeField] private AlertPopup alertPopUp;

    /// <summary>
    /// Popup de felicitaciones mostrado al completar el puzzle.
    /// </summary>
    [SerializeField] private CongratsPopup gmeOverPopup;

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
    /// HUD de contador de monedas.
    /// </summary>
    [SerializeField] private GameObject coinsCounterContainer;


    #endregion

    #region Branding configuration


    /// <summary>
    /// Nombre de la escena de gameplay actual.
    /// </summary>
    [SerializeField] private string gameplaySceneName = "GameScene";

    #endregion
    /// <summary>
    /// Indica que el flujo de victoria está esperando el cierre del RewardPopup
    /// para iniciar la recarga del gameplay.
    /// </summary>
    private bool isWaitingRewardPopupBeforeReload;
    /// <summary>
    /// Evita enviar GAME_FINISHED más de una vez durante
    /// el mismo flujo de finalización.
    /// </summary>
    private bool nextLevelMessageSent;

    /// <summary>
    /// Evita iniciar más de una vez la recarga del siguiente nivel.
    /// </summary>
    private bool nextLevelReloadStarted;
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
        gmeOverPopup.OnConfirmRequested += OnCongratsConfirmed;

        if (popupManager != null)
        {
            popupManager.PopupClosed += HandlePopupClosed;
        }
        gamePlayManager.GameplayEnded += OnGameplayEnded;
        gamePlayManager.GameplayCompleted += HandleGameplayCompleted;  
        gamePlayManager.GameplayEnterRequested += OnGameplayEnterRequested;
    }

    /// <summary>
    /// Cancela la suscripción a eventos para evitar referencias inválidas
    /// cuando el objeto es deshabilitado.
    /// </summary>
    private void OnDisable()
    {
        gmeOverPopup.OnConfirmRequested -= OnCongratsConfirmed;

        if (popupManager != null)
        {
            popupManager.PopupClosed -= HandlePopupClosed;
        }
        gamePlayManager.GameplayEnded -= OnGameplayEnded;
        gamePlayManager.GameplayCompleted -= HandleGameplayCompleted;
        gamePlayManager.GameplayEnterRequested -= OnGameplayEnterRequested;
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
    public void OpenGameOver()
    {
        popupManager.OpenPopup(gmeOverPopup);
    }

    #endregion

    #region Gameplay Controls

    /// <summary>
    /// Inicia el flujo de salida del gameplay hacia el menú principal.
    /// </summary>
    public void BackToMainMenu()
    {
        gamePlayManager.ReportCurrentTelemetrySnapshot();

        GameEvents.RaiseGameEnded(GameTelemetryResult.Abandoned);

        OnGameplayExitCompleted();
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
     /// <summary>
    /// Ejecuta el flujo completo de recarga del gameplay:
    /// - (Opcional) Descarga configuración remota si es branded
    /// - Precarga los recursos del nivel
    /// - Recarga la escena actual
    /// </summary>
    private IEnumerator RunReloadGameplayFlow()
    {
        bool useApi = true; // producción
   
        string postBody = null;
   
        if (useApi && GameManager.Instance.IsBrandedMode)
        {
            var requestDto = new LevelConfigRequestDto
            {
                sessionToken = GameManager.Instance.SessionToken,
                userHash = GameManager.Instance.UserHash,
                isBrandedMode = GameManager.Instance.IsBrandedMode,
                campaignId = GameManager.Instance.CampaignId,
                gameTitle = GameManager.Instance.GameID
            };
   
            postBody = JsonUtility.ToJson(requestDto);
        }
   
        /// Construcción dinámica de pasos
        List<ILoadingStep> steps = new List<ILoadingStep>();
   
        if (GameManager.Instance.IsBrandedMode)
        {
            steps.Add(new LevelRemoteConfigStep(GameManager.Instance.ApiUrl, postBody));
        }
        else
        {
            DevLog.Log("[GameplayFlow] Modo NO branded → omitiendo carga remota");
   
            /// Importante: asegurar fallback explícito
            GameDataProvider.Instance.Initialize();
            GameManager.Instance.ConfigureAds(false);
        }
   
        steps.Add(new LevelResourcePreloadStep());
        steps.Add(new SceneLoadingStep(gameplaySceneName, 0f));
   
        LoadingContext loadingContext =
            new LoadingContext(loadingUI, steps.Count);
   
        foreach (ILoadingStep step in steps)
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
        if (nextLevelReloadStarted ||
        isWaitingRewardPopupBeforeReload)
        {
            return;
        }

        nextLevelMessageSent = false;
        nextLevelReloadStarted = false;

        Debug.Log("UIGamePlayController: Congrats confirmed, reloading gameplay.");
        GameEvents.RaiseGameEnded(GameTelemetryResult.Win);

        popupManager.CloseCurrentPopup(() =>
        {
            OpenRewardBeforeReload();
        });
    }


    /// Abre el RewardPopup utilizando los productos recibidos
    /// desde el JSON general.
    ///
    /// Si no existen productos válidos, continúa inmediatamente
    /// al siguiente nivel y notifica al frontend.
    /// </summary>
    private void OpenRewardBeforeReload()
    {
        if (rewardPopup == null)
        {
            DevLog.Log(
                "[UIGamePlayController] RewardPopup no está asignado. " +
                "Continuando al siguiente nivel.");

            CompleteNextLevelFlow();
            return;
        }

        GameDataProvider dataProvider = GameDataProvider.Instance;

        if (dataProvider == null || !dataProvider.IsInitialized)
        {
            DevLog.Log(
                "[UIGamePlayController] GameDataProvider no está inicializado. " +
                "Continuando al siguiente nivel.");

            CompleteNextLevelFlow();
            return;
        }

        CopagosRewardPopupData popupData =
            dataProvider.GetRewardPopupData();

        if (popupData == null ||
            popupData.products == null ||
            popupData.products.Count == 0)
        {
            DevLog.Log(
                "[UIGamePlayController] No existen productos de recompensa. " +
                "Continuando al siguiente nivel.");

            CompleteNextLevelFlow();
            return;
        }

        bool hasValidProducts = false;

        for (int i = 0; i < popupData.products.Count; i++)
        {
            CopagosProductData product = popupData.products[i];

            if (product != null && product.IsValid)
            {
                hasValidProducts = true;
                break;
            }
        }

        if (!hasValidProducts)
        {
            DevLog.Log(
                "[UIGamePlayController] No existen productos válidos. " +
                "Continuando al siguiente nivel.");

            CompleteNextLevelFlow();
            return;
        }

        rewardPopup.Setup(popupData);

        isWaitingRewardPopupBeforeReload = true;

        popupManager.UnlockOverlay();
        popupManager.OpenPopup(rewardPopup);

        DevLog.Log(
            "[UIGamePlayController] RewardPopup abierto. " +
            "GAME_FINISHED se enviará cuando el jugador lo cierre.");
    }
   /// <summary>
    /// Envía GAME_FINISHED y comienza la recarga exactamente una vez.
    /// </summary>
    private void CompleteNextLevelFlow()
    {
        if (nextLevelReloadStarted)
        {
            DevLog.Log(
                "[UIGamePlayController] El flujo del siguiente nivel " +
                "ya había comenzado.");

            return;
        }

        nextLevelReloadStarted = true;
        isWaitingRewardPopupBeforeReload = false;

        SendNextLevelMessageOnce();
        BeginWinReloadFlow();
    }

    /// <summary>
    /// Envía GAME_FINISHED una sola vez durante el flujo actual.
    /// </summary>
    private void SendNextLevelMessageOnce()
    {
        if (nextLevelMessageSent)
        {
            DevLog.Log(
                "[UIGamePlayController] GAME_FINISHED ya había sido enviado.");

            return;
        }

        CopagosWebGLBridge bridge =
            CopagosWebGLBridge.Instance;

        if (bridge == null)
        {
            DevLog.Log(
                "[UIGamePlayController] No existe CopagosWebGLBridge. " +
                "No se pudo enviar GAME_FINISHED.");

            return;
        }

        nextLevelMessageSent = true;

        bridge.SendNextLevelRequested();

        DevLog.Log(
            "[UIGamePlayController] Solicitud GAME_FINISHED enviada.");
    }

    /// <summary>
    /// Detecta el cierre del RewardPopup y continúa
    /// el flujo hacia el siguiente nivel.
    /// </summary>
    /// <param name="closedPopup">
    /// Popup que terminó de cerrarse.
    /// </param>
    private void HandlePopupClosed(
        PopupBase closedPopup)
    {
        if (!isWaitingRewardPopupBeforeReload)
        {
            return;
        }

        if (closedPopup != rewardPopup)
        {
            return;
        }

        DevLog.Log(
            "[UIGamePlayController] RewardPopup cerrado. " +
            "Enviando GAME_FINISHED.");

        CompleteNextLevelFlow();
    }

    /// <summary>
    /// Inicia la carga del siguiente gameplay después de cerrar el RewardPopup.
    /// </summary>
    private void BeginWinReloadFlow()
    {
        GameEvents.RaiseLoadStarted();

        uIAnimationManagerGameplay.ShowLoading();
        StartCoroutine(RunReloadGameplayFlow());
    }
    /// <summary>
    /// Maneja la confirmación del popup de felicitaciones.
    /// Reinicia el flujo de carga solicitando nueva data
    /// y recargando la escena de gameplay.
    /// </summary>
    private void OnGameOverConfirmed()
    {
        DevLog.Log("UIGamePlayController: Gameover confirmed, reloading gameplay.");
        GameEvents.RaiseGameEnded(GameTelemetryResult.Win);
        GameEvents.RaiseLoadStarted();
        uIAnimationManagerGameplay.ShowLoading();
        StartCoroutine(RunReloadGameplayFlow());
    }

    #endregion

    #region Gameplay Completion & Congrats Flow

    /// <summary>
    /// Configura los datos mostrados en el popup de felicitaciones.
    /// </summary>
    public void SetupGameOverPopup(
        string message,
        string statOne,
        string statTwo,
        string statThree)
    {
        gmeOverPopup.Setup(message, statOne, statTwo, statThree);
    }

    /// <summary>
    /// Se ejecuta inmediatamente cuando el jugador pierde.
    /// Oculta la UI antes del popup.
    /// </summary>
    private void OnGameplayEnded()
    {
        DevLog.Log("UI: GameplayEnded recibido → ocultando UI");

        hideUI();
    }

    /// <summary>
    /// Maneja el flujo completo cuando el puzzle es completado.
    /// Prepara la información, oculta el gameplay y muestra el popup.
    /// </summary>
    public void HandleGameplayCompleted(GameResultData result)
    {
        var data = GameDataProvider.Instance;


        if (data == null || !data.IsInitialized)
            return;


        var messages = data.Runtime.GameOverMessages;


        string victoryMessage = (messages != null && messages.Count > 0)
            ? messages[Random.Range(0, messages.Count)]
            : string.Empty;

        SetupGameOverPopup(
            victoryMessage,
            result.time,
            result.Distance.ToString(),
            result.coinsCollected.ToString()
        );

        popupManager.LockOverlay();
        OpenGameOver(); 
    }

    /// <summary>
    /// Oculta la UI del gameplay para mostrar el popup de felicitaciones.
    /// </summary>
    private void hideUI()
    {
        uIAnimationManagerGameplay.PlayExitAnimation();
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


    #endregion
}
