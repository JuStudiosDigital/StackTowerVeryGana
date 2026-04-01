using UnityEngine;
using System.Collections.Generic;

public class GameDataProvider : MonoBehaviour
{
    public static GameDataProvider Instance { get; private set; }

    [SerializeField]
    private GameData baseData;

    private GameRuntimeData runtimeData;

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

    public int GetContainersPerKey()
    {
        if (!IsInitialized) return 0;
        return runtimeData.ContainersPerKey;
    }

    public int GetKeysPerAction()
    {
        if (!IsInitialized) return 0;
        return runtimeData.KeysPerAction;
    }

    #endregion
}