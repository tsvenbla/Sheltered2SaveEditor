using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace Sheltered2SaveEditor;

public partial class App : Application
{
    // Main window instance.
    public static MainWindow MainWindow { get; } = new MainWindow()
    {
        ExtendsContentIntoTitleBar = true,
    };

    public App()
    {
        InitializeComponent();
        // Subscribe to global unhandled exceptions.
        UnhandledException += App_UnhandledException;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args) => MainWindow.Activate();

    // Use the fully qualified type to avoid ambiguity.
    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        // Mark the exception as handled.
        e.Handled = true;

        // Retrieve a logger instance from DI.
        ILogger<App> logger = DIContainer.Services.GetRequiredService<ILogger<App>>();
        logger.LogError(e.Exception, "An unhandled exception occurred.");

        // Display the error in a dialog window so the user can acknowledge it.
        await ShowErrorDialogAsync(e.Exception);
    }

    private static async Task ShowErrorDialogAsync(Exception ex)
    {
        // Create a ContentDialog to show error details.
        ContentDialog dialog = new()
        {
            Title = "An error occurred",
            Content = ex.Message,
            CloseButtonText = "OK",
            PrimaryButtonText = "Create Ticket",
            DefaultButton = ContentDialogButton.Primary
        };

        // Ensure the dialog is anchored in the visual tree.
        if (MainWindow.Content is FrameworkElement fe)
        {
            dialog.XamlRoot = fe.XamlRoot;
        }
        ContentDialogResult result = await dialog.ShowAsync();

        // If the user clicks "Create Ticket", launch the GitHub issues page.
        if (result == ContentDialogResult.Primary)
        {
            Uri uri = new("https://github.com/tsvenbla/Sheltered2SaveEditor/issues/new");
            _ = await Launcher.LaunchUriAsync(uri);
        }
    }
}