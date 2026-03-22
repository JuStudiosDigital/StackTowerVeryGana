using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ClawController : MonoBehaviour
{
    [Header("Movimiento automático")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float minX = -6f;
    [SerializeField] private float maxX = 6f;

    [Header("Salida lateral")]
    [SerializeField] private float exitSpeed = 8f;
    [SerializeField] private float exitOffset = 3f;

    [Header("Posición de reinicio")]
    [SerializeField] private float returnX = -8f; // 👈 configurable desde inspector

    [Header("Referencia")]
    [SerializeField] private ClawGrabber grabber;
    [SerializeField] private ContainerSpawner spawner;
      [Header("Animator")]
        [SerializeField] private Animator clawAnimator;
    private bool isActive = false;
    private bool isHolding = false;
    private bool isExiting = false;
    private bool isGameOver = false;
    private int direction = 1;

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
    if (isGameOver) return; // 🔴 clave
    if (isExiting) return;

    HandleInput();

    if (isActive)
    {
        AutoMove();
    }
}

    // 🔹 Movimiento horizontal automático
    private void AutoMove()
    {
        Vector3 pos = transform.position;

        pos.x += direction * speed * Time.deltaTime;

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

    // 🔹 Input (click / espacio / touch)
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
            clawAnimator.SetTrigger("open");
            Invoke("ReleaseContainer",0.2f);
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
        float returnStartX = returnX; // 👈 configurable

        // 👉 1. Salir hacia la derecha
        while (transform.position.x < exitTargetX)
        {
            transform.position += Vector3.right * exitSpeed * Time.deltaTime;
            yield return null;
        }

        // 👉 2. Spawn nuevo container
        clawAnimator.SetTrigger("hold");
        GameObject newContainer = spawner.Spawn();

        // 👉 3. Forzar agarre
        grabber.ForceGrab(newContainer);

        // 👉 4. Teleport a posición definida
        Vector3 pos = transform.position;
        pos.x = returnStartX;
        transform.position = pos;

        // 👉 5. Volver al rango normal
        while (transform.position.x < minX)
        {
            transform.position += Vector3.right * exitSpeed * Time.deltaTime;
            yield return null;
        }

        // 👉 6. Reset estado
        isExiting = false;
        isHolding = true;
    }
    private void HandleGameOver()
{
    isGameOver = true;

    // detener interacción
    isActive = false;
    isHolding = false;

    // iniciar salida final
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

    // 👉 aquí termina para siempre (no vuelve)
}
}