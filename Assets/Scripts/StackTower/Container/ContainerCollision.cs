using UnityEngine;

/// <summary>
/// Gestiona la detección de colisiones del contenedor y notifica eventos del sistema de juego.
/// </summary>
[RequireComponent(typeof(Container))]
public class ContainerCollision : MonoBehaviour
{
    #region References

    /// <summary>
    /// Referencia al componente Container asociado.
    /// </summary>
    private Container container;

    #endregion

    #region Unity

    /// <summary>
    /// Inicializa las referencias necesarias del componente.
    /// </summary>
    private void Awake()
    {
        container = GetComponent<Container>();
    }

    /// <summary>
    /// Detecta colisiones físicas y determina su efecto en el flujo del juego.
    /// </summary>
    /// <param name="collision">Información de la colisión detectada.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Sea"))
        {
            StackTowerEvents.RaiseGameOver();
            return;
        }

        container.NotifyFirstCollision();
    }

    #endregion
}