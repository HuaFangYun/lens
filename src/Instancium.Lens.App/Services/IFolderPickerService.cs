using System;
using System.Collections.Generic;
using System.Text;

namespace Instancium.Lens.App.Services
{
    public interface IFolderPickerService
    {
        Task<string?> PickFolderAsync(CancellationToken cancellationToken = default);
    }
}
