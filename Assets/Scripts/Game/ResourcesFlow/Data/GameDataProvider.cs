using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Punto único de acceso a los datos del juego.
/// Oculta la diferencia entre fallback y runtime.
/// </summary>
public class GameDataProvider : MonoBehaviour
{
    public static GameDataProvider Instance { get; private set; }

    [SerializeField]
    private GameData baseData;

    private GameRuntimeData runtimeData;

    /// <summary>
    /// Indica si el runtime ya fue inicializado.
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
    /// Inicializa el runtime data desde fallback.
    /// </summary>
    public void Initialize()
    {
        runtimeData = new GameRuntimeData();
        runtimeData.Initialize(baseData);
    }

    public GameRuntimeData Runtime
    {
        get
        {
            if (!IsInitialized)
            {
                DevLog.Warning("[GameDataProvider] Runtime solicitado sin inicializar");
                return null;
            }

            return runtimeData;
        }
    }

    #region Public Access API

    public List<Sprite> GetContainers()
    {
        if (!IsInitialized) return null;
        return runtimeData.ContainerSprites;
    }

    public List<Color> GetContainerColors()
    {
        if (!IsInitialized) return null;
        return runtimeData.ContainerColors;
    }

    public int GetKeysPerAction()
    {
        if (!IsInitialized) return 0;
        return runtimeData.KeysPerAction;
    }

    #endregion
}