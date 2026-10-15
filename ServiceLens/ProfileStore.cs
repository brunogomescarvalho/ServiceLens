using System.IO;
using System.Text.Json;
namespace ServiceLens;
public static class ProfileStore
{
    public static string FilePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ServiceLens", "profiles.json");
    public static List<Profile> Load() => File.Exists(FilePath) ? JsonSerializer.Deserialize<List<Profile>>(File.ReadAllText(FilePath)) ?? [] : [];
    public static void Save(IEnumerable<Profile> profiles)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var temp = FilePath + ".tmp";
        File.WriteAllText(temp, JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(temp, FilePath, true);
    }
}
