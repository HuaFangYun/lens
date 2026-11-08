using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Instancium.Vectra.Core.Contracts;
using Instancium.Vectra.Core.Models;
using Instancium.Vectra.Search;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Storage-root configuration for Vectra collections.
/// </summary>
public sealed record VectraLocationOptions
{
    /// <summary>Index subdirectory created inside the scanned root folder.</summary>
    public string IndexFolderName { get; init; } = ".instancium";

    /// <summary>Fallback root for profiles that were never bound to a folder.</summary>
    public string DefaultBaseDirectory { get; init; } = string.Empty;
}

/// <summary>
/// <see cref="IVectraEngine"/> facade resolving the physical storage root per profile:
/// bound profiles use "&lt;root&gt;/&lt;IndexFolderName&gt;", unbound ones fall back to the
/// default root so pre-existing indexes keep working.
/// WHY: indexes must survive app restarts and travel with the documents; a single
/// global BaseDirectory cannot satisfy per-folder storage while module constructors
/// keep consuming one IVectraEngine.
/// Contract: implements the complete Instancium.Vectra 1.1.1 IVectraEngine surface
/// (GetOrOpenCollection, ListCollectionsAsync, DeleteCollectionAsync, DisposeAsync).
/// ListCollectionsAsync aggregates every managed root reported by the resolver.
/// </summary>
public sealed class ProfileScopedVectraEngine : IVectraEngine
{
    private readonly IIndexLocationResolver _resolver;
    private readonly ConcurrentDictionary<string, IVectraEngine> _engines = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Initializes the facade with the index location resolver.</summary>
    public ProfileScopedVectraEngine(IIndexLocationResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <inheritdoc />
    public IVectraCollection GetOrOpenCollection(string name, CollectionOptions options) =>
        Resolve(name).GetOrOpenCollection(name, options);

    /// <inheritdoc />
    public async Task<IReadOnlyList<CollectionInfo>> ListCollectionsAsync(CancellationToken cancellationToken = default)
    {
        var result = new List<CollectionInfo>();
        foreach (var baseDirectory in _resolver.ManagedRoots().Distinct(StringComparer.OrdinalIgnoreCase))
        {
            IReadOnlyList<CollectionInfo> collections = await GetEngine(baseDirectory)
                .ListCollectionsAsync(cancellationToken)
                .ConfigureAwait(false);
            result.AddRange(collections);
        }
        return result;
    }

    /// <inheritdoc />
    public Task DeleteCollectionAsync(string name, CancellationToken cancellationToken = default) =>
        Resolve(name).DeleteCollectionAsync(name, cancellationToken);

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        foreach (IVectraEngine engine in _engines.Values)
        {
            await engine.DisposeAsync().ConfigureAwait(false);
        }
        _engines.Clear();
    }

    private IVectraEngine Resolve(string profileId) => GetEngine(_resolver.ForProfile(profileId));

    private IVectraEngine GetEngine(string baseDirectory) =>
        _engines.GetOrAdd(
            baseDirectory,
            directory => VectraEngineFactory.Create(new VectraOptions { BaseDirectory = directory }));
}
