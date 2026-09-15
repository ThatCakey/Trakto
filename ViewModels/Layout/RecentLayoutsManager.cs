using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Trakto.ViewModels.Layout;

public class RecentLayoutsManager
{
    private readonly string _settingsFilePath;
    private const int MaxRecent = 5;

    public List<string> RecentPaths { get; private set; } = new();

    public RecentLayoutsManager()
    {
        _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "recent_layouts.json");
        Load();
    }

    public void Add(string path)
    {
        RecentPaths.RemoveAll(p => p.Equals(path, StringComparison.OrdinalIgnoreCase));
        RecentPaths.Insert(0, path);

        if (RecentPaths.Count > MaxRecent)
        {
            RecentPaths.RemoveRange(MaxRecent, RecentPaths.Count - MaxRecent);
        }
        Save();
    }

    private void Load()
    {
        if (File.Exists(_settingsFilePath))
        {
            try
            {
                var json = File.ReadAllText(_settingsFilePath);
                var loaded = JsonSerializer.Deserialize<List<string>>(json);
                if (loaded != null)
                {
                    RecentPaths = loaded;
                }
            }
            catch
            {
                // Ignore load errors
            }
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(RecentPaths);
            File.WriteAllText(_settingsFilePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
