using UnityEngine;

public class ContainerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject containerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private ClawController claw;
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
        Spawn();
    }

    private void HandleContainerCollision(Container container)
{
    claw.StartExitSequence();
}

    public GameObject Spawn()
{
    if (GameManagerStackTower.IsGameOver) return null;

    return Instantiate(containerPrefab, spawnPoint.position, Quaternion.identity);
}

}