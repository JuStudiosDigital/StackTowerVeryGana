using DG.Tweening;
using UnityEngine;

/// <summary>
/// Gestiona todas las animaciones de interfaz de usuario durante el Gameplay.
/// Se encarga de la entrada y salida animada del HUD, botones superiores
/// y del control visual del estado de carga.
/// </summary>
/// <remarks>
/// Esta clase centraliza la lógica de animación UI para evitar duplicación
/// y mantener un comportamiento consistente en diferentes estados del juego
/// (inicio, pausa, game over, transición de escenas).
/// </remarks>
public class UIAnimationManagerGameplay : MonoBehaviour
{
    #region Inspector References

    /// <summary>
    /// Botones ubicados en la parte superior de la interfaz (ej. pausa, settings).
    /// </summary>
    [Header("Referencias UI")]
    [SerializeField] private RectTransform[] topButtons;

    /// <summary>
    /// Elementos principales del HUD de Gameplay (vida, score, energía, etc.).
    /// </summary>
    [SerializeField] private RectTransform[] hudElements;

    /// <summary>
    /// Contenedor visual del loading mostrado durante transiciones.
    /// </summary>
    [SerializeField] private RectTransform loadingContainer;

    #endregion

    #region Animation Settings

    /// <summary>
    /// Tipo de easing utilizado para la animación de entrada.
    /// Produce un efecto de aparición elástica.
    /// </summary>
    [Header("Animación Entrada / Salida")]
    [SerializeField] private Ease popEase = Ease.OutBack;

    /// <summary>
    /// Tipo de easing utilizado para la animación de salida.
    /// Produce un efecto de contracción hacia atrás.
    /// </summary>
    [SerializeField] private Ease exitEase = Ease.InBack;

    #endregion

    #region Tween Sequences

    /// <summary>
    /// Secuencia DOTween encargada de la animación de entrada del HUD.
    /// </summary>
    private Sequence entrySequence;

    /// <summary>
    /// Secuencia DOTween encargada de la animación de salida del HUD.
    /// </summary>
    private Sequence exitSequence;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa el estado visual de todos los elementos UI al iniciar la escena.
    /// </summary>
    private void Start()
    {
        ResetAllScales();
    }

    /// <summary>
    /// Limpia las secuencias activas cuando el objeto se deshabilita
    /// para evitar tweens huérfanos o referencias inválidas.
    /// </summary>
    private void OnDisable()
    {
        entrySequence?.Kill();
        exitSequence?.Kill();
    }

    #endregion

    #region Public Animation Controls

    /// <summary>
    /// Ejecuta la animación de entrada del HUD de Gameplay.
    /// Todos los elementos se animan de forma simultánea desde escala cero.
    /// </summary>
    /// <param name="enterDuration">Duración total de la animación de entrada.</param>
    public void PlayEntryAnimation(float enterDuration)
    {
        ResetAllScales();

        entrySequence?.Kill();
        entrySequence = DOTween.Sequence();

        foreach (RectTransform button in topButtons)
        {
            entrySequence.Join(
                button.DOScale(1f, enterDuration).SetEase(popEase)
            );
        }

        foreach (RectTransform hud in hudElements)
        {
            entrySequence.Join(
                hud.DOScale(1f, enterDuration).SetEase(popEase)
            );
        }
    }

    /// <summary>
    /// Ejecuta la animación de salida completa del HUD de Gameplay.
    /// Utilizada para estados como pausa, game over o transición de escena.
    /// </summary>
    /// <param name="onComplete">
    /// Acción opcional que se ejecuta al finalizar completamente la animación.
    /// </param>
    /// <param name="exitDuration">
    /// Duración total de la animación de salida.
    /// </param>
    public void PlayExitAnimation(System.Action onComplete = null, float exitDuration = 0.3f)
    {
        exitSequence?.Kill();
        exitSequence = DOTween.Sequence();

        foreach (RectTransform button in topButtons)
        {
            exitSequence.Join(
                button.DOScale(0f, exitDuration).SetEase(exitEase)
            );
        }

        foreach (RectTransform hud in hudElements)
        {
            exitSequence.Join(
                hud.DOScale(0f, exitDuration).SetEase(exitEase)
            );
        }

        exitSequence.OnComplete(() => onComplete?.Invoke());
    }

    /// <summary>
    /// Muestra visualmente el contenedor de loading del Gameplay.
    /// </summary>
    /// <remarks>
    /// No utiliza animación para evitar retrasos visuales
    /// durante cargas críticas.
    /// </remarks>
    public void ShowLoading()
    {
        DevLog.Log("UIAnimationManagerGameplay: Showing loading container.");
        if (loadingContainer == null)
            return;

        loadingContainer.gameObject.SetActive(true);
        loadingContainer.localScale = Vector3.one;
        DevLog.Log("UIAnimationManagerGameplay: Loading container shown.");
    }

    #endregion

    #region Internal Utilities

    /// <summary>
    /// Resetea la escala de todos los elementos UI a cero
    /// y oculta el contenedor de loading.
    /// </summary>
    /// <remarks>
    /// Este método garantiza un estado visual consistente
    /// antes de ejecutar cualquier animación.
    /// </remarks>
    private void ResetAllScales()
    {
        foreach (RectTransform button in topButtons)
        {
            button.localScale = Vector3.zero;
        }

        foreach (RectTransform hud in hudElements)
        {
            hud.localScale = Vector3.zero;
        }

        if (loadingContainer != null)
        {
            loadingContainer.localScale = Vector3.zero;
            loadingContainer.gameObject.SetActive(false);
        }
    }

    #endregion
}
