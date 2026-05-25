using UnityEngine;

/// <summary>
/// Genera monedas asociadas a la creación de contenedores.
/// Respeta la configuración dinámica de BrandingManager,
/// donde 0 desactiva completamente el spawn de monedas.
/// </summary>
public sealed class CoinSpawner : MonoBehaviour
{
    #region Inspector

    [Header("Prefab")]

    [SerializeField]
    [Tooltip("Prefab de la moneda que se instanciará junto al contenedor.")]
    private GameObject coinPrefab;

    [Header("Offset de spawn")]

    [SerializeField]
    [Tooltip("Distancia horizontal respecto al contenedor. Se aplica hacia izquierda o derecha aleatoriamente.")]
    private float horizontalDistance = 0f;

    [SerializeField]
    [Tooltip("Offset vertical respecto al contenedor.")]
    private float verticalOffset = 0f;

    [Header("Dependencias")]

    [SerializeField]
    [Tooltip("Referencia al estado del gameplay para evitar generar monedas después del Game Over.")]
    private StackTowerGameplayMechanic gameplayMechanic;

    [Header("Opcional")]

    [SerializeField]
    [Tooltip("Si está activo, solo genera monedas cuando los Ads están habilitados.")]
    private bool requireAdsEnabled = false;

    #endregion

    #region State

    private int containerCounter;

    #endregion

    #region Public API

    /// <summary>
    /// Evalúa si corresponde generar una moneda cuando se crea un nuevo contenedor.
    /// </summary>
    /// <param name="container">Contenedor recién generado.</param>
    public void OnContainerSpawned(Container container)
    {
        if (container == null)
            return;

        if (coinPrefab == null)
            return;

        if (requireAdsEnabled && !GameManager.Instance.IsAdsEnabled)
            return;

        if (gameplayMechanic != null && gameplayMechanic.IsGameOver)
            return;

        int requiredContainersPerCoin = BrandingManager.Instance != null
            ? BrandingManager.Instance.GetContainersPerCoin()
            : 0;

        if (requiredContainersPerCoin <= 0)
        {
            containerCounter = 0;
            return;
        }

        containerCounter++;

        if (containerCounter < requiredContainersPerCoin)
            return;

        SpawnCoin(container);
        containerCounter = 0;
    }

    #endregion

    #region Core

    /// <summary>
    /// Instancia una moneda relativa al contenedor y la mantiene como hija
    /// para conservar coherencia espacial durante el movimiento.
    /// </summary>
    /// <param name="container">Contenedor usado como referencia de spawn.</param>
    private void SpawnCoin(Container container)
    {
        float direction = Random.value > 0.5f ? 1f : -1f;

        Vector3 offset = new Vector3(
            horizontalDistance * direction,
            verticalOffset,
            0f
        );

        Vector3 position = container.transform.position + offset;

        GameObject coin = Instantiate(
            coinPrefab,
            position,
            Quaternion.identity
        );

        coin.transform.SetParent(container.transform);
    }

    #endregion
}