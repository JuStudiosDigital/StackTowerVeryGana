using UnityEngine;

/// <summary>
/// Define un contrato genérico para manejar
/// recompensas durante una sesión de gameplay.
/// </summary>
public interface IGameplayRewardHandler
{
    /// <summary>
    /// Recompensa asociada a una acción del jugador.
    /// </summary>
    /// <param name="worldPosition">Posición mundial del evento.</param>
    void HandleActionReward(Vector3 worldPosition);

    /// <summary>
    /// Obtiene la cantidad total de recompensa acumulada.
    /// </summary>
    int GetTotalReward();
}