using UnityEngine;
using System.Collections.Generic;

public class CloudSystem : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject cloudPrefab;

    [Header("Referencia cámara")]
    [SerializeField] private Transform cameraTransform;

    [Header("Cantidad inicial")]
    [SerializeField] private int initialClouds = 12;

    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Header("Rango horizontal")]
    [SerializeField] private float minX = -10f;
    [SerializeField] private float maxX = 10f;

    [Header("Rango vertical inicial")]
    [SerializeField] private float minY = 2f;
    [SerializeField] private float maxY = 8f;

    [Header("Escala aleatoria")]
    [SerializeField] private float minScale = 0.8f;
    [SerializeField] private float maxScale = 1.5f;

    [Header("Spawn dinámico vertical")]
    [SerializeField] private int cloudsPerBatch = 3;
    [SerializeField] private float dynamicHeight = 10f;

    [Header("Reciclaje")]
    [SerializeField] private float recycleOffset = 2f;

    [Header("Spawn seguro (fuera de cámara)")]
    [SerializeField] private float safeSpawnOffset = 2f;

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
        InitializePool();
        lastSpawnedY = maxY;
    }

    private void Update()
    {
        MoveClouds();
        HandleVerticalRecycling();
    }

    // 🔹 Inicializa pool (solo una vez)
    private void InitializePool()
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

    // 🔹 Movimiento horizontal infinito
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

    // 🔹 Control vertical automático
    private void HandleVerticalRecycling()
    {
        if (GetCameraTop() > lastSpawnedY - dynamicHeight)
        {
            RecycleCloudsAbove();
        }
    }

    // 🔥 RECICLAJE CORRECTO (NUNCA EN CÁMARA)
    private void RecycleCloudsAbove()
    {
        float cameraTop = GetCameraTop();
        float cameraBottom = GetCameraBottom();

        float spawnMinY = cameraTop + safeSpawnOffset;
        float spawnMaxY = spawnMinY + dynamicHeight;

        int recycled = 0;

        foreach (var cloudData in clouds)
        {
            // 🔻 Solo si está completamente fuera por abajo
            if (cloudData.transform.position.y < cameraBottom - recycleOffset)
            {
                float newY = Random.Range(spawnMinY, spawnMaxY);

                cloudData.transform.position = new Vector3(
                    Random.Range(minX, maxX),
                    newY,
                    cloudData.transform.position.z
                );

                cloudData.minY = spawnMinY;
                cloudData.maxY = spawnMaxY;

                ApplyRandomScale(cloudData.transform);
                ApplyRandomSprite(cloudData.transform);

                recycled++;

                if (recycled >= cloudsPerBatch)
                    break;
            }
        }

        lastSpawnedY = spawnMaxY;
    }

    // 🔹 Cámara límites
    private float GetCameraTop()
    {
        Camera cam = Camera.main;
        return cam.transform.position.y + cam.orthographicSize;
    }

    private float GetCameraBottom()
    {
        Camera cam = Camera.main;
        return cam.transform.position.y - cam.orthographicSize;
    }

    // 🔹 Setup inicial
    private void SetupCloud(GameObject cloud, float minY, float maxY)
    {
        Transform t = cloud.transform;

        ApplyRandomScale(t);
        ApplyRandomSprite(t);

        clouds.Add(new CloudData(t, minY, maxY));
    }

    private void ApplyRandomScale(Transform cloud)
    {
        float scale = Random.Range(minScale, maxScale);
        cloud.localScale = Vector3.one * scale;
    }

    private void ApplyRandomSprite(Transform cloud)
    {
        var randomSprite = cloud.GetComponent<CloudRandomSprite>();
        if (randomSprite != null)
        {
            randomSprite.RefreshSprite();
        }
    }
}