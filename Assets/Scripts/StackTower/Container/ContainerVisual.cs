using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona la apariencia visual del contenedor utilizando datos de branding.
/// Garantiza aplicación incluso si el objeto ya estaba activo al iniciar.
/// </summary>
public class ContainerVisual : MonoBehaviour
{
    #region Inspector

    [Header("Renderers")]

    [SerializeField]
    [Tooltip("Renderizadores a los que se aplicará el color de branding.")]
    private SpriteRenderer[] renderers;

    #endregion

    #region Unity

    /// <summary>
    /// Intenta aplicar color al activarse.
    /// </summary>
    private void OnEnable()
    {
        TryApply();
    }

    /// <summary>
    /// Asegura aplicación en objetos ya activos al inicio.
    /// </summary>
    private void Start()
    {
        StartCoroutine(ApplyWhenReady());
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Espera a que BrandingManager esté disponible antes de aplicar el color.
    /// </summary>
    private IEnumerator ApplyWhenReady()
    {
        while (BrandingManager.Instance == null)
            yield return null;

        ApplyBrandingColor();
    }

    #endregion

    #region Core

    /// <summary>
    /// Intenta aplicar el color si el manager está disponible.
    /// </summary>
    private void TryApply()
    {
        if (BrandingManager.Instance == null)
            return;

        ApplyBrandingColor();
    }

    /// <summary>
    /// Aplica un color aleatorio proveniente del BrandingManager.
    /// </summary>
    private void ApplyBrandingColor()
    {
        Color color = BrandingManager.Instance.GetRandomColor();

        foreach (var rend in renderers)
        {
            if (rend != null)
                rend.color = color;
        }
    }

    #endregion
}