using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Controlador visual y funcional para botones de UI.
/// 
/// Extiende el comportamiento estándar de <see cref="Button"/> agregando:
/// - Animaciones de hover y click mediante DOTween.
/// - Reproducción de audio desacoplada.
/// - Sistema de bloqueo local y global de interacción.
/// 
/// Está diseñado para integrarse con un <c>GameManager</c> que exponga
/// control global de UI y un sistema de Audio centralizado.
/// </summary>
/// <remarks>
/// Responsabilidades:
/// - Controlar únicamente el feedback visual y sonoro del botón.
/// - No contiene lógica de negocio.
/// - No reemplaza el sistema interno de Button.
///
/// Seguridad:
/// - Todos los Tweens son cancelados al deshabilitar o destruir el objeto.
/// - Se utiliza <see cref="LinkBehaviour.KillOnDestroy"/> para evitar ejecución
///   de callbacks sobre referencias destruidas.
/// </remarks>
[RequireComponent(typeof(Button))]
public class UIButtonController : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    #region Inspector - Referencias

    /// <summary>
    /// Indica si el botón debe bloquear la UI global tras el primer click.
    /// Ideal para botones de navegación o confirmaciones críticas.
    /// </summary>
    [SerializeField]
    [Tooltip("Si está activo, el botón bloqueará la UI global tras el primer click.")]
    private bool onceTime = false;

    /// <summary>
    /// RectTransform que será animado.
    /// Si no se asigna manualmente, se utilizará el RectTransform del propio GameObject.
    /// </summary>
    [SerializeField]
    [Tooltip("RectTransform que será animado. Si se deja vacío se usa el del propio objeto.")]
    private RectTransform targetTransform;

    #endregion

    #region Inspector - Escala Hover

    /// <summary>
    /// Factor multiplicador aplicado a la escala original durante el hover.
    /// </summary>
    [SerializeField]
    [Tooltip("Multiplicador de escala aplicado cuando el puntero entra.")]
    private float hoverScale = 1.1f;

    /// <summary>
    /// Duración de la animación de escala en segundos.
    /// </summary>
    [SerializeField]
    [Tooltip("Duración de la animación de escala en segundos.")]
    private float animationDuration = 0.15f;

    #endregion

    #region Inspector - Animación Click

    /// <summary>
    /// Intensidad del efecto PunchScale aplicado al hacer click.
    /// </summary>
    [SerializeField]
    [Tooltip("Intensidad del efecto PunchScale al hacer click.")]
    private float clickPunchScale = 0.15f;

    /// <summary>
    /// Duración del efecto PunchScale en segundos.
    /// </summary>
    [SerializeField]
    [Tooltip("Duración del efecto PunchScale en segundos.")]
    private float clickPunchDuration = 0.2f;

    #endregion

    #region Inspector - Audio

    /// <summary>
    /// Tipo de sonido reproducido al entrar en hover.
    /// </summary>
    [SerializeField]
    [Tooltip("Tipo de sonido reproducido al hacer hover.")]
    private AudioTypeGame hoverSound = AudioTypeGame.HoverSound;

    /// <summary>
    /// Tipo de sonido reproducido al hacer click.
    /// </summary>
    [SerializeField]
    [Tooltip("Tipo de sonido reproducido al hacer click.")]
    private AudioTypeGame clickSound = AudioTypeGame.ButtonClick;

    #endregion

    #region Inspector - Eventos

    /// <summary>
    /// Evento invocado cuando el botón recibe un click válido.
    /// Permite desacoplar acciones desde el Inspector.
    /// </summary>
    [SerializeField]
    [Tooltip("Evento invocado cuando el botón es clickeado.")]
    private UnityEvent onClick;

    #endregion

    #region Runtime - Estado Interno

    /// <summary>
    /// Escala original almacenada para restaurar el estado visual.
    /// </summary>
    private Vector3 initialScale;

    /// <summary>
    /// Referencia al Tween activo.
    /// Se mantiene una única instancia para evitar solapamientos.
    /// </summary>
    private Tween activeTween;

    /// <summary>
    /// Indica si el puntero se encuentra actualmente dentro del área del botón.
    /// </summary>
    private bool isPointerInside;

    /// <summary>
    /// Bloqueo local para prevenir múltiples interacciones simultáneas.
    /// </summary>
    private bool _lock = false;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa referencias internas y almacena la escala original.
    /// </summary>
    private void Awake()
    {
        if (targetTransform == null)
        {
            targetTransform = transform as RectTransform;
        }

        initialScale = targetTransform.localScale;
    }

    /// <summary>
    /// Cancela cualquier animación activa y restaura la escala original
    /// cuando el objeto es deshabilitado.
    /// </summary>
    private void OnDisable()
    {
        KillActiveTween();

        if (targetTransform != null)
        {
            targetTransform.localScale = initialScale;
        }

        isPointerInside = false;
    }

    /// <summary>
    /// Garantiza la cancelación total de Tweens al destruir el objeto,
    /// evitando callbacks tardíos o referencias inválidas.
    /// </summary>
    private void OnDestroy()
    {
        KillActiveTween();
    }

    #endregion

    #region Pointer Events

    /// <summary>
    /// Maneja el evento de entrada del puntero.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_lock || GameManager.Instance.LockUI) return;
        if (isPointerInside) return;

        isPointerInside = true;

        AnimateScale(initialScale * hoverScale);
        PlayHoverSound();
    }

    /// <summary>
    /// Maneja el evento de salida del puntero.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_lock || GameManager.Instance.LockUI) return;

        isPointerInside = false;
        AnimateScale(initialScale);
    }

    /// <summary>
    /// Maneja el evento de click del botón.
    /// </summary>
    /// <param name="eventData">Datos del evento de puntero.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_lock || GameManager.Instance.LockUI) return;

        _lock = true;

        PlayClickAnimation();
        PlayClickSound();
        onClick?.Invoke();
    }

    #endregion

    #region Animaciones

    /// <summary>
    /// Ejecuta una animación de escala hacia el valor indicado.
    /// </summary>
    /// <param name="targetScale">Escala objetivo.</param>
    private void AnimateScale(Vector3 targetScale)
    {
        KillActiveTween();

        activeTween = targetTransform
            .DOScale(targetScale, animationDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Ejecuta la animación visual asociada al click.
    /// Puede bloquear la UI global dependiendo de la configuración.
    /// </summary>
    private void PlayClickAnimation()
    {
        if (onceTime)
        {
            GameManager.Instance.LockUI = true;
        }

        KillActiveTween();

        activeTween = targetTransform
            .DOPunchScale(Vector3.one * clickPunchScale, clickPunchDuration, 8, 0.8f)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            .OnComplete(() =>
            {
                if (this == null) return;

                if (!onceTime)
                {
                    _lock = false;
                }
            });
    }

    /// <summary>
    /// Cancela el Tween activo de forma segura y limpia la referencia interna.
    /// </summary>
    private void KillActiveTween()
    {
        if (activeTween != null)
        {
            activeTween.Kill(true);
            activeTween = null;
        }
    }

    #endregion

    #region Audio

    /// <summary>
    /// Reproduce el sonido configurado para hover.
    /// </summary>
    private void PlayHoverSound()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AudioManager?.Play(hoverSound);
        }
    }

    /// <summary>
    /// Reproduce el sonido configurado para click.
    /// </summary>
    private void PlayClickSound()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AudioManager?.Play(clickSound);
        }
    }

    #endregion
}
