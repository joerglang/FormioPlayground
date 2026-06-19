using System.Text.Json;

namespace FormioPlaygroundWeb.Services;

/// <summary>PLZ-Lookup für Baselland – lädt gemeinden-bl.json einmalig beim Start.</summary>
public sealed class GemeindenService(IWebHostEnvironment env)
{
    private readonly Dictionary<string, List<string>> _data = Load(env.WebRootPath);

    public IReadOnlyList<string> FindByPlz(string plz) =>
        _data.TryGetValue(plz.Trim(), out var list) ? list : [];

    private static Dictionary<string, List<string>> Load(string webRootPath)
    {
        var path = Path.Combine(webRootPath, "data", "gemeinden-bl.json");
        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json)
               ?? [];
    }
}