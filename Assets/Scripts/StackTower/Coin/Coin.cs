using UnityEngine;

/// <summary>
/// Representa una moneda recolectable dentro del juego.
/// Detecta una interacción válida y notifica al sistema mediante eventos.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    #region Unity

    /// <summary>
    /// Detecta la entrada de un collider y valida si corresponde a un contenedor.
    /// Si la validación es correcta, emite el evento de recolección y destruye la moneda.
    /// </summary>
    /// <param name="other">Collider que entra en contacto con el trigger.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Container>(out _))
            return;

        StackTowerEvents.RaiseCoinCollected(transform.position);

        Destroy(gameObject);
    }

    #endregion
}