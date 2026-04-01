using UnityEngine;

/// <summary>
/// Implementación concreta del sistema de recompensas basada en monedas.
/// Permite configuración dinámica desde BrandingManager sin depender del ciclo de vida de Unity.
/// </summary>
public class CoinGameplayRewardHandler : MonoBehaviour, IGameplayRewardHandler
{
    #region Serialized References

    /// <summary>
    /// Spawner responsable de instanciar monedas visuales.
    /// </summary>
    [SerializeField] private CoinRewardSpawner coinRewardSpawner;

    #endregion

    #region Reward Configuration

    [Header("Action Reward")]

    [SerializeField]
    [Tooltip("Monedas lógicas otorgadas por acción.")]
    private int coinsRewardedPerAction = 2;

    [SerializeField]
    [Tooltip("Monedas visuales generadas por acción.")]
    private int visualCoinsAction = 1;

    [SerializeField]
    [Tooltip("Radio de dispersión visual.")]
    private float actionRewardRadius = 0.3f;

    [Header("Completion Reward")]

    [SerializeField] private int visualCoinsComplete = 10;
    [SerializeField] private float completedRewardRadius = 2.5f;

    #endregion

    #region Private State

    private int totalCoinsCollected;

    /// <summary>
    /// Evita múltiples configuraciones redundantes.
    /// </summary>
    private bool isConfigured;

    #endregion

    #region Unity Lifecycle

    private void OnEnable()
    {
        BrandingManager.OnBrandingReady += ResolveRewardConfiguration;
    }

    private void OnDisable()
    {
        BrandingManager.OnBrandingReady -= ResolveRewardConfiguration;
    }

    #endregion

    #region Branding Configuration

    /// <summary>
    /// Sincroniza configuración desde BrandingManager.
    /// Puede llamarse manualmente o vía evento.
    /// </summary>
    public void ResolveRewardConfiguration()
    {
        if (isConfigured)
            return;

        var branding = BrandingManager.Instance;

        if (branding == null)
            return;

        int value = branding.CoinsPerAction;

        if (value > 0)
        {
            coinsRewardedPerAction = value;
            visualCoinsAction = value;
        }

        isConfigured = true;
    }

    #endregion

    #region IGameplayRewardHandler

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

    public int GetTotalReward()
    {
        return totalCoinsCollected;
    }

    #endregion
}