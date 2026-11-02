using System;
using System.Collections.Generic;
using System.Text;

namespace Instancium.Lens.App.Services
{
    public sealed class MacWindowsFolderPickerService : IFolderPickerService
    {
        public async Task<string?> PickFolderAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken);
            return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        }
    }
}
