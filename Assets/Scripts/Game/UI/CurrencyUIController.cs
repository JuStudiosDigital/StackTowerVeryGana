using DG.Tweening;
using TMPro;
using UnityEngine;

/// <summary>
/// Controla la visualización y animación del contador de monedas en la interfaz de usuario.
/// 
/// Responsabilidades:
/// - Mostrar el valor actual de monedas.
/// - Animar el conteo visual sin alterar el estado real del sistema.
/// - Reproducir retroalimentación visual asociada al incremento.
/// 
/// Esta clase NO modifica el valor real de monedas. Esa responsabilidad
/// pertenece exclusivamente al CurrencyManager.
/// </summary>
public class CurrencyUIController : MonoBehaviour
{
    #region Inspector References

    /// <summary>
    /// Texto que muestra la cantidad actual de monedas en pantalla.
    /// Debe estar correctamente asignado desde el Inspector.
    /// </summary>
    [Header("Referencias UI")]
    [Tooltip("Texto TMP que muestra la cantidad actual de monedas.")]
    [SerializeField] private TMP_Text coinsText;

    /// <summary>
    /// Transform del ícono de monedas utilizado para animaciones visuales.
    /// </summary>
    [Tooltip("Transform del ícono que recibirá el efecto visual tipo punch.")]
    [SerializeField] private Transform coinsIconTransform;

    #endregion

    #region Animation Settings

    /// <summary>
    /// Intensidad del efecto de escala tipo "punch" aplicado al ícono.
    /// </summary>
    [Header("Animación")]
    [Tooltip("Intensidad de la escala aplicada al efecto punch.")]
    [SerializeField] private float punchScale = 0.25f;

    /// <summary>
    /// Duración del efecto de escala tipo "punch".
    /// </summary>
    [Tooltip("Duración en segundos del efecto punch del ícono.")]
    [SerializeField] private float punchDuration = 0.2f;

    /// <summary>
    /// Duración de la animación de conteo de monedas.
    /// </summary>
    [Tooltip("Duración en segundos del tween que anima el conteo de monedas.")]
    [SerializeField] private float countTweenDuration = 0.25f;

    #endregion

    #region Runtime State

    /// <summary>
    /// Valor actual de monedas que se muestra en la UI.
    /// Este valor puede diferir temporalmente del valor real durante animaciones.
    /// </summary>
    private int displayedCoins;

    /// <summary>
    /// Tween activo encargado de animar el conteo de monedas.
    /// Se guarda para poder cancelarlo antes de iniciar uno nuevo
    /// o limpiarlo correctamente al destruir el objeto.
    /// </summary>
    private Tween countTween;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa el contador de monedas con el valor actual del CurrencyManager
    /// al iniciar la escena.
    /// </summary>
    private void Start()
    {
        displayedCoins = GameManager.Instance.CurrencyManager.CurrentCoins;
        UpdateText(displayedCoins);
    }

    /// <summary>
    /// Garantiza que ningún tween permanezca activo cuando el objeto
    /// sea destruido, evitando que DOTween intente animar referencias inválidas.
    /// </summary>
    private void OnDestroy()
    {
        /// Se elimina explícitamente el tween de conteo si aún existe.
        if (countTween != null && countTween.IsActive())
        {
            countTween.Kill();
            countTween = null;
        }

        /// Se asegura que cualquier tween asociado al Transform del ícono
        /// también sea cancelado.
        if (coinsIconTransform != null)
        {
            coinsIconTransform.DOKill();
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Incrementa visualmente la cantidad de monedas mostradas en UI
    /// aplicando una animación de conteo y un efecto visual en el ícono.
    /// 
    /// Este método es seguro frente a múltiples invocaciones consecutivas,
    /// ya que cancela cualquier tween previo antes de iniciar uno nuevo.
    /// </summary>
    /// <param name="amount">
    /// Cantidad de monedas a agregar. Valores menores o iguales a cero son ignorados.
    /// </param>
    public void AddCoinsAnimated(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (coinsText == null)
        {
            DevLog.Warning($"{nameof(CurrencyUIController)}: coinsText no está asignado.");
            return;
        }

        int startValue = displayedCoins;
        int endValue = displayedCoins + amount;

        displayedCoins = endValue;

        /// Se cancela cualquier tween anterior para evitar solapamientos.
        countTween?.Kill();

        countTween = DOTween.To(
            () => startValue,
            value =>
            {
                /// Se valida que el texto aún exista antes de actualizarlo,
                /// previniendo errores si el objeto fue destruido durante la animación.
                if (coinsText != null)
                {
                    UpdateText(value);
                }
            },
            endValue,
            countTweenDuration
        )
        .SetEase(Ease.OutCubic)
        .SetLink(gameObject); 
        /// SetLink asegura que el tween se destruya automáticamente
        /// si el GameObject es destruido.

        PlayPunch();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Actualiza el texto de monedas en pantalla con el valor recibido.
    /// </summary>
    /// <param name="value">Valor de monedas a mostrar.</param>
    private void UpdateText(int value)
    {
        if (coinsText == null)
        {
            return;
        }

        coinsText.text = value.ToString();
    }

    /// <summary>
    /// Reproduce un efecto visual de rebote en el ícono de monedas
    /// para reforzar la retroalimentación al jugador.
    /// </summary>
    private void PlayPunch()
    {
        if (coinsIconTransform == null)
        {
            return;
        }

        coinsIconTransform.DOKill();

        coinsIconTransform
            .DOPunchScale(
                Vector3.one * punchScale,
                punchDuration,
                vibrato: 8,
                elasticity: 0.9f
            )
            .SetLink(gameObject);
        /// Vincula el tween al ciclo de vida del GameObject
        /// para evitar ejecución posterior a su destrucción.
    }

    #endregion
}
