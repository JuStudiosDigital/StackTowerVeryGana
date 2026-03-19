using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// Controla el overlay oscuro que se muestra detrás de popups o modales.
/// Gestiona la animación de opacidad y la detección de clicks para cierre.
/// 
/// Seguridad:
/// - Garantiza que no queden tweens activos cuando el objeto es destruido.
/// - Evita que DOTween intente animar referencias inválidas.
/// </summary>
[RequireComponent(typeof(Image))]
public class PopupOverlayController : MonoBehaviour, IPointerClickHandler
{
    #region Serialized Fields

    /// <summary>
    /// Duración de la animación de fade al mostrar u ocultar el overlay.
    /// </summary>
    [SerializeField]
    [Tooltip("Duración en segundos de la animación de fade del overlay.")]
    private float fadeDuration = 0.25f;

    #endregion

    #region Private Fields

    /// <summary>
    /// Imagen utilizada como overlay visual.
    /// </summary>
    private Image overlayImage;

    /// <summary>
    /// Indica si el overlay está bloqueado para ignorar interacciones.
    /// </summary>
    private bool _lock = false;

    #endregion

    #region Public Properties

    /// <summary>
    /// Acción que se ejecuta cuando el usuario hace click sobre el overlay.
    /// Generalmente se utiliza para cerrar el popup activo.
    /// </summary>
    public System.Action OnOverlayClicked { get; set; }

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa el overlay asegurando opacidad cero y estado inactivo.
    /// </summary>
    private void Awake()
    {
        overlayImage = GetComponent<Image>();
        SetAlpha(0f);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Asegura la cancelación de cualquier tween activo
    /// cuando el objeto es deshabilitado.
    /// 
    /// Esto previene callbacks tardíos sobre objetos inactivos.
    /// </summary>
    private void OnDisable()
    {
        overlayImage?.DOKill();
    }

    /// <summary>
    /// Garantiza que no queden tweens activos asociados
    /// a este componente cuando el objeto es destruido.
    /// 
    /// Previene errores del tipo:
    /// "Target or field is missing/null".
    /// </summary>
    private void OnDestroy()
    {
        overlayImage?.DOKill();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Muestra el overlay activando el GameObject y animando su opacidad.
    /// Si existía un tween previo, se cancela antes de iniciar uno nuevo.
    /// </summary>
    public void Show()
    {
        overlayImage.DOKill();

        gameObject.SetActive(true);

        overlayImage
            .DOFade(0.6f, fadeDuration)
            .SetTarget(this);
    }

    /// <summary>
    /// Oculta el overlay animando su opacidad y desactivando el GameObject al finalizar.
    /// Si existía un tween previo, se cancela antes de iniciar uno nuevo.
    /// </summary>
    public void Hide()
    {
        overlayImage.DOKill();

        overlayImage
            .DOFade(0f, fadeDuration)
            .SetTarget(this)
            .OnComplete(() =>
            {
                if (gameObject != null)
                {
                    gameObject.SetActive(false);
                }
            });
    }

    /// <summary>
    /// Bloquea la interacción del overlay, evitando que dispare eventos de click.
    /// </summary>
    public void Lock()
    {
        _lock = true;
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Detecta clicks sobre el overlay y ejecuta la acción asociada,
    /// siempre que no se encuentre bloqueado.
    /// </summary>
    /// <param name="eventData">Información del evento de input.</param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_lock)
        {
            return;
        }

        OnOverlayClicked?.Invoke();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Establece directamente el valor de alpha de la imagen del overlay.
    /// </summary>
    /// <param name="alpha">Valor de opacidad entre 0 y 1.</param>
    private void SetAlpha(float alpha)
    {
        Color color = overlayImage.color;
        color.a = alpha;
        overlayImage.color = color;
    }

    #endregion
}
