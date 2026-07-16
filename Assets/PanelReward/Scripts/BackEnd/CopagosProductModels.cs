using System;
using System.Collections.Generic;
using System.Globalization;

/// <summary>
/// Representa el bloque reward_popup recibido dentro
/// del JSON general de configuración.
/// </summary>
[Serializable]
public sealed class RewardPopupConfigDto
{
    public string popup_title;
    public List<RewardProductDto> products = new List<RewardProductDto>();
}

/// <summary>
/// Representa un producto tal como viene en el JSON
/// general de configuración.
/// </summary>
[Serializable]
public sealed class RewardProductDto
{
    public string id;
    public string name;
    public string image_url;
    public string image_message;
    public string commercial;
    public float regular_price;
    public string keys_message;
    public float rating;
    public int max_keys_allowed;
    public int min_cash_cents;
    public int stock;
    public string category_name;
}

/// <summary>
/// Representa la información runtime de un producto promocional.
/// También se utiliza dentro del mensaje PRODUCT_CLICKED.
/// </summary>
[Serializable]
public sealed class CopagosProductData
{
    public string id;
    public string name;
    public string imageUrl;
    public string imageMessage;
    public string commercial;
    public float regularPrice;
    public string keysMessage;
    public float rating;
    public string maxKeysAllowed;
    public string minCashCents;
    public string stock;
    public string categoryName;

    public bool IsValid =>
        !string.IsNullOrWhiteSpace(id) &&
        !string.IsNullOrWhiteSpace(name);
}

/// <summary>
/// Contiene la información runtime necesaria para configurar
/// el popup de productos.
/// </summary>
[Serializable]
public sealed class CopagosRewardPopupData
{
    public string popupTitle;
    public List<CopagosProductData> products =
        new List<CopagosProductData>();

    public bool HasProducts =>
        products != null &&
        products.Count > 0;

    /// <summary>
    /// Convierte el bloque recibido desde el JSON en los datos
    /// que utiliza el popup y que se enviarán mediante PRODUCT_CLICKED.
    /// </summary>
    public static CopagosRewardPopupData FromDto(
        RewardPopupConfigDto source)
    {
        CopagosRewardPopupData result =
            new CopagosRewardPopupData();

        if (source == null)
        {
            return result;
        }

        result.popupTitle =
            source.popup_title ?? string.Empty;

        if (source.products == null)
        {
            return result;
        }

        for (int i = 0; i < source.products.Count; i++)
        {
            RewardProductDto sourceProduct =
                source.products[i];

            if (sourceProduct == null)
            {
                continue;
            }

            CopagosProductData runtimeProduct =
                new CopagosProductData
                {
                    id = sourceProduct.id,
                    name = sourceProduct.name,
                    imageUrl = sourceProduct.image_url,
                    imageMessage = sourceProduct.image_message,
                    commercial = sourceProduct.commercial,
                    regularPrice = sourceProduct.regular_price,
                    keysMessage = sourceProduct.keys_message,
                    rating = sourceProduct.rating,
                    maxKeysAllowed = sourceProduct.max_keys_allowed
                        .ToString(CultureInfo.InvariantCulture),
                    minCashCents = sourceProduct.min_cash_cents
                        .ToString(CultureInfo.InvariantCulture),
                    stock = sourceProduct.stock
                        .ToString(CultureInfo.InvariantCulture),
                    categoryName = sourceProduct.category_name
                };

            result.products.Add(runtimeProduct);
        }

        return result;
    }
}

/// <summary>
/// Mensaje enviado al frontend cuando el usuario selecciona
/// uno de los productos mostrados en el popup.
/// </summary>
[Serializable]
public sealed class CopagosProductClickedMessage
{
    public string type;
    public CopagosProductData product;
}