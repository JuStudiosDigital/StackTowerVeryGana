using System;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// Orquestador principal del gameplay.
/// 
/// Responsable de:
/// - Inicializar la mecánica activa
/// - Gestionar el ciclo de vida de la partida
/// - Controlar el reloj del juego
/// - Coordinar recompensas
/// - Emitir el resultado final hacia la UI
/// 
/// No contiene lógica de UI ni rendering.
/// </summary>
public sealed class GamePlayManager : MonoBehaviour
{
    #region Constants

    /// <summary>
    /// Clave lógica del sonido de recolección.
    /// </summary>
    private const string CoinSoundKey = "CoinSound";

    #endregion

    #region Serialized References - Core Systems

    [Header("Core Systems")]

    [Tooltip("Sistema encargado de medir y controlar el tiempo de la partida.")]
    [SerializeField] private GameClock gameClock;

    [Tooltip("Componente que implementa la mecánica principal del gameplay.")]
    [SerializeField] private MonoBehaviour gameplayMechanicBehaviour;

    [Tooltip("Componente encargado de procesar y acumular recompensas.")]
    [SerializeField] private MonoBehaviour gameplayRewardHandlerBehaviour;

    #endregion

    #region Serialized Configuration

    [Header("Configuration")]

    [Tooltip("Duración en segundos antes de finalizar el gameplay tras completarse.")]
    [SerializeField] private float celebrationDuration = 2f;

    [Tooltip("Duración de transición de entrada solicitada a la UI.")]
    [SerializeField] private float uiTransitionDuration = 0.6f;

    #endregion

    #region Interfaces (Resolved at Runtime)

    /// <summary>
    /// Mecánica activa del gameplay.
    /// </summary>
    private IGameplayMechanic gameplayMechanic;

    /// <summary>
    /// Fuente de eventos de recompensa por acción.
    /// </summary>
    private IPieceRewardSource pieceRewardSource;

    /// <summary>
    /// Handler encargado de acumular recompensas.
    /// </summary>
    private IGameplayRewardHandler gameplayRewardHandler;

    #endregion

    #region Events

    /// <summary>
    /// Evento solicitado a la UI para animación de entrada.
    /// </summary>
    public event Action<float> GameplayEnterRequested;

    /// <summary>
    /// Evento emitido al finalizar la partida con resultados.
    /// </summary>
    public event Action<GameResultData> GameplayCompleted;

    /// <summary>
    /// Evento emitido inmediatamente cuando el gameplay termina (sin delay).
    /// </summary>
    public event Action GameplayEnded;

    #endregion

    #region Private State

    /// <summary>
    /// Tween utilizado para el delay de finalización.
    /// </summary>
    private Tween celebrationTween;

    /// <summary>
    /// Indica si el gameplay ya finalizó (protección contra duplicados).
    /// </summary>
    private bool isCompleted;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Resuelve dependencias de interfaces requeridas.
    /// </summary>
    private void Awake()
    {
        gameplayMechanic = gameplayMechanicBehaviour as IGameplayMechanic;

        if (gameplayMechanic == null)
        {
            DevLog.Error("GamePlayManager: El componente asignado no implementa IGameplayMechanic.");
        }

        pieceRewardSource = gameplayMechanicBehaviour as IPieceRewardSource;

        gameplayRewardHandler = gameplayRewardHandlerBehaviour as IGameplayRewardHandler;

        if (gameplayRewardHandler == null)
        {
            DevLog.Warning("GamePlayManager: No se asignó un IGameplayRewardHandler.");
        }
    }

    /// <summary>
    /// Inicializa el gameplay y solicita animación de entrada.
    /// </summary>
    private void Start()
    {
        GameplayEnterRequested?.Invoke(uiTransitionDuration);

        gameplayMechanic?.StartMechanic();
        InitializeClock();
    }

    /// <summary>
    /// Libera tweens activos para evitar callbacks inválidos.
    /// </summary>
    private void OnDestroy()
    {
        celebrationTween?.Kill();
    }

    #endregion

    #region Event Binding

    /// <summary>
    /// Suscribe eventos de la mecánica activa.
    /// </summary>
    private void OnEnable()
    {
        if (gameplayMechanic == null)
            return;

        gameplayMechanic.OnMechanicCompleted += OnMechanicCompleted;

        if (pieceRewardSource != null)
        {
            pieceRewardSource.PieceRewardTriggered += PlayPieceRewardSound;

            if (gameplayRewardHandler != null)
            {
                pieceRewardSource.PieceRewardTriggered +=
                    gameplayRewardHandler.HandleActionReward;
            }
        }
    }

    /// <summary>
    /// Desuscribe eventos para evitar fugas de memoria.
    /// </summary>
    private void OnDisable()
    {
        if (gameplayMechanic == null)
            return;

        gameplayMechanic.OnMechanicCompleted -= OnMechanicCompleted;

        if (pieceRewardSource != null)
        {
            pieceRewardSource.PieceRewardTriggered -= PlayPieceRewardSound;

            if (gameplayRewardHandler != null)
            {
                pieceRewardSource.PieceRewardTriggered -=
                    gameplayRewardHandler.HandleActionReward;
            }
        }
    }

    #endregion

    #region Clock

    /// <summary>
    /// Configura e inicia el reloj del gameplay.
    /// </summary>
    private void InitializeClock()
    {
        if (gameClock == null)
        {
            DevLog.Error("GamePlayManager: GameClock no asignado.");
            return;
        }

        gameClock.SetNormalMode();
        gameClock.StartClock();
    }

    #endregion

    #region Gameplay Flow

    /// <summary>
    /// Se ejecuta cuando la mecánica reporta finalización.
    /// Controla el cierre del gameplay.
    /// </summary>
    private void OnMechanicCompleted()
    {
        if (isCompleted)
            return;

        isCompleted = true;

        DevLog.Log("GamePlayManager: Gameplay completado.");

        gameClock?.PauseClock();

        GameplayEnded?.Invoke();

        // Delay de celebración antes de finalizar
        celebrationTween = DOVirtual.DelayedCall(celebrationDuration, CompleteGameplay)
            .SetTarget(this)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Construye el resultado final y lo emite a la UI.
    /// </summary>
    private void CompleteGameplay()
    {
        int distance = ResolveDistanceSafe();

        int coins = gameplayRewardHandler != null
            ? gameplayRewardHandler.GetTotalReward()
            : 0;

        var resultData = new GameResultData(
            gameClock != null ? gameClock.GetTimeString() : "0",
            distance,
            coins
        );

        DevLog.Log("GamePlayManager: Emitiendo GameplayCompleted.");

        GameplayCompleted?.Invoke(resultData);
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Obtiene la distancia del jugador de forma segura.
    /// Evita dependencias rígidas.
    /// </summary>
    private int ResolveDistanceSafe()
    {
        var stack = gameplayMechanicBehaviour as StackTowerGameplayMechanic;

        if (stack == null)
        {
            DevLog.Warning("GamePlayManager: No se pudo obtener distancia (FlappyGameplayMechanic no encontrado).");
            return 0;
        }

        return stack.GetScore();
    }

    #endregion

    #region Audio

    /// <summary>
    /// Reproduce el sonido asociado a la obtención de recompensa.
    /// </summary>
    private void PlayPieceRewardSound(Vector3 _)
    {
        var data = GameDataProvider.Instance;


        if (data == null || !data.IsInitialized)
            return;


        AudioClip clip = data.Runtime.KeyWin;

        if (clip == null)
            return;

        GameManager.Instance?.AudioManager?.Play(clip);
    }

    #endregion
}