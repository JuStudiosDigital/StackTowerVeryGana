using UnityEngine;
using System.Collections.Generic;

public class CloudSystem : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject cloudPrefab;

    [Header("Cantidad inicial")]
    [SerializeField] private int initialClouds = 5;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Rango inicial")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;
    [SerializeField] private float minY = 2f;
    [SerializeField] private float maxY = 8f;

    [Header("Escala aleatoria")]
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.5f;

    [Header("Spawn dinámico")]
    [SerializeField] private int cloudsPerBatch = 3;
    [SerializeField] private float dynamicHeight = 10f;

    // 🔥 Clase interna para guardar rango de cada nube
    private class CloudData
    {
        public Transform transform;
        public float minY;
        public float maxY;

        public CloudData(Transform t, float minY, float maxY)
        {
            transform = t;
            this.minY = minY;
            this.maxY = maxY;
        }
    }

    private readonly List<CloudData> clouds = new();

    private float lastSpawnedY;

    private void Start()
    {
        SpawnInitialClouds();
        lastSpawnedY = maxY;
    }

    private void Update()
    {
        MoveClouds();
    }

    // 🔹 Spawn inicial
    private void SpawnInitialClouds()
    {
        for (int i = 0; i < initialClouds; i++)
        {
            Vector3 pos = new Vector3(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY),
                0f
            );

            GameObject cloud = Instantiate(cloudPrefab, pos, Quaternion.identity, transform);

            SetupCloud(cloud, minY, maxY);
        }
    }

    // 🔹 Movimiento + reciclaje respetando rango
    private void MoveClouds()
    {
        float movement = moveSpeed * Time.deltaTime;

        foreach (var cloudData in clouds)
        {
            var cloud = cloudData.transform;

            cloud.position += Vector3.left * movement;

            if (cloud.position.x < minX)
            {
                float newY = Random.Range(cloudData.minY, cloudData.maxY);

                cloud.position = new Vector3(
                    maxX,
                    newY,
                    cloud.position.z
                );

                ApplyRandomScale(cloud);
                ApplyRandomSprite(cloud);
            }
        }
    }

    // 🔹 Spawn dinámico por bloques (chunks)
    public void SpawnCloudsAbove(float cameraY)
    {
        float spawnMinY = lastSpawnedY;
        float spawnMaxY = lastSpawnedY + dynamicHeight;

        for (int i = 0; i < cloudsPerBatch; i++)
        {
            float y = Random.Range(spawnMinY, spawnMaxY);

            Vector3 pos = new Vector3(
                Random.Range(minX, maxX),
                y,
                0f
            );

            GameObject cloud = Instantiate(cloudPrefab, pos, Quaternion.identity, transform);

            SetupCloud(cloud, spawnMinY, spawnMaxY);
        }

        lastSpawnedY = spawnMaxY;
    }

    // 🔹 Setup general
    private void SetupCloud(GameObject cloud, float minY, float maxY)
    {
        ApplyRandomScale(cloud.transform);
        ApplyRandomSprite(cloud.transform);

        clouds.Add(new CloudData(cloud.transform, minY, maxY));
    }

    // 🔹 Escala
    private void ApplyRandomScale(Transform cloud)
    {
        float scale = Random.Range(minScale, maxScale);
        cloud.localScale = Vector3.one * scale;
    }

    // 🔹 Sprite
    private void ApplyRandomSprite(Transform cloud)
    {
        var randomSprite = cloud.GetComponent<CloudRandomSprite>();
        if (randomSprite != null)
        {
            randomSprite.RefreshSprite();
        }
    }
}