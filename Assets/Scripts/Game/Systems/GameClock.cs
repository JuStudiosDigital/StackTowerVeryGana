using TMPro;
using UnityEngine;

/// <summary>
/// Controla el cronómetro del gameplay.
/// Soporta modo normal (count-up) y modo countdown.
/// Es responsable únicamente de la medición y visualización del tiempo.
/// </summary>
public class GameClock : MonoBehaviour
{
    #region Enums

    /// <summary>
    /// Define los modos de funcionamiento del reloj.
    /// </summary>
    public enum ClockMode
    {
        /// <summary>
        /// El tiempo aumenta desde cero.
        /// </summary>
        Normal,

        /// <summary>
        /// El tiempo disminuye desde un valor inicial hasta cero.
        /// </summary>
        Countdown
    }

    #endregion

    #region Serialized Fields

    /// <summary>
    /// Referencia al texto UI donde se muestra el tiempo formateado.
    /// </summary>
    [Header("UI")]
    [SerializeField] private TMP_Text timeText;

    /// <summary>
    /// Modo de funcionamiento actual del reloj.
    /// </summary>
    [Header("Configuración")]
    [SerializeField] private ClockMode clockMode = ClockMode.Normal;

    /// <summary>
    /// Tiempo inicial en segundos utilizado cuando el reloj está en modo Countdown.
    /// </summary>
    [Tooltip("Tiempo inicial en segundos para el modo Countdown")]
    [SerializeField] private float countdownStartTime = 60f;

    #endregion

    #region Private Fields

    /// <summary>
    /// Tiempo interno actual del reloj en segundos.
    /// </summary>
    private float currentTime;

    /// <summary>
    /// Indica si el reloj se encuentra activo y avanzando.
    /// </summary>
    private bool isRunning;

    #endregion

    #region Public Properties

    /// <summary>
    /// Obtiene el tiempo actual del reloj en segundos.
    /// En modo Normal representa el tiempo transcurrido.
    /// En modo Countdown representa el tiempo restante.
    /// </summary>
    public float CurrentTime => currentTime;

    /// <summary>
    /// Obtiene el modo de funcionamiento actual del reloj.
    /// </summary>
    public ClockMode CurrentMode => clockMode;

    #endregion

    #region Unity Lifecycle

    /// <summary>
    /// Inicializa el estado del reloj al cargar el objeto.
    /// </summary>
    private void Awake()
    {
        ResetClock();
    }

    /// <summary>
    /// Actualiza el tiempo y la UI mientras el reloj esté activo.
    /// </summary>
    private void Update()
    {
        if (!isRunning)
        {
            return;
        }

        UpdateTime();
        UpdateTimeText();
    }

    #endregion

    #region Public Control Methods

    /// <summary>
    /// Inicia el avance del reloj.
    /// </summary>
    public void StartClock()
    {
        isRunning = true;
    }

    /// <summary>
    /// Detiene temporalmente el reloj sin reiniciar el tiempo.
    /// </summary>
    public void PauseClock()
    {
        isRunning = false;
    }

    /// <summary>
    /// Reanuda el avance del reloj luego de haber sido pausado.
    /// </summary>
    public void ResumeClock()
    {
        isRunning = true;
    }

    /// <summary>
    /// Reinicia el reloj según el modo actual y lo deja detenido.
    /// </summary>
    public void ResetClock()
    {
        isRunning = false;

        currentTime = clockMode == ClockMode.Countdown
            ? countdownStartTime
            : 0f;

        UpdateTimeText();
    }

    /// <summary>
    /// Configura el reloj en modo Normal (count-up) y reinicia su estado.
    /// </summary>
    public void SetNormalMode()
    {
        clockMode = ClockMode.Normal;
        ResetClock();
    }

    /// <summary>
    /// Configura el reloj en modo Countdown con un tiempo inicial específico
    /// y reinicia su estado.
    /// </summary>
    /// <param name="startTimeSeconds">
    /// Tiempo inicial del countdown expresado en segundos.
    /// </param>
    public void SetCountdownMode(float startTimeSeconds)
    {
        clockMode = ClockMode.Countdown;
        countdownStartTime = Mathf.Max(0f, startTimeSeconds);
        ResetClock();
    }

    #endregion

    #region Private Internal Logic

    /// <summary>
    /// Actualiza el tiempo interno del reloj según el modo configurado.
    /// </summary>
    private void UpdateTime()
    {
        if (clockMode == ClockMode.Normal)
        {
            currentTime += Time.deltaTime;
            return;
        }

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
        }
    }

    /// <summary>
    /// Actualiza el texto de la UI mostrando el tiempo en formato MM:SS.
    /// </summary>
    private void UpdateTimeText()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timeText.text = $"{minutes:00}:{seconds:00}";
    }

    #endregion

    #region Public Utility Methods

    /// <summary>
    /// Devuelve el tiempo actual formateado como una cadena MM:SS.
    /// Útil para UI externa, logs o sistemas que no dependan directamente del TMP_Text.
    /// </summary>
    /// <returns>
    /// Cadena representando el tiempo actual en formato MM:SS.
    /// </returns>
    public string GetTimeString()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        return $"{minutes:00}:{seconds:00}";
    }

    #endregion
}
