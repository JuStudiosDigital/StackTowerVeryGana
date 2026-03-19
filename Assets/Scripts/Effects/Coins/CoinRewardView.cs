using DG.Tweening;
using UnityEngine;
using System;

/// <summary>
/// Controla la representación visual de una moneda de recompensa.
/// Se encarga de animar la moneda desde el mundo hasta un objetivo en la UI,
/// incluyendo aparición, movimiento en arco, partículas y notificación final.
/// 
/// Seguridad:
/// - Vincula la secuencia de DOTween al GameObject mediante SetLink.
/// - Garantiza que no queden tweens activos si el objeto es destruido.
/// - Evita intentar animar referencias inválidas.
/// </summary>
public class CoinRewardView : MonoBehaviour
{
    #region Serialized Configuration

    /// <summary>
    /// Configuración relacionada con las animaciones de escala y llegada.
    /// </summary>
    [Header("Animación")]
    [Tooltip("Duración de la animación de escala inicial al aparecer.")]
    [SerializeField] private float spawnScaleDuration = 0.25f;

    [Tooltip("Duración total del movimiento hacia el objetivo.")]
    [SerializeField] private float moveDuration = 0.55f;

    [Tooltip("Escala máxima aplicada en el rebote al llegar al destino.")]
    [SerializeField] private float arrivalBounceScale = 1.25f;

    [Tooltip("Duración del rebote visual al llegar al destino.")]
    [SerializeField] private float arrivalBounceDuration = 0.15f;

    /// <summary>
    /// Configuración del movimiento curvo y retardos aleatorios.
    /// </summary>
    [Header("Movimiento")]
    [Tooltip("Altura del arco en el movimiento hacia el objetivo.")]
    [SerializeField] private float arcHeight = 1.2f;

    [Tooltip("Retardo mínimo aleatorio antes de iniciar el movimiento.")]
    [SerializeField] private float minMoveDelay = 0.03f;

    [Tooltip("Retardo máximo aleatorio antes de iniciar el movimiento.")]
    [SerializeField] private float maxMoveDelay = 0.12f;

    /// <summary>
    /// Sistemas de partículas utilizados durante la aparición y llegada.
    /// </summary>
    [Header("Partículas")]
    [Tooltip("Partícula que se reproduce al aparecer la moneda.")]
    [SerializeField] private ParticleSystem spawnParticle;

    [Tooltip("Partícula que se reproduce al llegar al destino.")]
    [SerializeField] private ParticleSystem arrivalParticle;

    #endregion

    #region Runtime State

    /// <summary>
    /// Transform de destino al que la moneda se desplaza (normalmente un elemento de UI).
    /// </summary>
    private Transform target;

    /// <summary>
    /// Valor de recompensa que representa esta moneda.
    /// </summary>
    private int rewardValue;

    /// <summary>
    /// Secuencia activa de DOTween.
    /// Se almacena para poder matarla explícitamente si el objeto se destruye.
    /// </summary>
    private Sequence activeSequence;

    #endregion

    #region Events

    /// <summary>
    /// Evento que se dispara cuando la moneda alcanza su destino.
    /// El parámetro indica el valor de la recompensa asociada.
    /// </summary>
    public event Action<int> ReachedTarget;

    #endregion

    #region Public API

    /// <summary>
    /// Inicializa la moneda con su destino y el valor de recompensa.
    /// Una vez inicializada, la animación comienza automáticamente.
    /// </summary>
    /// <param name="target">Transform objetivo en la UI.</param>
    /// <param name="rewardValue">Valor de la recompensa asociada.</param>
    public void Initialize(Transform target, int rewardValue)
    {
        if (target == null)
        {
            DevLog.Warning("[CoinRewardView] Target no válido. La animación no se ejecutará.");
            return;
        }

        this.target = target;
        this.rewardValue = rewardValue;

        PlayAnimation();
    }

    #endregion

    #region Animation Flow

    /// <summary>
    /// Ejecuta la secuencia completa de animación de la moneda:
    /// aparición con escala, movimiento en arco hacia el objetivo,
    /// partículas de llegada y rebote final.
    /// 
    /// La secuencia se vincula al GameObject mediante SetLink para
    /// evitar errores si el objeto es destruido antes de finalizar.
    /// </summary>
    private void PlayAnimation()
    {
        transform.localScale = Vector3.zero;

        PlaySpawnParticle();

        float delay = UnityEngine.Random.Range(minMoveDelay, maxMoveDelay);

        Vector3 startPosition = transform.position;
        Vector3 endPosition = target.position;

        Vector3 midPoint = Vector3.Lerp(startPosition, endPosition, 0.5f);
        midPoint.y += arcHeight;

        activeSequence = DOTween.Sequence();
        activeSequence.SetDelay(delay);

        /// <summary>
        /// Vincula la secuencia al GameObject.
        /// Si el objeto es destruido, DOTween matará automáticamente la secuencia.
        /// </summary>
        activeSequence.SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        // Escala inicial al aparecer
        activeSequence.Join(
            transform.DOScale(Vector3.one, spawnScaleDuration)
                     .SetEase(Ease.OutBack)
        );

        // Movimiento curvo hacia el objetivo
        activeSequence.Join(
            transform.DOPath(
                new Vector3[] { startPosition, midPoint, endPosition },
                moveDuration,
                PathType.CatmullRom
            )
            .SetEase(Ease.InCubic)
            .OnComplete(PlayArrivalParticle)
        );

        // Rebote visual al llegar al destino
        activeSequence.Append(
            transform.DOScale(arrivalBounceScale, arrivalBounceDuration)
                     .SetEase(Ease.OutQuad)
        );

        activeSequence.Append(
            transform.DOScale(Vector3.one, arrivalBounceDuration * 0.8f)
                     .SetEase(Ease.OutBack)
        );

        activeSequence.OnComplete(OnReachedTarget);
        activeSequence.OnKill(() => activeSequence = null);
    }

    #endregion

    #region Particles

    /// <summary>
    /// Instancia y reproduce la partícula de aparición en la posición actual.
    /// La partícula se destruye automáticamente al finalizar.
    /// </summary>
    private void PlaySpawnParticle()
    {
        if (spawnParticle == null)
        {
            return;
        }

        ParticleSystem particle = Instantiate(
            spawnParticle,
            transform.position,
            Quaternion.identity
        );

        particle.Play();
        Destroy(particle.gameObject, particle.main.duration + 1f);
    }

    /// <summary>
    /// Instancia y reproduce la partícula de llegada en la posición final.
    /// La partícula se destruye automáticamente al finalizar.
    /// </summary>
    private void PlayArrivalParticle()
    {
        if (arrivalParticle == null)
        {
            return;
        }

        ParticleSystem particle = Instantiate(
            arrivalParticle,
            transform.position,
            Quaternion.identity
        );

        particle.Play();
        Destroy(particle.gameObject, particle.main.duration + 1f);
    }

    #endregion

    #region Completion

    /// <summary>
    /// Maneja la finalización de la animación.
    /// Notifica el valor de la recompensa y destruye el objeto de la moneda.
    /// </summary>
    private void OnReachedTarget()
    {
        ReachedTarget?.Invoke(rewardValue);
        Destroy(gameObject);
    }

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Garantiza que cualquier tween activo sea eliminado
    /// si el objeto es destruido antes de finalizar la animación.
    /// Previene errores de referencia nula en DOTween.
    /// </summary>
    private void OnDestroy()
    {
        if (activeSequence != null && activeSequence.IsActive())
        {
            activeSequence.Kill();
        }
    }

    #endregion
}
