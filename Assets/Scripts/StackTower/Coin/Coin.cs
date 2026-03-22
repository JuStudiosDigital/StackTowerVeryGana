using UnityEngine;

public class Coin : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 👉 detectar contenedores
        if (collision.gameObject.CompareTag("Container"))
        {
            Destroy(gameObject);
        }
    }
}