using System.Diagnostics;
namespace ServiceLens;
public static class WindowsServices
{
    public static async Task<string> QueryAsync(string? service = null)
    {
        var start = new ProcessStartInfo("sc.exe") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false, CreateNoWindow = true };
        start.ArgumentList.Add("query");
        if (service is not null) start.ArgumentList.Add(service);
        else { start.ArgumentList.Add("state="); start.ArgumentList.Add("all"); start.ArgumentList.Add("bufsize="); start.ArgumentList.Add("65536"); }
        using var process = Process.Start(start) ?? throw new InvalidOperationException("Não foi possível consultar os serviços.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(); throw new TimeoutException("Tempo excedido ao consultar serviços."); }
        return (await stdout) + (await stderr);
    }
}
