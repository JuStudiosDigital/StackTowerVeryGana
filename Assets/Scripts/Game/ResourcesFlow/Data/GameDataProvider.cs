using UnityEngine;

/// <summary>
/// Proveedor centralizado de datos del juego en tiempo de ejecución.
/// Expone una API unificada que abstrae la fuente de datos (fallback o remoto).
/// </summary>
public class GameDataProvider : MonoBehaviour
{
    /// <summary>
    /// Instancia global accesible del proveedor de datos.
    /// </summary>
    public static GameDataProvider Instance { get; private set; }

    [SerializeField]
    [Tooltip("Fuente de datos base utilizada como fallback cuando no hay configuración remota.")]
    private GameData baseData;

    /// <summary>
    /// Contenedor mutable con el estado actual del juego.
    /// </summary>
    private GameRuntimeData runtimeData;

    /// <summary>
    /// Indica si el runtime ha sido inicializado correctamente.
    /// </summary>
    public bool IsInitialized => runtimeData != null;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Inicializa el estado runtime a partir de los datos base (fallback).
    /// Debe ejecutarse antes de cualquier acceso a datos.
    /// </summary>
    public void Initialize()
    {
        runtimeData = new GameRuntimeData();
        runtimeData.Initialize(baseData);
    }

    /// <summary>
    /// Acceso directo al contenedor de datos runtime.
    /// </summary>
    public GameRuntimeData Runtime => runtimeData;

    #region Public Access API

    /// <summary>
    /// Retorna la imagen principal del gameplay.
    /// </summary>
    public Texture2D GetPuzzleImage() => runtimeData.PuzzleImage;

    /// <summary>
    /// Retorna el audio de fondo configurado.
    /// </summary>
    public AudioClip GetMusic() => runtimeData.Music;

    /// <summary>
    /// Retorna la cantidad de llaves otorgadas por acción.
    /// </summary>
    public int GetKeysPerAction() => runtimeData.KeysPerAction;

    #endregion
}