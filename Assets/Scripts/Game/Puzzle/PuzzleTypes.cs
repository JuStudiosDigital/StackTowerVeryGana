
/// <summary>
/// Define los tamaños disponibles del rompecabezas.
/// </summary>
public enum PuzzleGridSize
{
    Grid4x4 = 4,
    Grid6x6 = 6,
    Grid8x8 = 8
}

/// <summary>
/// Define la prioridad visual de una pieza en el tablero.
/// </summary>
public struct VisualPriority
{
    /// <summary>
    /// Orden de renderizado del sprite.
    /// </summary>
    public int SortingOrder { get; }

    /// <summary>
    /// Profundidad en el eje Z.
    /// </summary>
    public float ZPosition { get; }

    public VisualPriority(int sortingOrder, float zPosition)
    {
        SortingOrder = sortingOrder;
        ZPosition = zPosition;
    }
}
