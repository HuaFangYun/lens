using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Instancium.Lens.App.Services;

/// <summary>
/// JSON-file backed <see cref="IIndexLocationRegistry"/> with atomic replace writes.
/// WHY: a partially written state file must never corrupt the only map that lets
/// chat find the index after a restart.
/// </summary>
public sealed class IndexLocationRegistry : IIndexLocationRegistry
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _statePath;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Dictionary<string, string>? _map;

    /// <summary>Initializes the registry over the per-user AppData state file.</summary>
    public IndexLocationRegistry()
    {
        _statePath = Path.Combine(FileSystem.AppDataDirectory, "lens-index-locations.json");
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> ListBindings()
    {
        var map = Load();
        return new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase);
    }

    /// <inheritdoc />
    public async Task BindAsync(string profileId, string rootFolder, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder);

        await _gate.WaitAsync(cancellationToken);
        try
        {
            var map = Load();
            map[profileId] = Path.GetFullPath(rootFolder);
            Directory.CreateDirectory(Path.GetDirectoryName(_statePath)!);
            var tempPath = _statePath + ".tmp";
            await File.WriteAllTextAsync(tempPath, JsonSerializer.Serialize(map, JsonOptions), cancellationToken);
            File.Move(tempPath, _statePath, overwrite: true);
            _map = map;
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public bool TryResolve(string profileId, out string? rootFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        var map = Load();
        return map.TryGetValue(profileId, out rootFolder);
    }

    private Dictionary<string, string> Load()
    {
        if (_map is not null)
        {
            return _map;
        }

        try
        {
            if (File.Exists(_statePath))
            {
                _map = JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(_statePath))
                       ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return _map;
            }
        }
        catch (JsonException)
        {
            // WHY: a corrupt state file degrades to an empty map instead of bricking startup.
        }

        _map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        return _map;
    }
}
