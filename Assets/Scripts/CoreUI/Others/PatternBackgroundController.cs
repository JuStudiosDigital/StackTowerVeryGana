using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla un fondo de patrón animado para UI basado en <see cref="RawImage"/>.
/// 
/// Responsabilidades:
/// - Ajustar automáticamente el tamaño para cubrir toda la pantalla,
///   incluso cuando el patrón está rotado.
/// - Controlar el escalado visual del patrón mediante coordenadas UV.
/// - Animar el desplazamiento del patrón sin depender de <c>Update()</c>.
/// - Gestionar transiciones de fade in y fade out.
/// 
/// Diseño:
/// - Utiliza DOTween para animaciones desacopladas del ciclo de vida de Unity.
/// - Implementa UV scrolling en lugar de transformaciones físicas para minimizar costo en UI.
/// - Incluye protecciones contra callbacks tardíos cuando el objeto es destruido.
/// 
/// Este componente está diseñado para ser reutilizable en sistemas UI
/// que requieran fondos dinámicos ligeros y visualmente atractivos.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class PatternBackgroundController : MonoBehaviour
{
    #region Inspector Configuration

    /// <summary>
    /// Configuración visual del patrón (escala y rotación).
    /// </summary>
    [Header("Patrón")]
    [Tooltip("Escala visual del patrón a nivel de UV. Valores mayores generan un patrón más repetido.")]
    [SerializeField] private float patternScale = 3f;

    [Tooltip("Rotación en grados aplicada al patrón para generar efecto diagonal.")]
    [SerializeField] private float rotationAngle = -20f;

    /// <summary>
    /// Opacidad objetivo máxima del patrón.
    /// Representa el valor final tras ejecutar <see cref="FadeIn"/>.
    /// </summary>
    [Range(0f, 1f)]
    [Tooltip("Valor máximo de opacidad alcanzado tras el FadeIn.")]
    [SerializeField] private float targetOpacity = 0.15f;

    /// <summary>
    /// Configuración de las transiciones de fade.
    /// Controla tiempos y comportamiento de interpolación.
    /// </summary>
    [Header("Fade")]
    [Tooltip("Duración del efecto FadeIn en segundos.")]
    [SerializeField] private float fadeInDuration = 0.6f;

    [Tooltip("Duración del efecto FadeOut en segundos.")]
    [SerializeField] private float fadeOutDuration = 0.4f;

    [Tooltip("Curva de interpolación utilizada en las transiciones de opacidad.")]
    [SerializeField] private Ease fadeEase = Ease.OutSine;

    /// <summary>
    /// Configuración de la animación de desplazamiento del patrón.
    /// Define dirección, velocidad y tipo de interpolación.
    /// </summary>
    [Header("Animación")]
    [Tooltip("Dirección del desplazamiento UV del patrón.")]
    [SerializeField] private Vector2 scrollDirection = new Vector2(1f, 0.5f);

    [Tooltip("Velocidad de desplazamiento UV del patrón.")]
    [SerializeField] private float scrollSpeed = 0.1f;

    [Tooltip("Curva de interpolación del tween virtual de desplazamiento.")]
    [SerializeField] private Ease scrollEase = Ease.Linear;

    #endregion

    #region Branding Configuration
    
    /// <summary>
    /// Indica si este fondo debe utilizar una textura dinámica proveniente
    /// del sistema de branding del nivel o sesión actual.
    /// 
    /// Debe activarse únicamente en contextos de gameplay.
    /// </summary>
    [Header("Branding")]
    [Tooltip("Si está activo, se intentará reemplazar la textura por una proveniente del sistema de branding.")]
    [SerializeField] private bool useBrandingTexture;

    /// <summary>
    /// Clave lógica utilizada para solicitar la textura al sistema de recursos.
    /// 
    /// Este valor debe coincidir con la clave registrada en <c>ResourceService</c>.
    /// </summary>
    [Tooltip("Clave utilizada para solicitar la textura al ResourceService.")]
    [SerializeField] private string brandingResourceKey = "LogoWatermark";

    #endregion

    #region Private State

    /// <summary>
    /// Referencia al componente <see cref="RawImage"/> asociado.
    /// Se obtiene dinámicamente en <see cref="Start"/>.
    /// </summary>
    private RawImage rawImage;

    /// <summary>
    /// Rectángulo UV utilizado para controlar escala y desplazamiento del patrón.
    /// 
    /// Se manipula directamente para evitar modificaciones costosas en el transform.
    /// </summary>
    private Rect uvRect;

    /// <summary>
    /// Tween encargado del desplazamiento infinito del patrón.
    /// Se mantiene referencia para permitir su finalización segura.
    /// </summary>
    private Tween scrollTween;

    /// <summary>
    /// Tween encargado de las transiciones de opacidad.
    /// Solo uno puede estar activo simultáneamente.
    /// </summary>
    private Tween fadeTween;

    /// <summary>
    /// Indica si el objeto se encuentra en proceso de destrucción.
    /// 
    /// Se utiliza como protección adicional ante callbacks tardíos
    /// que puedan ejecutarse después de <see cref="OnDestroy"/>.
    /// </summary>
    private bool isBeingDestroyed;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa dependencias y ejecuta la secuencia inicial del fondo:
    /// - Aplica branding si corresponde.
    /// - Ajusta tamaño y rotación.
    /// - Configura UV.
    /// - Inicia animación de desplazamiento.
    /// - Ejecuta fade in.
    /// </summary>
    private void Start()
    {
        rawImage = GetComponent<RawImage>();

        if (rawImage == null)
        {
            return;
        }

        TryApplyBrandingTexture();

        ResizeToCoverScreen();
        ApplyRotation();
        InitializeUvRect();
        SetAlpha(0f);
        StartScrolling();
        FadeIn();
    }

    /// <summary>
    /// Finaliza correctamente los tweens activos al destruir el objeto.
    /// 
    /// Garantiza:
    /// - No queden animaciones ejecutándose sobre referencias inválidas.
    /// - No se generen <c>MissingReferenceException</c>.
    /// - Se eviten fugas de memoria en DOTween.
    /// </summary>
    private void OnDestroy()
    {
        isBeingDestroyed = true;

        scrollTween?.Kill();
        fadeTween?.Kill();
    }

    #endregion

    #region Layout & Transform

    /// <summary>
    /// Redimensiona el <see cref="RawImage"/> para cubrir completamente
    /// el rectángulo padre, incluso cuando existe rotación.
    /// 
    /// Estrategia:
    /// Se calcula la diagonal del rectángulo padre y se utiliza como
    /// tamaño cuadrado mínimo que garantiza cobertura total.
    /// </summary>
    private void ResizeToCoverScreen()
    {
        if (rawImage == null) return;

        RectTransform rectTransform = rawImage.rectTransform;
        RectTransform parentRectTransform = rectTransform.parent as RectTransform;

        if (parentRectTransform == null) return;

        Rect parentRect = parentRectTransform.rect;

        float width = parentRect.width;
        float height = parentRect.height;
        float diagonal = Mathf.Sqrt((width * width) + (height * height));

        rectTransform.anchorMin = Vector2.one * 0.5f;
        rectTransform.anchorMax = Vector2.one * 0.5f;
        rectTransform.pivot = Vector2.one * 0.5f;

        rectTransform.sizeDelta = new Vector2(diagonal, diagonal);
        rectTransform.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// Aplica una rotación fija sobre el eje Z para generar
    /// un patrón visual diagonal.
    /// </summary>
    private void ApplyRotation()
    {
        if (rawImage == null) return;

        rawImage.rectTransform.localRotation =
            Quaternion.Euler(0f, 0f, rotationAngle);
    }

    #endregion

    #region UV Configuration

    /// <summary>
    /// Inicializa el rectángulo UV ajustando su tamaño en función
    /// del aspect ratio del rectángulo renderizado.
    /// 
    /// Esto garantiza que el patrón conserve proporciones correctas
    /// independientemente de la resolución o layout.
    /// </summary>
    private void InitializeUvRect()
    {
        if (rawImage == null) return;

        Rect rect = rawImage.rectTransform.rect;
        float aspectRatio = rect.width / rect.height;

        if (aspectRatio >= 1f)
        {
            uvRect.size = new Vector2(patternScale * aspectRatio, patternScale);
        }
        else
        {
            uvRect.size = new Vector2(patternScale, patternScale / aspectRatio);
        }

        uvRect.position = Vector2.zero;
        rawImage.uvRect = uvRect;
    }

    #endregion

    #region Animation

    /// <summary>
    /// Inicia el desplazamiento infinito del patrón.
    /// 
    /// Implementación:
    /// - Utiliza un tween virtual para evitar depender de <c>Update()</c>.
    /// - Se ejecuta en tiempo no escalado.
    /// - Se enlaza al GameObject mediante <c>SetLink</c> para garantizar
    ///   su destrucción automática cuando el objeto es destruido.
    /// </summary>
    private void StartScrolling()
    {
        if (rawImage == null) return;

        Vector2 direction = scrollDirection.normalized;

        scrollTween = DOVirtual.Float(
                0f,
                1f,
                1f,
                _ =>
                {
                    /// Protección adicional contra ejecución tardía
                    if (isBeingDestroyed || rawImage == null)
                    {
                        return;
                    }

                    uvRect.position += direction * scrollSpeed * Time.unscaledDeltaTime;
                    rawImage.uvRect = uvRect;
                }
            )
            .SetEase(scrollEase)
            .SetLoops(-1)
            .SetUpdate(true)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    #endregion

    #region Fade Control

    /// <summary>
    /// Ejecuta un fade in progresivo hasta <see cref="targetOpacity"/>.
    /// Si existe un tween previo activo, se finaliza antes de iniciar uno nuevo.
    /// </summary>
    public void FadeIn()
    {
        if (rawImage == null) return;

        fadeTween?.Kill();

        fadeTween = rawImage
            .DOFade(targetOpacity, fadeInDuration)
            .SetEase(fadeEase)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Ejecuta un fade out progresivo hasta alpha 0.
    /// </summary>
    public void FadeOut()
    {
        if (rawImage == null) return;

        fadeTween?.Kill();

        fadeTween = rawImage
            .DOFade(0f, fadeOutDuration)
            .SetEase(fadeEase)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Establece inmediatamente el valor de alpha sin animación.
    /// </summary>
    private void SetAlpha(float value)
    {
        if (rawImage == null) return;

        Color color = rawImage.color;
        color.a = value;
        rawImage.color = color;
    }

    #endregion

    #region Branding Integration

    /// <summary>
    /// Intenta obtener una textura dinámica desde el sistema de recursos
    /// y aplicarla al <see cref="RawImage"/> si está habilitado el branding.
    /// 
    /// No lanza excepciones si el servicio no está disponible.
    /// </summary>
    private void TryApplyBrandingTexture()
    {
        if (!useBrandingTexture)
        {
            return;
        }

        if (rawImage == null)
        {
            return;
        }

        Texture brandingTexture = ResourceService.Instance
            ?.GetTexture(brandingResourceKey);

        if (brandingTexture != null)
        {
            rawImage.texture = brandingTexture;
        }
    }

    #endregion
}
