using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class ClawController : MonoBehaviour
{
    [Header("Movimiento automático")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float minX = -6f;
    [SerializeField] private float maxX = 6f;

    [Header("Salida")]
    [SerializeField] private float exitY = 8f;
 

    [Header("Referencia")]
    [SerializeField] private ClawGrabber grabber;
    [SerializeField] private ContainerSpawner spawner;
    [Header("Salida lateral")]
    [SerializeField] private float exitSpeed = 8f;
    [SerializeField] private float exitOffset = 3f;
    private bool isActive = false;
    private bool isHolding = false;
    private int direction = 1;

    private bool isExiting = false;

    private void Update()
{
    if (GameManagerStackTower.IsGameOver) return;

    if (isExiting) return;

    HandleInput();

    if (isActive)
    {
        AutoMove();
    }
}

    // 🔹 Movimiento horizontal
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

    // 🔹 INPUT
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
            grabber.Release();
        }
    }

    // 🔴 LLAMADO DESDE EL EVENTO
    public void StartExitSequence()
    {
        if (isExiting) return;

        StartCoroutine(ExitRoutine());
    }

    private IEnumerator ExitRoutine()
{
    isExiting = true;

    float exitTargetX = maxX + exitOffset;
    float returnStartX = minX - exitOffset;

    // 👉 1. Salir
    while (transform.position.x < exitTargetX)
    {
        transform.position += Vector3.right * exitSpeed * Time.deltaTime;
        yield return null;
    }

    // 👉 2. Spawn
    GameObject newContainer = spawner.Spawn();

    // 👉 3. Forzar que la garra lo tenga
    grabber.ForceGrab(newContainer);

    // 👉 4. Teleport fuera de cámara (izquierda)
    Vector3 pos = transform.position;
    pos.x = returnStartX;
    transform.position = pos;

    // 👉 5. Volver al rango
    while (transform.position.x < minX)
    {
        transform.position += Vector3.right * exitSpeed * Time.deltaTime;
        yield return null;
    }

    // 👉 6. Reset estado
    isExiting = false;
    isHolding = true;
}
}