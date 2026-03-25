using UnityEngine;

/// <summary>
/// Gestiona la configuración visual del contenedor,
/// aplicando variaciones de color a sus renderizadores asociados.
/// </summary>
public class ContainerVisual : MonoBehaviour
{
    #region Inspector

    [Header("Configuración de color")]

    [SerializeField]
    [Tooltip("Lista de colores disponibles que pueden asignarse aleatoriamente al contenedor.")]
    private Color[] availableColors;

    [SerializeField]
    [Tooltip("Conjunto de SpriteRenderers a los que se aplicará el color seleccionado.")]
    private SpriteRenderer[] renderers;

    #endregion

    #region Unity

    /// <summary>
    /// Inicializa la apariencia visual del contenedor al momento de su creación.
    /// </summary>
    private void Awake()
    {
        ApplyRandomColor();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Selecciona un color aleatorio de la lista disponible y lo aplica a todos los renderizadores definidos.
    /// Si no hay colores configurados, se registra una advertencia y no se realiza ninguna modificación.
    /// </summary>
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

    #endregion
}