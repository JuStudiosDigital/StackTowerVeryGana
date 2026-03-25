using UnityEngine;
using System;

/// <summary>
/// Representa un contenedor dentro del juego.
/// Su responsabilidad se limita a gestionar su estado interno
/// y emitir eventos asociados a su primera colisión válida.
/// </summary>
public class Container : MonoBehaviour
{
    #region Events

    /// <summary>
    /// Evento estático que se dispara cuando el contenedor registra su primera colisión válida.
    /// Permite a otros sistemas reaccionar sin acoplamiento directo.
    /// </summary>
    public static event Action<Container> OnFirstCollision;

    #endregion

    #region State

    /// <summary>
    /// Indica si el contenedor ya ha registrado una colisión válida.
    /// Previene múltiples emisiones del mismo evento.
    /// </summary>
    private bool hasCollided = false;

    #endregion

    #region Public API

    /// <summary>
    /// Notifica al sistema que el contenedor ha tenido su primera colisión válida.
    /// Garantiza que el evento se emita una única vez durante el ciclo de vida del objeto.
    /// </summary>
    public void NotifyFirstCollision()
    {
        if (hasCollided) return;

        hasCollided = true;

        OnFirstCollision?.Invoke(this);

        StackTowerEvents.RaiseContainerPlaced();
    }

    #endregion
}