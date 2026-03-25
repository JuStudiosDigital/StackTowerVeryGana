using UnityEngine;
using System.Collections;

/// <summary>
/// Controla el desplazamiento vertical de la cámara en respuesta a eventos del gameplay.
/// Implementa un movimiento escalonado suave basado en interpolación temporal.
/// </summary>
public class CameraStepper : MonoBehaviour
{
    #region Inspector

    [Header("Movimiento vertical")]

    [SerializeField]
    [Tooltip("Altura que la cámara asciende en cada paso.")]
    private float stepHeight = 2.5f;

    [SerializeField]
    [Tooltip("Duración del desplazamiento vertical en segundos.")]
    private float moveDuration = 0.5f;

    [Header("Sistema de nubes")]

    [SerializeField]
    [Tooltip("Referencia opcional al sistema de nubes para sincronización visual.")]
    private CloudSystem cloudSystem;

    #endregion

    #region State

    /// <summary>
    /// Indica si la cámara se encuentra actualmente en movimiento.
    /// Evita superposición de múltiples desplazamientos simultáneos.
    /// </summary>
    private bool isMoving = false;

    #endregion

    #region Unity

    /// <summary>
    /// Suscribe el componente al evento de primera colisión de contenedores.
    /// </summary>
    private void OnEnable()
    {
        Container.OnFirstCollision += HandleStep;
    }

    /// <summary>
    /// Desuscribe el componente del evento para prevenir referencias inválidas.
    /// </summary>
    private void OnDisable()
    {
        Container.OnFirstCollision -= HandleStep;
    }

    #endregion

    #region Event Handlers

    /// <summary>
    /// Maneja el evento de colisión de un contenedor y desencadena el desplazamiento vertical.
    /// </summary>
    /// <param name="container">Contenedor que provocó el evento.</param>
    private void HandleStep(Container container)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveUpRoutine());
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// Ejecuta el movimiento vertical interpolado de la cámara utilizando una curva suavizada.
    /// </summary>
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
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.position = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        transform.position = targetPos;

        isMoving = false;
    }

    #endregion
}