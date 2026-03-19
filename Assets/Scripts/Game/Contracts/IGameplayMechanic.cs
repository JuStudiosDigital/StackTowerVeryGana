using System;

/// <summary>
/// Contrato base para cualquier mecánica de gameplay.
/// </summary>
public interface IGameplayMechanic
{
    /// <summary>
    /// Ejecuta la animación de entrada de la mecánica.
    /// Debe invocar el callback cuando finalice.
    /// </summary>
    void PlayEnterAnimation(Action onComplete = null);

    /// <summary>
    /// Inicia la lógica principal de la mecánica.
    /// </summary>
    void StartMechanic();

    /// <summary>
    /// Reinicia completamente la mecánica.
    /// </summary>
    void RestartMechanic();

    /// <summary>
    /// Ejecuta la animación de salida de la mecánica.
    /// </summary>
    void PlayExitAnimation(Action onComplete = null);

    /// <summary>
    /// Evento disparado cuando la mecánica se completa.
    /// </summary>
    event Action OnMechanicCompleted;

    /// <summary>
    /// Evento disparado cuando el jugador realiza una acción.
    /// </summary>
    event Action OnPlayerAction;
}
