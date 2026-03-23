using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CloudRandomSprite : MonoBehaviour
{
    [Header("Sprites disponibles")]
    [SerializeField] private Sprite[] sprites;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyRandomSprite();
    }

    private void ApplyRandomSprite()
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("CloudRandomSprite: No hay sprites asignados");
            return;
        }

        int index = Random.Range(0, sprites.Length);
        spriteRenderer.sprite = sprites[index];
    }

    // ✅ ESTE ES EL MÉTODO QUE TE FALTA
    public void RefreshSprite()
    {
        ApplyRandomSprite();
    }
}