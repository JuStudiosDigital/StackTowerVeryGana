using System;
using UnityEngine;

/// <summary>
/// Define un contrato opcional para mecánicas
/// que otorgan recompensas por una acción puntual
/// con posición en mundo.
/// </summary>
public interface IPieceRewardSource
{
    /// <summary>
    /// Evento emitido cuando se debe otorgar
    /// una recompensa por acción del jugador.
    /// Proporciona la posición en mundo.
    /// </summary>
    event Action<Vector3> PieceRewardTriggered;
}
