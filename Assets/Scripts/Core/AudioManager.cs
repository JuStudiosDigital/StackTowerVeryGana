using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema centralizado de audio del juego.
/// Maneja efectos, música y estado global de sonido.
/// Permite reproducir, detener y alternar sonidos según el tipo definido en AudioTypeGame.
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region Inspector Fields

    /// <summary>
    /// Configuración de audio general, incluye lista de AudioClipData.
    /// </summary>
    [SerializeField] private AudioSettingsData audioSettings;

    /// <summary>
    /// Sprite para indicar que el sonido está activado.
    /// </summary>
    [SerializeField] private Sprite soundOnSprite;

    /// <summary>
    /// Sprite para indicar que el sonido está desactivado.
    /// </summary>
    [SerializeField] private Sprite soundOffSprite;

    /// <summary>
    /// Fuente dedicada para reproducir AudioClips dinámicos
    /// provenientes de recursos remotos o locales.
    /// </summary>
    [SerializeField] private AudioSource dynamicAudioSource;

    #endregion

    #region Properties

    /// <summary>
    /// Sprite público para mostrar cuando el sonido está activado.
    /// </summary>
    public Sprite SoundOnSprite => soundOnSprite;

    /// <summary>
    /// Sprite público para mostrar cuando el sonido está desactivado.
    /// </summary>
    public Sprite SoundOffSprite => soundOffSprite;

    /// <summary>
    /// Indica si el sonido global del juego está habilitado.
    /// </summary>
    public bool IsSoundEnabled => isSoundEnabled;

    #endregion

    #region Private Fields

    /// <summary>
    /// Diccionario que relaciona tipos de audio con sus AudioSource correspondientes.
    /// </summary>
    private readonly Dictionary<AudioTypeGame, AudioSource> audioSources = new();

    /// <summary>
    /// Estado interno del sonido (true = activado, false = desactivado).
    /// </summary>
    private bool isSoundEnabled = true;

    #endregion

    #region Unity Callbacks

    /// <summary>
    /// Inicializa el AudioManager al iniciar el juego.
    /// Valida la configuración y crea los AudioSources.
    /// </summary>
    private void Awake()
    {
        if (audioSettings == null)
        {
            DevLog.Error("AudioSettingsData no asignado en AudioManager.");
            return;
        }

        InitializeAudioSources();
    }

    #endregion

    #region Audio Initialization

    /// <summary>
    /// Inicializa todas las fuentes de audio desde los datos de ScriptableObject.
    /// Evita duplicar AudioSources si ya existen para un tipo de audio.
    /// </summary>
    private void InitializeAudioSources()
    {
        foreach (AudioClipData clipData in audioSettings.AudioClips)
        {
            if (audioSources.ContainsKey(clipData.AudioType))
            {
                continue;
            }

            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = clipData.AudioClip;
            source.volume = clipData.Volume;
            source.loop = clipData.Loop;
            source.playOnAwake = false;
            source.mute = !isSoundEnabled;

            audioSources.Add(clipData.AudioType, source);
        }
    }

    #endregion

    #region Audio Control

    /// <summary>
    /// Reproduce un sonido específico según su tipo.
    /// Evita reproducir sonidos en loop si ya están activos.
    /// </summary>
    /// <param name="audioType">Tipo de audio a reproducir.</param>
    public void Play(AudioTypeGame audioType)
    {
        if (!isSoundEnabled || !audioSources.TryGetValue(audioType, out AudioSource source))
        {
            return;
        }

        if (source.loop && source.isPlaying)
        {
            return;
        }

        source.Play();
    }

    /// <summary>
    /// Reproduce un AudioClip dinámico (remoto o local).
    /// No interfiere con los sonidos tipados del sistema.
    /// </summary>
    public void Play(AudioClip clip)
    {
        DevLog.Log("[AudioManager] Reproduciendo AudioClip dinámico.");
        if (!isSoundEnabled || clip == null || dynamicAudioSource == null)
        {
            return;
        }
        DevLog.Log("[AudioManager] AudioClip válido y sonido habilitado. Reproduciendo...");
        dynamicAudioSource.PlayOneShot(clip);
    }


    /// <summary>
    /// Detiene un sonido específico según su tipo.
    /// </summary>
    /// <param name="audioType">Tipo de audio a detener.</param>
    public void Stop(AudioTypeGame audioType)
    {
        if (audioSources.ContainsKey(audioType))
        {
            audioSources[audioType].Stop();
        }
    }

    #endregion

    #region Sound Management

    /// <summary>
    /// Habilita o deshabilita todo el audio del juego.
    /// Actualiza el estado de mute de todos los AudioSources.
    /// </summary>
    /// <param name="enabled">Nuevo estado del sonido global.</param>
    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;

        foreach (AudioSource source in audioSources.Values)
        {
            source.mute = !enabled;
        }
    }

    /// <summary>
    /// Alterna el estado global del sonido entre activado y desactivado.
    /// </summary>
    public void ToggleSound()
    {
        SetSoundEnabled(!isSoundEnabled);
    }

    #endregion
}
