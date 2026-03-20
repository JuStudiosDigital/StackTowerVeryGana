using UnityEngine;
using System;

public class Container : MonoBehaviour
{
    public static event Action<Container> OnFirstCollision;

    private bool hasCollided = false;

    [Header("Game Over")]
    [SerializeField] private float gameOverY = 10f;

    private bool gameOverTriggered = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasCollided) return;

        hasCollided = true;
        OnFirstCollision?.Invoke(this);
    }

    private void Update()
    {
        if (gameOverTriggered) return;

        if (GameManagerStackTower.IsGameOver) return;

        if (transform.position.y >= gameOverY)
        {
            gameOverTriggered = true;
            GameManagerStackTower.TriggerGameOver();
        }
    }
}