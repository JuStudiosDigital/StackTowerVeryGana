using UnityEngine;

/// <summary>
/// Gestiona la asignación dinámica de sprites para una nube.
/// Permite aplicar variaciones visuales aleatorias durante la inicialización
/// o en tiempo de ejecución mediante actualización explícita.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class CloudRandomSprite : MonoBehaviour
{
    #region Inspector

    [Header("Configuración de sprites")]

    [SerializeField]
    [Tooltip("Colección de sprites disponibles para asignación aleatoria.")]
    private Sprite[] sprites;

    #endregion

    #region References

    /// <summary>
    /// Referencia al componente SpriteRenderer asociado al objeto.
    /// </summary>
    private SpriteRenderer spriteRenderer;

    #endregion

    #region Unity

    /// <summary>
    /// Inicializa las referencias necesarias y aplica un sprite aleatorio inicial.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        ApplyRandomSprite();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Fuerza la asignación de un nuevo sprite aleatorio.
    /// </summary>
    public void RefreshSprite()
    {
        ApplyRandomSprite();
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Selecciona y asigna un sprite aleatorio desde la colección disponible.
    /// Si no hay sprites configurados, se registra una advertencia.
    /// </summary>
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

    #endregion
}