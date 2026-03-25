using UnityEngine;

/// <summary>
/// Gestiona la detección, sujeción y liberación de objetos interactuables.
/// Controla el estado físico de los objetos agarrados y su vinculación al punto de agarre.
/// </summary>
public class ClawGrabber : MonoBehaviour
{
    #region Inspector

    [Header("Configuración de agarre")]

    [SerializeField]
    [Tooltip("Transform que define el punto exacto donde se posicionan los objetos agarrados.")]
    private Transform grabPoint;

    [SerializeField]
    [Tooltip("Radio de detección utilizado para identificar objetos agarrables cercanos.")]
    private float grabRadius = 0.8f;

    [SerializeField]
    [Tooltip("Máscara de capas que define qué objetos pueden ser agarrados.")]
    private LayerMask grabbableLayer;

    #endregion

    #region State

    /// <summary>
    /// Referencia al Rigidbody2D actualmente agarrado.
    /// </summary>
    private Rigidbody2D grabbedBody;

    /// <summary>
    /// Referencia al Transform del objeto actualmente agarrado.
    /// </summary>
    private Transform grabbedTransform;

    /// <summary>
    /// Indica si actualmente existe un objeto agarrado.
    /// </summary>
    public bool IsHolding => grabbedBody != null;

    #endregion

    #region Unity

    /// <summary>
    /// Inicializa el estado físico de los objetos agarrables al inicio de la escena.
    /// </summary>
    private void Start()
    {
        InitializeGrabbables();
    }

    /// <summary>
    /// Dibuja una representación visual del área de detección en el editor.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (grabPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Configura todos los objetos dentro de las capas válidas como Kinematic
    /// para evitar interacción física antes de ser agarrados.
    /// </summary>
    private void InitializeGrabbables()
    {
        var all = FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);

        foreach (var rb in all)
        {
            if (((1 << rb.gameObject.layer) & grabbableLayer) != 0)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Intenta detectar y agarrar un objeto dentro del radio definido.
    /// </summary>
    public void TryGrab()
    {
        if (IsHolding) return;

        Collider2D hit = Physics2D.OverlapCircle(
            grabPoint.position,
            grabRadius,
            grabbableLayer
        );

        if (hit == null) return;

        Attach(hit.attachedRigidbody, hit.transform);
    }

    /// <summary>
    /// Fuerza el agarre de un objeto específico, validando sus componentes requeridos.
    /// </summary>
    /// <param name="obj">Objeto que será forzado a ser agarrado.</param>
    public void ForceGrab(GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning("ForceGrab: objeto es null");
            return;
        }

        if (!obj.TryGetComponent(out Rigidbody2D rb))
        {
            Debug.LogWarning("ForceGrab: el objeto no tiene Rigidbody2D");
            return;
        }

        Attach(rb, obj.transform);
    }

    /// <summary>
    /// Libera el objeto actualmente agarrado, restaurando su comportamiento físico.
    /// </summary>
    public void Release()
    {
        if (!IsHolding) return;

        grabbedTransform.SetParent(null);
        grabbedBody.bodyType = RigidbodyType2D.Dynamic;

        grabbedBody = null;
        grabbedTransform = null;
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Asocia un objeto al punto de agarre y ajusta su estado físico para mantenerlo fijo.
    /// </summary>
    /// <param name="rb">Rigidbody2D del objeto a sujetar.</param>
    /// <param name="target">Transform del objeto a sujetar.</param>
    private void Attach(Rigidbody2D rb, Transform target)
    {
        grabbedBody = rb;
        grabbedTransform = target;

        grabbedBody.linearVelocity = Vector2.zero;
        grabbedBody.angularVelocity = 0f;
        grabbedBody.bodyType = RigidbodyType2D.Kinematic;

        grabbedTransform.SetParent(grabPoint);
        grabbedTransform.localPosition = Vector3.zero;
    }

    #endregion
}