using UnityEngine;

/// <summary>
/// Maneja la economía básica del juego (monedas).
/// Permite agregar, gastar y consultar monedas.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    /// <summary>
    /// Cantidad inicial de monedas.
    /// </summary>
    [Header("Configuración")]
    [SerializeField] private int initialCoins = 0;

    private int currentCoins;

    /// <summary>
    /// Cantidad actual de monedas.
    /// </summary>
    public int CurrentCoins => currentCoins;

    /// <summary>
    /// Inicializa el gestor de moneda.
    private void Awake()
    {
        currentCoins = Mathf.Max(0, initialCoins);
    }

    /// <summary>
    /// Agrega monedas al total actual.
    /// </summary>
    /// <param name="amount">Cantidad a agregar.</param>
    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        currentCoins += amount;
    }

    /// <summary>
    /// Intenta gastar monedas.
    /// </summary>
    /// <param name="amount">Cantidad a gastar.</param>
    /// <returns>True si se pudo gastar.</returns>
    public bool SpendCoins(int amount)
    {
        if (amount <= 0 || currentCoins < amount)
        {
            return false;
        }

        currentCoins -= amount;
        return true;
    }

    /// <summary>
    /// Reinicia las monedas a un valor específico.
    /// </summary>
    public void SetCoins(int amount)
    {
        currentCoins = Mathf.Max(0, amount);
    }
}
