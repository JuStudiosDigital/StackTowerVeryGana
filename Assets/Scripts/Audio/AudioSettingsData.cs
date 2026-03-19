using UnityEngine;

/// <summary>
/// ScriptableObject que contiene la configuración de audio de la aplicación.
/// Almacena un conjunto de AudioClipData que puede ser utilizado por sistemas de audio.
/// </summary>
[CreateAssetMenu(fileName = "AudioSettingsData", menuName = "Scriptable Objects/AudioSettingsData")]
public class AudioSettingsData : ScriptableObject
{
    #region Campos serializados

    /// <summary>
    /// Arreglo de AudioClipData que representa todos los clips de audio configurados.
    /// Se puede editar desde el Inspector.
    /// </summary>
    [SerializeField] 
    private AudioClipData[] audioClips;

    #endregion

    #region Propiedades

    /// <summary>
    /// Propiedad pública de solo lectura que expone los clips de audio.
    /// Permite acceso seguro a los datos sin modificar el arreglo directamente.
    /// </summary>
    public AudioClipData[] AudioClips => audioClips;

    #endregion
}
