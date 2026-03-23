using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ClawController : MonoBehaviour
{
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

    [Header("Referencia")]
    [SerializeField] private ClawGrabber grabber;
    [SerializeField] private ContainerSpawner spawner;

    [Header("Animator")]
    [SerializeField] private Animator clawAnimator;

    private float currentSpeed;

    private bool isActive = false;
    private bool isHolding = false;
    private bool isExiting = false;
    private bool isGameOver = false;
    private int direction = 1;

    private void Start()
    {
        currentSpeed = startSpeed;
    }

    private void OnEnable()
    {
        GameManagerStackTower.OnGameOver += HandleGameOver;
    }

    private void OnDisable()
    {
        GameManagerStackTower.OnGameOver -= HandleGameOver;
    }

    private void Update()
    {
        if (isGameOver) return;
        if (isExiting) return;

        HandleInput();

        if (isActive)
        {
            // 🔥 velocidad progresiva
            currentSpeed += acceleration * Time.deltaTime;
            currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

            AutoMove();
        }
    }

    // 🔹 Movimiento horizontal automático
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

    // 🔹 Input
    private void HandleInput()
    {
        bool pressed = false;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            pressed = true;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.enterKey.wasPressedThisFrame)
                pressed = true;
        }

        if (Touchscreen.current != null &&
            Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            pressed = true;

        if (!pressed) return;

        if (!isActive)
        {
            isActive = true;
            isHolding = true;
            grabber.TryGrab();
        }
        else if (isHolding)
        {
            isHolding = false;

            // 🎬 animación de soltar
            clawAnimator.SetTrigger("open");

            Invoke(nameof(ReleaseContainer), 0.2f);
        }
    }

    private void ReleaseContainer()
    {
        grabber.Release();
    }

    // 🔴 Llamado desde ContainerSpawner
    public void StartExitSequence()
    {
        if (isExiting) return;

        StartCoroutine(ExitRoutine());
    }

    private IEnumerator ExitRoutine()
    {
        isExiting = true;

        float exitTargetX = maxX + exitOffset;

        // 👉 salir hacia la derecha
        while (transform.position.x < exitTargetX)
        {
            transform.position += Vector3.right * exitSpeed * Time.deltaTime;
            yield return null;
        }

        // 🎬 animación de agarrar
        clawAnimator.SetTrigger("hold");

        // 👉 spawn
        GameObject newContainer = spawner.Spawn();

        // 👉 forzar agarre
        grabber.ForceGrab(newContainer);

        // 👉 teleport
        Vector3 pos = transform.position;
        pos.x = returnX;
        transform.position = pos;

        // 👉 volver al rango
        while (transform.position.x < minX)
        {
            transform.position += Vector3.right * exitSpeed * Time.deltaTime;
            yield return null;
        }

        isExiting = false;
        isHolding = true;
    }

    private void HandleGameOver()
    {
        isGameOver = true;

        isActive = false;
        isHolding = false;

        StartCoroutine(ExitForeverRoutine());
    }

    private IEnumerator ExitForeverRoutine()
    {
        isExiting = true;

        float exitTargetX = maxX + exitOffset;

        while (transform.position.x < exitTargetX)
        {
            transform.position += Vector3.right * exitSpeed * Time.deltaTime;
            yield return null;
        }
    }
}