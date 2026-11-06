namespace Instancium.Lens.App.WinUI;

/// <summary>Windows (WinUI 3) entry point.</summary>
public partial class App : MauiWinUIApplication
{
    /// <summary>Initializes the WinUI application object.</summary>
    public App()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
