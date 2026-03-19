/// <summary>
/// Define un contrato opcional para mecánicas
/// que soportan una imagen guía con toggle.
/// </summary>
public interface IGuideImageMechanic
{
    /// <summary>
    /// Alterna la visibilidad de la imagen guía.
    /// </summary>
    void ToggleGuideImage();

    /// <summary>
    /// Indica si la imagen guía está actualmente visible.
    /// </summary>
    bool IsGuideImageVisible { get; }
}
