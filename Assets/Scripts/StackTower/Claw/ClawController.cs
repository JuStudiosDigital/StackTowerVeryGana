using UnityEngine;
using System.Collections;

/// <summary>
/// Controlador principal de la garra.
/// Gestiona movimiento, input y flujo de gameplay.
/// </summary>
public class ClawController : MonoBehaviour
{
    private enum ClawState
    {
        Idle,
        Moving,
        Holding,
        Exiting,
        GameOver
    }

    [Header("Velocidad progresiva")]
    [SerializeField] private float startSpeed = 4f;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 0.5f;

    [Header("Movimiento automático")]
    [SerializeField] private float minX = -6f;
    [SerializeField] private float maxX = 6f;

    [Header("Salida lateral")]
    [SerializeField] private float exitSpeed = 8f;
    [SerializeField] private float exitOffset = 3f;

    [Header("Posición de reinicio")]
    [SerializeField] private float returnX = -8f;

    [Header("Referencias")]
    [SerializeField] private ClawGrabber grabber;
    [SerializeField] private ContainerSpawner spawner;
    [SerializeField] private Animator clawAnimator;

    private float currentSpeed;
    private int direction = 1;

    private ClawState currentState = ClawState.Idle;

    private void Start()
    {
        currentSpeed = startSpeed;
    }

    private void OnEnable()
    {
        GameManagerStackTower.OnGameOver += HandleGameOver;
        ClawInput.OnPress += HandlePress;
    }

    private void OnDisable()
    {
        GameManagerStackTower.OnGameOver -= HandleGameOver;
        ClawInput.OnPress -= HandlePress;
    }

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

    /// <summary>
    /// Movimiento horizontal automático.
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

    /// <summary>
    /// Manejo de input desacoplado.
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

    private void ReleaseContainer()
    {
        grabber.Release();
    }

    /// <summary>
    /// Inicia la secuencia de salida lateral.
    /// </summary>
    public void StartExitSequence()
    {
        if (currentState == ClawState.Exiting) return;

        StartCoroutine(ExitRoutine());
    }

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

    /// <summary>
    /// Detiene completamente la garra en su posición actual al hacer Game Over.
    /// </summary>
    private void HandleGameOver()
    {
        currentState = ClawState.GameOver;

        // 🔴 Detener cualquier movimiento activo (coroutines)
        StopAllCoroutines();

        // 🔴 Detener aceleración
        currentSpeed = 0f;
    }
}