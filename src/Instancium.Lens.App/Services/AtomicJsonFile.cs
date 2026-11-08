using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Process-safe JSON state file with atomic replace writes and an in-memory cache.
/// WHY: a partially written state file must never corrupt the only map that survives
/// restarts; the location registry and the stats store share this guarantee.
/// </summary>
/// <typeparam name="T">Root state type persisted in the file.</typeparam>
internal sealed class AtomicJsonFile<T> where T : class
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly string _path;
    private readonly Func<T> _empty;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private T? _cache;

    /// <summary>Initializes the state file wrapper.</summary>
    internal AtomicJsonFile(string path, Func<T> empty)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(empty);
        _path = path;
        _empty = empty;
    }

    /// <summary>Reads the cached state, loading it from disk once.</summary>
    internal T Read()
    {
        if (_cache is not null)
        {
            return _cache;
        }
        try
        {
            if (File.Exists(_path))
            {
                _cache = JsonSerializer.Deserialize<T>(File.ReadAllText(_path)) ?? _empty();
                return _cache;
            }
        }
        catch (JsonException)
        {
        }
        catch (IOException)
        {
        }
        _cache = _empty();
        return _cache;
    }

    /// <summary>Applies the mutation and persists it atomically (temp file + move).</summary>
    internal async Task<T> UpdateAsync(Func<T, T> mutate, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mutate);
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var next = mutate(Read());
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var tempPath = _path + ".tmp";
            await File.WriteAllTextAsync(
                tempPath,
                JsonSerializer.Serialize(next, JsonOptions),
                cancellationToken).ConfigureAwait(false);
            File.Move(tempPath, _path, overwrite: true);
            _cache = next;
            return next;
        }
        finally
        {
            _gate.Release();
        }
    }
}
