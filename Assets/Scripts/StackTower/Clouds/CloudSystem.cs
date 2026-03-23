using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de nubes con reciclaje horizontal y vertical.
/// Optimizado para evitar instanciación constante.
/// </summary>
public class CloudSystem : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject cloudPrefab;

    [Header("Referencia cámara")]
    [SerializeField] private Camera targetCamera;

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

    /// <summary>
    /// Datos internos de cada nube.
    /// </summary>
    private struct CloudData
    {
        public Transform transform;
        public float minY;
        public float maxY;

        public CloudData(Transform transform, float minY, float maxY)
        {
            this.transform = transform;
            this.minY = minY;
            this.maxY = maxY;
        }
    }

    private readonly List<CloudData> clouds = new();

    private float lastSpawnedY;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

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

    /// <summary>
    /// Inicializa el pool de nubes.
    /// </summary>
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

    /// <summary>
    /// Movimiento horizontal continuo.
    /// </summary>
    private void MoveClouds()
    {
        float movement = moveSpeed * Time.deltaTime;

        for (int i = 0; i < clouds.Count; i++)
        {
            var cloudData = clouds[i];
            var cloud = cloudData.transform;

            cloud.position += Vector3.left * movement;

            if (cloud.position.x < minX)
            {
                float newY = Random.Range(cloudData.minY, cloudData.maxY);

                cloud.position = new Vector3(maxX, newY, cloud.position.z);

                ApplyRandomScale(cloud);
                ApplyRandomSprite(cloud);
            }
        }
    }

    /// <summary>
    /// Controla reciclaje vertical según la cámara.
    /// </summary>
    private void HandleVerticalRecycling()
    {
        if (GetCameraTop() > lastSpawnedY - dynamicHeight)
        {
            RecycleCloudsAbove();
        }
    }

    /// <summary>
    /// Recicla nubes fuera de pantalla hacia arriba.
    /// </summary>
    private void RecycleCloudsAbove()
    {
        float cameraTop = GetCameraTop();
        float cameraBottom = GetCameraBottom();

        float spawnMinY = cameraTop + safeSpawnOffset;
        float spawnMaxY = spawnMinY + dynamicHeight;

        int recycled = 0;

        for (int i = 0; i < clouds.Count; i++)
        {
            var cloudData = clouds[i];

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

                clouds[i] = cloudData;

                recycled++;

                if (recycled >= cloudsPerBatch)
                    break;
            }
        }

        lastSpawnedY = spawnMaxY;
    }

    private float GetCameraTop()
    {
        return targetCamera.transform.position.y + targetCamera.orthographicSize;
    }

    private float GetCameraBottom()
    {
        return targetCamera.transform.position.y - targetCamera.orthographicSize;
    }

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