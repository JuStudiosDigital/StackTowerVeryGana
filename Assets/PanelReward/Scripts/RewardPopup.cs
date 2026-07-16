using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Popup de recompensas promocionales mostrado al finalizar el juego.
/// Recibe su información mediante Setup y envía al frontend el producto
/// seleccionado por el usuario.
/// </summary>
public sealed class RewardPopup : PopupBase
{
    #region Serialized Fields

    [Header("Title")]

    [Tooltip("RectTransform del título visual del popup.")]
    [SerializeField] private RectTransform rewardTitle;

    [Tooltip("Texto utilizado para mostrar el título personalizado.")]
    [SerializeField] private TMP_Text rewardTitleText;

    [Tooltip("Título utilizado cuando no se recibe uno personalizado.")]
    [SerializeField] private string defaultPopupTitle = "¡Felicitaciones!";

    [Tooltip("Escala adicional aplicada al título durante el pulso.")]
    [SerializeField] private float titlePulseScale = 1.06f;

    [Tooltip("Duración de cada tramo de la animación del título.")]
    [SerializeField] private float titlePulseDuration = 0.55f;

    [Tooltip("Espera antes de iniciar la animación del título.")]
    [SerializeField] private float titlePulseDelay = 0.15f;

    [Header("Products")]

    [Tooltip("Vistas disponibles para mostrar productos.")]
    [SerializeField] private RewardProductView[] productViews =
        new RewardProductView[3];

    [Tooltip("Productos usados como fallback mientras no se hayan asignado datos externos.")]
    [SerializeField] private List<CopagosProductData> fallbackProducts =
        new List<CopagosProductData>();

    [Tooltip("Texto mostrado en el botón después de agregar el producto.")]
    [SerializeField] private string productAddedButtonText = "¡Agregado!";

    [Header("Product Appearance")]

    [Tooltip("Espera entre la aparición de cada producto.")]
    [SerializeField] private float productStaggerDelay = 0.12f;

    [Tooltip("Duración de la animación de entrada de cada producto.")]
    [SerializeField] private float productAppearDuration = 0.28f;

    [Header("Opening Particles")]

    [Tooltip("Partículas reproducidas cuando se abre el popup.")]
    [SerializeField] private ParticleSystem[] openingFireworks =
        new ParticleSystem[2];

    [Tooltip("Reinicia las partículas antes de reproducirlas.")]
    [SerializeField] private bool clearParticlesBeforePlay = true;

    [Header("Formatting")]

    [Tooltip("Símbolo monetario utilizado para mostrar los precios.")]
    [SerializeField] private string currencySymbol = "$";

    [Tooltip("Color hexadecimal base del mensaje de llaves.")]
    [SerializeField] private string keysMessageBaseColor = "#FFFFFF";

    [Tooltip("Color hexadecimal de las partes resaltadas con [[...]].")]
    [SerializeField] private string keysMessageHighlightColor = "#FFD83D";

    [Tooltip("Texto utilizado cuando el producto no tiene rating válido.")]
    [SerializeField] private string emptyRatingText = "0.0";

    #endregion

    #region Private Fields

    private readonly List<Tween> activeTweens = new List<Tween>();

    private readonly List<Coroutine> activeImageLoadingCoroutines =
        new List<Coroutine>();

    private readonly List<Sprite> runtimeSprites =
        new List<Sprite>();

    private readonly List<CopagosProductData> visibleProducts =
        new List<CopagosProductData>();

    private CopagosRewardPopupData currentPopupData;

    private Vector3 titleInitialScale = Vector3.one;

    #endregion

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake();

        if (rewardTitle != null)
        {
            titleInitialScale = rewardTitle.localScale;
        }

        CacheProductInitialStates();
        HideAllProductViewsImmediate();
    }

    private void OnDisable()
    {
        StopImageLoading();
        KillActiveTweens();
    }

    protected override void OnDestroy()
    {
        StopImageLoading();
        DestroyRuntimeSprites();
        KillActiveTweens();

        base.OnDestroy();
    }

    #endregion

    #region Public API

    /// <summary>
    /// Configura el título y los productos que mostrará el popup.
    /// </summary>
    public void Setup(CopagosRewardPopupData popupData)
    {
        currentPopupData = popupData;
    }

    /// <summary>
    /// Configura el popup mediante un título y una lista de productos.
    /// </summary>
    public void Setup(
        string popupTitle,
        List<CopagosProductData> products)
    {
        currentPopupData = new CopagosRewardPopupData
        {
            popupTitle = popupTitle,
            products = products ?? new List<CopagosProductData>()
        };
    }

    /// <summary>
    /// Mantiene compatibilidad con llamadas que solo entregan productos.
    /// </summary>
    public void Setup(List<CopagosProductData> products)
    {
        Setup(string.Empty, products);
    }

    /// <summary>
    /// Elimina la información asignada para volver a utilizar
    /// el fallback configurado en el Inspector.
    /// </summary>
    public void ClearSetup()
    {
        currentPopupData = null;
        visibleProducts.Clear();
    }

    public override void Show()
    {
        StopImageLoading();
        DestroyRuntimeSprites();
        KillActiveTweens();

        BuildVisibleProducts();

        base.Show();

        ApplyPopupTitle();
        ConfigureProductViews();
        AnimateTitle();
        AnimateProducts();
        PlayOpeningFireworks();
    }

    /// <summary>
    /// Solicita el cierre del popup desde un botón del Inspector.
    /// </summary>
    public void CloseFromButton()
    {
        RequestClose();
    }

    /// <summary>
    /// Selecciona el primer producto visible.
    /// </summary>
    public void AddProductOneToCart()
    {
        AddProductToCart(0);
    }

    /// <summary>
    /// Selecciona el segundo producto visible.
    /// </summary>
    public void AddProductTwoToCart()
    {
        AddProductToCart(1);
    }

    /// <summary>
    /// Selecciona el tercer producto visible.
    /// </summary>
    public void AddProductThreeToCart()
    {
        AddProductToCart(2);
    }

    #endregion

    #region Product Setup

    private void BuildVisibleProducts()
    {
        visibleProducts.Clear();

        List<CopagosProductData> sourceProducts =
            GetSourceProducts();

        if (sourceProducts == null || productViews == null)
        {
            return;
        }

        int maxProducts = productViews.Length;

        for (int i = 0;
             i < sourceProducts.Count &&
             visibleProducts.Count < maxProducts;
             i++)
        {
            CopagosProductData product = sourceProducts[i];

            if (product != null && product.IsValid)
            {
                visibleProducts.Add(product);
            }
        }
    }

    private List<CopagosProductData> GetSourceProducts()
    {
        if (currentPopupData != null &&
            currentPopupData.HasProducts)
        {
            return currentPopupData.products;
        }

        return fallbackProducts;
    }

    private void ApplyPopupTitle()
    {
        if (rewardTitleText == null)
        {
            return;
        }

        string popupTitle = currentPopupData?.popupTitle;

        rewardTitleText.text =
            string.IsNullOrWhiteSpace(popupTitle)
                ? SanitizeRichText(defaultPopupTitle)
                : SanitizeRichText(popupTitle);
    }

    private void ConfigureProductViews()
    {
        if (productViews == null)
        {
            return;
        }

        for (int i = 0; i < productViews.Length; i++)
        {
            RewardProductView view = productViews[i];

            if (view == null || view.Root == null)
            {
                continue;
            }

            view.ResetAddToCartState();

            bool hasProduct =
                i < visibleProducts.Count &&
                visibleProducts[i] != null;

            view.Root.gameObject.SetActive(hasProduct);

            if (!hasProduct)
            {
                ClearProductView(view);
                continue;
            }

            ApplyProductData(view, visibleProducts[i]);
        }
    }

    private void ApplyProductData(
        RewardProductView view,
        CopagosProductData product)
    {
        if (view.BrandAndProductText != null)
        {
            view.BrandAndProductText.text =
                BuildBrandAndProductText(product);
        }

        if (view.PriceText != null)
        {
            view.PriceText.text =
                BuildRegularPriceText(product);
        }

        if (view.ImageMessageText != null)
        {
            view.ImageMessageText.text =
                SanitizeRichText(product.imageMessage);
        }

        if (view.KeysMessageText != null)
        {
            view.KeysMessageText.text =
                BuildKeysMessageText(product.keysMessage);
        }

        if (view.RatingText != null)
        {
            view.RatingText.text =
                BuildRatingText(product.rating);
        }

        if (view.ProductImage != null)
        {
            view.ProductImage.sprite = null;
            view.ProductImage.enabled = false;
        }

        if (!string.IsNullOrWhiteSpace(product.imageUrl))
        {
            Coroutine routine = StartCoroutine(
                LoadProductImage(product.imageUrl, view));

            activeImageLoadingCoroutines.Add(routine);
        }
    }

    private void ClearProductView(RewardProductView view)
    {
        if (view.ProductImage != null)
        {
            view.ProductImage.sprite = null;
            view.ProductImage.enabled = false;
        }

        if (view.BrandAndProductText != null)
        {
            view.BrandAndProductText.text = string.Empty;
        }

        if (view.PriceText != null)
        {
            view.PriceText.text = string.Empty;
        }

        if (view.ImageMessageText != null)
        {
            view.ImageMessageText.text = string.Empty;
        }

        if (view.KeysMessageText != null)
        {
            view.KeysMessageText.text = string.Empty;
        }

        if (view.RatingText != null)
        {
            view.RatingText.text = string.Empty;
        }

        view.ResetAddToCartState();
    }

    private void AddProductToCart(int index)
    {
        if (index < 0 || index >= visibleProducts.Count)
        {
            DevLog.Log(
                $"[RewardPopup] Producto visible {index} no configurado.");

            return;
        }

        if (productViews == null ||
            index >= productViews.Length)
        {
            DevLog.Log(
                $"[RewardPopup] No existe una vista para el producto {index}.");

            return;
        }

        RewardProductView productView = productViews[index];

        if (productView == null)
        {
            DevLog.Log(
                $"[RewardPopup] La vista del producto {index} es null.");

            return;
        }

        if (productView.IsAdded)
        {
            DevLog.Log(
                $"[RewardPopup] El producto {index} ya fue agregado.");

            return;
        }

        CopagosProductData selectedProduct =
            visibleProducts[index];

        if (selectedProduct == null || !selectedProduct.IsValid)
        {
            DevLog.Log(
                $"[RewardPopup] Producto visible {index} inválido.");

            return;
        }

        if (CopagosWebGLBridge.Instance == null)
        {
            DevLog.Log(
                "[RewardPopup] No existe CopagosWebGLBridge en escena.");

            return;
        }

        CopagosWebGLBridge.Instance.SendProductClicked(
            selectedProduct);

        productView.SetAddedState(productAddedButtonText);
    }

    #endregion

    #region Image Loading

    private IEnumerator LoadProductImage(
        string imageUrl,
        RewardProductView view)
    {
        using UnityWebRequest request =
            UnityWebRequestTexture.GetTexture(imageUrl);

        request.timeout = 10;

        yield return request.SendWebRequest();

        activeImageLoadingCoroutines.RemoveAll(
            coroutine => coroutine == null);

        if (request.result != UnityWebRequest.Result.Success)
        {
            DevLog.Log(
                $"[RewardPopup] Error cargando imagen: " +
                $"{imageUrl} - {request.error}");

            yield break;
        }

        Texture2D texture =
            DownloadHandlerTexture.GetContent(request);

        if (texture == null)
        {
            DevLog.Log(
                $"[RewardPopup] La textura descargada es null: {imageUrl}");

            yield break;
        }

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f));

        runtimeSprites.Add(sprite);

        if (view.ProductImage == null)
        {
            yield break;
        }

        view.ProductImage.sprite = sprite;
        view.ProductImage.enabled = true;
    }

    #endregion

    #region Animations

    private void AnimateTitle()
    {
        if (rewardTitle == null)
        {
            return;
        }

        rewardTitle.DOKill();
        rewardTitle.localScale = titleInitialScale;

        Tween titleTween = rewardTitle
            .DOScale(
                titleInitialScale * titlePulseScale,
                titlePulseDuration)
            .SetDelay(titlePulseDelay)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetLink(
                gameObject,
                LinkBehaviour.KillOnDestroy);

        activeTweens.Add(titleTween);
    }

    private void AnimateProducts()
    {
        if (productViews == null)
        {
            return;
        }

        for (int i = 0; i < productViews.Length; i++)
        {
            RewardProductView view = productViews[i];

            if (view == null ||
                view.Root == null ||
                !view.Root.gameObject.activeSelf)
            {
                continue;
            }

            AnimateProductView(view, i);
        }
    }

    private void AnimateProductView(
        RewardProductView view,
        int index)
    {
        float delay = index * productStaggerDelay;

        view.Root.DOKill();
        view.Root.localScale = Vector3.zero;

        Tween appearTween = view.Root
            .DOScale(
                view.RootInitialScale,
                productAppearDuration)
            .SetDelay(delay)
            .SetEase(Ease.OutBack)
            .SetLink(
                gameObject,
                LinkBehaviour.KillOnDestroy);

        activeTweens.Add(appearTween);
    }

    #endregion

    #region Particles

    private void PlayOpeningFireworks()
    {
        if (openingFireworks == null)
        {
            return;
        }

        for (int i = 0; i < openingFireworks.Length; i++)
        {
            ParticleSystem particle = openingFireworks[i];

            if (particle == null)
            {
                continue;
            }

            if (clearParticlesBeforePlay)
            {
                particle.Clear(true);
            }

            particle.Play(true);
        }
    }

    #endregion

    #region Text Formatting

    private string BuildBrandAndProductText(
        CopagosProductData product)
    {
        string commercial =
            SanitizeRichText(product.commercial);

        string productName =
            SanitizeRichText(product.name);

        if (string.IsNullOrWhiteSpace(commercial))
        {
            return productName;
        }

        if (string.IsNullOrWhiteSpace(productName))
        {
            return $"<b>{commercial}</b>";
        }

        return $"<b>{commercial}</b>: {productName}";
    }

    private string BuildRegularPriceText(
        CopagosProductData product)
    {
        return $"<s>{FormatPrice(product.regularPrice)}</s>";
    }

    private string BuildKeysMessageText(string rawText)
    {
        if (string.IsNullOrWhiteSpace(rawText))
        {
            return string.Empty;
        }

        const string openToken = "[[";
        const string closeToken = "]]";

        string baseColor =
            string.IsNullOrWhiteSpace(keysMessageBaseColor)
                ? "#FFFFFF"
                : keysMessageBaseColor;

        string highlightColor =
            string.IsNullOrWhiteSpace(keysMessageHighlightColor)
                ? "#FFD83D"
                : keysMessageHighlightColor;

        StringBuilder builder = new StringBuilder();
        int currentIndex = 0;

        builder.Append("<color=");
        builder.Append(baseColor);
        builder.Append(">");

        while (currentIndex < rawText.Length)
        {
            int openIndex = rawText.IndexOf(
                openToken,
                currentIndex,
                StringComparison.Ordinal);

            if (openIndex < 0)
            {
                builder.Append(
                    SanitizeRichText(
                        rawText.Substring(currentIndex)));

                break;
            }

            builder.Append(
                SanitizeRichText(
                    rawText.Substring(
                        currentIndex,
                        openIndex - currentIndex)));

            int highlightStartIndex =
                openIndex + openToken.Length;

            int closeIndex = rawText.IndexOf(
                closeToken,
                highlightStartIndex,
                StringComparison.Ordinal);

            if (closeIndex < 0)
            {
                builder.Append(
                    SanitizeRichText(
                        rawText.Substring(openIndex)));

                break;
            }

            string highlightedContent =
                SanitizeRichText(
                    rawText.Substring(
                        highlightStartIndex,
                        closeIndex - highlightStartIndex));

            builder.Append("</color><color=");
            builder.Append(highlightColor);
            builder.Append("><b>");
            builder.Append(highlightedContent);
            builder.Append("</b></color><color=");
            builder.Append(baseColor);
            builder.Append(">");

            currentIndex =
                closeIndex + closeToken.Length;
        }

        builder.Append("</color>");

        return builder.ToString();
    }

    private string BuildRatingText(float rating)
    {
        if (rating <= 0f)
        {
            return emptyRatingText;
        }

        return rating.ToString(
            "0.0",
            CultureInfo.InvariantCulture);
    }

    private string FormatPrice(float value)
    {
        int roundedValue = Mathf.RoundToInt(value);

        string formattedValue = roundedValue
            .ToString(
                "N0",
                CultureInfo.InvariantCulture)
            .Replace(",", ".");

        return $"{currencySymbol}{formattedValue}";
    }

    private string SanitizeRichText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value
            .Replace("<", string.Empty)
            .Replace(">", string.Empty);
    }

    #endregion

    #region Cleanup

    private void CacheProductInitialStates()
    {
        if (productViews == null)
        {
            return;
        }

        for (int i = 0; i < productViews.Length; i++)
        {
            productViews[i]?.CacheInitialState();
        }
    }

    private void HideAllProductViewsImmediate()
    {
        if (productViews == null)
        {
            return;
        }

        for (int i = 0; i < productViews.Length; i++)
        {
            RewardProductView view = productViews[i];

            if (view?.Root == null)
            {
                continue;
            }

            view.Root.gameObject.SetActive(false);
        }
    }

    private void StopImageLoading()
    {
        for (int i = 0;
             i < activeImageLoadingCoroutines.Count;
             i++)
        {
            Coroutine routine =
                activeImageLoadingCoroutines[i];

            if (routine != null)
            {
                StopCoroutine(routine);
            }
        }

        activeImageLoadingCoroutines.Clear();
    }

    private void DestroyRuntimeSprites()
    {
        for (int i = 0; i < runtimeSprites.Count; i++)
        {
            Sprite sprite = runtimeSprites[i];

            if (sprite == null)
            {
                continue;
            }

            if (sprite.texture != null)
            {
                Destroy(sprite.texture);
            }

            Destroy(sprite);
        }

        runtimeSprites.Clear();
    }

    private void KillActiveTweens()
    {
        for (int i = 0; i < activeTweens.Count; i++)
        {
            Tween tween = activeTweens[i];

            if (tween != null && tween.IsActive())
            {
                tween.Kill();
            }
        }

        activeTweens.Clear();

        if (rewardTitle != null)
        {
            rewardTitle.DOKill();
        }

        if (productViews == null)
        {
            return;
        }

        for (int i = 0; i < productViews.Length; i++)
        {
            productViews[i]?.KillTweens();
        }
    }

    #endregion
}

/// <summary>
/// Referencias visuales y estado de un producto dentro del RewardPopup.
/// </summary>
[Serializable]
public sealed class RewardProductView
{
    #region Serialized Fields

    [Header("Root")]

    [Tooltip("Contenedor raíz de la tarjeta del producto.")]
    [SerializeField] private RectTransform root;

    [Header("Content")]

    [Tooltip("Imagen principal del producto.")]
    [SerializeField] private Image productImage;

    [Tooltip("Texto que muestra la marca y el nombre del producto.")]
    [SerializeField] private TMP_Text brandAndProductText;

    [FormerlySerializedAs("pricesText")]
    [Tooltip("Texto utilizado para mostrar el precio normal tachado.")]
    [SerializeField] private TMP_Text priceText;

    [FormerlySerializedAs("attractionText")]
    [Tooltip("Texto que acompaña la imagen del producto.")]
    [SerializeField] private TMP_Text imageMessageText;

    [FormerlySerializedAs("customOfferText")]
    [Tooltip("Mensaje de llaves con resaltado mediante [[...]].")]
    [SerializeField] private TMP_Text keysMessageText;

    [Tooltip("Texto utilizado para mostrar el rating del producto.")]
    [SerializeField] private TMP_Text ratingText;

    [Header("Cart Button")]

    [Tooltip("TMP ubicado dentro del botón de agregar el producto al carrito.")]
    [SerializeField] private TMP_Text addToCartButtonText;

    #endregion

    #region Private Fields

    private string initialAddToCartButtonText = string.Empty;
    private bool isAdded;

    #endregion

    #region Public Properties

    public RectTransform Root => root;
    public Image ProductImage => productImage;
    public TMP_Text BrandAndProductText => brandAndProductText;
    public TMP_Text PriceText => priceText;
    public TMP_Text ImageMessageText => imageMessageText;
    public TMP_Text KeysMessageText => keysMessageText;
    public TMP_Text RatingText => ratingText;
    public bool IsAdded => isAdded;

    public Vector3 RootInitialScale { get; private set; } =
        Vector3.one;

    #endregion

    #region Public Methods

    /// <summary>
    /// Guarda el estado visual inicial de la tarjeta y su botón.
    /// </summary>
    public void CacheInitialState()
    {
        if (root != null)
        {
            RootInitialScale = root.localScale;
        }

        if (addToCartButtonText != null)
        {
            initialAddToCartButtonText =
                addToCartButtonText.text;
        }
    }

    /// <summary>
    /// Marca visualmente el producto como agregado.
    /// </summary>
    public void SetAddedState(string addedText)
    {
        if (isAdded)
        {
            return;
        }

        isAdded = true;

        if (addToCartButtonText == null)
        {
            return;
        }

        addToCartButtonText.text =
            string.IsNullOrWhiteSpace(addedText)
                ? "¡Agregado!"
                : addedText;
    }

    /// <summary>
    /// Restaura el texto y el estado original del botón.
    /// </summary>
    public void ResetAddToCartState()
    {
        isAdded = false;

        if (addToCartButtonText == null)
        {
            return;
        }

        addToCartButtonText.text =
            initialAddToCartButtonText;
    }

    public void KillTweens()
    {
        if (root != null)
        {
            root.DOKill();
        }
    }

    #endregion
}