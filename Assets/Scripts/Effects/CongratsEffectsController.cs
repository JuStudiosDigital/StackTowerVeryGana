using UnityEngine;
using System.Collections;

/// <summary>
/// Controla los efectos visuales de celebración al ganar una partida.
/// Se encarga de reproducir partículas persistentes laterales y
/// de instanciar una secuencia de fuegos artificiales en pantalla.
/// </summary>
public class CongratsEffectsController : MonoBehaviour
{
    #region Inspector References

    /// <summary>
    /// Referencia al popup de felicitación que dispara el inicio de los efectos.
    /// </summary>
    [Header("Referencia")]
    [SerializeField] private CongratsPopup congratsPopup;

    #endregion

    #region Persistent Particle Systems

    /// <summary>
    /// Sistema de partículas persistente ubicado en el lado izquierdo.
    /// </summary>
    [Header("Particle Systems Persistentes")]
    [SerializeField] private ParticleSystem leftParticleSystem;

    /// <summary>
    /// Sistema de partículas persistente ubicado en el lado derecho.
    /// </summary>
    [SerializeField] private ParticleSystem rightParticleSystem;

    #endregion

    #region Fireworks Configuration

    /// <summary>
    /// Prefab del sistema de partículas usado como fuego artificial aleatorio.
    /// </summary>
    [Header("Fuegos Artificiales")]
    [SerializeField] private ParticleSystem randomParticlePrefab;

    /// <summary>
    /// Cantidad total de fuegos artificiales a instanciar.
    /// </summary>
    [SerializeField] private int randomParticlesCount = 6;

    #endregion

    #region Timing Configuration

    /// <summary>
    /// Intervalo base entre cada fuego artificial.
    /// </summary>
    [Header("Timing")]
    [SerializeField] private float spawnInterval = 0.25f;

    /// <summary>
    /// Variación aleatoria aplicada al intervalo de spawn para evitar patrones rígidos.
    /// </summary>
    [SerializeField] private float spawnIntervalRandomOffset = 0.1f;

    #endregion

    #region Camera & Spawn Area

    /// <summary>
    /// Cámara utilizada para convertir posiciones viewport a mundo.
    /// </summary>
    [Header("Cámara")]
    [SerializeField] private Camera targetCamera;

    /// <summary>
    /// Rango vertical (viewport) donde pueden aparecer los fuegos artificiales.
    /// </summary>
    [Header("Área de Spawn (Viewport)")]
    [SerializeField] private Vector2 verticalViewportRange = new Vector2(0.65f, 0.9f);

    /// <summary>
    /// Rango horizontal (viewport) donde pueden aparecer los fuegos artificiales.
    /// </summary>
    [SerializeField] private Vector2 horizontalViewportRange = new Vector2(0.3f, 0.7f);

    #endregion

    #region Runtime State

    /// <summary>
    /// Referencia a la coroutine activa de spawn de fuegos artificiales.
    /// Permite detenerla y reiniciarla de forma segura.
    /// </summary>
    private Coroutine spawnRoutine;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa y valida dependencias necesarias antes del uso.
    /// </summary>
    private void Awake()
    {
        ValidateDependencies();
    }

    /// <summary>
    /// Se suscribe a los eventos necesarios cuando el objeto es habilitado.
    /// </summary>
    private void OnEnable()
    {
        congratsPopup.OnTitleAnimationStarted += PlayEffects;
    }

    /// <summary>
    /// Se desuscribe de los eventos para evitar referencias colgantes.
    /// </summary>
    private void OnDisable()
    {
        congratsPopup.OnTitleAnimationStarted -= PlayEffects;
    }

    #endregion

    #region Effects Control

    /// <summary>
    /// Ejecuta todos los efectos visuales de celebración.
    /// </summary>
    private void PlayEffects()
    {
        if (GameManager.Instance.IsAdsEnabled)
        {
            PlayPersistentParticles();
        }      
        StartFireworks();
    }

    /// <summary>
    /// Activa los sistemas de partículas persistentes laterales.
    /// </summary>
    private void PlayPersistentParticles()
    {
        leftParticleSystem?.Play();
        rightParticleSystem?.Play();
    }

    /// <summary>
    /// Inicia la secuencia de fuegos artificiales controlada por coroutine.
    /// Si existe una secuencia previa, la detiene primero.
    /// </summary>
    private void StartFireworks()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        spawnRoutine = StartCoroutine(SpawnFireworksRoutine());
    }

    #endregion

    #region Fireworks Spawning

    /// <summary>
    /// Coroutine que instancia fuegos artificiales de forma secuencial,
    /// aplicando una variación temporal para un efecto más orgánico.
    /// </summary>
    private IEnumerator SpawnFireworksRoutine()
    {
        for (int i = 0; i < randomParticlesCount; i++)
        {
            SpawnSingleFirework();

            float delay = spawnInterval + Random.Range(
                -spawnIntervalRandomOffset,
                spawnIntervalRandomOffset
            );

            yield return new WaitForSeconds(Mathf.Max(0.05f, delay));
        }
    }

    /// <summary>
    /// Instancia un único fuego artificial en una posición aleatoria
    /// dentro de un rango de viewport en la parte superior de la pantalla.
    /// </summary>
    private void SpawnSingleFirework()
    {
        float viewportX = Random.Range(
            horizontalViewportRange.x,
            horizontalViewportRange.y
        );

        float viewportY = Random.Range(
            verticalViewportRange.x,
            verticalViewportRange.y
        );

        Vector3 viewportPosition = new Vector3(
            viewportX,
            viewportY,
            targetCamera.nearClipPlane + 6f
        );

        Vector3 worldPosition = targetCamera.ViewportToWorldPoint(viewportPosition);

        Instantiate(randomParticlePrefab, worldPosition, Quaternion.identity);
    }

    #endregion

    #region Validation

    /// <summary>
    /// Valida y asigna automáticamente dependencias críticas si no fueron configuradas.
    /// </summary>
    private void ValidateDependencies()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    #endregion
}
