using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

/// <summary>
/// Popup de felicitaciones con información dinámica del juego.
/// Maneja la presentación visual del estado de victoria,
/// incluyendo animaciones controladas mediante DOTween.
/// </summary>
public class CongratsPopup : PopupBase
{
    /// <summary>
    /// Evento disparado cuando el jugador confirma el popup.
    /// </summary>
    public event Action OnConfirmRequested;

    /// <summary>
    /// Evento disparado cuando comienza la animación del título de victoria.
    /// </summary>
    public event Action OnTitleAnimationStarted;

    [Header("Texto")]

    [Tooltip("Mensaje principal mostrado en el popup.")]
    [SerializeField] private TMP_Text messageText;

    [Tooltip("Texto estadístico primario.")]
    [SerializeField] private TMP_Text statOneText;

    [Tooltip("Texto estadístico secundario.")]
    [SerializeField] private TMP_Text statTwoText;

    [Tooltip("Texto estadístico terciario.")]
    [SerializeField] private TMP_Text statThreeText;

    [Header("Título")]

    [Tooltip("RectTransform del título animado de victoria.")]
    [SerializeField] private RectTransform titleImage;

    [Tooltip("Delay antes de iniciar la animación del título.")]
    [SerializeField] private float titleAnimationDelay = 0.15f;

    [Header("Otros")]

    [Tooltip("Contenedor visual del contador de monedas.")]
    [SerializeField] private GameObject coinsCounterContainer;

    /// <summary>
    /// Referencia al tween activo del título.
    /// Se mantiene para asegurar limpieza explícita y evitar
    /// ejecuciones sobre objetos destruidos.
    /// </summary>
    private Tween titleTween;

    /// <summary>
    /// Configura dinámicamente el contenido textual del popup.
    /// </summary>
    public void Setup(string message, string statOne, string statTwo, string statThree)
    {
        messageText.text = message;
        statOneText.text = statOne;
        statTwoText.text = statTwo;
        statThreeText.text = statThree;
    }

    /// <summary>
    /// Muestra el popup e inicia la animación visual.
    /// </summary>
    public override void Show()
    {
        base.Show();

        AnimateTitle();

        if (!GameManager.Instance.IsAdsEnabled)
        {
            coinsCounterContainer.SetActive(false);
        }
    }

    /// <summary>
    /// Ejecuta la animación de escala del título.
    /// La animación queda vinculada al GameObject para
    /// garantizar que DOTween la destruya automáticamente
    /// si el objeto es eliminado.
    /// </summary>
    private void AnimateTitle()
    {
        if (titleImage == null)
            return;

        // Limpieza preventiva en caso de reutilización del popup.
        titleTween?.Kill();

        titleImage.localScale = Vector3.zero;

        OnTitleAnimationStarted?.Invoke();

        titleTween = titleImage
            .DOScale(Vector3.one, 0.35f)
            .SetDelay(titleAnimationDelay)
            .SetEase(Ease.OutBack)
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    /// <summary>
    /// Llamado por el botón del popup.
    /// Dispara evento externo y solicita cierre.
    /// </summary>
    public void Confirm()
    {
        DevLog.Log("CongratsPopup: Confirm pressed.");
        OnConfirmRequested?.Invoke();
        RequestClose();
    }

    /// <summary>
    /// Garantiza que ningún tween quede activo
    /// cuando el objeto es deshabilitado.
    /// Previene ejecuciones tardías en UI reutilizable.
    /// </summary>
    private void OnDisable()
    {
        titleTween?.Kill();
    }
}
