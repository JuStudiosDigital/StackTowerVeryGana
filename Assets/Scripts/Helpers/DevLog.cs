
using System.Diagnostics;

/// <summary>
/// Sistema centralizado de logging para desarrollo.
/// 
/// Las llamadas a estos métodos se eliminan completamente
/// en compilación si el símbolo UNITY_EDITOR no está definido.
/// 
/// No genera sobrecarga en producción.
/// </summary>
public static class DevLog
{
    #region Public Logging API

    /// <summary>
    /// Imprime un mensaje informativo en consola.
    /// Solo en Editor.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    /// <summary>
    /// Imprime un mensaje de advertencia.
    /// Solo en Editor.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void Warning(object message)
    {
        UnityEngine.Debug.LogWarning(message);
    }

    /// <summary>
    /// Imprime un mensaje de error.
    /// Solo en Editor.
    /// </summary>
    [Conditional("UNITY_EDITOR")]
    public static void Error(object message)
    {
        UnityEngine.Debug.LogError(message);
    }

    #endregion
}
