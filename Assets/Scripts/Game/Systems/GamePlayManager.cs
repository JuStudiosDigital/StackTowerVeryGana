using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Gestiona el flujo principal del gameplay.
/// Coordina la animación de entrada y salida del tablero,
/// la inicialización de la mecánica activa,
/// el control del reloj, las recompensas
/// y la comunicación con la capa de UI.
/// </summary>
public class GamePlayManager : MonoBehaviour
{

    /// <summary>
    /// Clave lógica del sonido al colocar una ficha.
    /// </summary>
    private const string CoinSoundKey = "CoinSound";

    /// <summary>
    /// Clave lógica del sonido de victoria.
    /// </summary>
    private const string VictorySoundKey = "VictorySound";

    #region Serialized References - Game Systems

    /// <summary>
    /// Reloj del juego encargado de medir, pausar
    /// y controlar el tiempo total de la partida.
    /// </summary>
    [Header("Game Systems")]
    [Tooltip("Referencia al sistema de reloj del gameplay.")]
    [SerializeField] private GameClock gameClock;

    #endregion

    #region Serialized References - Gameplay Mechanic

    /// <summary>
    /// Componente base que implementa la mecánica
    /// principal del gameplay (puzzle, runner, etc.).
    /// </summary>
    [Tooltip("Componente que implementa la mecánica principal del gameplay.")]
    [SerializeField] private MonoBehaviour gameplayMechanicBehaviour;

    /// <summary>
    /// Mecánica principal activa del gameplay.
    /// </summary>
    private IGameplayMechanic gameplayMechanic;

    /// <summary>
    /// Mecánica opcional que soporta imagen guía.
    /// </summary>
    private IGuideImageMechanic guideImageMechanic;

    /// <summary>
    /// Fuente opcional de recompensas por acciones del jugador.
    /// </summary>
    private IPieceRewardSource pieceRewardSource;

    /// <summary>
    /// Proveedor de la posición para la recompensa final.
    /// </summary>
    private ICompletionRewardAnchor completionRewardAnchor;

    /// <summary>
    /// Cantidad total de movimientos realizados
    /// durante la partida actual.
    /// </summary>
    public int TotalMoves { get; private set; }

    #endregion

    #region Serialized References - UI Events

    /// <summary>
    /// Evento emitido cuando el gameplay solicita
    /// la animación de entrada de la UI.
    /// El parámetro representa la duración de la transición.
    /// </summary>
    public event Action<float> GameplayEnterRequested;

    /// <summary>
    /// Evento emitido cuando el gameplay solicita
    /// la animación de salida de la UI.
    /// El parámetro representa la duración de la transición.
    /// </summary>
    public event Action<float> GameplayExitRequested;

    /// <summary>
    /// Evento emitido cuando el gameplay finaliza
    /// entregando los resultados de la partida.
    /// </summary>
    public event Action<GameResultData> GameplayCompleted;

    /// <summary>
    /// Duración estándar de las transiciones de UI.
    /// </summary>
    [Tooltip("Duración en segundos de las animaciones de transición de la UI.")]
    [SerializeField] private float uiTransitionDuration = 0.6f;

    #endregion

    #region Serialized References - Celebration & Rewards
    /// <summary>
    /// Duración de la celebración antes de cerrar el gameplay.
    /// </summary>
    [Tooltip("Tiempo en segundos que se espera antes de cerrar el gameplay tras completarlo.")]
    [SerializeField] private float celebrationDuration = 2f;
    [Tooltip("Componente que implementa la lógica de recompensas del gameplay.")]
    [SerializeField] private MonoBehaviour gameplayRewardHandlerBehaviour;
    private IGameplayRewardHandler gameplayRewardHandler;
    #endregion

    #region Private State

    /// <summary>
    /// Indica si el gameplay ya está
    /// en proceso de salida.
    /// </summary>
    private bool isExiting;

    /// <summary>
    /// Referencia al tween utilizado para la celebración.
    /// Se almacena para poder cancelarlo si el objeto es destruido
    /// antes de que finalice su ejecución.
    /// </summary>
    private Tween celebrationTween;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Resuelve las dependencias de interfaces
    /// implementadas por la mecánica activa.
    /// </summary>
    private void Awake()
    {
        gameplayMechanic = gameplayMechanicBehaviour as IGameplayMechanic;

        if (gameplayMechanic == null)
        {
            DevLog.Error(
                "GamePlayManager requiere un componente que implemente IGameplayMechanic."
            );
        }

        guideImageMechanic = gameplayMechanicBehaviour as IGuideImageMechanic;
        pieceRewardSource = gameplayMechanicBehaviour as IPieceRewardSource;
        completionRewardAnchor = gameplayMechanicBehaviour as ICompletionRewardAnchor;
    
        gameplayRewardHandler =
        gameplayRewardHandlerBehaviour as IGameplayRewardHandler;

        if (gameplayRewardHandler == null)
        {
            DevLog.Warning(
                "No se asignó un IGameplayRewardHandler. El gameplay no otorgará recompensas."
            );
        }
    }

    /// <summary>
    /// Inicia el gameplay ejecutando la animación
    /// de entrada y arrancando la mecánica activa.
    /// </summary>
    private void Start()
    {
        GameplayEnterRequested?.Invoke(uiTransitionDuration);

        gameplayMechanic.PlayEnterAnimation(() =>
        {
            gameplayMechanic.StartMechanic();
            InitializeClock();
        });
    }

     /// <summary>
    /// Asegura la cancelación de tweens activos asociados
    /// a este objeto para evitar callbacks tardíos
    /// después de su destrucción.
    /// </summary>
    private void OnDestroy()
    {
        celebrationTween?.Kill();
    }

    #endregion

    #region Event Binding

    /// <summary>
    /// Vincula los eventos de la mecánica activa
    /// con la lógica del gameplay.
    /// </summary>
    private void OnEnable()
    {
        if (gameplayMechanic == null)
            return;

        gameplayMechanic.OnPlayerAction += RegisterPlayerMove;
        gameplayMechanic.OnMechanicCompleted += OnMechanicCompleted;

        if (pieceRewardSource != null && gameplayRewardHandler != null)
        {
            pieceRewardSource.PieceRewardTriggered +=
                gameplayRewardHandler.HandleActionReward;
        }
        if (pieceRewardSource != null)
        {
            pieceRewardSource.PieceRewardTriggered += PlayPieceRewardSound;
        }
    }

    /// <summary>
    /// Desvincula los eventos de la mecánica activa
    /// para evitar fugas de memoria.
    /// </summary>
    private void OnDisable()
    {
        if (gameplayMechanic == null)
            return;

        gameplayMechanic.OnPlayerAction -= RegisterPlayerMove;
        gameplayMechanic.OnMechanicCompleted -= OnMechanicCompleted;

        if (pieceRewardSource != null && gameplayRewardHandler != null)
        {
            pieceRewardSource.PieceRewardTriggered -=
                gameplayRewardHandler.HandleActionReward;
        }
        if (pieceRewardSource != null)
        {
            pieceRewardSource.PieceRewardTriggered -= PlayPieceRewardSound;
        }

    }

    #endregion

    #region Clock Management

    /// <summary>
    /// Inicializa y arranca el reloj del gameplay
    /// en modo normal.
    /// </summary>
    private void InitializeClock()
    {
        gameClock.SetNormalMode();
        gameClock.StartClock();
    }

    #endregion

    #region Guide Image Control

    /// <summary>
    /// Alterna la visibilidad de la imagen guía
    /// si la mecánica activa lo soporta.
    /// </summary>
    public void ToggleGuideImage()
    {
        if (guideImageMechanic == null)
            return;

        guideImageMechanic.ToggleGuideImage();
    }

    /// <summary>
    /// Indica si la imagen guía está visible
    /// cuando la mecánica activa lo soporta.
    /// </summary>
    public bool IsGuideImageVisible =>
        guideImageMechanic != null && guideImageMechanic.IsGuideImageVisible;

    #endregion

    #region Gameplay Control

    /// <summary>
    /// Reinicia completamente el gameplay,
    /// incluyendo la mecánica y el reloj.
    /// </summary>
    public void RestartGame()
    {
        gameplayMechanic.RestartMechanic();
        InitializeClock();
    }

    #endregion

    #region Gameplay Rewards

    /// <summary>
    /// Maneja la finalización de la mecánica activa.
    /// </summary>
    private void OnMechanicCompleted()
    {
        gameClock.PauseClock();
        gameplayRewardHandler?.HandleCompletionReward();
        PlayVictorySound();
        celebrationTween = DOVirtual.DelayedCall(celebrationDuration, CompleteGameplay)
            .SetTarget(this)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Registra un movimiento realizado por el jugador.
    /// </summary>
    private void RegisterPlayerMove()
    {
        TotalMoves++;
    }

    #endregion

    #region Completion Flow

    /// <summary>
    /// Finaliza el gameplay y emite
    /// los resultados finales.
    /// </summary>
    private void CompleteGameplay()
    {
        var resultData = new GameResultData(
            gameClock.GetTimeString(),
            TotalMoves,
            gameplayRewardHandler != null
                ? gameplayRewardHandler.GetTotalReward()
                : 0
        );

        GameplayCompleted?.Invoke(resultData);
    }

    #endregion

    #region Exit Flow

    /// <summary>
    /// Inicia el flujo de salida del gameplay
    /// evitando ejecuciones duplicadas.
    /// </summary>
    /// <param name="onComplete">Callback opcional al finalizar.</param>
    public void ExitGameplay(Action onComplete = null)
    {
        DevLog.Log("GamePlayManager: ExitGameplay called.");
        if (isExiting)
            return;

        DevLog.Log("GamePlayManager: Starting exit flow.");

        isExiting = true;
        HideGameplayVisuals(onComplete);
    }

    /// <summary>
    /// Oculta los elementos visuales del gameplay
    /// ejecutando las animaciones de salida.
    /// </summary>
    /// <param name="onComplete">Callback opcional al finalizar.</param>
    public void HideGameplayVisuals(Action onComplete = null)
    {
        DevLog.Log("GamePlayManager: Hiding gameplay visuals.");
        GameplayExitRequested?.Invoke(uiTransitionDuration);
        DevLog.Log("GamePlayManager: Requested UI exit animation.");
        gameplayMechanic.PlayExitAnimation(onComplete);
    }

    #endregion

    #region Audio Handling
    /// <summary>
    /// Reproduce el sonido asociado a la recompensa por ficha.
    /// </summary>
    private void PlayPieceRewardSound(Vector3 _)
    {
        DevLog.Log("Playing piece reward sound.");
        AudioClip clip =
            ResourceService.Instance?.GetAudioClip(CoinSoundKey);
        DevLog.Log($"Retrieved clip: {clip}");
        if (clip == null)
            return;

        GameManager.Instance?.AudioManager?.Play(clip);
    }
    /// <summary>
    /// Reproduce el sonido de victoria del nivel.
    /// </summary>
    private void PlayVictorySound()
    {
        DevLog.Log("Playing victory sound.");
        AudioClip clip =
            ResourceService.Instance?.GetAudioClip(VictorySoundKey);

        if (clip == null)
            return;

        GameManager.Instance?.AudioManager?.Play(clip);
    }

    #endregion

}
