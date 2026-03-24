using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Responsable de orquestar el spawn, animación y aplicación lógica
/// de recompensas visuales en forma de monedas, desde una posición
/// en el mundo hasta un objetivo en la UI.
/// </summary>
/// <remarks>
/// Esta clase actúa únicamente como coordinador visual y de flujo.
/// No contiene lógica de economía más allá de la distribución
/// de valores entre monedas visuales.
/// </remarks>
public class CoinRewardSpawner : MonoBehaviour
{
    #region Serialized References

    /// <summary>
    /// Prefab visual de la moneda que será instanciada en el mundo.
    /// </summary>
    [Header("Configuración Visual")]
    [SerializeField] private CoinRewardView coinPrefab;

    /// <summary>
    /// Transform objetivo en la UI al cual las monedas se desplazan.
    /// </summary>
    [SerializeField] private Transform uiCoinTarget;

    /// <summary>
    /// Controlador responsable de reflejar cambios de moneda en la UI.
    /// </summary>
    [Header("UI")]
    [SerializeField] private CurrencyUIController currencyUIController;

    #endregion

    #region Public API

    /// <summary>
    /// Genera monedas visuales en una posición del mundo,
    /// las dispersa dentro de un radio y distribuye la recompensa total
    /// entre ellas de forma equilibrada.
    /// </summary>
    /// <param name="worldPosition">
    /// Posición base en el mundo desde donde se generan las monedas.
    /// </param>
    /// <param name="visualCoinsCount">
    /// Cantidad de monedas visuales a instanciar.
    /// </param>
    /// <param name="totalReward">
    /// Recompensa total que se repartirá entre las monedas.
    /// </param>
    /// <param name="spawnRadius">
    /// Radio máximo de dispersión inicial de las monedas.
    /// </param>
    public void SpawnCoins(
        Vector3 worldPosition,
        int visualCoinsCount,
        int totalReward,
        float spawnRadius)
    {
        if (visualCoinsCount <= 0 || totalReward <= 0)
        {
            return;
        }

        List<int> distributedRewards = DistributeReward(
            visualCoinsCount,
            totalReward
        );

        for (int i = 0; i < visualCoinsCount; i++)
        {
            Vector3 spawnPosition = worldPosition + GetRandomOffset(spawnRadius);

            CoinRewardView coin = Instantiate(
                coinPrefab,
                spawnPosition,
                Quaternion.identity,
                transform
            );

            coin.Initialize(new Vector3(uiCoinTarget.position.x, uiCoinTarget.position.y+ 1.386f, 0f), distributedRewards[i]);
            coin.ReachedTarget += OnCoinReachedTarget;
        }
    }

    #endregion

    #region Coin Callbacks

    /// <summary>
    /// Callback ejecutado cuando una moneda visual alcanza su destino en la UI.
    /// Aplica tanto la recompensa lógica como la animación visual correspondiente.
    /// </summary>
    /// <param name="reward">
    /// Valor de la recompensa asociada a la moneda.
    /// </param>
    private void OnCoinReachedTarget(int reward)
    {
        GameManager.Instance.CurrencyManager.AddCoins(reward);
        currencyUIController.AddCoinsAnimated(reward);
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Genera un desplazamiento aleatorio bidimensional
    /// dentro de un radio circular especificado.
    /// </summary>
    /// <param name="radius">
    /// Radio máximo del desplazamiento.
    /// </param>
    /// <returns>
    /// Vector de desplazamiento aleatorio en el plano XY.
    /// </returns>
    private Vector3 GetRandomOffset(float radius)
    {
        Vector2 random = Random.insideUnitCircle * radius;
        return new Vector3(random.x, random.y, 0f);
    }

    /// <summary>
    /// Distribuye una recompensa total entre un número fijo
    /// de monedas visuales de forma uniforme.
    /// </summary>
    /// <remarks>
    /// Si el total no es divisible exactamente,
    /// el residuo se reparte incrementando en uno
    /// el valor de las primeras monedas.
    /// </remarks>
    /// <param name="coinCount">
    /// Número de monedas visuales.
    /// </param>
    /// <param name="totalReward">
    /// Recompensa total a distribuir.
    /// </param>
    /// <returns>
    /// Lista de valores individuales para cada moneda.
    /// </returns>
    private List<int> DistributeReward(int coinCount, int totalReward)
    {
        List<int> result = new List<int>(coinCount);

        int baseValue = totalReward / coinCount;
        int remainder = totalReward % coinCount;

        for (int i = 0; i < coinCount; i++)
        {
            int value = baseValue;

            if (remainder > 0)
            {
                value++;
                remainder--;
            }

            result.Add(value);
        }

        return result;
    }

    #endregion
}
