using UnityEngine;
using System;

/// <summary>
/// Representa un contenedor en el juego.
/// Maneja únicamente estado y eventos.
/// </summary>
public class Container : MonoBehaviour
{
    /// <summary>
    /// Evento que se dispara en la primera colisión válida.
    /// </summary>
    public static event Action<Container> OnFirstCollision;

    private bool hasCollided = false;

    /// <summary>
    /// Notifica la primera colisión del contenedor.
    /// </summary>
    public void NotifyFirstCollision()
{
    if (hasCollided) return;

    hasCollided = true;

    OnFirstCollision?.Invoke(this);

    // 🔥 NUEVO
    StackTowerEvents.RaiseContainerPlaced();
}
}