using System.Threading;
using System.Threading.Tasks;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Persists the binding between a profile (Vectra collection name) and the root
/// folder whose index subdirectory physically stores the collection.
/// WHY: the index lives next to the scanned documents; the query path must resolve
/// the same location after a restart without re-picking the folder.
/// </summary>
public interface IIndexLocationRegistry
{
    /// <summary>Binds the profile to the scanned root folder and persists the map.</summary>
    Task BindAsync(string profileId, string rootFolder, CancellationToken cancellationToken = default);

    /// <summary>Resolves the previously bound root folder for the profile.</summary>
    bool TryResolve(string profileId, out string? rootFolder);

    /// <summary>Returns an immutable snapshot of all persisted profile-to-root bindings.</summary>
    IReadOnlyDictionary<string, string> ListBindings();
}
