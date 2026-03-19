using System;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Clase base abstracta para todos los popups del proyecto.
/// Centraliza la lógica de animación de entrada y salida,
/// así como la notificación de cierre hacia sistemas externos.
/// 
/// Responsabilidades:
/// - Controlar animaciones de entrada/salida.
/// - Evitar superposición de tweens.
/// - Proteger contra ejecuciones inválidas si el objeto es destruido.
/// - Exponer un evento desacoplado para solicitud de cierre.
/// </summary>
public abstract class PopupBase : MonoBehaviour
{
    #region Events

    /// <summary>
    /// Evento disparado cuando el popup solicita ser cerrado.
    /// La responsabilidad de ejecutar el cierre final recae en el sistema oyente.
    /// </summary>
    public event Action CloseRequested;

    #endregion

    #region Serialized Fields

    /// <summary>
    /// Duración en segundos de las animaciones de entrada y salida del popup.
    /// </summary>
    [Tooltip("Duración en segundos de las animaciones de entrada y salida del popup.")]
    [SerializeField] private float animationDuration = 0.3f;

    #endregion

    #region Protected Fields

    /// <summary>
    /// Referencia al RectTransform del popup.
    /// Se utiliza para aplicar las animaciones de escala.
    /// </summary>
    protected RectTransform rectTransform;

    #endregion

    #region Private Fields

    /// <summary>
    /// Tween activo utilizado para controlar las animaciones actuales
    /// y evitar superposición de tweens.
    /// 
    /// Se enlaza al GameObject mediante SetLink para garantizar
    /// que se destruya automáticamente si el objeto es eliminado.
    /// </summary>
    private Tween activeTween;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa las referencias base del popup y establece su estado inicial.
    /// El popup comienza desactivado y con escala cero para permitir
    /// una animación de entrada controlada.
    /// </summary>
    protected virtual void Awake()
    {
        rectTransform = transform as RectTransform;

        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.zero;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Garantiza la liberación segura del tween activo al destruirse el objeto.
    /// 
    /// Aunque SetLink ya protege contra ejecuciones sobre objetos destruidos,
    /// esta limpieza explícita evita referencias residuales y mantiene
    /// el ciclo de vida completamente controlado.
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (activeTween != null && activeTween.IsActive())
        {
            activeTween.Kill();
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Muestra el popup aplicando una animación de escala.
    /// Cancela cualquier tween activo previo para evitar conflictos.
    /// 
    /// El tween se enlaza al GameObject para asegurar que no intente
    /// ejecutarse si el objeto es destruido antes de completarse.
    /// </summary>
    public virtual void Show()
    {
        if (rectTransform == null)
        {
            return;
        }

        gameObject.SetActive(true);

        activeTween?.Kill();

        activeTween = rectTransform
            .DOScale(Vector3.one, animationDuration)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Oculta el popup aplicando una animación de salida.
    /// Una vez finalizada la animación, desactiva el GameObject
    /// y ejecuta el callback opcional.
    /// 
    /// El tween se enlaza al GameObject para evitar errores
    /// si el objeto es destruido antes de finalizar la animación.
    /// </summary>
    /// <param name="onComplete">
    /// Acción opcional que se ejecuta al finalizar completamente la animación de cierre.
    /// </param>
    public virtual void Hide(Action onComplete)
    {
        if (rectTransform == null)
        {
            return;
        }

        activeTween?.Kill();

        activeTween = rectTransform
            .DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.InBack)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
            .OnComplete(() =>
            {
                if (this == null) return;

                gameObject.SetActive(false);
                onComplete?.Invoke();
            });
    }

    #endregion

    #region Protected Methods

    /// <summary>
    /// Notifica a los oyentes que este popup solicita ser cerrado.
    /// No ejecuta directamente la animación de cierre para permitir
    /// que un sistema externo decida el flujo.
    /// </summary>
    protected void RequestClose()
    {
        CloseRequested?.Invoke();
    }

    #endregion
}
