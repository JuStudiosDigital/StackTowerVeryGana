using UnityEngine;

public class ClawGrabber : MonoBehaviour
{
    [SerializeField] private Transform grabPoint;
    [SerializeField] private float grabRadius = 0.8f;
    [SerializeField] private LayerMask grabbableLayer;

    private Rigidbody2D grabbedBody;
    private Transform grabbedTransform;

    private void Start()
    {
        // 🔒 Al inicio: todos los contenedores quietos
        var all = GameObject.FindObjectsByType<Rigidbody2D>(FindObjectsSortMode.None);
        foreach (var rb in all)
        {
            if (((1 << rb.gameObject.layer) & grabbableLayer) != 0)
            {
                rb.bodyType = RigidbodyType2D.Kinematic;
            }
        }
    }

    public void TryGrab()
    {
        if (grabbedBody != null) return;

        Collider2D hit = Physics2D.OverlapCircle(
            grabPoint.position,
            grabRadius,
            grabbableLayer
        );

        if (hit == null) return;

        grabbedBody = hit.attachedRigidbody;
        grabbedTransform = hit.transform;

        // 🔗 Pegar a la garra
        grabbedTransform.SetParent(grabPoint);
        grabbedTransform.localPosition = Vector3.zero;
    }

    public void Release()
    {
        if (grabbedBody == null) return;

        // 🔓 Activar física
        grabbedTransform.SetParent(null);
        grabbedBody.bodyType = RigidbodyType2D.Dynamic;

        grabbedBody = null;
        grabbedTransform = null;
    }

    private void OnDrawGizmos()
    {
        if (grabPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(grabPoint.position, grabRadius);
    }
    public void ForceGrab(GameObject obj)
{
    var rb = obj.GetComponent<Rigidbody2D>();
    if (rb == null) return;

    grabbedBody = rb;
    grabbedTransform = obj.transform;

    // 🔒 modo quieto
    grabbedBody.bodyType = RigidbodyType2D.Kinematic;

    // 🔗 pegar a la garra
    grabbedTransform.SetParent(grabPoint);
    grabbedTransform.localPosition = Vector3.zero;
}
}