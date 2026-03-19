using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Representa una ficha individual del rompecabezas.
/// Gestiona su interacción (click, drag, rotación), validación de colocación,
/// animaciones y prioridad visual dentro del tablero.
/// </summary>
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class PuzzlePiece : MonoBehaviour
{
    #region Components & References

    /// <summary>
    /// Renderer encargado de mostrar visualmente la ficha.
    /// Se utiliza para controlar el sorting order.
    /// </summary>
    private SpriteRenderer spriteRenderer;

    /// <summary>
    /// Referencia al gestor principal del tablero.
    /// Permite consultar prioridades visuales y notificar colocaciones.
    /// </summary>
    private PuzzleBoardManager boardManager;

    /// <summary>
    /// Cámara principal usada para convertir coordenadas de pantalla a mundo.
    /// </summary>
    private Camera mainCamera;

    #endregion

    #region Position & Transform Data

    /// <summary>
    /// Posición correcta donde la ficha debe encajar.
    /// </summary>
    private Vector3 correctPosition;

    /// <summary>
    /// Posición inicial de la ficha al comenzar el puzzle o tras un shuffle.
    /// </summary>
    private Vector3 startPosition;

    /// <summary>
    /// Offset entre la posición del puntero y la ficha al iniciar un drag.
    /// Evita saltos bruscos al arrastrar.
    /// </summary>
    private Vector3 dragOffset;

    /// <summary>
    /// Posición del puntero en el momento exacto del press.
    /// Se usa para distinguir click de drag.
    /// </summary>
    private Vector3 pointerDownPosition;

    /// <summary>
    /// Profundidad Z fija asignada durante la interacción actual.
    /// </summary>
    private float interactionZ;

    #endregion

    #region Interaction State

    /// <summary>
    /// Indica si el puntero está presionado actualmente sobre la ficha.
    /// </summary>
    private bool isPointerDown;

    /// <summary>
    /// Indica si la ficha ya fue colocada correctamente y bloqueada.
    /// </summary>
    private bool isLocked;

    /// <summary>
    /// Evita que una misma interacción dispare múltiples rotaciones.
    /// </summary>
    private bool rotationConsumed;

    /// <summary>
    /// Tipos de interacción posibles con la ficha.
    /// </summary>
    private enum PointerInteractionType
    {
        None,
        ClickCandidate,
        Drag
    }

    /// <summary>
    /// Tipo de interacción actual detectada.
    /// </summary>
    private PointerInteractionType interactionType = PointerInteractionType.None;

    #endregion

    #region Rotation & Validation

    /// <summary>
    /// Rotación actual de la ficha en grados.
    /// </summary>
    private int currentRotation;

    /// <summary>
    /// Rotación correcta esperada para validar la ficha.
    /// </summary>
    private const int correctRotation = 0;

    /// <summary>
    /// Distancia máxima permitida para que la ficha encaje.
    /// </summary>
    private const float snapDistance = 0.3f;

    /// <summary>
    /// Distancia mínima para considerar que una interacción es un drag.
    /// </summary>
    private const float dragThreshold = 0.15f;

    /// <summary>
    /// Escala global aplicada a la ficha.
    /// </summary>
    private float pieceScale;

    /// <summary>
    /// Evento disparado cuando la pieza realiza un movimiento válido.
    /// </summary>
    public event System.Action OnPieceAction;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa referencias internas necesarias para la ficha.
    /// </summary>
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Asegura que todos los tweens asociados a este Transform
    /// sean eliminados cuando el objeto es destruido.
    /// 
    /// Previene errores como MissingReferenceException cuando
    /// DOTween intenta animar un objeto ya destruido.
    /// </summary>
    private void OnDestroy()
    {
        if (transform != null)
        {
            transform.DOKill();
        }
    }

    /// <summary>
    /// Actualiza la interacción del usuario mientras la ficha no esté bloqueada.
    /// </summary>
    private void Update()
    {
        if (isLocked)
            return;

        HandlePointerInput();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Inicializa la ficha con su sprite, posición correcta,
    /// referencia al tablero y escala global.
    /// </summary>
    public void Initialize(
        Sprite sprite,
        Vector3 targetPosition,
        PuzzleBoardManager manager,
        float globalScale)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 2;

        boardManager = manager;
        pieceScale = globalScale;

        correctPosition = targetPosition;
        startPosition = targetPosition;

        transform.position = targetPosition;
        transform.localScale = Vector3.one * pieceScale;

        GetComponent<BoxCollider2D>().size = sprite.bounds.size;

        currentRotation = 0;
        transform.rotation = Quaternion.identity;
    }

    #endregion

    #region Pointer Handling

    /// <summary>
    /// Gestiona la entrada del puntero separando claramente
    /// click, drag y liberación.
    /// </summary>
    private void HandlePointerInput()
    {
        if (Pointer.current == null)
            return;

        Vector3 pointerWorldPosition = GetPointerWorldPosition();

        if (Pointer.current.press.wasPressedThisFrame && IsPointerOverPiece(pointerWorldPosition))
        {
            OnPointerDown(pointerWorldPosition);
        }

        if (isPointerDown && Pointer.current.press.isPressed)
        {
            HandleDrag(pointerWorldPosition);
        }

        if (isPointerDown && Pointer.current.press.wasReleasedThisFrame)
        {
            OnPointerUp(pointerWorldPosition);
        }
    }

    /// <summary>
    /// Maneja el inicio de una interacción sobre la ficha.
    /// </summary>
    private void OnPointerDown(Vector3 pointerWorldPosition)
    {
        isPointerDown = true;
        rotationConsumed = false;

        interactionType = PointerInteractionType.ClickCandidate;

        pointerDownPosition = pointerWorldPosition;
        dragOffset = transform.position - pointerWorldPosition;

        BringToFront();

        transform.DOScale(Vector3.one * pieceScale, 0.15f);
    }

    /// <summary>
    /// Maneja el arrastre de la ficha mientras el puntero está presionado.
    /// </summary>
    private void HandleDrag(Vector3 pointerWorldPosition)
    {
        if (interactionType == PointerInteractionType.Drag)
        {
            Vector3 newPosition = pointerWorldPosition + dragOffset;
            newPosition.z = interactionZ;
            transform.position = newPosition;
            return;
        }

        float distance = Vector3.Distance(pointerDownPosition, pointerWorldPosition);

        if (distance > dragThreshold)
        {
            interactionType = PointerInteractionType.Drag;
        }

        if (interactionType == PointerInteractionType.Drag)
        {
            Vector3 newPosition = pointerWorldPosition + dragOffset;
            newPosition.z = interactionZ;
            transform.position = newPosition;
        }
    }

    /// <summary>
    /// Maneja la liberación del puntero y decide si es drag o click.
    /// </summary>
    private void OnPointerUp(Vector3 pointerWorldPosition)
    {
        isPointerDown = false;

        if (interactionType == PointerInteractionType.Drag)
        {
            ValidatePlacement();
            interactionType = PointerInteractionType.None;
            return;
        }

        if (interactionType != PointerInteractionType.ClickCandidate)
            return;

        if (rotationConsumed)
            return;

        rotationConsumed = true;
        interactionType = PointerInteractionType.None;

        Rotate90();
    }

    #endregion

    #region Rotation & Placement

    /// <summary>
    /// Rota la ficha 90 grados en sentido horario.
    /// </summary>
    private void Rotate90()
    {
        BringToFront();
        transform.DOKill();

        currentRotation = (currentRotation + 90) % 360;

        transform
            .DORotate(new Vector3(0f, 0f, currentRotation), 0.15f)
            .SetEase(Ease.OutQuad);

        ValidatePlacement();
    }

    /// <summary>
    /// Valida si la ficha está correctamente colocada
    /// en posición y rotación.
    /// </summary>
    private void ValidatePlacement()
    {
        NotifyMoveCompleted();
        Vector2 currentXY = transform.position;
        Vector2 correctXY = correctPosition;

        if (Vector2.Distance(currentXY, correctXY) <= snapDistance &&
            IsRotationCorrect())
        {
            LockPiece();
        }
        else
        {
            ReturnToStart();
        }
    }

    /// <summary>
    /// Indica si la rotación actual coincide con la correcta.
    /// </summary>
    private bool IsRotationCorrect()
    {
        return Mathf.Abs(Mathf.DeltaAngle(currentRotation, correctRotation)) < 1f;
    }

    /// <summary>
    /// Bloquea la ficha en su posición correcta y notifica al tablero.
    /// </summary>
    private void LockPiece()
    {
        isLocked = true;

        transform.DOMove(new Vector3(correctPosition.x, correctPosition.y, interactionZ), 0.2f);
        transform.DORotate(Vector3.zero, 0.2f);
        transform.DOScale(Vector3.one * pieceScale, 0.15f);

        boardManager.NotifyPiecePlaced(gameObject.transform.position);
    }

    /// <summary>
    /// Devuelve la ficha a su posición inicial.
    /// </summary>
    private void ReturnToStart()
    {
        transform.DOScale(Vector3.one * pieceScale * 0.9f, 0.15f);
    }

    #endregion

    #region Randomization & Movement

    /// <summary>
    /// Aplica una rotación aleatoria válida (múltiplos de 90°).
    /// </summary>
    private void ApplyRandomRotation()
    {
        int[] rotations = { 0, 90, 180, 270 };
        currentRotation = rotations[Random.Range(0, rotations.Length)];

        transform.DORotate(new Vector3(0f, 0f, currentRotation), 0.3f);
    }

    /// <summary>
    /// Mueve la ficha a una nueva posición inicial y
    /// aplica una rotación aleatoria.
    /// </summary>
    public void MoveToStartPosition(Vector3 position)
    {
        startPosition = position;

        transform.DOMove(new Vector3(position.x, position.y, interactionZ), 0.6f)
            .SetEase(Ease.InOutQuad);

        transform.DOScale(Vector3.one * pieceScale * 0.9f, 0.6f);

        ApplyRandomRotation();
    }

    #endregion

    #region Utility

    /// <summary>
    /// Determina si el puntero se encuentra sobre la ficha.
    /// </summary>
    private bool IsPointerOverPiece(Vector3 position)
    {
        Collider2D hit = Physics2D.OverlapPoint(position);
        return hit != null && hit.gameObject == gameObject;
    }

    /// <summary>
    /// Convierte la posición del puntero de pantalla a mundo.
    /// </summary>
    private Vector3 GetPointerWorldPosition()
    {
        Vector3 screenPosition = Pointer.current.position.ReadValue();
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0f;
        return worldPosition;
    }

    /// <summary>
    /// Llamar cuando la pieza termina de moverse.
    /// </summary>
    private void NotifyMoveCompleted()
    {
        OnPieceAction?.Invoke();
    }

    #endregion

    #region Visual Priority & Animations

    /// <summary>
    /// Eleva visualmente la ficha asignándole la siguiente prioridad
    /// de render y profundidad Z.
    /// </summary>
    private void BringToFront()
    {
        VisualPriority priority = boardManager.GetNextVisualPriority();

        spriteRenderer.sortingOrder = priority.SortingOrder;

        Vector3 position = transform.position;
        position.z = priority.ZPosition;
        transform.position = position;

        interactionZ = priority.ZPosition;
    }

    /// <summary>
    /// Asigna la prioridad visual inicial a la ficha.
    /// Debe llamarse al iniciar el puzzle.
    /// </summary>
    public void AssignInitialVisualPriority()
    {
        BringToFront();
    }

    /// <summary>
    /// Reproduce la animación de entrada de la ficha.
    /// </summary>
    public void PlayEntryAnimation(float delay)
    {
        transform
            .DOScale(Vector3.one * pieceScale, 0.35f)
            .SetEase(Ease.OutBack)
            .SetDelay(delay);
    }

    /// <summary>
    /// Reproduce la animación de salida de la ficha.
    /// </summary>
    public void PlayExitAnimation()
    {
        transform
            .DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InBack);
    }

    #endregion
}
