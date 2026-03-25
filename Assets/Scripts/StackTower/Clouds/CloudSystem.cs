using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema responsable de gestionar un conjunto de nubes reutilizables.
/// Implementa reciclaje horizontal y vertical para evitar instanciación en tiempo de ejecución
/// y mantener un flujo continuo visual alineado con el desplazamiento de cámara.
/// </summary>
public class CloudSystem : MonoBehaviour
{
    #region Inspector

    [Header("Prefab")]

    [SerializeField]
    [Tooltip("Prefab de la nube que será instanciado y reutilizado dentro del sistema.")]
    private GameObject cloudPrefab;

    [Header("Referencia cámara")]

    [SerializeField]
    [Tooltip("Cámara utilizada como referencia para calcular límites visibles y reciclaje vertical.")]
    private Camera targetCamera;

    [Header("Cantidad inicial")]

    [SerializeField]
    [Tooltip("Cantidad inicial de nubes creadas en el pool al iniciar el sistema.")]
    private int initialClouds = 12;

    [Header("Movimiento")]

    [SerializeField]
    [Tooltip("Velocidad constante de desplazamiento horizontal de las nubes.")]
    private float moveSpeed = 1.5f;

    [Header("Rango horizontal")]

    [SerializeField]
    [Tooltip("Límite mínimo en el eje X donde las nubes son recicladas.")]
    private float minX = -10f;

    [SerializeField]
    [Tooltip("Límite máximo en el eje X donde las nubes reaparecen.")]
    private float maxX = 10f;

    [Header("Rango vertical inicial")]

    [SerializeField]
    [Tooltip("Altura mínima inicial para la distribución de nubes.")]
    private float minY = 2f;

    [SerializeField]
    [Tooltip("Altura máxima inicial para la distribución de nubes.")]
    private float maxY = 8f;

    [Header("Escala aleatoria")]

    [SerializeField]
    [Tooltip("Escala mínima aplicada aleatoriamente a cada nube.")]
    private float minScale = 0.8f;

    [SerializeField]
    [Tooltip("Escala máxima aplicada aleatoriamente a cada nube.")]
    private float maxScale = 1.5f;

    [Header("Spawn dinámico vertical")]

    [SerializeField]
    [Tooltip("Cantidad de nubes recicladas por ciclo cuando la cámara avanza verticalmente.")]
    private int cloudsPerBatch = 3;

    [SerializeField]
    [Tooltip("Altura adicional utilizada para generar nuevas nubes fuera de la vista de la cámara.")]
    private float dynamicHeight = 10f;

    [Header("Reciclaje")]

    [SerializeField]
    [Tooltip("Distancia adicional fuera de la pantalla inferior antes de considerar una nube para reciclaje.")]
    private float recycleOffset = 2f;

    [Header("Spawn seguro (fuera de cámara)")]

    [SerializeField]
    [Tooltip("Offset aplicado por encima del límite superior de la cámara para evitar aparición visible.")]
    private float safeSpawnOffset = 2f;

    #endregion

    #region Internal Data

    /// <summary>
    /// Contenedor de datos inmutable por iteración que agrupa referencias necesarias
    /// para evitar llamadas repetidas a componentes durante el ciclo de actualización.
    /// </summary>
    private struct CloudData
    {
        /// <summary>
        /// Transform asociado a la nube.
        /// </summary>
        public Transform transform;

        /// <summary>
        /// Componente encargado de variar el sprite de la nube.
        /// </summary>
        public CloudRandomSprite sprite;

        /// <summary>
        /// Límite inferior de reposicionamiento vertical.
        /// </summary>
        public float minY;

        /// <summary>
        /// Límite superior de reposicionamiento vertical.
        /// </summary>
        public float maxY;

        /// <summary>
        /// Inicializa una nueva instancia de datos de nube.
        /// </summary>
        public CloudData(Transform transform, CloudRandomSprite sprite, float minY, float maxY)
        {
            this.transform = transform;
            this.sprite = sprite;
            this.minY = minY;
            this.maxY = maxY;
        }
    }

    /// <summary>
    /// Lista que contiene todas las nubes activas gestionadas por el sistema.
    /// </summary>
    private readonly List<CloudData> clouds = new();

    /// <summary>
    /// Última altura máxima en la que se generaron nubes dinámicamente.
    /// </summary>
    private float lastSpawnedY;

    #endregion

    #region Unity

    /// <summary>
    /// Inicializa el sistema y configura el estado inicial del pool.
    /// </summary>
    private void Start()
    {
        InitializePool();
        lastSpawnedY = maxY;
    }

    /// <summary>
    /// Ejecuta el ciclo principal de actualización del sistema de nubes.
    /// </summary>
    private void Update()
    {
        MoveClouds();
        HandleVerticalRecycling();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Crea y configura las nubes iniciales dentro del pool.
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
    /// Configura una nube recién creada, aplicando variaciones visuales y almacenando sus datos.
    /// </summary>
    private void SetupCloud(GameObject cloud, float minY, float maxY)
    {
        Transform t = cloud.transform;
        CloudRandomSprite sprite = cloud.GetComponent<CloudRandomSprite>();

        ApplyRandomScale(t);

        if (sprite != null)
            sprite.RefreshSprite();

        clouds.Add(new CloudData(t, sprite, minY, maxY));
    }

    #endregion

    #region Update Loop

    /// <summary>
    /// Aplica desplazamiento horizontal continuo y recicla nubes que salen del límite izquierdo.
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
                ApplyRandomSprite(cloudData);
            }
        }
    }

    /// <summary>
    /// Evalúa si es necesario generar nuevas nubes por avance vertical de la cámara.
    /// </summary>
    private void HandleVerticalRecycling()
    {
        if (GetCameraTop() > lastSpawnedY - dynamicHeight)
        {
            RecycleCloudsAbove();
        }
    }

    /// <summary>
    /// Reposiciona nubes que han salido por la parte inferior hacia una nueva región superior.
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
                ApplyRandomSprite(cloudData);

                clouds[i] = cloudData;

                recycled++;

                if (recycled >= cloudsPerBatch)
                    break;
            }
        }

        lastSpawnedY = spawnMaxY;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Calcula el límite superior visible de la cámara en coordenadas del mundo.
    /// </summary>
    private float GetCameraTop()
    {
        return targetCamera.transform.position.y + targetCamera.orthographicSize;
    }

    /// <summary>
    /// Calcula el límite inferior visible de la cámara en coordenadas del mundo.
    /// </summary>
    private float GetCameraBottom()
    {
        return targetCamera.transform.position.y - targetCamera.orthographicSize;
    }

    /// <summary>
    /// Aplica una escala aleatoria uniforme a la nube.
    /// </summary>
    private void ApplyRandomScale(Transform cloud)
    {
        float scale = Random.Range(minScale, maxScale);
        cloud.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// Actualiza el sprite de la nube utilizando la referencia previamente cacheada.
    /// </summary>
    private void ApplyRandomSprite(CloudData cloudData)
    {
        if (cloudData.sprite != null)
        {
            cloudData.sprite.RefreshSprite();
        }
    }

    #endregion
}