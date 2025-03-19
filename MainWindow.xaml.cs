using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Sheltered2SaveEditor.Core;
using Sheltered2SaveEditor.Core.Constants;
using Sheltered2SaveEditor.Infrastructure.Navigation;
using Sheltered2SaveEditor.Infrastructure.UI.Dialogs;
using Sheltered2SaveEditor.ViewModels;
using System;

namespace Sheltered2SaveEditor;

/// <summary>
/// The main application window, responsible for hosting the navigation structure
/// and routing between different pages of the application.
/// </summary>
internal sealed partial class MainWindow : Window, IDisposable
{
    // Public property to expose the ViewModel for x:Bind
    internal MainWindowViewModel ViewModel => _viewModel!;

    private readonly IDialogService? _dialogService;
    private readonly ILogger<MainWindow>? _logger;
    private readonly INavigationService? _navigationService;
    private readonly MainWindowViewModel? _viewModel;
    private readonly IPageNavigationRegistry? _pageRegistry;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// Sets up the navigation structure and event handlers.
    /// </summary>
    internal MainWindow()
    {
        try
        {
            // Setup logging first to capture any initialization errors
            _logger = DIContainer.Services.GetRequiredService<ILogger<MainWindow>>();
            _logger.LogInformation("Initializing MainWindow");

            // Initialize XAML components
            InitializeComponent();

            // Get remaining required services 
            _dialogService = DIContainer.Services.GetRequiredService<IDialogService>();
            _navigationService = DIContainer.Services.GetRequiredService<INavigationService>();
            _viewModel = DIContainer.Services.GetRequiredService<MainWindowViewModel>();
            _pageRegistry = DIContainer.Services.GetRequiredService<IPageNavigationRegistry>();

            // Set DataContext for traditional data binding
            if (Content is FrameworkElement rootElement)
            {
                rootElement.DataContext = _viewModel;
            }

            // Initialize services and subscribe to events
            InitializeNavigationService();
            SubscribeToEvents();

            // Navigate to HomePage on startup
            _logger.LogInformation("Application started. Navigating to HomePage.");
            NavigateToPage(NavigationKeys.Home);
        }
        catch (Exception ex)
        {
            // Handle any errors during initialization
            _logger?.LogCritical(ex, "Failed to initialize MainWindow");
            ShowCriticalErrorMessage(ex);
        }
    }

    /// <summary>
    /// Initializes the navigation service with the content frame.
    /// </summary>
    private void InitializeNavigationService()
    {
        try
        {
            // Get the frame provider
            FrameProvider frameProvider = DIContainer.Services.GetRequiredService<FrameProvider>();
            frameProvider.Initialize(ContentFrameControl);

            // Update the navigation service
            if (_navigationService is NavigationService navigationService)
            {
                navigationService.Initialize(ContentFrameControl);
                _logger?.LogInformation("Navigation service initialized with content frame");
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to initialize navigation service");
            throw; // Re-throw to be caught by the MainWindow constructor
        }
    }

    /// <summary>
    /// Subscribes to application events.
    /// </summary>
    private void SubscribeToEvents()
    {
        // Subscribe to app-wide events
        AppDataHelper.SaveFileLoaded += OnSaveFileLoaded;
        ContentFrameControl.Navigated += OnFrameNavigated;
    }

    /// <summary>
    /// Handles the SaveFileLoaded event by updating the view model's navigation state.
    /// This enables menu items in the navigation pane once a save file is loaded.
    /// </summary>
    private void OnSaveFileLoaded(object? sender, EventArgs e)
    {
        if (_logger != null && _viewModel != null)
        {
            _logger.LogInformation("Save file loaded. Enabling navigation options.");
            _ = DispatcherQueue.TryEnqueue(() => _viewModel.UpdateNavigationState(true));
        }
    }

    /// <summary>
    /// Handles NavigationView selection changes and navigates to the selected page.
    /// </summary>
    private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem selectedItem &&
            selectedItem.Tag is string navigationKey)
        {
            _logger?.LogDebug("Navigation selection changed to {NavigationKey}", navigationKey);
            NavigateToPage(navigationKey);
        }
    }

    /// <summary>
    /// Handles back button requests in the NavigationView.
    /// </summary>
    private void OnNavigationViewBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
    {
        if (ContentFrameControl.CanGoBack)
        {
            _logger?.LogDebug("Back navigation requested");
            SafeNavigateBack();
        }
    }

    /// <summary>
    /// Safely navigates back, handling any exceptions.
    /// </summary>
    private void SafeNavigateBack()
    {
        try
        {
            ContentFrameControl.GoBack();
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during back navigation");
            ShowNavigationError(ex, "previous page");
        }
    }

    /// <summary>
    /// Handles navigation failures in the content frame.
    /// </summary>
    private void OnContentFrameNavigationFailed(object sender, NavigationFailedEventArgs e)
    {
        _logger?.LogError(e.Exception, "Navigation failed to {PageType}", e.SourcePageType.Name);
        e.Handled = true;
        ShowNavigationError(e.Exception, e.SourcePageType.Name);
    }

    /// <summary>
    /// Handles the frame's Navigated event to update UI state after navigation.
    /// </summary>
    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
        if (_viewModel == null || _pageRegistry == null) return;

        // Update the Back button state
        bool canGoBack = ContentFrameControl.CanGoBack;
        _viewModel.UpdateBackNavigationState(canGoBack);

        // Update the navigation menu selection based on the current page
        if (e.SourcePageType != null)
        {
            string navigationKey = _pageRegistry.GetKeyByPageType(e.SourcePageType);
            _logger?.LogDebug("Frame navigated to {PageType}, key: {NavigationKey}",
                    e.SourcePageType.Name, navigationKey);
            UpdateNavigationSelection(navigationKey);
        }
    }

    /// <summary>
    /// Navigates to a page based on its navigation key.
    /// </summary>
    /// <param name="navigationKey">The key identifying the page to navigate to.</param>
    private void NavigateToPage(string navigationKey)
    {
        if (_pageRegistry == null) return;

        try
        {
            Type pageType = _pageRegistry.GetPageTypeByKey(navigationKey);

            if (ContentFrameControl.CurrentSourcePageType != pageType)
            {
                _logger?.LogDebug("Navigating to {PageType} with key {NavigationKey}",
                        pageType.Name, navigationKey);
                _ = ContentFrameControl.Navigate(pageType);
            }
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Failed to navigate to page with key {NavigationKey}", navigationKey);
            ShowNavigationError(ex, navigationKey);
        }
    }

    /// <summary>
    /// Displays an error dialog when navigation fails.
    /// </summary>
    private async void ShowNavigationError(Exception ex, string destination)
    {
        if (_dialogService != null)
        {
            _ = await _dialogService.ShowErrorDialogAsync(
                "Navigation Error",
                $"Failed to navigate to {destination}. Error: {ex.Message}");
        }
    }

    /// <summary>
    /// Displays a critical error message that occurred during initialization.
    /// </summary>
    private async void ShowCriticalErrorMessage(Exception ex)
    {
        try
        {
            ContentDialog dialog = new()
            {
                Title = "Critical Error",
                Content = $"A critical error occurred during application initialization: {ex.Message}\n\nThe application may not function correctly.",
                CloseButtonText = "OK",
                XamlRoot = Content?.XamlRoot
            };

            if (Content?.XamlRoot != null)
            {
                _ = await dialog.ShowAsync();
            }
        }
        catch
        {
            // Last resort - can't even show an error dialog
            // Nothing more we can do here except logging, which already happened
        }
    }

    /// <summary>
    /// Updates the NavigationView's selected item based on the navigation key.
    /// </summary>
    private void UpdateNavigationSelection(string navigationKey)
    {
        // Search in main menu items
        foreach (object? item in NavigationViewControl.MenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == navigationKey)
            {
                NavigationViewControl.SelectedItem = navItem;
                return;
            }
        }

        // If not found in main menu, search in footer menu
        foreach (object? item in NavigationViewControl.FooterMenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == navigationKey)
            {
                NavigationViewControl.SelectedItem = navItem;
                return;
            }
        }
    }

    /// <summary>
    /// Releases resources used by the MainWindow.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and managed resources used by the MainWindow.
    /// </summary>
    /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            // Unsubscribe from events
            AppDataHelper.SaveFileLoaded -= OnSaveFileLoaded;

            if (ContentFrameControl != null)
            {
                ContentFrameControl.Navigated -= OnFrameNavigated;
            }
        }

        _disposed = true;
    }

    // Finalizer
    ~MainWindow()
    {
        Dispose(false);
    }
}