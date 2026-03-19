using DG.Tweening;
using UnityEngine;

/// <summary>
/// Controla las animaciones de entrada del UI de carga.
/// Este controlador se encarga de:
/// - Mostrar el slider con un efecto tipo "pop".
/// - Mostrar el texto de carga sincronizado con el slider.
/// 
/// La animación se ejecuta automáticamente cuando el GameObject se habilita
/// y se limpia correctamente cuando se deshabilita para evitar fugas de memoria
/// o ejecuciones de tweens sobre objetos destruidos.
/// 
/// Seguridad:
/// - Todos los tweens están vinculados mediante SetLink.
/// - La secuencia se elimina explícitamente en OnDisable y OnDestroy.
/// - Se fuerzan kills adicionales sobre los targets para máxima robustez.
/// </summary>
public class LoadingUIAnimationController : MonoBehaviour
{
    #region Inspector References

    /// <summary>
    /// Referencia al RectTransform del slider de carga.
    /// Se anima mediante escala para el efecto de entrada.
    /// </summary>
    [Header("Referencias UI")]
    [Tooltip("RectTransform del slider que se animará con efecto pop.")]
    [SerializeField] private RectTransform slider;

    /// <summary>
    /// Referencia al RectTransform del texto de carga.
    /// Se anima de forma sincronizada con el slider.
    /// </summary>
    [Tooltip("RectTransform del texto de loading animado en sincronía con el slider.")]
    [SerializeField] private RectTransform loadingText;

    #endregion

    #region Animation Settings

    /// <summary>
    /// Duración de la animación tipo "pop" para los elementos UI.
    /// </summary>
    [Header("Entrada (Pop)")]
    [Tooltip("Duración en segundos del efecto pop.")]
    [SerializeField] private float popDuration = 0.35f;

    /// <summary>
    /// Curva de easing utilizada para la animación de entrada.
    /// </summary>
    [Tooltip("Tipo de easing aplicado a la animación de entrada.")]
    [SerializeField] private Ease popEase = Ease.OutBack;

    /// <summary>
    /// Retardo antes de iniciar la animación del UI.
    /// Permite sincronizar el loading con otros sistemas.
    /// </summary>
    [Header("Timing")]
    [Tooltip("Tiempo de espera antes de iniciar la animación del UI.")]
    [SerializeField] private float delayBeforeUI = 0.2f;

    #endregion

    #region Private State

    /// <summary>
    /// Secuencia principal de DOTween utilizada para la animación de entrada.
    /// Se mantiene como referencia para poder detenerla correctamente.
    /// </summary>
    private Sequence entrySequence;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Se ejecuta cuando el GameObject es habilitado.
    /// Inicia automáticamente la animación de entrada del loading.
    /// </summary>
    private void OnEnable()
    {
        PlayAnimation();
    }

    /// <summary>
    /// Se ejecuta cuando el GameObject es deshabilitado.
    /// Finaliza y limpia la secuencia de animación activa para evitar
    /// ejecuciones pendientes o referencias inválidas.
    /// </summary>
    private void OnDisable()
    {
        KillSequence();
        KillTargetTweens();
    }

    /// <summary>
    /// Se ejecuta cuando el objeto es destruido.
    /// Garantiza que no queden tweens activos asociados a este GameObject.
    /// </summary>
    private void OnDestroy()
    {
        KillSequence();
        KillTargetTweens();
    }

    #endregion

    #region Animation Logic

    /// <summary>
    /// Ejecuta la animación de entrada del UI de carga.
    /// Restablece el estado inicial y construye una secuencia DOTween
    /// con un retardo previo y animaciones sincronizadas.
    /// 
    /// La secuencia queda vinculada al GameObject mediante SetLink,
    /// asegurando su eliminación automática si el objeto es destruido.
    /// </summary>
    private void PlayAnimation()
    {
        if (!IsValid())
            return;

        KillSequence();
        KillTargetTweens();

        ResetState();

        entrySequence = DOTween.Sequence()
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        entrySequence.AppendInterval(delayBeforeUI);

        entrySequence.Append(
            slider.DOScale(1f, popDuration)
                  .SetEase(popEase)
                  .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
        );

        entrySequence.Join(
            loadingText.DOScale(1f, popDuration)
                       .SetEase(popEase)
                       .SetLink(gameObject, LinkBehaviour.KillOnDestroy)
        );
    }

    /// <summary>
    /// Restablece el estado visual inicial de los elementos UI
    /// antes de ejecutar la animación.
    /// 
    /// Se utiliza escala cero para permitir el efecto "pop".
    /// Incluye validación defensiva ante destrucción inesperada.
    /// </summary>
    private void ResetState()
    {
        if (!IsValid())
            return;

        slider.localScale = Vector3.zero;
        loadingText.localScale = Vector3.zero;
    }

    /// <summary>
    /// Finaliza y libera la secuencia activa si existe.
    /// 
    /// IMPORTANTE:
    /// Se utiliza Kill(false) para evitar que DOTween intente completar
    /// el tween antes de destruirlo. Completar el tween puede provocar
    /// accesos a objetos ya destruidos si el target fue eliminado
    /// durante un cambio de escena o desactivación jerárquica.
    /// </summary>
    private void KillSequence()
    {
        if (entrySequence == null)
            return;

        if (entrySequence.IsActive())
        {
            entrySequence.Kill(false);
        }

        entrySequence = null;
    }


    /// <summary>
    /// Fuerza la eliminación de cualquier tween activo
    /// que esté apuntando directamente a los targets UI.
    /// 
    /// Se utiliza Kill con complete = false para evitar
    /// evaluaciones finales sobre objetos potencialmente destruidos.
    /// </summary>
    private void KillTargetTweens()
    {
        if (slider != null)
            DOTween.Kill(slider, false);
    
        if (loadingText != null)
            DOTween.Kill(loadingText, false);
    }

    /// <summary>
    /// Valida que las referencias necesarias estén correctamente asignadas
    /// antes de intentar crear animaciones.
    /// </summary>
    private bool IsValid()
    {
        return slider != null && loadingText != null;
    }

    #endregion
}
