using UnityEngine;

/// <summary>
/// Representa una moneda recolectable dentro del juego.
/// Su única responsabilidad es detectar la interacción válida con el jugador
/// y notificar el evento correspondiente antes de ser destruida.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    #region Unity

    /// <summary>
    /// Detecta la entrada de un collider y valida si corresponde a un contenedor.
    /// En caso afirmativo, emite el evento de recolección y elimina la moneda de la escena.
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