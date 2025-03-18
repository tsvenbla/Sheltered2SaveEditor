using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Helpers;
using Sheltered2SaveEditor.Navigation;
using Sheltered2SaveEditor.Services;
using Sheltered2SaveEditor.ViewModels;
using System;

namespace Sheltered2SaveEditor;

/// <summary>
/// Provides a centralized container for dependency injection services.
/// </summary>
public static class DIContainer
{
    private static readonly Lazy<ServiceProvider> _serviceProviderLazy = new(ConfigureServices);

    /// <summary>
    /// Gets the service provider with all registered services.
    /// </summary>
    public static ServiceProvider Services => _serviceProviderLazy.Value;

    /// <summary>
    /// Configures and builds the service collection.
    /// </summary>
    /// <returns>A <see cref="ServiceProvider"/> containing all registered services.</returns>
    private static ServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        // Register logging providers
        RegisterLogging(services);

        // Register core services
        RegisterCoreServices(services);

        // Register navigation services
        RegisterNavigationServices(services);

        // Register view models
        RegisterViewModels(services);

        return services.BuildServiceProvider();
    }

    private static void RegisterLogging(ServiceCollection services) => _ = services.AddLogging(static configure =>
                                                                            {
                                                                                // Use Debug and Console for development
                                                                                _ = configure.AddDebug();
                                                                                _ = configure.AddConsole();
                                                                                // Set minimum log level
                                                                                _ = configure.SetMinimumLevel(LogLevel.Information);
                                                                            });

    private static void RegisterCoreServices(ServiceCollection services)
    {
        // Register file services
        _ = services.AddSingleton<IFilePickerService, FilePickerService>();
        _ = services.AddSingleton<IXorCipherService, XorCipherService>();
        _ = services.AddSingleton<IFileService, FileService>();
        _ = services.AddSingleton<FileValidator>();

        // Register dialog service
        _ = services.AddSingleton<IDialogService, DialogService>();
    }

    private static void RegisterNavigationServices(ServiceCollection services)
    {
        // Register navigation registry and helpers
        _ = services.AddSingleton<IPageNavigationRegistry>(provider =>
        {
            ILogger<PageNavigationRegistry> logger = provider.GetRequiredService<ILogger<PageNavigationRegistry>>();
            return new PageNavigationRegistry(logger)
                .RegisterDefaultPages()  // Register the standard pages
                .Build();               // Build the registry to make it immutable
        });

        _ = services.AddSingleton<FrameProvider>();

        // Register navigation service
        _ = services.AddSingleton<INavigationService, NavigationService>();
    }

    private static void RegisterViewModels(ServiceCollection services)
    {
        // Register main window view model
        _ = services.AddSingleton<MainWindowViewModel>();

        // Register page view models
        _ = services.AddSingleton<HomePageViewModel>();
        _ = services.AddSingleton<CharactersViewModel>();

        // Register skill view models
        _ = services.AddTransient<StrengthSkillsViewModel>();
    }
}