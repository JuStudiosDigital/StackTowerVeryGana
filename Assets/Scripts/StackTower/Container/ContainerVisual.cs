using UnityEngine;

/// <summary>
/// Controla la apariencia visual del contenedor.
/// </summary>
public class ContainerVisual : MonoBehaviour
{
    [Header("Configuración de color")]

    [SerializeField]
    [Tooltip("Colores disponibles para el contenedor.")]
    private Color[] availableColors;

    [SerializeField]
    [Tooltip("Renderers a los que se aplicará el color.")]
    private SpriteRenderer[] renderers;

    private void Awake()
    {
        ApplyRandomColor();
    }

    private void ApplyRandomColor()
    {
        if (availableColors == null || availableColors.Length == 0)
        {
            Debug.LogWarning("ContainerVisual: No hay colores asignados.");
            return;
        }

        int index = Random.Range(0, availableColors.Length);
        Color selectedColor = availableColors[index];

        foreach (var rend in renderers)
        {
            if (rend != null)
                rend.color = selectedColor;
        }
    }
}