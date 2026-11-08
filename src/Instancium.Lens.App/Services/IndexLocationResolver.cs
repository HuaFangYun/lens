using System;
using System.Collections.Generic;
using System.IO;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Single source of truth for the physical index root shown in the UI and consumed
/// by the engine facade.
/// WHY: DoD 4.0 item 2 forbids the UI and the storage engine disagreeing on where a
/// profile index lives; the resolution formula must exist exactly once.
/// </summary>
public interface IIndexLocationResolver
{
    /// <summary>Index root of a bound profile, or the default root when unbound.</summary>
    string ForProfile(string profileId);

    /// <summary>Index root of a folder that is about to be scanned (pre-bind preview).</summary>
    string ForFolder(string folderPath);

    /// <summary>All managed roots: the default root plus every bound profile root.</summary>
    IReadOnlyList<string> ManagedRoots();
}

/// <summary>Registry-backed <see cref="IIndexLocationResolver"/>.</summary>
public sealed class IndexLocationResolver : IIndexLocationResolver
{
    private readonly IIndexLocationRegistry _registry;
    private readonly VectraLocationOptions _options;
    private readonly VectraStorageInfo _storage;

    /// <summary>Initializes the resolver with the binding registry and storage roots.</summary>
    public IndexLocationResolver(
        IIndexLocationRegistry registry,
        VectraLocationOptions options,
        VectraStorageInfo storage)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
    }

    /// <inheritdoc />
    public string ForProfile(string profileId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(profileId);
        return _registry.TryResolve(profileId, out var root) && !string.IsNullOrWhiteSpace(root)
            ? ForFolder(root)
            : _storage.BaseDirectory;
    }

    /// <inheritdoc />
    public string ForFolder(string folderPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);
        return Path.Combine(Path.GetFullPath(folderPath), _options.IndexFolderName);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> ManagedRoots()
    {
        var roots = new List<string> { _storage.BaseDirectory };
        foreach (var root in _registry.ListBindings().Values)
        {
            if (!string.IsNullOrWhiteSpace(root))
            {
                roots.Add(ForFolder(root));
            }
        }
        return roots;
    }
}
