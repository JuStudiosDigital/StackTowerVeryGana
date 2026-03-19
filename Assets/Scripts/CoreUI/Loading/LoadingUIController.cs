using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controlador de la interfaz de carga.
/// 
/// Responsabilidad:
/// - Representar visualmente el progreso de carga mediante un <see cref="Slider"/>.
/// - Suavizar las transiciones de progreso usando DOTween.
/// 
/// Seguridad:
/// - Vincula el Tween al ciclo de vida del GameObject para evitar callbacks
///   sobre referencias destruidas.
/// - Cancela explícitamente Tweens activos al destruirse.
/// </summary>
public class LoadingUIController : MonoBehaviour
{
    /// <summary>
    /// Slider que representa visualmente el progreso de carga.
    /// Debe estar configurado con un rango normalizado (0–1).
    /// </summary>
    [SerializeField]
    [Tooltip("Slider que representa el progreso de carga (rango esperado 0–1).")]
    private Slider slider;

    /// <summary>
    /// Tween activo encargado de suavizar el cambio de progreso.
    /// Se mantiene referencia para poder cancelarlo correctamente.
    /// </summary>
    private Tween progressTween;

    /// <summary>
    /// Valor actual mostrado en el slider.
    /// Se mantiene separado del valor objetivo para permitir interpolación.
    /// </summary>
    private float currentValue;

    /// <summary>
    /// Reinicia completamente el estado visual de la carga.
    /// 
    /// - Cancela cualquier Tween activo.
    /// - Restablece el progreso a cero.
    /// </summary>
    public void ResetProgress()
    {
        progressTween?.Kill();
        progressTween = null;

        currentValue = 0f;

        if (slider != null)
        {
            slider.value = 0f;
        }
    }

    /// <summary>
    /// Establece un nuevo progreso objetivo para la barra de carga.
    /// El valor se anima suavemente usando DOTween.
    /// 
    /// El Tween:
    /// - Se vincula al GameObject mediante <see cref="Tween.SetLink(GameObject)"/>
    ///   para garantizar su cancelación automática si el objeto es destruido.
    /// - Cancela cualquier Tween previo para evitar acumulaciones.
    /// </summary>
    /// <param name="target">
    /// Progreso objetivo normalizado (0 a 1).
    /// </param>
    public void SetProgress(float target)
    {
        if (slider == null)
        {
            return;
        }

        target = Mathf.Clamp01(target);

        // Cancelamos el tween anterior para evitar acumulaciones
        progressTween?.Kill();

        // Tween suave hacia el nuevo valor objetivo
        progressTween = DOTween.To(
            () => currentValue,
            value =>
            {
                currentValue = value;

                // Protección ante destrucción tardía del objeto
                if (slider != null)
                {
                    slider.value = currentValue;
                }
            },
            target,
            0.25f
        )
        .SetEase(Ease.OutSine)
        .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Asegura la cancelación del Tween activo cuando el objeto es destruido.
    /// Esto previene intentos de animación sobre referencias inválidas.
    /// </summary>
    private void OnDestroy()
    {
        progressTween?.Kill();
        progressTween = null;
    }
}
