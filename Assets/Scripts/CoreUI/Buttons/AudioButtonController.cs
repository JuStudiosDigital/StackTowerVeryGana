using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla el comportamiento de un botón de interfaz de usuario
/// encargado de activar o desactivar el sonido del juego.
/// 
/// Este componente se encarga de:
/// - Consultar el estado actual del audio desde el AudioManager.
/// - Actualizar el sprite del botón según dicho estado.
/// - Notificar al AudioManager cuando el usuario solicita un cambio.
/// </summary>
public class AudioButtonController : MonoBehaviour
{
    #region Serialized Fields

    /// <summary>
    /// Referencia a la imagen del botón de audio que se actualizará
    /// visualmente según el estado del sonido (activado o desactivado).
    /// </summary>
    [SerializeField] private Image audioImage;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Método llamado por Unity al iniciar el objeto.
    /// 
    /// Se utiliza para sincronizar el estado visual inicial del botón
    /// con el estado actual del sistema de audio.
    /// </summary>
    private void Start()
    {
        UpdateAudioImage();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Alterna el estado del sonido del juego.
    /// 
    /// Este método debe ser llamado desde la UI (por ejemplo,
    /// desde un botón) y se encarga de:
    /// - Solicitar al AudioManager el cambio de estado del sonido.
    /// - Actualizar inmediatamente la imagen del botón para reflejar el nuevo estado.
    /// </summary>
    public void ToggleMusic()
    {
        GameManager.Instance.AudioManager.ToggleSound();
        UpdateAudioImage();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Actualiza el sprite del botón de audio según el estado actual del sonido.
    /// 
    /// Si el sonido está habilitado, se muestra el sprite correspondiente
    /// al estado "activo"; de lo contrario, se muestra el sprite de estado "desactivado".
    /// </summary>
    private void UpdateAudioImage()
    {
        if (GameManager.Instance.AudioManager.IsSoundEnabled)
        {
            audioImage.sprite = GameManager.Instance.AudioManager.SoundOnSprite;
        }
        else
        {
            audioImage.sprite = GameManager.Instance.AudioManager.SoundOffSprite;
        }
    }

    #endregion
}
