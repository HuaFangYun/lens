#if WINDOWS
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Windows implementation of <see cref="IFolderPicker"/> using WinUI/WinRT <see cref="FolderPicker"/>.
/// </summary>
public sealed class WindowsFolderPicker : IFolderPicker
{
    /// <inheritdoc />
    public async Task<string?> PickFolderAsync(CancellationToken cancellationToken = default)
    {
        var picker = new FolderPicker();
        picker.FileTypeFilter.Add("*");

        var window = Application.Current?.Windows.Count > 0 ? Application.Current.Windows[0] : null;
        if (window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window winuiWindow)
        {
            nint hwnd = WindowNative.GetWindowHandle(winuiWindow);
            InitializeWithWindow.Initialize(picker, hwnd);
        }

        StorageFolder? folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }
}
#endif
