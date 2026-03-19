using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestiona todas las animaciones y el flujo visual del Main Menu.
/// 
/// Responsabilidades:
/// - Animación de entrada de logo y botones.
/// - Animación idle (zoom sutil) del logo.
/// - Animación de salida al seleccionar una opción.
/// - Visualización del slider de carga.
/// 
/// Este componente se enfoca exclusivamente en animaciones y flujo visual,
/// sin manejar lógica de negocio ni navegación directa de escenas.
/// </summary>
public class UIAnimationManagerMainMenu : MonoBehaviour
{
    #region Inspector References

    /// <summary>
    /// Referencia al logo principal del menú.
    /// </summary>
    [Header("Referencias UI")]
    [Tooltip("RectTransform del logo principal que será animado.")]
    [SerializeField] private RectTransform logo;

    /// <summary>
    /// Botones ubicados en la parte superior del menú.
    /// </summary>
    [Tooltip("Botones secundarios ubicados en la parte superior del menú.")]
    [SerializeField] private RectTransform[] topButtons;

    /// <summary>
    /// Botones principales del menú.
    /// </summary>
    [Tooltip("Botones principales del menú que activan la transición de salida.")]
    [SerializeField] private RectTransform[] mainButtons;

    /// <summary>
    /// Slider visual utilizado para representar la carga.
    /// </summary>
    [Tooltip("Elemento visual que representa el progreso de carga.")]
    [SerializeField] private RectTransform loadingSlider;

    #endregion

    #region Scene Configuration

    /// <summary>
    /// Nombre de la escena objetivo a cargar después del menú.
    /// Actualmente no se utiliza directamente en este componente,
    /// pero se mantiene para compatibilidad con el flujo existente.
    /// </summary>
    [Header("Escena")]
    [Tooltip("Nombre de la escena destino que será cargada posteriormente.")]
    [SerializeField] private string targetSceneName;

    #endregion

    #region Animation Settings

    /// <summary>
    /// Duración de las animaciones de entrada tipo "pop".
    /// </summary>
    [Header("Animación Entrada / Salida")]
    [Tooltip("Duración en segundos de la animación de entrada.")]
    [SerializeField] private float popDuration = 0.35f;

    /// <summary>
    /// Duración de las animaciones de salida.
    /// </summary>
    [Tooltip("Duración en segundos de la animación de salida.")]
    [SerializeField] private float exitDuration = 0.25f;

    /// <summary>
    /// Curva de animación utilizada para la entrada.
    /// </summary>
    [Tooltip("Ease aplicado a la animación de entrada.")]
    [SerializeField] private Ease popEase = Ease.OutBack;

    /// <summary>
    /// Curva de animación utilizada para la salida.
    /// </summary>
    [Tooltip("Ease aplicado a la animación de salida.")]
    [SerializeField] private Ease exitEase = Ease.InBack;

    #endregion

    #region Logo Idle Animation

    /// <summary>
    /// Escala máxima utilizada en la animación idle del logo.
    /// </summary>
    [Header("Logo Idle Zoom")]
    [Tooltip("Escala máxima alcanzada durante el efecto idle del logo.")]
    [SerializeField] private float logoIdleScale = 1.05f;

    /// <summary>
    /// Duración del ciclo completo de zoom idle del logo.
    /// </summary>
    [Tooltip("Duración completa del ciclo de animación idle.")]
    [SerializeField] private float logoIdleDuration = 1.4f;

    #endregion

    #region Tween References

    /// <summary>
    /// Secuencia DOTween utilizada para la animación de entrada.
    /// </summary>
    private Sequence entrySequence;

    /// <summary>
    /// Secuencia DOTween utilizada para la animación de salida.
    /// </summary>
    private Sequence exitSequence;

    /// <summary>
    /// Tween encargado de la animación idle infinita del logo.
    /// </summary>
    private Tween logoIdleTween;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa el menú ejecutando la animación de entrada.
    /// </summary>
    private void Start()
    {
        PlayEntryAnimation();
    }

    /// <summary>
    /// Limpia correctamente todas las animaciones activas
    /// para evitar referencias colgantes o tweens activos.
    /// </summary>
    private void OnDisable()
    {
        KillAllTweens();
    }

    /// <summary>
    /// Garantiza la destrucción segura de todos los tweens asociados
    /// cuando el objeto es destruido.
    /// </summary>
    private void OnDestroy()
    {
        KillAllTweens();
    }

    #endregion

    #region Entry Animation

    /// <summary>
    /// Ejecuta la animación de entrada del menú principal,
    /// incluyendo logo, botones superiores y botones principales.
    /// 
    /// Se asegura de matar cualquier secuencia previa antes de crear una nueva,
    /// evitando duplicación de tweens.
    /// </summary>
    private void PlayEntryAnimation()
    {
        KillAllTweens();
        ResetAllScales();

        entrySequence = DOTween.Sequence()
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        /// Logo
        if (logo != null)
        {
            entrySequence.Append(
                logo.DOScale(1f, popDuration)
                    .SetEase(popEase)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            );
        }

        /// Inicio de animación idle del logo
        entrySequence.AppendCallback(StartLogoIdleAnimation);

        /// Botones superiores
        foreach (RectTransform button in topButtons)
        {
            if (button == null) continue;

            entrySequence.Append(
                button.DOScale(1f, popDuration)
                    .SetEase(popEase)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            );
        }

        /// Botones principales
        foreach (RectTransform button in mainButtons)
        {
            if (button == null) continue;

            entrySequence.Append(
                button.DOScale(1f, popDuration)
                    .SetEase(popEase)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            );
        }
    }

    #endregion

    #region Exit Animation

    /// <summary>
    /// Método llamado desde los botones principales del menú.
    /// Detiene la animación idle del logo y ejecuta la salida.
    /// </summary>
    public void OnMainButtonPressed()
    {
        logoIdleTween?.Kill();
        PlayExitAnimation();
    }

    /// <summary>
    /// Ejecuta la animación de salida del menú,
    /// ocultando todos los elementos visuales.
    /// </summary>
    private void PlayExitAnimation()
    {
        exitSequence?.Kill();

        exitSequence = DOTween.Sequence()
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        /// Logo
        if (logo != null)
        {
            exitSequence.Append(
                logo.DOScale(0f, exitDuration)
                    .SetEase(exitEase)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            );
        }

        /// Botones superiores
        foreach (RectTransform button in topButtons)
        {
            if (button == null) continue;

            exitSequence.Join(
                button.DOScale(0f, exitDuration)
                    .SetEase(exitEase)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            );
        }

        /// Botones principales
        foreach (RectTransform button in mainButtons)
        {
            if (button == null) continue;

            exitSequence.Join(
                button.DOScale(0f, exitDuration)
                    .SetEase(exitEase)
                    .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            );
        }

        /// Slider de carga
        exitSequence.AppendCallback(() =>
        {
            if (this != null && loadingSlider != null)
            {
                ShowLoadingSlider();
            }
        });
    }

    #endregion

    #region Logo Idle

    /// <summary>
    /// Inicia la animación idle infinita del logo,
    /// utilizando un efecto de zoom sutil en loop.
    /// 
    /// Se vincula el tween al GameObject para asegurar
    /// su destrucción automática si el objeto desaparece.
    /// </summary>
    private void StartLogoIdleAnimation()
    {
        if (logo == null) return;

        logoIdleTween = logo
            .DOScale(logoIdleScale, logoIdleDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    #endregion

    #region Loading UI

    /// <summary>
    /// Activa visualmente el slider de carga.
    /// La animación de entrada puede ser añadida posteriormente
    /// sin afectar el flujo actual.
    /// </summary>
    private void ShowLoadingSlider()
    {
        if (loadingSlider == null) return;

        loadingSlider.gameObject.SetActive(true);
    }

    #endregion

    #region Utilities

    /// <summary>
    /// Elimina de forma segura todas las animaciones activas.
    /// Centraliza la limpieza para evitar duplicación de código.
    /// </summary>
    private void KillAllTweens()
    {
        entrySequence?.Kill();
        exitSequence?.Kill();
        logoIdleTween?.Kill();
    }

    /// <summary>
    /// Resetea la escala de todos los elementos animados
    /// para garantizar estados iniciales consistentes.
    /// </summary>
    private void ResetAllScales()
    {
        if (logo != null)
            logo.localScale = Vector3.zero;

        foreach (RectTransform button in topButtons)
        {
            if (button != null)
                button.localScale = Vector3.zero;
        }

        foreach (RectTransform button in mainButtons)
        {
            if (button != null)
                button.localScale = Vector3.zero;
        }

        if (loadingSlider != null)
            loadingSlider.localScale = Vector3.zero;
    }

    #endregion
}
