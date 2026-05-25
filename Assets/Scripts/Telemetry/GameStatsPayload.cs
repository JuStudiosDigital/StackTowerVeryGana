using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

/// <summary>
/// Payload final enviado al servicio REST de telemetría.
/// Agrupa las métricas dinámicas dentro de "payload".
/// Incluye formateo opcional para JSON legible (indentado).
/// </summary>
[Serializable]
public sealed class GameStatsPayload
{
    #region Metadata

    public string SessionToken { get; set; }
    public string UserHash { get; set; }
    public bool IsBrandedMode { get; set; }
    public string CampaignId { get; set; }
    public string GameTitle { get; set; }

    public int ErrorCount { get; set; }
    public string[] ErrorTypes { get; set; }
    public float LoadTime { get; set; }

    #endregion

    #region Payload Entries

    private readonly List<GameStatsPayloadEntry> entries = new();

    public void AddInt(string key, int value)
    {
        entries.Add(GameStatsPayloadEntry.CreateInt(key, value));
    }

    public void AddDecimal(string key, float value)
    {
        entries.Add(GameStatsPayloadEntry.CreateDecimal(key, value));
    }

    public void AddDouble(string key, double value)
    {
        entries.Add(GameStatsPayloadEntry.CreateDouble(key, value));
    }

    public void AddBoolean(string key, bool value)
    {
        entries.Add(GameStatsPayloadEntry.CreateBoolean(key, value));
    }

    public void AddString(string key, string value)
    {
        entries.Add(GameStatsPayloadEntry.CreateString(key, value));
    }

    #endregion

    #region Serialization

    /// <summary>
    /// Serializa el payload completo a JSON.
    /// </summary>
    public string ToJson()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append('{');

        AppendString(builder, "sessionToken", SessionToken);
        builder.Append(',');

        AppendString(builder, "userHash", UserHash);
        builder.Append(',');

        AppendBool(builder, "isBrandedMode", IsBrandedMode);
        builder.Append(',');

        AppendString(builder, "campaignId", CampaignId);
        builder.Append(',');

        AppendString(builder, "gameTitle", GameTitle);
        builder.Append(',');

        // payload dinámico
        builder.Append("\"payload\":[");
        for (int i = 0; i < entries.Count; i++)
        {
            if (i > 0)
                builder.Append(',');

            builder.Append(entries[i].ToJson());
        }
        builder.Append("],");

        AppendInt(builder, "error_count", ErrorCount);
        builder.Append(',');

        AppendStringArray(builder, "error_types", ErrorTypes);
        builder.Append(',');

        AppendFloat(builder, "load_time", LoadTime);

        builder.Append('}');

        // 🔥 JSON indentado
        return FormatJson(builder.ToString());
    }

    #endregion

    #region JSON Helpers

    private static void AppendString(StringBuilder builder, string key, string value)
    {
        builder.Append('"').Append(key).Append("\":");
        builder.Append('"').Append(Escape(value)).Append('"');
    }

    private static void AppendInt(StringBuilder builder, string key, int value)
    {
        builder.Append('"').Append(key).Append("\":");
        builder.Append(value);
    }

    private static void AppendFloat(StringBuilder builder, string key, float value)
    {
        builder.Append('"').Append(key).Append("\":");
        builder.Append(value.ToString(CultureInfo.InvariantCulture));
    }

    private static void AppendBool(StringBuilder builder, string key, bool value)
    {
        builder.Append('"').Append(key).Append("\":");
        builder.Append(value ? "true" : "false");
    }

    private static void AppendStringArray(StringBuilder builder, string key, string[] values)
    {
        builder.Append('"').Append(key).Append("\":[");

        if (values != null)
        {
            for (int i = 0; i < values.Length; i++)
            {
                if (i > 0)
                    builder.Append(',');

                builder.Append('"').Append(Escape(values[i])).Append('"');
            }
        }

        builder.Append(']');
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    #endregion

    #region Pretty Print

    /// <summary>
    /// Convierte un JSON compacto en formato indentado legible.
    /// No altera los datos, solo presentación.
    /// </summary>
    private static string FormatJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return json;

        StringBuilder pretty = new StringBuilder();
        int indent = 0;
        bool inQuotes = false;

        for (int i = 0; i < json.Length; i++)
        {
            char c = json[i];

            switch (c)
            {
                case '"':
                    pretty.Append(c);
                    if (i == 0 || json[i - 1] != '\\')
                        inQuotes = !inQuotes;
                    break;

                case '{':
                case '[':
                    pretty.Append(c);
                    if (!inQuotes)
                    {
                        pretty.Append('\n');
                        indent++;
                        pretty.Append(new string('\t', indent));
                    }
                    break;

                case '}':
                case ']':
                    if (!inQuotes)
                    {
                        pretty.Append('\n');
                        indent--;
                        pretty.Append(new string('\t', indent));
                    }
                    pretty.Append(c);
                    break;

                case ',':
                    pretty.Append(c);
                    if (!inQuotes)
                    {
                        pretty.Append('\n');
                        pretty.Append(new string('\t', indent));
                    }
                    break;

                case ':':
                    pretty.Append(c);
                    if (!inQuotes)
                        pretty.Append(' ');
                    break;

                default:
                    pretty.Append(c);
                    break;
            }
        }

        return pretty.ToString();
    }

    #endregion
}

/// <summary>
/// Representa una entrada individual dentro del arreglo "payload".
/// </summary>
public readonly struct GameStatsPayloadEntry
{
    private readonly string key;
    private readonly string type;
    private readonly string rawValue;
    private readonly bool quoteValue;

    private GameStatsPayloadEntry(string key, string type, string rawValue, bool quoteValue)
    {
        this.key = key;
        this.type = type;
        this.rawValue = rawValue;
        this.quoteValue = quoteValue;
    }

    public static GameStatsPayloadEntry CreateInt(string key, int value)
    {
        return new GameStatsPayloadEntry(key, "INT", value.ToString(CultureInfo.InvariantCulture), false);
    }

    public static GameStatsPayloadEntry CreateDecimal(string key, float value)
    {
        return new GameStatsPayloadEntry(key, "DECIMAL", value.ToString(CultureInfo.InvariantCulture), false);
    }

    public static GameStatsPayloadEntry CreateDouble(string key, double value)
    {
        return new GameStatsPayloadEntry(key, "DOUBLE", value.ToString(CultureInfo.InvariantCulture), false);
    }

    public static GameStatsPayloadEntry CreateBoolean(string key, bool value)
    {
        return new GameStatsPayloadEntry(key, "BOOLEAN", value ? "true" : "false", false);
    }

    public static GameStatsPayloadEntry CreateString(string key, string value)
    {
        return new GameStatsPayloadEntry(key, "STRING", Escape(value), true);
    }

    public string ToJson()
    {
        StringBuilder builder = new StringBuilder();

        builder.Append('{');
        builder.Append("\"key\":\"").Append(Escape(key)).Append("\",");
        builder.Append("\"type\":\"").Append(type).Append("\",");
        builder.Append("\"value\":");

        if (quoteValue)
            builder.Append('"').Append(rawValue).Append('"');
        else
            builder.Append(rawValue);

        builder.Append('}');

        return builder.ToString();
    }

    private static string Escape(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }
}