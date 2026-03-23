using UnityEngine;

/// <summary>
/// Representa una moneda recolectable.
/// Solo emite el evento de recolección.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Coin : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<Container>(out _))
            return;

        // ✅ Enviar posición real
        StackTowerEvents.RaiseCoinCollected(transform.position);

        Destroy(gameObject);
    }
}