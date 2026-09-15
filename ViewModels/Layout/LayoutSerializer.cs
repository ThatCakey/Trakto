using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Trakto.ViewModels.Layout;

public static class LayoutSerializer
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task SaveAsync(LayoutNode rootNode, string path)
    {
        await using var stream = File.Create(path);
        await JsonSerializer.SerializeAsync(stream, rootNode, Options);
    }

    public static async Task<LayoutNode?> LoadAsync(string path)
    {
        if (!File.Exists(path)) return null;

        await using var stream = File.OpenRead(path);
        var node = await JsonSerializer.DeserializeAsync<LayoutNode>(stream, Options);
        return node;
    }
}
