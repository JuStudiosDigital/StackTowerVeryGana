using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class ClawInput : MonoBehaviour
{
    public static event Action OnPress;

    public static bool InputEnabled = true;

    private void Update()
    {
        if (!InputEnabled) return;

        if (!IsPressed()) return;

        // 🔥 SOLO validar UI cuando hay input real
        if (IsPointerOverUI()) return;

        OnPress?.Invoke();
    }

    private bool IsPressed()
    {
        // Mouse
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            return true;

        // Teclado (no depende de UI)
        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame)
                return true;
        }

        // Touch
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        // 🔴 SOLO validar si es mouse o touch
        if (Mouse.current == null && Touchscreen.current == null)
            return false;

        Vector2 screenPosition = Vector2.zero;

        // Mouse
        if (Mouse.current != null)
            screenPosition = Mouse.current.position.ReadValue();

        // Touch
        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.isPressed)
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();

        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // 🔥 SOLO bloquear si realmente hay UI interactiva
        foreach (var r in results)
        {
            if (r.gameObject.GetComponent<UnityEngine.UI.Selectable>() != null)
                return true;
        }

        return false;
    }
}