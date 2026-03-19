using UnityEngine;

/// <summary>
/// Representa un clip de audio junto con su configuración específica para el juego.
/// Utiliza ScriptableObject para permitir la creación de assets independientes en el Editor.
/// </summary>
[CreateAssetMenu(fileName = "AudioClipData", menuName = "Scriptable Objects/AudioClipData")]
public class AudioClipData : ScriptableObject
{
    #region Campos serializados

    /// <summary>
    /// Tipo de audio que representa este clip (efecto, música, ambiente, etc.).
    /// </summary>
    [SerializeField] 
    private AudioTypeGame audioType;

    /// <summary>
    /// Clip de audio que será reproducido.
    /// </summary>
    [SerializeField] 
    private AudioClip audioClip;

    /// <summary>
    /// Volumen del clip de audio, de 0 a 1.
    /// </summary>
    [Range(0f, 1f)]
    [SerializeField] 
    private float volume = 1f;

    /// <summary>
    /// Indica si el clip debe reproducirse en loop.
    /// </summary>
    [SerializeField] 
    private bool loop;

    #endregion

    #region Propiedades públicas

    /// <summary>
    /// Obtiene el tipo de audio de este clip.
    /// </summary>
    public AudioTypeGame AudioType => audioType;

    /// <summary>
    /// Obtiene el AudioClip asociado.
    /// </summary>
    public AudioClip AudioClip => audioClip;

    /// <summary>
    /// Obtiene el volumen de reproducción del clip.
    /// </summary>
    public float Volume => volume;

    /// <summary>
    /// Indica si el clip se reproducirá en loop.
    /// </summary>
    public bool Loop => loop;

    #endregion
}
