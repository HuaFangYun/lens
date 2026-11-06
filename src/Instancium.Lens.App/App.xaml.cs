namespace Instancium.Lens.App;

/// <summary>Application entry; hosts the Blazor hybrid shell in a single window.</summary>
public partial class App : Application
{
    /// <summary>Initializes the application shell.</summary>
    public App()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new MainPage()) { Title = "Instancium Lens" };
    }
}
