using UnityEngine;
using System.Collections;

/// <summary>
/// Controlador principal de la garra.
/// Gestiona el movimiento horizontal automático, la interacción mediante input
/// y la transición entre estados del flujo de juego.
/// </summary>
public class ClawController : MonoBehaviour
{
    #region Types

    /// <summary>
    /// Define los estados posibles de la garra durante el ciclo de juego.
    /// </summary>
    private enum ClawState
    {
        Idle,
        Moving,
        Holding,
        Exiting,
        GameOver
    }

    #endregion

    #region Inspector

    [Header("Velocidad progresiva")]

    [SerializeField]
    [Tooltip("Velocidad inicial de desplazamiento horizontal de la garra.")]
    private float startSpeed = 4f;

    [SerializeField]
    [Tooltip("Velocidad máxima que puede alcanzar la garra.")]
    private float maxSpeed = 10f;

    [SerializeField]
    [Tooltip("Incremento de velocidad aplicado progresivamente en el tiempo.")]
    private float acceleration = 0.5f;

    [Header("Movimiento automático")]

    [SerializeField]
    [Tooltip("Límite mínimo en el eje X para el movimiento de la garra.")]
    private float minX = -6f;

    [SerializeField]
    [Tooltip("Límite máximo en el eje X para el movimiento de la garra.")]
    private float maxX = 6f;

    [Header("Salida lateral")]

    [SerializeField]
    [Tooltip("Velocidad utilizada durante la secuencia de salida lateral.")]
    private float exitSpeed = 8f;

    [SerializeField]
    [Tooltip("Distancia adicional fuera del límite horizontal antes de reiniciar la posición.")]
    private float exitOffset = 3f;

    [Header("Posición de reinicio")]

    [SerializeField]
    [Tooltip("Posición en el eje X a la que se reposiciona la garra tras la salida lateral.")]
    private float returnX = -8f;

    [Header("Referencias")]

    [SerializeField]
    [Tooltip("Componente encargado de gestionar el agarre y liberación de objetos.")]
    private ClawGrabber grabber;

    [SerializeField]
    [Tooltip("Sistema responsable de generar nuevos contenedores.")]
    private ContainerSpawner spawner;

    [SerializeField]
    [Tooltip("Animator utilizado para controlar las animaciones de la garra.")]
    private Animator clawAnimator;

    #endregion

    #region State

    /// <summary>
    /// Velocidad actual de la garra.
    /// </summary>
    private float currentSpeed;

    /// <summary>
    /// Dirección de movimiento horizontal (1 derecha, -1 izquierda).
    /// </summary>
    private int direction = 1;

    /// <summary>
    /// Estado actual de la garra.
    /// </summary>
    private ClawState currentState = ClawState.Idle;

    #endregion

    #region Unity

    /// <summary>
    /// Inicializa la velocidad base de la garra.
    /// </summary>
    private void Start()
    {
        currentSpeed = startSpeed;
    }

    /// <summary>
    /// Suscribe los eventos necesarios para el control de input y estado global.
    /// </summary>
    private void OnEnable()
    {
        StackTowerEvents.OnGameOver += HandleGameOver;
        ClawInput.OnPress += HandlePress;
    }

    /// <summary>
    /// Desuscribe los eventos para evitar referencias inválidas.
    /// </summary>
    private void OnDisable()
    {
        StackTowerEvents.OnGameOver -= HandleGameOver;
        ClawInput.OnPress -= HandlePress;
    }

    /// <summary>
    /// Ejecuta la lógica de movimiento continuo de la garra según su estado actual.
    /// </summary>
    private void Update()
    {
        if (currentState == ClawState.GameOver) return;
        if (currentState == ClawState.Exiting) return;

        if (currentState == ClawState.Moving || currentState == ClawState.Holding)
        {
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            AutoMove();
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// Aplica movimiento horizontal automático dentro de los límites definidos,
    /// invirtiendo la dirección al alcanzar los extremos.
    /// </summary>
    private void AutoMove()
    {
        Vector3 pos = transform.position;

        pos.x += direction * currentSpeed * Time.deltaTime;

        if (pos.x >= maxX)
        {
            pos.x = maxX;
            direction = -1;
        }
        else if (pos.x <= minX)
        {
            pos.x = minX;
            direction = 1;
        }

        transform.position = pos;
    }

    #endregion

    #region Input

    /// <summary>
    /// Maneja la entrada del usuario para controlar el comportamiento de la garra
    /// de forma desacoplada del sistema de input.
    /// </summary>
    private void HandlePress()
    {
        if (currentState == ClawState.GameOver) return;
        if (currentState == ClawState.Exiting) return;

        if (currentState == ClawState.Idle)
        {
            currentState = ClawState.Holding;
            grabber.TryGrab();
        }
        else if (currentState == ClawState.Holding)
        {
            currentState = ClawState.Moving;

            clawAnimator.SetTrigger("open");
            Invoke(nameof(ReleaseContainer), 0.2f);
        }
    }

    /// <summary>
    /// Libera el contenedor actualmente sujeto por la garra.
    /// </summary>
    private void ReleaseContainer()
    {
        grabber.Release();
    }

    #endregion

    #region Flow

    /// <summary>
    /// Inicia la secuencia de salida lateral de la garra.
    /// </summary>
    public void StartExitSequence()
    {
        if (currentState == ClawState.Exiting) return;

        StartCoroutine(ExitRoutine());
    }

    /// <summary>
    /// Ejecuta la secuencia completa de salida, reposicionamiento y reentrada de la garra.
    /// </summary>
    private IEnumerator ExitRoutine()
    {
        currentState = ClawState.Exiting;

        float exitTargetX = maxX + exitOffset;

        while (transform.position.x < exitTargetX)
        {
            transform.position += Vector3.right * exitSpeed * Time.deltaTime;
            yield return null;
        }

        clawAnimator.SetTrigger("hold");

        GameObject newContainer = spawner.Spawn();

        grabber.ForceGrab(newContainer);

        Vector3 pos = transform.position;
        pos.x = returnX;
        transform.position = pos;

        while (transform.position.x < minX)
        {
            transform.position += Vector3.right * exitSpeed * Time.deltaTime;
            yield return null;
        }

        currentState = ClawState.Holding;
    }

    #endregion

    #region Handlers

    /// <summary>
    /// Detiene completamente la garra al producirse un evento de fin de juego.
    /// </summary>
    private void HandleGameOver()
    {
        currentState = ClawState.GameOver;

        StopAllCoroutines();

        currentSpeed = 0f;
    }

    #endregion
}