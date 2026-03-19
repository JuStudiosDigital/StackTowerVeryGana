using UnityEngine;

/// <summary>
/// Popup informativo simple.
/// Solo muestra información y solicita cerrarse.
/// </summary>
public class InfoPopup : PopupBase
{
    /// <summary>
    /// Llamado por el botón de cerrar.
    /// </summary>
    public void Close()
    {
        RequestClose();
    }
}