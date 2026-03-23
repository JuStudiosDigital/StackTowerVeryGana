using UnityEngine;

/// <summary>
/// Maneja la lógica de colisiones del contenedor.
/// </summary>
[RequireComponent(typeof(Container))]
public class ContainerCollision : MonoBehaviour
{
    private Container container;

    private void Awake()
    {
        container = GetComponent<Container>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 🔴 Game Over si toca el mar (usando tag por pragmatismo)
        if (collision.gameObject.CompareTag("Sea"))
        {
            StackTowerEvents.RaiseGameOver();
            return;
        }

        // ✅ Primera colisión válida
        container.NotifyFirstCollision();
    }
}