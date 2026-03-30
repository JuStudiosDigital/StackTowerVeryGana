using UnityEngine;
using System;

/// <summary>
/// Representa un contenedor dentro del sistema de juego.
/// 
/// Su responsabilidad está intencionalmente limitada a:
/// - Gestionar su estado de colisión
/// - Emitir eventos cuando ocurre una colisión válida
/// 
/// Nota de diseño:
/// Este componente no conoce sistemas externos (score, UI, monedas).
/// Solo actúa como fuente de eventos, permitiendo un alto desacoplamiento.
/// </summary>
public class Container : MonoBehaviour
{
    #region Events

    /// <summary>
    /// Evento que se dispara cuando el contenedor registra su primera colisión válida.
    /// 
    /// Se utiliza para:
    /// - Avanzar el flujo del juego (garra, cámara)
    /// - Sincronizar sistemas dependientes del impacto
    /// </summary>
    public static event Action<Container> OnFirstCollision;

    #endregion

    #region State

    /// <summary>
    /// Indica si el contenedor ya ha colisionado.
    /// 
    /// Previene múltiples emisiones del mismo evento debido a:
    /// - Rebotes físicos
    /// - Múltiples contactos en un mismo frame
    /// </summary>
    private bool hasCollided = false;

    #endregion

    #region Public API

    /// <summary>
    /// Notifica que el contenedor ha tenido su primera colisión válida.
    /// 
    /// Flujo:
    /// 1. Valida que no se haya procesado previamente
    /// 2. Marca el estado interno
    /// 3. Emite evento local (OnFirstCollision)
    /// 4. Emite evento global (StackTowerEvents)
    /// 
    /// Nota de diseño:
    /// Se separan dos niveles de eventos:
    /// 
    /// - Evento directo (OnFirstCollision):
    ///   Usado por sistemas cercanos al objeto (ej: garra, cámara)
    /// 
    /// - Evento global (StackTowerEvents):
    ///   Usado por sistemas de alto nivel (score, minimapa, analytics)
    /// 
    /// Esto permite distintos niveles de desacoplamiento sin perder claridad.
    /// </summary>
    public void NotifyFirstCollision()
    {
        if (hasCollided) return;

        hasCollided = true;

        /// Evento directo (bajo nivel, inmediato)
        OnFirstCollision?.Invoke(this);

        /// Evento global (alto nivel, fuente de verdad del gameplay)
        StackTowerEvents.RaiseContainerPlaced(this);
    }

    #endregion
}