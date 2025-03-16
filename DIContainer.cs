using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Helpers;
using Sheltered2SaveEditor.Navigation;
using Sheltered2SaveEditor.Services;
using Sheltered2SaveEditor.ViewModels;

namespace Sheltered2SaveEditor;

/// <summary>
/// Provides a centralized container for dependency injection services.
/// </summary>
public static class DIContainer
{
    /// <summary>
    /// Gets the service provider with all registered services.
    /// </summary>
    public static ServiceProvider Services { get; } = ConfigureServices();

    /// <summary>
    /// Configures and builds the service collection.
    /// </summary>
    /// <returns>A <see cref="ServiceProvider"/> containing all registered services.</returns>
    private static ServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        // Register logging providers
        _ = services.AddLogging(static configure =>
        {
            // Use Debug and Console for development
            _ = configure.AddDebug();
            _ = configure.AddConsole();
        });

        // Register navigation services
        _ = services.AddSingleton<PageNavigationRegistry>();
        _ = services.AddSingleton<FrameProvider>();
        _ = services.AddSingleton<INavigationService>(provider =>
        {
            FrameProvider frameProvider = provider.GetRequiredService<FrameProvider>();
            return new NavigationService(frameProvider);
        });

        // Register view models
        _ = services.AddSingleton<MainWindowViewModel>();
        _ = services.AddSingleton<CharactersViewModel>();
        _ = services.AddSingleton<HomePageViewModel>();
        // Register other view models as needed

        // Register services
        _ = services.AddSingleton<IFilePickerService, FilePickerService>();
        _ = services.AddSingleton<IXorCipherService, XorCipherService>();
        _ = services.AddSingleton<FileValidator>();

        return services.BuildServiceProvider();
    }
}