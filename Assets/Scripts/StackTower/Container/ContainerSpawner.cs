using UnityEngine;

/// <summary>
/// Responsable de la creación de contenedores y de la coordinación con sistemas dependientes.
/// </summary>
public class ContainerSpawner : MonoBehaviour
{
    #region Inspector

    /// <summary>
    /// Prefab del contenedor que será instanciado.
    /// </summary>
    [SerializeField]
    [Tooltip("Prefab del contenedor que será instanciado.")]
    private GameObject containerPrefab;

    /// <summary>
    /// Punto en el mundo donde se generan los contenedores.
    /// </summary>
    [SerializeField]
    [Tooltip("Transform que define la posición de spawn de los contenedores.")]
    private Transform spawnPoint;

    /// <summary>
    /// Referencia al controlador de la garra.
    /// </summary>
    [SerializeField]
    [Tooltip("Controlador encargado del comportamiento de la garra.")]
    private ClawController claw;

    /// <summary>
    /// Sistema encargado del agarre de contenedores.
    /// </summary>
    [SerializeField]
    [Tooltip("Componente responsable de agarrar y soltar contenedores.")]
    private ClawGrabber grabber;

    /// <summary>
    /// Referencia a la mecánica principal del juego.
    /// </summary>
    [SerializeField]
    [Tooltip("Referencia a la mecánica principal para validar el estado del juego.")]
    private StackTowerGameplayMechanic gameplayMechanic;

    /// <summary>
    /// Sistema encargado de generar monedas.
    /// </summary>
    [SerializeField]
    [Tooltip("Sistema responsable de generar monedas al crear contenedores.")]
    private CoinSpawner coinSpawner;

    #endregion

    #region Unity

    /// <summary>
    /// Suscribe el manejador al evento de primera colisión de contenedores.
    /// </summary>
    private void OnEnable()
    {
        Container.OnFirstCollision += HandleContainerCollision;
    }

    /// <summary>
    /// Desuscribe el manejador del evento para evitar referencias inválidas.
    /// </summary>
    private void OnDisable()
    {
        Container.OnFirstCollision -= HandleContainerCollision;
    }

    /// <summary>
    /// Inicializa el sistema generando el primer contenedor.
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

        if (container != null)
            grabber.ForceGrab(container);
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Maneja la primera colisión de un contenedor.
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
    /// </summary>
    /// <returns>Instancia del contenedor generado o null si el juego ha terminado.</returns>
    public GameObject Spawn()
    {
        if (gameplayMechanic != null && gameplayMechanic.IsGameOver)
            return null;

        return SpawnInternal();
    }

    #endregion

    #region Core

    /// <summary>
    /// Instancia un contenedor y notifica al sistema de monedas.
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