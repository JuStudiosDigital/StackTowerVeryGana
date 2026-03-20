using UnityEngine;
using System.Collections;

public class CameraStepper : MonoBehaviour
{
    [Header("Movimiento vertical")]
    [SerializeField] private float stepHeight = 2.5f;
    [SerializeField] private float moveDuration = 0.5f;

    private bool isMoving = false;

    private void OnEnable()
    {
        Container.OnFirstCollision += HandleStep;
    }

    private void OnDisable()
    {
        Container.OnFirstCollision -= HandleStep;
    }

    private void HandleStep(Container container)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveUpRoutine());
        }
    }

    private IEnumerator MoveUpRoutine()
    {
        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + Vector3.up * stepHeight;

        float time = 0f;

        while (time < moveDuration)
        {
            time += Time.deltaTime;

            float t = time / moveDuration;

            // 👉 SmoothStep para evitar movimiento brusco
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
    }
}