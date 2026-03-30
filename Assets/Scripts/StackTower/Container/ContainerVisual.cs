using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona la aplicación del color de branding sobre los renderizadores del contenedor.
/// </summary>
public class ContainerVisual : MonoBehaviour
{
    #region Inspector

    /// <summary>
    /// Renderizadores a los que se aplicará el color de branding.
    /// </summary>
    [Header("Renderers")]
    [SerializeField]
    [Tooltip("Lista de SpriteRenderer a los que se aplicará el color de branding.")]
    private SpriteRenderer[] renderers;

    #endregion

    #region Unity

    /// <summary>
    /// Intenta aplicar el color al activarse el objeto.
    /// </summary>
    private void OnEnable()
    {
        TryApply();
    }

    /// <summary>
    /// Inicia la rutina de aplicación cuando el sistema de branding esté disponible.
    /// </summary>
    private void Start()
    {
        StartCoroutine(ApplyWhenReady());
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Espera hasta que BrandingManager esté disponible antes de aplicar el color.
    /// </summary>
    /// <returns>Coroutine de espera.</returns>
    private IEnumerator ApplyWhenReady()
    {
        while (BrandingManager.Instance == null)
            yield return null;

        ApplyBrandingColor();
    }

    #endregion

    #region Core

    /// <summary>
    /// Aplica el color si el sistema de branding está disponible.
    /// </summary>
    private void TryApply()
    {
        if (BrandingManager.Instance == null)
            return;

        ApplyBrandingColor();
    }

    /// <summary>
    /// Aplica un color aleatorio a todos los renderizadores configurados.
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