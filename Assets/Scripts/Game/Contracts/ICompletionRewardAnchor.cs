using UnityEngine;

/// <summary>
/// Proporciona una posición en mundo para recompensas
/// al completar una mecánica.
/// </summary>
public interface ICompletionRewardAnchor
{
    /// <summary>
    /// Posición en mundo donde deben aparecer
    /// las recompensas de finalización.
    /// </summary>
    Vector3 CompletionRewardPosition { get; }
}
