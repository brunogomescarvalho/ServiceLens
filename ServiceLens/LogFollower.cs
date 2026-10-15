using System.IO;
using System.Text;
namespace ServiceLens;
// Polling tolera recriação da pasta e arquivos sem depender de FileSystemWatcher.
public sealed class LogFollower : IDisposable
{
    private FileStream? stream;
    private Decoder decoder = Encoding.UTF8.GetDecoder();
    private string pending = "";
    private string? current;
    private DateTime creation;
    private long position;
    public string? Current => current;
    public IReadOnlyList<string> Read(Profile profile)
    {
        var candidate = Directory.EnumerateFiles(profile.Folder, profile.Pattern, SearchOption.TopDirectoryOnly)
            .Select(p => new FileInfo(p)).OrderByDescending(f => f.LastWriteTimeUtc).ThenBy(f => f.FullName).FirstOrDefault();
        if (candidate is null) return [];
        var output = new List<string>();
        if (stream is null || current != candidate.FullName || creation != candidate.CreationTimeUtc || candidate.Length < position)
        {
            if (pending.Length > 0) output.Add(pending);
            Dispose();
            current = candidate.FullName;
            creation = candidate.CreationTimeUtc;
            stream = new FileStream(current, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            position = Math.Max(0, stream.Length - 256 * 1024);
            stream.Seek(position, SeekOrigin.Begin);
            if (position > 0) // ignora a primeira linha parcial da janela inicial
            {
                int b;
                while ((b = stream.ReadByte()) != -1 && b != 10) { }
                position = stream.Position;
            }
            output.Add($"── Arquivo: {current} ──");
        }
        // Limita o trabalho por ciclo para manter a interface responsiva.
        var bytes = new byte[64 * 1024];
        var chars = new char[Encoding.UTF8.GetMaxCharCount(bytes.Length)];
        for (var batch = 0; batch < 8; batch++)
        {
            var count = stream.Read(bytes, 0, bytes.Length);
            if (count == 0) break;
            position += count;
            var length = decoder.GetChars(bytes, 0, count, chars, 0, false);
            pending += new string(chars, 0, length);
            var start = 0;
            int end;
            while ((end = pending.IndexOf('\n', start)) >= 0)
            {
                output.Add(pending[start..end].TrimEnd('\r').TrimStart('\uFEFF'));
                start = end + 1;
            }
            pending = pending[start..];
            if (pending.Length > 1024 * 1024) { output.Add(pending + " [linha dividida em 1 MiB]"); pending = ""; }
        }
        return output;
    }
    public void Dispose()
    {
        stream?.Dispose(); stream = null; position = 0; current = null;
        pending = ""; decoder = Encoding.UTF8.GetDecoder();
    }
}
