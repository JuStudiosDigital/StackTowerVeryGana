using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using UnityEngine.Networking;

/// <summary>
/// Administrador central del juego.
/// Responsable de mantener el estado global y exponer sistemas principales.
/// Implementado como Singleton persistente entre escenas.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton

    /// <summary>
    /// Instancia única y global del GameManager.
    /// Garantiza acceso centralizado a los sistemas principales del juego.
    /// </summary>
    public static GameManager Instance { get; private set; }

    #endregion

    #region Serialized Dependencies

    /// <summary>
    /// Referencia al administrador de audio del juego.
    /// Gestiona música y efectos sonoros globales.
    /// </summary>
    [SerializeField] private AudioManager audioManager;

    /// <summary>
    /// Referencia al administrador de moneda y economía del juego.
    /// </summary>
    [SerializeField] private CurrencyManager currencyManager;

    /// <summary>
    /// Indica si el nivel permite mostrar publicidad.
    /// </summary>
    public bool IsAdsEnabled { get; private set; }

    #endregion

    #region Level State

    /// <summary>
    /// Identificador del nivel actual.
    /// Controla el progreso y la lógica dependiente del nivel.
    /// </summary>
    private int levelID = 1;

    /// <summary>
    /// Propiedad pública para acceder o modificar el identificador del nivel actual.
    /// </summary>
    public int LevelID
    {
        get => levelID;
        set => levelID = value;
    }

    #endregion

    #region UI Locking

    /// <summary>
    /// Indica si la interfaz de usuario se encuentra bloqueada temporalmente.
    /// Se utiliza para evitar interacciones simultáneas o repetidas.
    /// </summary>
    private bool lockUI = false;

    /// <summary>
    /// Propiedad que controla el bloqueo de la interfaz de usuario.
    /// Al activarse, programa automáticamente el desbloqueo tras un tiempo fijo.
    /// </summary>
    public bool LockUI
    {
        get => lockUI;
        set
        {
            lockUI = value;

            if (lockUI)
            {
                CancelInvoke(nameof(UnLockUI));
                Invoke(nameof(UnLockUI), 1f);
            }
        }
    }

    /// <summary>
    /// Desbloquea la interfaz de usuario.
    /// Método invocado de forma diferida para restaurar la interacción.
    /// </summary>
    private void UnLockUI()
    {
        lockUI = false;
    }

    #endregion

    #region Public Accessors

    /// <summary>
    /// Acceso de solo lectura al administrador de audio.
    /// </summary>
    public AudioManager AudioManager => audioManager;

    /// <summary>
    /// Acceso de solo lectura al administrador de moneda.
    /// </summary>
    public CurrencyManager CurrencyManager => currencyManager;

    [Header("Session Data")]
    public string SessionToken { get; private set; }
    public string UserHash { get; private set; }
    public bool IsBrandedMode { get; private set; }
    public string CampaignId { get; private set; }
    [HideInInspector]public string GameID = "stack-tower";

    #if UNITY_EDITOR
    [Header("Editor Config")]
    [SerializeField] private string editorApiUrl = "https://justudios.co/test-verygana/stack-tower/config/stacktowerconfig.json";
    #endif
    

    #endregion

    [Header("Config")]
    public string ApiUrl { get; private set; }

    #if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string GetURLParameter(string paramName);
    #endif

  #region Unity Lifecycle

    /// <summary>
    /// Inicializa el Singleton y asegura su persistencia entre escenas.
    /// Destruye instancias duplicadas para mantener una única fuente de verdad.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitializeGame());
            return;
        }

        Destroy(gameObject);
    }

    private IEnumerator InitializeGame()
    {
        DevLog.Log("[GameManager] Inicializando sistema...");

        yield return LoadConfig();

        InitializeFromUrl();

        DevLog.Log("[GameManager] Inicialización completa");
    }


    [System.Serializable]
    private class ConfigData
    {
        public string apiUrl;
    }
    private IEnumerator LoadConfig()
    {
    #if UNITY_EDITOR
        ApiUrl = editorApiUrl;
        DevLog.Log($"[GameManager] API URL (EDITOR): {ApiUrl}");
        yield break;
    #else
        string path = GetConfigPath();

        DevLog.Log($"[GameManager] Cargando config desde: {path}");

        UnityWebRequest request = UnityWebRequest.Get(path);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            DevLog.Log("[GameManager] Error cargando config: " + request.error);
            ApiUrl = "";
        }
        else
        {
            try
            {
                ConfigData config = JsonUtility.FromJson<ConfigData>(request.downloadHandler.text);

                if (config == null || string.IsNullOrEmpty(config.apiUrl))
                    throw new System.Exception("Config inválida");

                ApiUrl = config.apiUrl;
                DevLog.Log($"[GameManager] API URL cargada: {ApiUrl}");
            }
            catch
            {
                DevLog.Log("[GameManager] JSON inválido, usando fallback");
                ApiUrl = "";
            }
        }
    #endif
    }
    private string GetConfigPath()
    {
    #if UNITY_WEBGL && !UNITY_EDITOR
        var uri = new System.Uri(Application.absoluteURL);
        string baseUrl = uri.GetLeftPart(System.UriPartial.Path);
    
        if (!baseUrl.EndsWith("/"))
            baseUrl = baseUrl.Substring(0, baseUrl.LastIndexOf('/') + 1);
    
        return baseUrl + "/config.json";
    #else
        return "file://" + Application.dataPath + "/../config.json";
    #endif
    }

    /// <summary>
    /// Ejecutado al inicio del juego.
    /// Inicia la música de fondo principal.
    /// </summary>
    private void Start()
    {
        AudioManager.Play(AudioTypeGame.BackgroundMusic);
    }

    #endregion

    /// <summary>
    /// Actualiza el estado de publicidad según el resultado real del pipeline.
    /// </summary>
    public void ConfigureAds(bool adsEnabled)
    {
        IsAdsEnabled = adsEnabled;
        DevLog.Log($"[GameManager] Publicidad habilitada: {IsAdsEnabled}");
    }

    
    private void InitializeFromUrl()
    {
        DevLog.Log($"[GameManager] Inciando lectura de parámetros");
#if UNITY_WEBGL && !UNITY_EDITOR
        SessionToken = GetURLParameter("session_token");
        UserHash = GetURLParameter("user_hash");
        string brandedRaw = GetURLParameter("is_branded_mode");
        CampaignId = GetURLParameter("campaign_id");
#else
        // Valores de prueba para editor
        SessionToken = "editor_session";
        UserHash = "editor_user";
        string brandedRaw = "false";
        CampaignId = "editor_campaign";
#endif

        ParseBrandedMode(brandedRaw);

        DevLogLogSession();
    }

    private void ParseBrandedMode(string value)
    {

        // TODO: Quitar para producción, solo para pruebas rápidas en editor sin parámetros

        if (string.IsNullOrEmpty(value))
        {
            IsBrandedMode = false;
            return;
        }

        value = value.ToLower();

        IsBrandedMode =
            value == "true" ||
            value == "1" ||
            value == "yes";
    }

    private void DevLogLogSession()
    {
        DevLog.Log("===== SESSION PARAMETERS =====");
        DevLog.Log($"SessionToken: {SessionToken}");
        DevLog.Log($"UserHash: {UserHash}");
        DevLog.Log($"IsBrandedMode: {IsBrandedMode}");
        DevLog.Log($"CampaignId: {CampaignId}");
        DevLog.Log("================================");
    }
}