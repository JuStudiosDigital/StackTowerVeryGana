using UnityEngine;

/// <summary>
/// Gestiona la detección de colisiones del contenedor y traduce dichas interacciones
/// en eventos del sistema de juego.
/// </summary>
[RequireComponent(typeof(Container))]
public class ContainerCollision : MonoBehaviour
{
    #region References

    /// <summary>
    /// Referencia al componente <see cref="Container"/> asociado,
    /// utilizado para notificar colisiones válidas.
    /// </summary>
    private Container container;

    #endregion

    #region Unity

    /// <summary>
    /// Inicializa las referencias necesarias para el funcionamiento del componente.
    /// </summary>
    private void Awake()
    {
        container = GetComponent<Container>();
    }

    /// <summary>
    /// Detecta colisiones físicas y determina su impacto en la lógica del juego.
    /// Puede provocar el fin de la partida o notificar una colisión válida del contenedor.
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