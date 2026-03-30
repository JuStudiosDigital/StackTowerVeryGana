using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sistema de mini mapa que representa una proyección escalada del stack real.
/// 
/// Replica:
/// - Posición (X/Y)
/// - Altura
/// - Desalineación
/// - Color visual
/// 
/// No depende de físicas ni jerarquía del mundo.
/// Se alimenta exclusivamente de eventos, lo que lo hace determinista y desacoplado.
/// </summary>
public sealed class StackTowerMiniMapSystem : MonoBehaviour
{
    #region Inspector

    [SerializeField]
    [Tooltip("Prefab visual sin físicas usado para representar cada contenedor.")]
    private GameObject previewPrefab;

    /// <summary>
    /// Factor de escala aplicado a posiciones del mundo.
    /// Controla el tamaño del minimapa respecto al gameplay real.
    /// </summary>
    [SerializeField]
    private float scaleFactor = 0.2f;

    /// <summary>
    /// Offset del minimapa dentro de la cámara.
    /// Permite ubicarlo en pantalla sin depender de UI.
    /// </summary>
    [SerializeField]
    private Vector2 offset = new Vector2(5f, -3f);

    /// <summary>
    /// Padre opcional para organización jerárquica.
    /// No afecta la lógica, solo orden en escena.
    /// </summary>
    [SerializeField]
    private Transform containerParent;

    #endregion

    #region State

    /// <summary>
    /// Lista de instancias visuales activas del minimapa.
    /// Permite limpieza controlada en GameOver.
    /// </summary>
    private readonly List<Transform> previews = new();

    #endregion

    #region Unity

    /// <summary>
    /// Suscribe a eventos del sistema.
    /// 
    /// Nota:
    /// Se usa OnContainerPlaced como fuente de verdad,
    /// evitando depender de físicas o polling.
    /// </summary>
    private void OnEnable()
    {
        StackTowerEvents.OnContainerPlaced += HandlePlaced;
        StackTowerEvents.OnGameOver += HandleGameOver;
    }

    /// <summary>
    /// Desuscripción defensiva.
    /// </summary>
    private void OnDisable()
    {
        StackTowerEvents.OnContainerPlaced -= HandlePlaced;
        StackTowerEvents.OnGameOver -= HandleGameOver;
    }

    #endregion

    #region Handlers

    /// <summary>
    /// Crea una representación visual del contenedor en el minimapa.
    /// 
    /// Flujo:
    /// 1. Obtiene posición real
    /// 2. Aplica escala
    /// 3. Aplica offset visual
    /// 4. Instancia preview
    /// 5. Copia apariencia
    /// </summary>
    private void HandlePlaced(Container container)
    {
        if (container == null || previewPrefab == null)
            return;

        Vector3 world = container.transform.position;

        /// Escalado del espacio de mundo a espacio de minimapa
        Vector3 scaled = new Vector3(
            world.x * scaleFactor,
            world.y * scaleFactor,
            0f
        );

        /// Posición final en escena (mini mapa)
        Vector3 finalPos = transform.position + (Vector3)offset + scaled;

        GameObject instance = Instantiate(
            previewPrefab,
            finalPos,
            Quaternion.identity,
            containerParent
        );

        /// Replica apariencia visual
        CopyColor(container, instance);

        previews.Add(instance.transform);
    }

    #endregion

    #region Visual Sync

    /// <summary>
    /// Copia el color del contenedor real al preview.
    /// 
    /// Nota de diseño:
    /// Se realiza copia por índice para mantener bajo costo.
    /// Se asume misma estructura de renderers entre prefabs.
    /// </summary>
    private void CopyColor(Container source, GameObject target)
    {
        var src = source.GetComponentsInChildren<SpriteRenderer>();
        var dst = target.GetComponentsInChildren<SpriteRenderer>();

        int count = Mathf.Min(src.Length, dst.Length);

        for (int i = 0; i < count; i++)
        {
            dst[i].color = src[i].color;
        }
    }

    #endregion

    #region Cleanup

    /// <summary>
    /// Limpia todas las instancias del minimapa.
    /// 
    /// Se ejecuta en GameOver para evitar acumulación de objetos
    /// entre sesiones.
    /// </summary>
    private void HandleGameOver()
    {
        foreach (var t in previews)
        {
            if (t != null)
                Destroy(t.gameObject);
        }

        previews.Clear();
    }

    #endregion
}