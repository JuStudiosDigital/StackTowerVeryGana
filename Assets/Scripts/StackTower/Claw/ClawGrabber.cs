using UnityEngine;

/// <summary>
/// Se encarga de detectar, agarrar y soltar objetos.
/// </summary>
public class ClawGrabber : MonoBehaviour
{
    [Header("Configuración de agarre")]

    [SerializeField]
    [Tooltip("Punto donde se sujetan los objetos.")]
    private Transform grabPoint;

    [SerializeField]
    [Tooltip("Radio de detección para encontrar objetos agarrables.")]
    private float grabRadius = 0.8f;

    [SerializeField]
    [Tooltip("Capas válidas para agarrar.")]
    private LayerMask grabbableLayer;

    private Rigidbody2D grabbedBody;
    private Transform grabbedTransform;

    /// <summary>
    /// Indica si actualmente hay un objeto agarrado.
    /// </summary>
    public bool IsHolding => grabbedBody != null;

    private void Start()
    {
        InitializeGrabbables();
    }

    /// <summary>
    /// Inicializa los objetos agarrables como Kinematic al inicio.
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

    /// <summary>
    /// Intenta agarrar un objeto dentro del radio.
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
    /// Fuerza el agarre de un objeto específico.
    /// </summary>
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
    /// Libera el objeto actualmente agarrado.
    /// </summary>
    public void Release()
    {
        if (!IsHolding) return;

        grabbedTransform.SetParent(null);
        grabbedBody.bodyType = RigidbodyType2D.Dynamic;

        grabbedBody = null;
        grabbedTransform = null;
    }

    /// <summary>
    /// Asocia un objeto al punto de agarre.
    /// </summary>
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

    private void OnDrawGizmos()
    {
        if (grabPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
    }
}