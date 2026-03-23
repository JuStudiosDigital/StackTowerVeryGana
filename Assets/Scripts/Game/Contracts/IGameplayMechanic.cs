using System;

/// <summary>
/// Contrato base para cualquier mecánica de gameplay.
/// </summary>
public interface IGameplayMechanic
{

    /// <summary>
    /// Inicia la lógica principal de la mecánica.
    /// </summary>
    void StartMechanic();

    /// <summary>
    /// Evento disparado cuando la mecánica se completa.
    /// </summary>
    event Action OnMechanicCompleted;
    
}