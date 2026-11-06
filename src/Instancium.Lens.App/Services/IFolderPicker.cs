using System;
using System.Threading;
using System.Threading.Tasks;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Service abstraction for picking a folder in the file system.
/// </summary>
public interface IFolderPicker
{
    /// <summary>
    /// Prompts the user to pick a folder.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path to the selected folder, or null if canceled.</returns>
    Task<string?> PickFolderAsync(CancellationToken cancellationToken = default);
}
