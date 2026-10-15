namespace ServiceLens;
public sealed class Profile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Label { get; set; } = "";
    public string ServiceName { get; set; } = "";
    public string Folder { get; set; } = "";
    public string Pattern { get; set; } = "*.log";
}
public sealed record LogLine(string Text, string Severity)
{
    public string Color => Severity switch { "Erro" => "#FF9999", "Aviso" => "#F9D777", _ => "#DCE6F4" };
    public static LogLine Parse(string text)
    {
        var upper = text.ToUpperInvariant();
        var severity = upper.Contains("ERROR") || upper.Contains("FATAL") || upper.Contains("[ERR]") || upper.Contains("EXCEPTION") ? "Erro"
            : upper.Contains("WARN") || upper.Contains("[WRN]") ? "Aviso"
            : upper.Contains("INFO") || upper.Contains("[INF]") ? "Informação" : "Outros";
        return new(text, severity);
    }
}
