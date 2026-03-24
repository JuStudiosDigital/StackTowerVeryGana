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

    /// <summary>
    /// Configuración de recompensas otorgadas durante acciones intermedias.
    /// </summary>
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

    /// <summary>
    /// Configuración de recompensas otorgadas al completar un objetivo.
    /// </summary>
    [Header("Completion Reward")]


    #endregion

    #region Private State

    /// <summary>
    /// Contador interno de monedas lógicas acumuladas
    /// durante la sesión de gameplay.
    /// </summary>
    private int totalCoinsCollected;



    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa las dependencias internas del sistema de recompensas.
    /// 
    /// Convierte de forma segura el comportamiento serializado
    /// a la interfaz esperada, permitiendo desacoplamiento
    /// y flexibilidad de implementación.
    /// </summary>
    private void Awake()
    {        
        ResolveRewardConfiguration();
    }

    #endregion

    #region Branding configuration

    /// <summary>
    /// Inicializa la configuración de recompensas desde el sistema de recursos.
    /// Usa valores locales como fallback.
    /// </summary>
    private void ResolveRewardConfiguration()
    {
        if (ResourceService.Instance == null)
            return;

        coinsRewardedPerAction = ResourceService.Instance.GetReward(
            rewards => rewards.coins_per_action
        );
    }

    #endregion

    #region IGameplayRewardHandler Implementation

    /// <summary>
    /// Maneja la recompensa asociada a una acción de gameplay.
    /// 
    /// Genera monedas visuales en la posición indicada y
    /// suma la cantidad correspondiente al total lógico.
    /// </summary>
    /// <param name="worldPosition">
    /// Posición en el mundo donde se generará la recompensa visual.
    /// </param>
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
    /// Devuelve el total de monedas lógicas acumuladas
    /// durante la ejecución actual.
    /// </summary>
    /// <returns>
    /// Cantidad total de monedas recolectadas.
    /// </returns>
    public int GetTotalReward()
    {
        return totalCoinsCollected;
    }

    #endregion
}