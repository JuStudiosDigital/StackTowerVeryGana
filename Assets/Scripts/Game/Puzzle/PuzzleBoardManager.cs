using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestiona la creación, animación y control general del tablero de rompecabezas.
/// Se encarga de generar las piezas, la imagen guía, el shuffle inicial,
/// las prioridades visuales y el flujo de reinicio y salida.
/// </summary>
public class PuzzleBoardManager :      MonoBehaviour,
    IGameplayMechanic,
    IGuideImageMechanic,
    IPieceRewardSource,
    ICompletionRewardAnchor
{
    #region Inspector Configuration

    [Header("Puzzle Image")]

    /// <summary>
    /// Imagen base utilizada para generar el rompecabezas.
    /// Debe ser cuadrada y divisible por el tamaño de la grilla.
    /// </summary>
    [SerializeField] private Texture2D puzzleImage;

    /// <summary>
    /// Clave lógica del recurso de imagen del puzzle.
    /// </summary>
    [SerializeField] private string puzzleImageResourceKey = "PuzzleImage";

    /// <summary>
    /// Tamaño de la grilla del rompecabezas.
    /// </summary>
    [SerializeField] private PuzzleGridSize gridSize = PuzzleGridSize.Grid4x4;

    [Header("Puzzle Layout")]

    /// <summary>
    /// Centro del tablero en coordenadas de mundo.
    /// </summary>
    [SerializeField] private Vector3 puzzleCenter;

    /// <summary>
    /// Escala global aplicada al rompecabezas completo.
    /// </summary>
    [SerializeField, Range(0.1f, 2f)] private float puzzleScale = 1f;

    /// <summary>
    /// Separación entre piezas del rompecabezas.
    /// </summary>
    [SerializeField] private float pieceSpacing = 0.02f;

    [Header("Guide Image")]

    /// <summary>
    /// Alpha utilizado para renderizar la imagen guía.
    /// </summary>
    [SerializeField, Range(0f, 1f)] private float guideImageAlpha = 0.1f;

    /// <summary>
    /// Contenedor donde se instancia la imagen guía.
    /// </summary>
    [SerializeField] private Transform guideContainer;

    [Header("References")]

    /// <summary>
    /// Prefab utilizado para crear cada ficha del rompecabezas.
    /// </summary>
    [SerializeField] private GameObject puzzlePiecePrefab;

    /// <summary>
    /// Contenedor que agrupa visualmente todas las piezas del rompecabezas.
    /// </summary>
    [SerializeField] private Transform piecesContainer;

    [Header("Animation")]

    /// <summary>
    /// Retraso antes de iniciar el shuffle inicial.
    /// </summary>
    [SerializeField] private float shuffleDelay = 0.5f;

    /// <summary>
    /// Rango horizontal para las piezas posicionadas en el lado derecho.
    /// </summary>
    [SerializeField] private Vector2 rightSideXRange = new Vector2(9f, 10f);

    /// <summary>
    /// Rango horizontal para las piezas posicionadas en el lado izquierdo.
    /// </summary>
    [SerializeField] private Vector2 leftSideXRange = new Vector2(-10f, -9f);

    [Header("Screen Constraints")]

    /// <summary>
    /// Espacio reservado en la parte superior de la pantalla para UI.
    /// </summary>
    [SerializeField] private float topUiReservedSpace = 2f;

    /// <summary>
    /// Margen adicional desde el borde superior visible de la cámara.
    /// </summary>
    [SerializeField] private float topEdgePadding = 0.5f;

    /// <summary>
    /// Margen adicional desde el borde inferior visible de la cámara.
    /// </summary>
    [SerializeField] private float bottomEdgePadding = 0.5f;

    /// <summary>
    /// Incremento aplicado a la profundidad Z de cada pieza seleccionada.
    /// </summary>
    [SerializeField] private float zStep = -0.01f;

    #endregion

    #region Gameplay Events

    /// <summary>
    /// Evento disparado cada vez que una ficha es colocada correctamente.
    /// </summary>
    public event System.Action<Vector3> PiecePlaced;

    /// <summary>
    /// Evento disparado cuando el jugador interactúa con una ficha.
    /// </summary>
    public event System.Action OnPlayerMove;

    /// <summary>
    /// Evento disparado cuando todas las fichas han sido colocadas correctamente.
    /// </summary>
    public event System.Action PuzzleCompleted;

    /// <inheritdoc/>
    public event System.Action OnPlayerAction;

    /// <inheritdoc/>
    public event System.Action OnMechanicCompleted;

    /// <inheritdoc/>
    public event System.Action<Vector3> PieceRewardTriggered;

    /// <inheritdoc/>
    public Vector3 CompletionRewardPosition => boardRoot.position;

    #endregion

    #region Runtime State

    /// <summary>
    /// Lista de todas las piezas activas del rompecabezas.
    /// </summary>
    private readonly List<PuzzlePiece> puzzlePieces = new List<PuzzlePiece>();

    /// <summary>
    /// Cantidad de piezas colocadas correctamente.
    /// </summary>
    private int placedPiecesCount;

    /// <summary>
    /// Tamaño de cada pieza en unidades de mundo.
    /// </summary>
    private float pieceWorldSize;

    /// <summary>
    /// SpriteRenderer utilizado para la imagen guía.
    /// </summary>
    private SpriteRenderer guideSpriteRenderer;

    #endregion

    #region Visual Priority Management

    /// <summary>
    /// Último sorting order asignado.
    /// </summary>
    public int LastSortingOrder { get; private set; }

    /// <summary>
    /// Última posición Z utilizada.
    /// </summary>
    public float LastZPosition { get; private set; }

    #endregion

    #region Guide Image State

    /// <summary>
    /// Indica si la imagen guía está visible.
    /// </summary>
    private bool isGuideImageVisible = true;

    /// <summary>
    /// Estado público de visibilidad de la imagen guía.
    /// </summary>
    public bool IsGuideImageVisible => isGuideImageVisible;

    #endregion

    #region Board Animation

    /// <summary>
    /// Transform raíz del tablero completo.
    /// </summary>
    [Tooltip("Transform raíz que agrupa visualmente todo el tablero.")]
    [SerializeField] private Transform boardRoot;

    /// <summary>
    /// Duración de la animación de entrada del tablero.
    /// </summary>
    [Tooltip("Tiempo que tarda el tablero en escalar desde cero hasta su tamaño final.")]
    [SerializeField] private float enterDuration = 0.6f;

    /// <summary>
    /// Curva de easing utilizada en la animación de entrada.
    /// </summary>
    [Tooltip("Tipo de interpolación utilizada para la animación de entrada.")]
    [SerializeField] private Ease enterEase = Ease.OutBack;

    #endregion

    #region Enums

    /// <summary>
    /// Define el tipo de salida visual del tablero.
    /// </summary>
    public enum PuzzleExitMode
    {
        FullExit,
        Restart
    }
    #endregion

    #region Gameplay Lifecycle
     /// <summary>
    /// Crea el rompecabezas completo incluyendo imagen guía,
    /// generación de piezas, animaciones iniciales y shuffle.
    /// </summary>
    public void CreatePuzzle()
    {
        ResolvePuzzleImage();
        CreateGuideImage();
        GeneratePuzzle();
        StartCoroutine(AssignInitialVisualPrioritiesDelayed());
        AnimatePiecesEntry();
        StartCoroutine(ShuffleAfterDelay());
    }
    /// <summary>
    /// Reinicia completamente el puzzle ejecutando animaciones
    /// de salida y recreación.
    /// </summary>
    public void RestartPuzzle()
    {
        PlayPuzzleExitAnimation(PuzzleExitMode.Restart, () =>
        {
            ResetPuzzleState();
            RecreatePuzzle();
        });
    }

    /// <summary>
    /// Garantiza que todos los tweens asociados a este objeto
    /// sean detenidos al destruir el tablero, evitando que
    /// DOTween intente operar sobre referencias inválidas.
    /// </summary>
    private void OnDestroy()
    {
        if (boardRoot != null)
        {
            boardRoot.DOKill();
        }

        if (guideSpriteRenderer != null)
        {
            guideSpriteRenderer.transform.DOKill();
        }

        foreach (PuzzlePiece piece in puzzlePieces)
        {
            if (piece != null)
            {
                piece.transform.DOKill();
            }
        }

        transform.DOKill();
    }

    #endregion

    #region Gameplay Progress
    /// <summary>
    /// Notifica al tablero que una ficha fue colocada correctamente.
    /// Incrementa el contador interno y evalúa el estado de completado.
    /// </summary>
    /// <param name="pieceWorldPosition">
    /// Posición en mundo donde se colocó la ficha.
    /// </param>
    public void NotifyPiecePlaced(Vector3 pieceWorldPosition)
    {
        placedPiecesCount++;

        PiecePlaced?.Invoke(pieceWorldPosition);
        PieceRewardTriggered?.Invoke(pieceWorldPosition);

        if (placedPiecesCount >= puzzlePieces.Count / 4)
        {
            PuzzleCompleted?.Invoke();
            OnMechanicCompleted?.Invoke();
        }
    }

    #endregion

    #region Visual Priority Management
    
    /// <summary>
    /// Devuelve la siguiente prioridad visual disponible para una pieza.
    /// </summary>
    public VisualPriority GetNextVisualPriority()
    {
        LastSortingOrder++;
        LastZPosition += zStep;

        return new VisualPriority(LastSortingOrder, LastZPosition);
    }
    
    /// <summary>
    /// Asigna la prioridad visual inicial a todas las piezas del rompecabezas.
    /// Debe ejecutarse antes de cualquier interacción o shuffle.
    /// </summary>
    private void AssignInitialVisualPriorities()
    {
        LastSortingOrder = 0;
        LastZPosition = 0f;

        foreach (PuzzlePiece piece in puzzlePieces)
        {
            piece.AssignInitialVisualPriority();
        }
    }

    /// <summary>
    /// Espera un frame para asegurar que todas las piezas
    /// estén completamente inicializadas antes de asignar
    /// la prioridad visual inicial.
    /// </summary>
    private IEnumerator AssignInitialVisualPrioritiesDelayed()
    {
        yield return null;
    
        AssignInitialVisualPriorities();
    }

    #endregion

    #region Puzzle Exit & Board Transitions
    /// <summary>
    /// Ejecuta la animación de salida del puzzle completo,
    /// adaptándose al modo de salida solicitado.
    /// </summary>
    public void PlayPuzzleExitAnimation(
        PuzzleExitMode exitMode,
        System.Action onComplete = null)
    {
        StopAllCoroutines();

        SetGuideImageVisible(true);
        AnimateGuideExit();
        AnimatePiecesExit();

        if (exitMode == PuzzleExitMode.FullExit)
        {
            boardRoot
                .DOScale(Vector3.zero, enterDuration * 0.8f)
                .SetEase(Ease.InBack)
                .SetLink(boardRoot.gameObject)
                .OnComplete(() => onComplete?.Invoke());
        }
        else
        {
            DOVirtual
                .DelayedCall(0.35f, () =>
                {
                    if (this != null)
                    {
                        onComplete?.Invoke();
                    }
                })
                .SetLink(gameObject);
        }
    }

    #endregion

    #region Guide Image Control

    /// <summary>
    /// Controla la visibilidad de la imagen guía mediante una animación de alpha.
    /// </summary>
    public void SetGuideImageVisible(bool visible)
    {
        if (guideSpriteRenderer == null)
            return;

        if (isGuideImageVisible == visible)
            return;

        isGuideImageVisible = visible;

        float targetAlpha = visible ? guideImageAlpha : 0f;

        guideSpriteRenderer
            .DOFade(targetAlpha, 0.25f)
            .SetEase(Ease.OutQuad)
            .SetLink(guideSpriteRenderer.gameObject);
    }

    /// <summary>
    /// Alterna el estado de visibilidad de la imagen guía.
    /// </summary>
    public void ToggleGuideImage()
    {
        SetGuideImageVisible(!isGuideImageVisible);
    }

    #endregion

    #region Puzzle Creation

    /// <summary>
    /// Crea la imagen guía sin recortar y con alpha configurable.
    /// </summary>
    private void CreateGuideImage()
    {
        isGuideImageVisible = true;
        GameObject guideObject = new GameObject("PuzzleGuideImage");
        guideObject.transform.position = puzzleCenter;
        guideObject.transform.SetParent(guideContainer);

        guideSpriteRenderer = guideObject.AddComponent<SpriteRenderer>();
        guideSpriteRenderer.sprite = Sprite.Create(
            puzzleImage,
            new Rect(0, 0, puzzleImage.width, puzzleImage.height),
            Vector2.one * 0.5f,
            100
        );

        guideSpriteRenderer.color = new Color(1f, 1f, 1f, guideImageAlpha);
        guideSpriteRenderer.sortingOrder = 1;

        guideObject.transform.localScale = Vector3.zero;
        AnimateGuideEntry(guideObject.transform);
    }

    /// <summary>
    /// Genera todas las piezas del rompecabezas.
    /// </summary>
    private void GeneratePuzzle()
    {
        gridSize = GameManager.Instance.LevelID == 1 ? PuzzleGridSize.Grid4x4 :
            GameManager.Instance.LevelID == 2 ? PuzzleGridSize.Grid6x6 :
            PuzzleGridSize.Grid8x8; 
        int grid = (int)gridSize;
        int textureSize = puzzleImage.width;
        int pieceSize = textureSize / grid;

        pieceWorldSize = (pieceSize / 100f) * puzzleScale;

        Vector3 origin = CalculateBoardOrigin(grid);

        for (int row = 0; row < grid; row++)
        {
            for (int col = 0; col < grid; col++)
            {
                CreatePiece(row, col, pieceSize, origin);
            }
        }
    }

    #endregion

    #region Branding configuration

    /// <summary>
    /// Resuelve la imagen del puzzle desde el sistema de recursos.
    /// Si no existe, se mantiene la imagen local configurada.
    /// </summary>
    private void ResolvePuzzleImage()
    {
        Texture2D remotePuzzleImage =
            ResourceService.Instance?.GetTexture(puzzleImageResourceKey);

        if (remotePuzzleImage != null)
        {
            puzzleImage = remotePuzzleImage;
        }
    }

    #endregion

    #region Puzzle Pieces

    /// <summary>
    /// Crea una ficha individual del rompecabezas.
    /// </summary>
    private void CreatePiece(int row, int col, int pieceSize, Vector3 origin)
    {
        Rect rect = new Rect(col * pieceSize, row * pieceSize, pieceSize, pieceSize);

        Sprite sprite = Sprite.Create(
            puzzleImage,
            rect,
            Vector2.one * 0.5f,
            100
        );

        GameObject pieceObject = Instantiate(puzzlePiecePrefab, piecesContainer);
        PuzzlePiece piece = pieceObject.GetComponent<PuzzlePiece>();

        Vector3 correctPosition = new Vector3(
            origin.x + col * (pieceWorldSize + pieceSpacing * puzzleScale),
            origin.y + row * (pieceWorldSize + pieceSpacing * puzzleScale),
            0f
        );

        piece.Initialize(sprite, correctPosition, this, puzzleScale);
        piece.OnPieceAction += HandlePieceAction;
        puzzlePieces.Add(piece);

        pieceObject.transform.localScale = Vector3.zero;
    }

    #endregion

    #region Animation

    /// <summary>
    /// Ejecuta la animación de entrada de la imagen guía,
    /// escalándola suavemente desde cero hasta su tamaño final.
    /// </summary>
    /// <param name="guideTransform">
    /// Transform de la imagen guía a animar.
    /// </param>
    private void AnimateGuideEntry(Transform guideTransform)
    {
        guideTransform
            .DOScale(Vector3.one * puzzleScale, 0.5f)
            .SetEase(Ease.OutBack)
            .SetDelay(0.1f)
            .SetLink(guideTransform.gameObject);
    }

    /// <summary>
    /// Ejecuta la animación de salida de la imagen guía,
    /// reduciendo su escala hasta desaparecer.
    /// </summary>
    private void AnimateGuideExit()
    {
        if (guideContainer == null || guideContainer.childCount == 0)
            return;

        guideContainer.GetChild(0)
            .DOScale(Vector3.zero, 0.35f)
            .SetEase(Ease.InBack)
            .SetLink(guideContainer.GetChild(0).gameObject);
    }

    /// <summary>
    /// Ejecuta la animación de entrada de todas las piezas del rompecabezas,
    /// aplicando un pequeño retraso incremental para generar un efecto en cascada.
    /// </summary>
    private void AnimatePiecesEntry()
    {
        float delay = 0f;

        foreach (PuzzlePiece piece in puzzlePieces)
        {
            piece.PlayEntryAnimation(delay);
            delay += 0.02f;
        }
    }

    /// <summary>
    /// Ejecuta la animación de salida de todas las piezas del rompecabezas.
    /// </summary>
    private void AnimatePiecesExit()
    {
        foreach (PuzzlePiece piece in puzzlePieces)
        {
            piece.PlayExitAnimation();
        }
    }

    #endregion

    #region Shuffle & Positioning

    /// <summary>
    /// Realiza el desordenamiento inicial de las piezas luego de un retraso,
    /// distribuyéndolas aleatoriamente en columnas laterales visibles.
    /// </summary>
    private IEnumerator ShuffleAfterDelay()
    {
        yield return new WaitForSeconds(shuffleDelay);
    
        ShufflePieces(puzzlePieces);
    
        int halfCount = puzzlePieces.Count / 2;
    
        CalculateVerticalBounds(Camera.main, out float minY, out float maxY);
    
        for (int i = 0; i < puzzlePieces.Count; i++)
        {
            bool placeOnLeftSide = i < halfCount;
    
            Vector3 randomPosition = GetRandomSidePosition(
                placeOnLeftSide,
                minY,
                maxY
            );
    
            puzzlePieces[i].MoveToStartPosition(randomPosition);
    
            yield return new WaitForSeconds(0.03f);
        }
    }

    /// <summary>
    /// Baraja una lista usando el algoritmo Fisher-Yates.
    /// </summary>
    private void ShufflePieces(List<PuzzlePiece> pieces)
    {
        for (int i = pieces.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            (pieces[i], pieces[randomIndex]) = (pieces[randomIndex], pieces[i]);
        }
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Calcula el origen del tablero del rompecabezas en coordenadas de mundo,
    /// tomando como referencia el centro configurado, el tamaño de grilla
    /// y la separación entre piezas.
    /// </summary>
    /// <param name="grid">
    /// Cantidad de filas y columnas del rompecabezas.
    /// </param>
    /// <returns>
    /// Posición de origen del tablero.
    /// </returns>
    private Vector3 CalculateBoardOrigin(int grid)
    {
        float boardSize =
            grid * pieceWorldSize +
            (grid - 1) * pieceSpacing * puzzleScale;

        return puzzleCenter - new Vector3(
            boardSize / 2f - pieceWorldSize / 2f,
            boardSize / 2f - pieceWorldSize / 2f,
            0f
        );
    }

    /// <summary>
    /// Calcula los límites verticales visibles en pantalla,
    /// considerando márgenes configurados y el espacio reservado para UI.
    /// </summary>
    /// <param name="camera">
    /// Cámara principal utilizada para el cálculo.
    /// </param>
    /// <param name="minY">
    /// Límite inferior válido en coordenadas de mundo.
    /// </param>
    /// <param name="maxY">
    /// Límite superior válido en coordenadas de mundo.
    /// </param>
    private void CalculateVerticalBounds(Camera camera, out float minY, out float maxY)
    {
        float cameraBottomY = camera.ViewportToWorldPoint(Vector3.zero).y;
        float cameraTopY = camera.ViewportToWorldPoint(Vector3.up).y;

        minY = cameraBottomY + bottomEdgePadding;
        maxY = cameraTopY - topUiReservedSpace - topEdgePadding;

        if (minY > maxY)
        {
            minY = maxY;
        }
    }

    /// <summary>
    /// Devuelve una posición aleatoria dentro de una columna lateral
    /// (izquierda o derecha) y un rango vertical permitido.
    /// </summary>
    /// <param name="isRightSide">
    /// Indica si la posición debe generarse en el lado derecho.
    /// </param>
    /// <param name="minY">
    /// Límite inferior vertical.
    /// </param>
    /// <param name="maxY">
    /// Límite superior vertical.
    /// </param>
    /// <returns>
    /// Posición aleatoria válida en coordenadas de mundo.
    /// </returns>
    private Vector3 GetRandomSidePosition(bool isRightSide, float minY, float maxY)
    {
        float x = isRightSide
            ? Random.Range(rightSideXRange.x, rightSideXRange.y)
            : Random.Range(leftSideXRange.x, leftSideXRange.y);

        return new Vector3(x, Random.Range(minY, maxY), 0f);
    }

    /// <summary>
    /// Recibe las acciones de las piezas y las propaga como movimientos del jugador.
    /// </summary>
    private void HandlePieceAction()
    {
        OnPlayerMove?.Invoke();
        OnPlayerAction?.Invoke();
    }

    #endregion

    #region Reset & Recreation

    /// <summary>
    /// Resetea completamente el estado lógico y visual del rompecabezas,
    /// destruyendo piezas activas, limpiando referencias y reiniciando contadores.
    /// </summary>
    private void ResetPuzzleState()
    {
        placedPiecesCount = 0;
        LastSortingOrder = 0;
        LastZPosition = 0f;
        isGuideImageVisible = true;

        StopAllCoroutines();

        foreach (PuzzlePiece piece in puzzlePieces)
        {
            if (piece != null)
            {
                piece.transform.DOKill();
                Destroy(piece.gameObject);
            }
        }

        puzzlePieces.Clear();

        if (guideSpriteRenderer != null)
        {
            guideSpriteRenderer.transform.DOKill();
            Destroy(guideSpriteRenderer.gameObject);
            guideSpriteRenderer = null;
        }
    }

    /// <summary>
    /// Recrea el rompecabezas completo ejecutando nuevamente
    /// la generación, asignación de prioridades y animaciones de entrada.
    /// </summary>
    private void RecreatePuzzle()
    {
        CreateGuideImage();
        GeneratePuzzle();
        StartCoroutine(AssignInitialVisualPrioritiesDelayed());
        AnimatePiecesEntry();
        StartCoroutine(ShuffleAfterDelay());
    }

    #endregion

    #region Interface Implementations

    /// <inheritdoc/>
    public void PlayEnterAnimation(System.Action onComplete = null)
    {
        boardRoot.localScale = Vector3.zero;

        boardRoot
            .DOScale(Vector3.one, enterDuration)
            .SetEase(enterEase)
            .SetLink(boardRoot.gameObject)
            .OnComplete(() => onComplete?.Invoke());
    }


    /// <inheritdoc/>
    public void StartMechanic()
    {
        CreatePuzzle();
    }
    /// <inheritdoc/>
    public void RestartMechanic()
    {
        RestartPuzzle();
    }
    /// <inheritdoc/>
    public void PlayExitAnimation(System.Action onComplete = null)
    {
        PlayPuzzleExitAnimation(PuzzleExitMode.FullExit, onComplete);
    }
    #endregion
}
