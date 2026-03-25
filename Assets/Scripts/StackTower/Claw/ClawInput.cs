using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

/// <summary>
/// Gestiona la detección de entrada del usuario de forma centralizada.
/// Traduce múltiples fuentes de input (mouse, teclado y touch) en un evento unificado,
/// considerando la interacción con elementos de interfaz para evitar conflictos.
/// </summary>
public class ClawInput : MonoBehaviour
{
    #region Events

    /// <summary>
    /// Evento que se dispara cuando se detecta una entrada válida del usuario.
    /// </summary>
    public static event Action OnPress;

    #endregion

    #region Configuration

    /// <summary>
    /// Indica si el sistema de entrada está habilitado.
    /// Permite bloquear globalmente la interacción del usuario.
    /// </summary>
    public static bool InputEnabled = true;

    #endregion

    #region Unity

    /// <summary>
    /// Evalúa continuamente la entrada del usuario y emite eventos cuando corresponde.
    /// </summary>
    private void Update()
    {
        if (!InputEnabled) return;

        if (!IsPressed()) return;

        if (IsPointerOverUI()) return;

        OnPress?.Invoke();
    }

    #endregion

    #region Input Detection

    /// <summary>
    /// Determina si se ha producido una entrada válida desde cualquier dispositivo soportado.
    /// </summary>
    /// <returns>True si se detecta una pulsación válida; de lo contrario, false.</returns>
    private bool IsPressed()
    {
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame)
                return true;
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
    }

    #endregion

    #region UI Validation

    /// <summary>
    /// Determina si la entrada actual ocurre sobre un elemento interactivo de la interfaz de usuario.
    /// </summary>
    /// <returns>True si el puntero está sobre UI interactiva; de lo contrario, false.</returns>
    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (Mouse.current == null && Touchscreen.current == null)
            return false;

        Vector2 screenPosition = Vector2.zero;

        if (Mouse.current != null)
            screenPosition = Mouse.current.position.ReadValue();

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<UnityEngine.UI.Selectable>() != null)
                return true;
        }

        return false;
    }

    #endregion
}