#if WINDOWS
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WinRT.Interop;

namespace Instancium.Lens.App.Services;

/// <summary>
/// Windows implementation of <see cref="IFolderPicker"/> over the Win32
/// IFileOpenDialog with FOS_PICKFOLDERS.
/// WHY: the WinRT FolderPicker dialog keeps the commit button disabled until a
/// folder node is explicitly selected, which is impossible inside an empty
/// folder; the Win32 dialog always commits the folder currently browsed.
/// </summary>
public sealed class WindowsFolderPicker : IFolderPicker
{
    private const int HResultCancelled = unchecked((int)0x800704C7);

    /// <inheritdoc />
    public Task<string?> PickFolderAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var dialog = (IFileOpenDialog)new FileOpenDialogRCW();
        try
        {
            dialog.GetOptions(out var options);
            dialog.SetOptions(options
                | (uint)FileDialogOptions.PickFolders
                | (uint)FileDialogOptions.ForceFileSystem);
            dialog.SetTitle("Select a folder to index");

            var hr = dialog.Show(GetOwnerWindowHandle());
            if (hr == HResultCancelled)
            {
                return Task.FromResult<string?>(null);
            }

            Marshal.ThrowExceptionForHR(hr);
            dialog.GetResult(out var item);
            try
            {
                item.GetDisplayName(SIGDN.FileSysPath, out var pathPtr);
                try
                {
                    return Task.FromResult(Marshal.PtrToStringUni(pathPtr));
                }
                finally
                {
                    Marshal.FreeCoTaskMem(pathPtr);
                }
            }
            finally
            {
                Marshal.ReleaseComObject(item);
            }
        }
        finally
        {
            Marshal.ReleaseComObject(dialog);
        }
    }

    /// <summary>Resolves the WinUI window handle so the dialog is modal to the app.</summary>
    private static nint GetOwnerWindowHandle()
    {
        var window = Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0
            ? Microsoft.Maui.Controls.Application.Current.Windows[0]
            : null;
        return window?.Handler?.PlatformView is Microsoft.UI.Xaml.Window winuiWindow
            ? WindowNative.GetWindowHandle(winuiWindow)
            : 0;
    }

    [ComImport]
    [Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7")]
    [ClassInterface(ClassInterfaceType.None)]
    private class FileOpenDialogRCW
    {
    }

    [ComImport]
    [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IFileOpenDialog
    {
        // WHY: vtable order is a runtime contract; it MUST mirror shobjidl.idl
        // (IModalWindow.Show, then IFileDialog in IDL order: SetOptions precedes
        // GetOptions). Any reorder corrupts the native stack at the first call.
        [PreserveSig] int Show(nint parentWindow);
        [PreserveSig] int SetFileTypes(uint cFileTypes, nint rgFilterSpec);
        [PreserveSig] int SetFileTypeIndex(uint iFileType);
        [PreserveSig] int GetFileTypeIndex(out uint piFileType);
        [PreserveSig] int Advise(nint pfde, out uint pdwCookie);
        [PreserveSig] int Unadvise(uint dwCookie);
        [PreserveSig] int SetOptions(uint fos);
        [PreserveSig] int GetOptions(out uint pfos);
        [PreserveSig] int SetDefaultFolder(nint psi);
        [PreserveSig] int SetFolder(nint psi);
        [PreserveSig] int GetFolder(out nint ppsi);
        [PreserveSig] int GetCurrentSelection(out nint ppsi);
        [PreserveSig] int SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        [PreserveSig] int GetFileName(out nint pszName);
        [PreserveSig] int SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
        [PreserveSig] int SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
        [PreserveSig] int SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);
        [PreserveSig] int GetResult([MarshalAs(UnmanagedType.Interface)] out IShellItem ppsi);
    }

    [ComImport]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItem
    {
        [PreserveSig] int BindToHandler(nint pbc, ref Guid bhid, ref Guid riid, out nint ppv);
        [PreserveSig] int GetParent(out nint ppsi);
        [PreserveSig] int GetDisplayName(SIGDN sigdnName, out nint ppszName);
        [PreserveSig] int GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
        [PreserveSig] int Compare(nint psi, uint hint, out int piOrder);
    }

    private enum SIGDN : uint
    {
        FileSysPath = 0x80058000
    }

    [Flags]
    private enum FileDialogOptions : uint
    {
        PickFolders = 0x00000020,
        ForceFileSystem = 0x00000040
    }
}
#endif
