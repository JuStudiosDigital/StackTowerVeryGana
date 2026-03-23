using UnityEngine;

/// <summary>
/// Asigna un sprite aleatorio a la nube.
/// Permite refrescar el sprite dinámicamente.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CloudRandomSprite : MonoBehaviour
{
    [Header("Configuración de sprites")]

    [SerializeField]
    [Tooltip("Lista de sprites posibles para la nube.")]
    private Sprite[] sprites;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyRandomSprite();
    }

    /// <summary>
    /// Aplica un sprite aleatorio.
    /// </summary>
    public void RefreshSprite()
    {
        ApplyRandomSprite();
    }

    private void ApplyRandomSprite()
    {
        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning("CloudRandomSprite: No hay sprites asignados.");
            return;
        }

        int index = Random.Range(0, sprites.Length);
        spriteRenderer.sprite = sprites[index];
    }
}