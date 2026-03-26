using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Aplica branding visual (imagen y texto) dentro del prefab u objeto en escena.
/// Garantiza inicialización incluso si el objeto ya está activo al inicio.
/// </summary>
public sealed class BrandingDisplay : MonoBehaviour
{
    #region References

    private SpriteRenderer imageRenderer;
    private TMP_Text textRenderer;

    #endregion

    #region Unity

    /// <summary>
    /// Cachea referencias locales.
    /// </summary>
    private void Awake()
    {
        CacheLocalComponents();
    }

    /// <summary>
    /// Asegura aplicación de branding incluso en objetos ya activos en escena.
    /// </summary>
    private void Start()
    {
        StartCoroutine(ApplyWhenReady());
    }

    /// <summary>
    /// Permite compatibilidad con pooling.
    /// </summary>
    private void OnEnable()
    {
        TryApply();
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Espera a que BrandingManager esté disponible antes de aplicar branding.
    /// </summary>
    private IEnumerator ApplyWhenReady()
    {
        while (BrandingManager.Instance == null)
            yield return null;

        ApplyBranding();
    }

    #endregion

    #region Setup

    private void CacheLocalComponents()
    {
        var sprites = GetComponentsInChildren<SpriteRenderer>(true);
        var texts = GetComponentsInChildren<TMP_Text>(true);

        foreach (var sr in sprites)
        {
            if (sr.gameObject != gameObject && sr.transform.IsChildOf(transform))
            {
                imageRenderer = sr;
                break;
            }
        }

        foreach (var txt in texts)
        {
            if (txt.gameObject != gameObject && txt.transform.IsChildOf(transform))
            {
                textRenderer = txt;
                break;
            }
        }
    }

    #endregion

    #region Core

    /// <summary>
    /// Intenta aplicar branding si el manager está disponible.
    /// </summary>
    private void TryApply()
    {
        if (BrandingManager.Instance == null)
            return;

        ApplyBranding();
    }

    /// <summary>
    /// Aplica imagen y texto desde el sistema de branding.
    /// </summary>
    private void ApplyBranding()
    {
        if (imageRenderer != null)
        {
            var sprite = BrandingManager.Instance.GetRandomImage();
            if (sprite != null)
                imageRenderer.sprite = sprite;
        }

        if (textRenderer != null)
        {
            textRenderer.text = BrandingManager.Instance.GetRandomText();
        }
    }

    #endregion
}