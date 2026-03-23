using UnityEngine;

/// <summary>
/// Se encarga de generar containers y notificar sistemas dependientes.
/// </summary>
public class ContainerSpawner : MonoBehaviour
{
    [Header("Referencias")]

    [SerializeField] private GameObject containerPrefab;

    [SerializeField] private Transform spawnPoint;

    [SerializeField] private ClawController claw;

    [SerializeField] private ClawGrabber grabber;

    [SerializeField] private CoinSpawner coinSpawner;

    [SerializeField] private StackTowerGameplayMechanic gameplayMechanic;
    private bool firstSpawnDone = false;

    private void OnEnable()
    {
        Container.OnFirstCollision += HandleContainerCollision;
    }

    private void OnDisable()
    {
        Container.OnFirstCollision -= HandleContainerCollision;
    }

    private void Start()
    {
        SpawnInitial();
    }

    /// <summary>
    /// Genera el primer container ya agarrado por la garra.
    /// </summary>
    private void SpawnInitial()
    {
        GameObject container = SpawnInternal();

        if (container == null) return;

        grabber.ForceGrab(container);

        firstSpawnDone = true;
    }

    private void HandleContainerCollision(Container container)
    {
        claw.StartExitSequence();
    }

    /// <summary>
    /// Spawn estándar usado por la garra.
    /// </summary>
    public GameObject Spawn()
    {
        // Referencia al mechanic (inyectada)
        if (gameplayMechanic != null && gameplayMechanic.IsGameOver)
        return null;

        return SpawnInternal();
    }

    /// <summary>
    /// Lógica interna de spawn reutilizable.
    /// </summary>
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
}