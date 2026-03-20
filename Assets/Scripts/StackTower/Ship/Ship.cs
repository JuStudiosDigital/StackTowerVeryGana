using UnityEngine;

public class Ship : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Container"))
        {
            GameManagerStackTower.TriggerGameOver();
        }
    }
}