using UnityEngine;

/// <summary>
/// Genera monedas asociadas a la creación de contenedores.
/// 
/// Este sistema no depende de físicas ni de colisiones, sino del momento de spawn,
/// lo que garantiza un comportamiento determinista y controlado.
/// 
/// La lógica de frecuencia está desacoplada mediante BrandingManager,
/// permitiendo configuración dinámica sin modificar código.
/// </summary>
public class CoinSpawner : MonoBehaviour
{
    #region Inspector

    [Header("Prefab")]

    [SerializeField]
    [Tooltip("Prefab de la moneda.")]
    private GameObject coinPrefab;

    [Header("Offset de spawn")]

    /// <summary>
    /// Distancia horizontal respecto al contenedor.
    /// Se aplica con dirección aleatoria para distribuir monedas a izquierda o derecha.
    /// </summary>
    [SerializeField]
    private float horizontalDistance = 1.5f;

    /// <summary>
    /// Offset vertical fijo para posicionar la moneda por encima del contenedor.
    /// </summary>
    [SerializeField]
    private float verticalOffset = 1.5f;

    [Header("Dependencias")]

    /// <summary>
    /// Referencia al estado del gameplay para evitar generar monedas en GameOver.
    /// </summary>
    [SerializeField]
    private StackTowerGameplayMechanic gameplayMechanic;

    [Header("Opcional")]

    /// <summary>
    /// Permite condicionar el spawn a la activación de Ads.
    /// 
    /// Nota de diseño:
    /// Esto desacopla la lógica de monetización del sistema principal,
    /// evitando hardcodear dependencias globales.
    /// </summary>
    [SerializeField]
    private bool requireAdsEnabled = false;

    #endregion

    #region State

    /// <summary>
    /// Contador interno de contenedores generados desde el último spawn de moneda.
    /// </summary>
    private int containerCounter = 0;

    #endregion

    #region Public API

    /// <summary>
    /// Método invocado por ContainerSpawner al crear un nuevo contenedor.
    /// 
    /// Se encarga de:
    /// - Validar condiciones de gameplay
    /// - Acumular progreso
    /// - Determinar si corresponde generar una moneda
    /// </summary>
    public void OnContainerSpawned(Container container)
    {
        if (container == null) return;

        /// Evita generar monedas si la monetización no está activa
        if (requireAdsEnabled && !GameManager.Instance.IsAdsEnabled)
            return;

        /// Evita generar monedas cuando el juego ha terminado
        if (gameplayMechanic != null && gameplayMechanic.IsGameOver)
            return;

        containerCounter++;

        /// Obtiene configuración dinámica desde Branding
        int required = BrandingManager.Instance != null
            ? BrandingManager.Instance.GetContainersPerCoin()
            : 3;

        /// Cuando se alcanza el umbral, se genera moneda
        if (containerCounter >= required)
        {
            SpawnCoin(container);
            containerCounter = 0;
        }
    }

    #endregion

    #region Core

    /// <summary>
    /// Instancia una moneda relativa al contenedor.
    /// 
    /// Decisiones de diseño:
    /// - Posición relativa: asegura coherencia visual con el container
    /// - Dirección aleatoria: mejora distribución y legibilidad
    /// - Parenting: mantiene sincronización si el contenedor se mueve
    /// </summary>
    private void SpawnCoin(Container container)
    {
        if (coinPrefab == null) return;

        /// Selección de lado (izquierda/derecha)
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

        /// Se vincula al contenedor para mantener coherencia espacial
        coin.transform.SetParent(container.transform);
    }

    #endregion
}