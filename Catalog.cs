using System.Text.Json;
using Microsoft.Win32;

namespace FirawynixDock;

internal sealed record CatalogSnapshot(List<CatalogItem> Items, string Source);

internal sealed class CatalogItem
{
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required string Category { get; init; }
    public required string Type { get; init; }
    public required string? Url { get; init; }
    public required string? ImageUrl { get; init; }
    public required string? Target { get; init; }
    public string Action => Type == "web" && Target is not null ? "Abrir site" :
        Type != "web" && Target is not null ? "Abrir programa" : "Abrir Center";
}

internal static class Catalog
{
    private const string ApiBase = "https://jogos.firawynix.com.br";
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(12) };
    private static readonly string CenterData = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "br.com.firawynix.center");
    private static readonly string DockData = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FirawynixDock");

    public static CatalogSnapshot LoadLocal()
    {
        foreach (var (path, source) in new[]
        {
            (Path.Combine(CenterData, "catalogo.json"), "Firawynix Center"),
            (Path.Combine(DockData, "catalogo.json"), "catálogo salvo"),
            (Path.Combine(AppContext.BaseDirectory, "catalogo-snapshot.json"), "catálogo incluído")
        })
        {
            try
            {
                if (File.Exists(path)) return new CatalogSnapshot(Parse(File.ReadAllText(path)), source);
            }
            catch (Exception) { /* Tenta o próximo catálogo disponível. */ }
        }
        return new CatalogSnapshot([], "catálogo indisponível");
    }

    public static async Task<CatalogSnapshot?> RefreshRemoteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await Http.GetStringAsync($"{ApiBase}/api/games", cancellationToken);
            var remote = Parse(json);
            if (remote.Count == 0) return null;
            Directory.CreateDirectory(DockData);
            await File.WriteAllTextAsync(Path.Combine(DockData, "catalogo.json"), json, cancellationToken);
            var local = LoadLocal().Items;
            var known = remote.Select(x => x.Slug).ToHashSet(StringComparer.OrdinalIgnoreCase);
            remote.AddRange(local.Where(x => !known.Contains(x.Slug)));
            return new CatalogSnapshot(remote, "catálogo atualizado");
        }
        catch (Exception) { return null; }
    }

    private static List<CatalogItem> Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var folders = ReadMap("pastas.json");
        var selected = ReadMap("locais.json");
        var items = new List<CatalogItem>();
        foreach (var section in new[] { "jogos", "projetos" })
        {
            if (!doc.RootElement.TryGetProperty(section, out var group) || group.ValueKind != JsonValueKind.Array)
                continue;
            foreach (var entry in group.EnumerateArray())
            {
                if (entry.TryGetProperty("disponivel", out var available) && available.ValueKind == JsonValueKind.False)
                    continue;
                var slug = Text(entry, "slug");
                if (string.IsNullOrWhiteSpace(slug)) continue;
                var type = Text(entry, "tipo") ?? "web";
                var url = SafeUrl(Text(entry, "jogarUrl") ?? Text(entry, "siteUrl"));
                var image = SafeUrl(Text(entry, "icone") ?? Text(entry, "capa"));
                var target = type == "web" ? url : DetectInstalled(entry, slug, selected, folders);
                items.Add(new CatalogItem
                {
                    Name = Text(entry, "nome") ?? slug,
                    Slug = slug,
                    Category = section == "jogos" ? "Jogos" : "Projetos",
                    Type = type,
                    Url = url,
                    ImageUrl = image,
                    Target = target
                });
            }
        }
        return items.DistinctBy(x => x.Slug, StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static string? DetectInstalled(JsonElement entry, string slug,
        Dictionary<string, string> selected, Dictionary<string, string> folders)
    {
        var installer = Text(entry, "winInstalador");
        var extension = installer == "cmd" ? ".cmd" : ".exe";
        if (selected.TryGetValue(slug, out var selectedPath) && ValidExecutable(selectedPath, extension))
            return selectedPath;

        var executable = Text(entry, "winExe");
        if (!string.IsNullOrWhiteSpace(executable) && !Path.IsPathRooted(executable) &&
            !executable.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Contains(".."))
        {
            var registryFolder = RegistryInstallFolder(Text(entry, "winChave"));
            if (registryFolder is not null)
            {
                var path = Path.Combine(registryFolder, executable);
                if (ValidExecutable(path, extension)) return path;
            }
            if (folders.TryGetValue(slug, out var folder))
            {
                var path = Path.Combine(folder, executable);
                if (ValidExecutable(path, extension)) return path;
            }
        }

        if (entry.TryGetProperty("winCaminhos", out var paths) && paths.ValueKind == JsonValueKind.Array)
        {
            foreach (var candidate in paths.EnumerateArray())
            {
                if (candidate.ValueKind != JsonValueKind.String) continue;
                var raw = candidate.GetString();
                if (raw is null || raw.Contains("..")) continue;
                var expanded = Environment.ExpandEnvironmentVariables(raw);
                if (expanded.Contains('%')) continue;
                if (ValidExecutable(expanded, extension)) return expanded;
            }
        }
        return null;
    }

    private static string? RegistryInstallFolder(string? key)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > 120 ||
            key.Any(c => !char.IsAsciiLetterOrDigit(c) && !"{}_-. ".Contains(c))) return null;
        const string uninstall = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\";
        foreach (var (hive, view) in new[]
        {
            (RegistryHive.CurrentUser, RegistryView.Default),
            (RegistryHive.LocalMachine, RegistryView.Registry64),
            (RegistryHive.LocalMachine, RegistryView.Registry32)
        })
        {
            try
            {
                using var root = RegistryKey.OpenBaseKey(hive, view);
                using var entry = root.OpenSubKey(uninstall + key);
                var location = (entry?.GetValue("InstallLocation") as string)?.Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(location)) return location;
                var displayIcon = (entry?.GetValue("DisplayIcon") as string)?.Trim().Trim('"');
                if (displayIcon is null) continue;
                var comma = displayIcon.LastIndexOf(',');
                if (comma >= 0 && int.TryParse(displayIcon[(comma + 1)..], out _))
                    displayIcon = displayIcon[..comma].Trim().Trim('"');
                var folder = Path.GetDirectoryName(displayIcon);
                if (!string.IsNullOrWhiteSpace(folder)) return folder;
            }
            catch (Exception) { /* Ausência ou permissão negada não impede os próximos métodos. */ }
        }
        return null;
    }

    private static bool ValidExecutable(string path, string extension) =>
        Path.IsPathFullyQualified(path) &&
        string.Equals(Path.GetExtension(path), extension, StringComparison.OrdinalIgnoreCase) &&
        File.Exists(path);

    private static Dictionary<string, string> ReadMap(string name)
    {
        try
        {
            var path = Path.Combine(CenterData, name);
            return File.Exists(path)
                ? JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path)) ?? []
                : [];
        }
        catch (Exception) { return []; }
    }

    private static string? SafeUrl(string? value)
    {
        if (value?.StartsWith('/') == true) value = ApiBase + value;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps
            ? uri.AbsoluteUri : null;
    }

    private static string? Text(JsonElement entry, string name) =>
        entry.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() : null;
}
