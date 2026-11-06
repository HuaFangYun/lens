using Android.App;
using Android.Runtime;

namespace Instancium.Lens.App;

/// <summary>Android application object.</summary>
[Application]
public class MainApplication : MauiApplication
{
    /// <summary>Initializes the application object.</summary>
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
