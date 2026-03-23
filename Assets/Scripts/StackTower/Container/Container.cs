using UnityEngine;
using System;

public class Container : MonoBehaviour
{
    public static event Action<Container> OnFirstCollision;

    private bool hasCollided = false;

    [Header("Colores disponibles")]
    [SerializeField] private Color[] availableColors;

    [Header("Renderers a afectar")]
    [SerializeField] private SpriteRenderer[] renderers;

    private void Awake()
    {
        ApplyRandomColor();
    }

    private void ApplyRandomColor()
    {
        if (availableColors == null || availableColors.Length == 0)
        {
            Debug.LogWarning("No hay colores asignados en el Container.");
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, availableColors.Length);
        Color selectedColor = availableColors[randomIndex];

        foreach (var rend in renderers)
        {
            if (rend != null)
                rend.color = selectedColor;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 💀 GAME OVER si toca el mar
        if (collision.gameObject.CompareTag("Sea"))
        {
            GameManagerStackTower.TriggerGameOver();
            return;
        }

        // 👉 lógica normal (primer contacto)
        if (hasCollided) return;

        hasCollided = true;
        OnFirstCollision?.Invoke(this);
    }
}