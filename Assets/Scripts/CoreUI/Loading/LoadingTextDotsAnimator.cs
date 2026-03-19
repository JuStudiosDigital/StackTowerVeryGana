using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla una animación de opacidad en bucle para un texto de carga,
/// generando un efecto de pulso suave sin alterar el contenido ni la escala.
/// </summary>
/// <remarks>
/// Este componente requiere un <see cref="TextMeshProUGUI"/> en el mismo GameObject
/// y utiliza DOTween para la interpolación de la opacidad.
/// 
/// Se asegura de cancelar correctamente cualquier Tween activo cuando el objeto
/// es deshabilitado o destruido, evitando llamadas sobre referencias inválidas.
/// </remarks>
[RequireComponent(typeof(TextMeshProUGUI))]
public class LoadingTextDotsAnimator : MonoBehaviour
{
    #region Inspector Configuration

    /// <summary>
    /// Valor mínimo de opacidad que alcanzará el texto durante la animación.
    /// </summary>
    [Header("Animación")]
    [Tooltip("Valor mínimo de opacidad que alcanzará el texto durante el efecto de pulso.")]
    [SerializeField] private float minAlpha = 0.6f;

    /// <summary>
    /// Valor máximo de opacidad del texto.
    /// Este valor se utiliza como estado inicial antes de iniciar la animación.
    /// </summary>
    [Tooltip("Valor máximo de opacidad del texto. Se aplica como estado inicial antes de animar.")]
    [SerializeField] private float maxAlpha = 1f;

    /// <summary>
    /// Duración total del ciclo de desvanecimiento.
    /// Internamente se utiliza la mitad de este valor para cada fase del Yoyo.
    /// </summary>
    [Tooltip("Duración total del ciclo completo de desvanecimiento (ida y vuelta).")]
    [SerializeField] private float fadeDuration = 0.8f;

    #endregion

    #region Private State

    /// <summary>
    /// Referencia al componente de texto que será animado.
    /// </summary>
    private TextMeshProUGUI text;

    /// <summary>
    /// Tween activo encargado de animar la opacidad del texto.
    /// Se mantiene como referencia para poder cancelarlo correctamente.
    /// </summary>
    private Tween fadeTween;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa las referencias internas del componente.
    /// </summary>
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    /// <summary>
    /// Se ejecuta cuando el GameObject se habilita.
    /// Inicia la animación de opacidad.
    /// </summary>
    private void OnEnable()
    {
        StartFade();
    }

    /// <summary>
    /// Se ejecuta cuando el GameObject se deshabilita.
    /// 
    /// Cancela explícitamente el Tween activo para evitar:
    /// - Ejecución sobre objetos destruidos.
    /// - Fugas internas de tweens.
    /// - Duplicación de animaciones al reactivar el objeto.
    /// </summary>
    private void OnDisable()
    {
        KillTween();
    }

    /// <summary>
    /// Se ejecuta cuando el objeto es destruido.
    /// 
    /// Se asegura de que ningún Tween quede registrado intentando operar
    /// sobre una referencia inexistente.
    /// </summary>
    private void OnDestroy()
    {
        KillTween();
    }

    #endregion

    #region Animation Logic

    /// <summary>
    /// Inicia la animación de opacidad en bucle infinito utilizando un efecto Yoyo.
    /// </summary>
    /// <remarks>
    /// Antes de iniciar la animación:
    /// - Se restablece el estado visual.
    /// - Se cancela cualquier tween previo.
    /// - Se enlaza el Tween al GameObject mediante <see cref="Tween.SetLink"/>
    ///   para que DOTween lo destruya automáticamente si el objeto desaparece.
    /// </remarks>
    private void StartFade()
    {
        if (text == null)
            return;

        KillTween();
        ResetState();

        fadeTween = text
            .DOFade(minAlpha, fadeDuration * 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Restablece el estado inicial del texto antes de iniciar la animación.
    /// </summary>
    /// <remarks>
    /// Este método asegura que la opacidad comience siempre desde el valor máximo
    /// configurado en el Inspector.
    /// </remarks>
    private void ResetState()
    {
        if (text == null)
            return;

        text.alpha = maxAlpha;
    }

    /// <summary>
    /// Cancela y libera el Tween activo si existe.
    /// </summary>
    /// <remarks>
    /// Se utiliza tanto en <see cref="OnDisable"/> como en <see cref="OnDestroy"/>
    /// para garantizar que no queden animaciones activas referenciando este objeto.
    /// </remarks>
    private void KillTween()
    {
        if (fadeTween != null && fadeTween.IsActive())
        {
            fadeTween.Kill();
            fadeTween = null;
        }
    }

    #endregion
}
