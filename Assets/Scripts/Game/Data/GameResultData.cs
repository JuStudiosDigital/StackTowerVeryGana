/// <summary>
/// Estructura inmutable que contiene la información final de una partida.
/// Se utiliza para transportar los resultados del juego entre sistemas
/// sin exponer lógica ni permitir modificaciones posteriores.
/// </summary>
public struct GameResultData
{
    #region Fields

    /// <summary>
    /// Tiempo total de la partida representado como string formateado.
    /// El formato depende del sistema que construye este valor.
    /// </summary>
    public readonly string time;

    /// <summary>
    /// Número total de movimientos realizados durante la partida.
    /// </summary>
    public readonly int totalMoves;

    /// <summary>
    /// Cantidad total de monedas recolectadas durante la partida.
    /// </summary>
    public readonly int coinsCollected;

    #endregion

    #region Constructor

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="GameResultData"/> con los resultados finales de la partida.
    /// </summary>
    /// <param name="time">Tiempo total de la partida en formato string.</param>
    /// <param name="totalMoves">Cantidad total de movimientos realizados.</param>
    /// <param name="coinsCollected">Cantidad total de monedas recolectadas.</param>
    public GameResultData(string time, int totalMoves, int coinsCollected)
    {
        this.time = time;
        this.totalMoves = totalMoves;
        this.coinsCollected = coinsCollected;
    }

    #endregion
}
