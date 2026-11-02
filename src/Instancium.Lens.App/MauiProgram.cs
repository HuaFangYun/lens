using Instancium.Lens.App.Services;
using Instancium.Vectra.Sdk.DependencyInjection;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Instancium.Lens.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddVectraEngine(builder.Configuration, options =>
        {
            options.BaseStorageDirectory = FileSystem.AppDataDirectory;
            options.DefaultEmbeddingRepoId = "qwen3-embedding-0.6b";
            options.ProductFolderName = "lens-vectra";
        });


        builder.Services.AddTransient<HubConnection>(sp =>
        {

            return new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/gateway")
                .WithAutomaticReconnect()
                .Build();
        });

        builder.Services.AddSingleton<IFolderPickerService, MacWindowsFolderPickerService>();

        return builder.Build();
    }
}