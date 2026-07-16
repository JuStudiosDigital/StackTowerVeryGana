using System;
using UnityEngine;

/// <summary>
/// Gestiona la apertura y cierre de popups dentro de la escena,
/// garantizando que solo un popup pueda estar activo a la vez.
/// También coordina la visibilidad y el estado del overlay asociado.
/// </summary>
public class PopupManager : MonoBehaviour
{
    #region Events

    /// <summary>
    /// Evento disparado cuando un popup terminó completamente su cierre.
    /// </summary>
    public event Action<PopupBase> PopupClosed;

    #endregion

    #region Serialized Fields

    /// <summary>
    /// Controlador del overlay que se muestra detrás de los popups.
    /// Se utiliza para bloquear la interacción con el contenido inferior
    /// y detectar clics para cerrar el popup activo.
    /// </summary>
    [SerializeField] private PopupOverlayController overlay;

    #endregion

    #region Private Fields

    /// <summary>
    /// Referencia al popup actualmente abierto.
    /// Es nula cuando no hay ningún popup activo.
    /// </summary>
    private PopupBase currentPopup;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa las dependencias internas del manager.
    /// Asigna la acción de cierre cuando el overlay es clickeado.
    /// </summary>
    private void Awake()
    {
        overlay.OnOverlayClicked = CloseCurrentPopup;
    }

    #endregion

    #region Public API

    /// <summary>
    /// Abre un nuevo popup, cerrando previamente cualquier popup activo.
    /// Registra el evento de cierre del popup y muestra el overlay.
    /// </summary>
    /// <param name="popup">
    /// Popup que se desea mostrar. Debe heredar de <see cref="PopupBase"/>.
    /// </param>
    public void OpenPopup(PopupBase popup)
    {
        CloseCurrentPopup();

        currentPopup = popup;
        currentPopup.CloseRequested += CloseCurrentPopup;

        overlay.Show();
        popup.Show();
    }

    /// <summary>
    /// Cierra el popup actualmente activo, si existe.
    /// </summary>
    public void CloseCurrentPopup()
    {
        CloseCurrentPopup(null);
    }

    /// <summary>
    /// Cierra el popup actualmente activo y ejecuta un callback
    /// cuando terminó completamente la animación de cierre.
    /// </summary>
    /// <param name="onClosed">Acción ejecutada al finalizar el cierre.</param>
    public void CloseCurrentPopup(Action onClosed)
    {
        if (currentPopup == null)
        {
            onClosed?.Invoke();
            return;
        }

        PopupBase popupToClose = currentPopup;
        popupToClose.CloseRequested -= CloseCurrentPopup;
        currentPopup = null;

        popupToClose.Hide(() =>
        {
            overlay.Hide();

            PopupClosed?.Invoke(popupToClose);
            onClosed?.Invoke();
        });
    }

    /// <summary>
    /// Bloquea el overlay para impedir interacciones del usuario.
    /// Útil cuando el popup requiere una acción obligatoria antes de cerrarse.
    /// </summary>
    public void LockOverlay()
    {
        overlay.Lock();
    }
    /// <summary>
    /// Desbloquea el overlay para permitir cerrar popups tocando el fondo.
    /// </summary>
    public void UnlockOverlay()
    {
        overlay.Unlock();
    }
    #endregion
}
