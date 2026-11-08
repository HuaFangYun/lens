using System;
using System.Collections.Generic;
using System.IO;
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
    private readonly AtomicJsonFile<Dictionary<string, string>> _file;

    /// <summary>Initializes the registry over the per-user AppData state file.</summary>
    public IndexLocationRegistry()
    {
        _file = new AtomicJsonFile<Dictionary<string, string>>(
            Path.Combine(FileSystem.AppDataDirectory, "lens-index-locations.json"),
            () => new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, string> ListBindings() =>
        new Dictionary<string, string>(_file.Read(), StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public Task BindAsync(string profileId, string rootFolder, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootFolder);
        var fullRoot = Path.GetFullPath(rootFolder);
        return _file.UpdateAsync(
            map =>
            {
                var next = new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase)
                {
                    [profileId] = fullRoot
                };
                return next;
            },
            cancellationToken);
    }

    /// <inheritdoc />
    public bool TryResolve(string profileId, out string? rootFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        return _file.Read().TryGetValue(profileId, out rootFolder);
    }
}
