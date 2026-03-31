using UnityEngine;

/// <summary>
/// Implementación concreta del sistema de recompensas de gameplay basada en monedas.
/// 
/// Este componente se encarga de:
/// - Generar monedas visuales en el mundo.
/// - Contabilizar recompensas lógicas (monedas reales).
/// - Manejar recompensas por acción y por finalización.
/// 
/// Implementa <see cref="IGameplayRewardHandler"/> para permitir
/// una integración desacoplada con otros sistemas de gameplay.
/// </summary>
public class CoinGameplayRewardHandler : MonoBehaviour, IGameplayRewardHandler
{
    #region Serialized References

    /// <summary>
    /// Spawner responsable de instanciar monedas visuales en el mundo.
    /// Gestiona tanto la animación como la distribución espacial.
    /// </summary>
    [SerializeField] private CoinRewardSpawner coinRewardSpawner;

    #endregion

    #region Reward Configuration

    [Header("Action Reward")]

    /// <summary>
    /// Cantidad de monedas lógicas otorgadas por cada acción.
    /// </summary>
    [SerializeField] private int coinsRewardedPerAction = 2;

    /// <summary>
    /// Cantidad de monedas visuales generadas por acción.
    /// No necesariamente coincide con las monedas lógicas.
    /// </summary>
    [SerializeField] private int visualCoinsAction = 1;

    /// <summary>
    /// Radio de dispersión de las monedas visuales por acción.
    /// </summary>
    [SerializeField] private float actionRewardRadius = 0.3f;

    [Header("Completion Reward")]

    /// <summary>
    /// Cantidad de monedas visuales generadas al completar.
    /// </summary>
    [SerializeField] private int visualCoinsComplete = 10;

    /// <summary>
    /// Radio de dispersión de las monedas visuales de finalización.
    /// </summary>
    [SerializeField] private float completedRewardRadius = 2.5f;

    #endregion

    #region Private State

    /// <summary>
    /// Contador interno de monedas lógicas acumuladas
    /// durante la sesión de gameplay.
    /// </summary>
    private int totalCoinsCollected;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {        
        ResolveRewardConfiguration();
    }

    #endregion

    #region Branding configuration

    /// <summary>
    /// Inicializa la configuración de recompensas desde el sistema de branding.
    /// Usa valores locales como fallback.
    /// </summary>
    private void ResolveRewardConfiguration()
    {
        var branding = BrandingManager.Instance;

        if (branding == null)
            return;

        int value = branding.CoinsPerAction;

        if (value > 0)
        {
            coinsRewardedPerAction = value;
            visualCoinsAction = value;
        }
    }

    #endregion

    #region IGameplayRewardHandler Implementation

    /// <summary>
    /// Maneja la recompensa asociada a una acción de gameplay.
    /// </summary>
    public void HandleActionReward(Vector3 worldPosition)
    {
        coinRewardSpawner.SpawnCoins(
            worldPosition,
            visualCoinsAction,
            coinsRewardedPerAction,
            actionRewardRadius
        );

        totalCoinsCollected += coinsRewardedPerAction;
    }

    /// <summary>
    /// Devuelve el total de monedas lógicas acumuladas.
    /// </summary>
    public int GetTotalReward()
    {
        return totalCoinsCollected;
    }

    #endregion
}