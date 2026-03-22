using UnityEngine;
using System;

public class Container : MonoBehaviour
{
    public static event Action<Container> OnFirstCollision;

    private bool hasCollided = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 💀 GAME OVER si toca el mar
        if (collision.gameObject.CompareTag("Sea"))
        {
            GameManagerStackTower.TriggerGameOver();
            return;
        }

        // 👉 lógica normal (primer contacto)
        if (hasCollided) return;

        hasCollided = true;
        OnFirstCollision?.Invoke(this);
    }
}