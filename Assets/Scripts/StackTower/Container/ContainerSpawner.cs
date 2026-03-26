using UnityEngine;

/// <summary>
/// Responsable de la creación de contenedores y de la notificación a sistemas dependientes.
/// Coordina la inicialización, el flujo de spawn y la integración con la garra, monedas y branding.
/// </summary>
public class ContainerSpawner : MonoBehaviour
{
    #region Inspector

    [Header("Referencias")]

    [SerializeField]
    [Tooltip("Prefab del contenedor que será instanciado.")]
    private GameObject containerPrefab;

    [SerializeField]
    [Tooltip("Punto en el mundo donde se generarán los contenedores.")]
    private Transform spawnPoint;

    [SerializeField]
    [Tooltip("Referencia al controlador de la garra para coordinar su comportamiento.")]
    private ClawController claw;

    [SerializeField]
    [Tooltip("Componente encargado de agarrar automáticamente el contenedor generado.")]
    private ClawGrabber grabber;

    [SerializeField]
    [Tooltip("Sistema encargado de generar monedas asociadas a los contenedores.")]
    private CoinSpawner coinSpawner;

    [SerializeField]
    [Tooltip("Referencia a la mecánica principal para validar el estado del juego.")]
    private StackTowerGameplayMechanic gameplayMechanic;

    #endregion

    #region Unity

    /// <summary>
    /// Suscribe el sistema al evento de primera colisión de contenedores.
    /// </summary>
    private void OnEnable()
    {
        Container.OnFirstCollision += HandleContainerCollision;
    }

    /// <summary>
    /// Desuscribe el sistema del evento para evitar referencias inválidas.
    /// </summary>
    private void OnDisable()
    {
        Container.OnFirstCollision -= HandleContainerCollision;
    }

    /// <summary>
    /// Inicializa el flujo de juego generando el primer contenedor.
    /// </summary>
    private void Start()
    {
        SpawnInitial();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Genera el primer contenedor y lo asigna inmediatamente a la garra.
    /// </summary>
    private void SpawnInitial()
    {
        GameObject container = SpawnInternal();

        if (container == null) return;

        grabber.ForceGrab(container);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Maneja la primera colisión de un contenedor, iniciando la secuencia de salida de la garra.
    /// </summary>
    /// <param name="container">Contenedor que ha colisionado.</param>
    private void HandleContainerCollision(Container container)
    {
        claw.StartExitSequence();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Genera un contenedor si el estado del juego lo permite.
    /// Método utilizado por la garra para solicitar nuevas piezas.
    /// </summary>
    /// <returns>Instancia del contenedor generado o null si el juego ha terminado.</returns>
    public GameObject Spawn()
    {
        if (gameplayMechanic != null && gameplayMechanic.IsGameOver)
            return null;

        return SpawnInternal();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Lógica interna de instanciación del contenedor.
    /// Aplica branding visual y notifica a sistemas dependientes.
    /// </summary>
    /// <returns>Instancia del contenedor creado.</returns>
    private GameObject SpawnInternal()
    {
        GameObject container = Instantiate(
            containerPrefab,
            spawnPoint.position,
            Quaternion.identity
        );

        var containerComponent = container.GetComponent<Container>();

        if (coinSpawner != null && containerComponent != null)
        {
            coinSpawner.OnContainerSpawned(containerComponent);
        }

        return container;
    }

    #endregion
}