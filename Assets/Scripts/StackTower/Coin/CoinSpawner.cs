using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Referencia")]
    [SerializeField] private GameObject coinPrefab;

    [Header("Configuración")]
    [SerializeField] private int containersPerSpawn = 3;

    [Header("Posición relativa")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.5f, 0f);

    private int containerCounter = 0;

    // 👉 Este método lo llama ContainerSpawner al crear el container
    public void OnContainerSpawned(Container container)
    {
        if (GameManagerStackTower.IsGameOver) return;

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

        Vector3 worldPosition = container.transform.position + localOffset;

        GameObject coin = Instantiate(
            coinPrefab,
            worldPosition,
            Quaternion.identity
        );

        // 🔗 se mueve con el container
        coin.transform.SetParent(container.transform);
    }
}