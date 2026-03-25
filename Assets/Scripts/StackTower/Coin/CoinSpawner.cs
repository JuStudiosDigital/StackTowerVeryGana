using UnityEngine;

/// <summary>
/// Responsable de generar monedas en función de la cantidad de contenedores creados.
/// Controla la frecuencia de aparición y posiciona las monedas relativas al contenedor.
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    #region Inspector

    [Header("Referencia")]

    [SerializeField]
    [Tooltip("Prefab de la moneda que será instanciada.")]
    private GameObject coinPrefab;

    [Header("Configuración")]

    [SerializeField]
    [Tooltip("Cantidad de contenedores necesarios para generar una moneda.")]
    private int containersPerSpawn = 3;

    [Header("Posición relativa")]

    [SerializeField]
    [Tooltip("Offset local aplicado desde la posición del contenedor para ubicar la moneda.")]
    private Vector3 localOffset = new Vector3(0f, 1.5f, 0f);

    [SerializeField]
    [Tooltip("Referencia a la mecánica principal para validar estado de juego.")]
    private StackTowerGameplayMechanic gameplayMechanic;

    #endregion

    #region State

    /// <summary>
    /// Contador interno de contenedores generados desde el último spawn de moneda.
    /// </summary>
    private int containerCounter = 0;

    #endregion

    #region Public API

    /// <summary>
    /// Notifica al sistema que un nuevo contenedor ha sido generado.
    /// Incrementa el contador y evalúa si corresponde generar una moneda.
    /// </summary>
    /// <param name="container">Contenedor recientemente creado.</param>
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

    #endregion

    #region Private Methods

    /// <summary>
    /// Instancia una moneda en relación a la posición del contenedor
    /// y la vincula jerárquicamente a este.
    /// </summary>
    /// <param name="container">Contenedor base para posicionamiento.</param>
    private void SpawnCoin(Container container)
    {
        if (coinPrefab == null)
        {
            Debug.LogWarning("CoinSpawner: coinPrefab no asignado");
            return;
        }

        if (Random.Range(0, 1) == 0)
        {
            localOffset = Vector3.Scale(new Vector3(-1f, 1f, 1f), localOffset);
        }

        Vector3 worldPosition = container.transform.position + localOffset;

        GameObject coin = Instantiate(
            coinPrefab,
            worldPosition,
            Quaternion.identity
        );

        coin.transform.SetParent(container.transform);
    }

    #endregion
}