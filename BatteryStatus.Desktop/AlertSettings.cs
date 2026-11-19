using System;
using System.IO;
using System.Text.Json;

namespace BatteryStatus.Desktop;

internal sealed record AlertSettings(bool HighEnabled, int High, bool LowEnabled, int Low)
{
    internal static AlertSettings Default => new(true, 80, true, 25);

    internal bool IsValid => Low >= 1 && High <= 100 && Low < High;

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "BatteryStatus.Desktop", "alerts.json");

    internal static AlertSettings Load()
    {
        try
        {
            var settings = JsonSerializer.Deserialize<AlertSettings>(File.ReadAllText(FilePath));
            if (settings is { IsValid: true })
                return settings;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException)
        {
            // Missing or invalid preferences fall back to defaults.
        }

        return Default;
    }

    internal void Save()
    {
        var path = FilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(this));
        File.Move(path + ".tmp", path, overwrite: true);
    }
}
