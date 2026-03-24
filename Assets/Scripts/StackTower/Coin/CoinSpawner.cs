using UnityEngine;

/// <summary>
/// Se encarga de generar monedas en función de los containers.
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private GameObject coinPrefab;

    [Header("Configuración")]
    [SerializeField] private int containersPerSpawn = 3;

    [Header("Posición relativa")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.5f, 0f);

    [SerializeField] private StackTowerGameplayMechanic gameplayMechanic;
    private int containerCounter = 0;

    /// <summary>
    /// Llamado cuando se crea un container.
    /// </summary>
    public void OnContainerSpawned(Container container)
    {
         if (!GameManager.Instance.IsAdsEnabled)
        {
            return;
        }
        if (gameplayMechanic != null && gameplayMechanic.IsGameOver)
        return;

        containerCounter++;

        if (containerCounter >= containersPerSpawn)
        {
            SpawnCoin(container);
            containerCounter = 0;
        }
    }

    private void SpawnCoin(Container container)
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("CoinSpawner: coinPrefab no asignado");
            return;
        }
        if (Random.Range(0, 1) == 0)
        {
            localOffset = Vector3.Scale( new Vector3(-1f,1f,1f) , localOffset);
        }
        Vector3 worldPosition = container.transform.position + localOffset;

        GameObject coin = Instantiate(
            coinPrefab,
            worldPosition,
            Quaternion.identity
        );

        coin.transform.SetParent(container.transform);
    }
}