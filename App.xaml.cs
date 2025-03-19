using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Sheltered2SaveEditor.Infrastructure.UI.Dialogs;
using System;
using System.Threading.Tasks;

namespace Sheltered2SaveEditor;

/// <summary>
/// Provides the entry point and lifecycle management for the application.
/// </summary>
public partial class App : Application
{
    // Main window instance with initial configuration
    public static MainWindow MainWindow { get; } = new()
    {
        ExtendsContentIntoTitleBar = true,
    };

    private readonly ILogger<App> _logger = null!;
    private readonly IDialogService _dialogService = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App()
    {
        try
        {
            // Initialize component first to ensure resource dictionaries are loaded
            InitializeComponent();

            // Setup logging and services
            _logger = DIContainer.Services.GetRequiredService<ILogger<App>>();
            _dialogService = DIContainer.Services.GetRequiredService<IDialogService>();

            // Subscribe to global unhandled exceptions
            UnhandledException += App_UnhandledException;

            _logger.LogInformation("Application initialized successfully");
        }
        catch (Exception ex)
        {
            // If we can't even initialize the logger, use system diagnostics as fallback
            System.Diagnostics.Debug.WriteLine($"Critical initialization error: {ex}");

            // In a real app, we might want to show a native message box here
            // since our dialog service may not be available
        }
    }

    /// <summary>
    /// Handles the application launch event.
    /// </summary>
    /// <param name="args">The launch activation arguments.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            _logger.LogInformation("Application launch started");

            // Activate the main window
            MainWindow.Activate();

            _logger.LogInformation("Application launch completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during application launch");
            // We can't use the dialog service yet as the window might not be fully initialized
        }
    }

    /// <summary>
    /// Handles unhandled exceptions in the application.
    /// </summary>
    /// <param name="sender">The source of the unhandled exception event.</param>
    /// <param name="e">Event data that provides information about the exception.</param>
    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
        try
        {
            // Mark the exception as handled to prevent app termination
            e.Handled = true;

            // Log the exception
            _logger.LogError(e.Exception, "An unhandled exception occurred");

            // Display the error dialog asynchronously
            _ = await Task.Run(async () => await MainWindow.DispatcherQueue.EnqueueAsync(async () => await _dialogService.ShowErrorDialogAsync(
                        "An error occurred",
                        e.Exception.Message,
                        includeGitHubOption: true)));
        }
        catch (Exception ex)
        {
            // If the error handling itself fails, log the error
            // but don't attempt to show another dialog to avoid potential infinite loops
            _logger.LogCritical(ex, "Error handling unhandled exception");
        }
    }
}

// Extension method to simplify DispatcherQueue usage with async/await
public static class DispatcherQueueExtensions
{
    public static Task<bool> EnqueueAsync(this Microsoft.UI.Dispatching.DispatcherQueue dispatcher, Func<Task> function)
    {
        TaskCompletionSource<bool> taskCompletionSource = new();

        bool queued = dispatcher.TryEnqueue(async () =>
        {
            try
            {
                await function();
                taskCompletionSource.SetResult(true);
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        if (!queued)
        {
            taskCompletionSource.SetResult(false);
        }

        return taskCompletionSource.Task;
    }
}