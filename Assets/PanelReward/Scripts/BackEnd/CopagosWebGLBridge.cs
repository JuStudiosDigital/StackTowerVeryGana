using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// Centraliza la comunicación saliente entre Unity WebGL
/// y el frontend contenedor.
/// </summary>
[DisallowMultipleComponent]
public sealed class CopagosWebGLBridge : MonoBehaviour
{
    #region Constants

    private const string ProductClickedMessageType = "PRODUCT_CLICKED";
    private const string NextLevelMessageType = "GAME_FINISHED";

    #endregion

    #region Singleton

    public static CopagosWebGLBridge Instance { get; private set; }

    #endregion

    #region Serialized Fields

    [Header("Frontend Communication")]

    [Tooltip("Origen permitido del frontend. En producción debe contener la URL exacta del contenedor.")]
    [SerializeField] private string frontendTargetOrigin = "*";

    #endregion

    #region WebGL Imports

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void Copagos_PostMessageToParent(
        string jsonMessage,
        string targetOrigin);
#endif

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            return;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Envía al frontend la información del producto seleccionado
    /// por el usuario.
    /// </summary>
    public void SendProductClicked(CopagosProductData product)
    {
        if (product == null || !product.IsValid)
        {
            DevLog.Log(
                "[CopagosWebGLBridge] No se puede enviar PRODUCT_CLICKED. " +
                "El producto es null o inválido.");

            return;
        }

        CopagosProductClickedMessage message =
            new CopagosProductClickedMessage
            {
                type = ProductClickedMessageType,
                product = product
            };

        SendMessage(
            message,
            ProductClickedMessageType);
    }

    /// <summary>
    /// Envía al frontend una notificación cuando el jugador
    /// presiona el botón para continuar al siguiente nivel.
    /// </summary>
    public void SendNextLevelRequested()
    {
        GameManager gameManager = GameManager.Instance;

        NextLevelMessage message =
            new NextLevelMessage
            {
                type = NextLevelMessageType,
                gameId = gameManager != null
                    ? gameManager.GameID
                    : string.Empty,
                sessionToken = gameManager != null
                    ? gameManager.SessionToken
                    : string.Empty,
                userHash = gameManager != null
                    ? gameManager.UserHash
                    : string.Empty,
                campaignId = gameManager != null
                    ? gameManager.CampaignId
                    : string.Empty,
                completedLevelId = gameManager != null
                    ? gameManager.LevelID
                    : 0
            };

        SendMessage(
            message,
            NextLevelMessageType);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Serializa y envía un mensaje al frontend contenedor.
    /// </summary>
    private void SendMessage(
        object message,
        string messageType)
    {
        if (message == null)
        {
            DevLog.Log(
                $"[CopagosWebGLBridge] No se puede enviar {messageType}. " +
                "El mensaje es null.");

            return;
        }

        string json = JsonUtility.ToJson(message);

        if (string.IsNullOrWhiteSpace(json))
        {
            DevLog.Log(
                $"[CopagosWebGLBridge] No se pudo serializar {messageType}.");

            return;
        }

        PostMessageToParent(json);

        DevLog.Log(
            $"[CopagosWebGLBridge] {messageType} enviado: {json}");
    }

    private void PostMessageToParent(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            DevLog.Log(
                "[CopagosWebGLBridge] No se puede enviar un mensaje vacío.");

            return;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        Copagos_PostMessageToParent(
            json,
            frontendTargetOrigin);
#else
        DevLog.Log(
            $"[CopagosWebGLBridge] PostMessage simulado fuera de WebGL: {json}");
#endif
    }

    #endregion

    #region Message Models

    /// <summary>
    /// Mensaje enviado cuando el jugador solicita continuar
    /// al siguiente nivel.
    /// </summary>
    [Serializable]
    private sealed class NextLevelMessage
    {
        public string type;
        public string gameId;
        public string sessionToken;
        public string userHash;
        public string campaignId;
        public int completedLevelId;
    }

    #endregion
}