using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Sheltered2SaveEditor.Helpers;
using Sheltered2SaveEditor.Navigation;
using Sheltered2SaveEditor.Services;
using Sheltered2SaveEditor.ViewModels;
using System;

namespace Sheltered2SaveEditor;

/// <summary>
/// The main application window, responsible for hosting the navigation structure
/// and routing between different pages of the application.
/// </summary>
public sealed partial class MainWindow : Window, IDisposable
{
    private readonly ILogger<MainWindow> _logger;
    private readonly INavigationService _navigationService;
    private readonly MainWindowViewModel _viewModel;
    private readonly PageNavigationRegistry _pageRegistry;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// Sets up the navigation structure and event handlers.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        // Retrieve required services from the DI container
        _logger = DIContainer.Services.GetRequiredService<ILogger<MainWindow>>();
        _navigationService = DIContainer.Services.GetRequiredService<INavigationService>();
        _viewModel = DIContainer.Services.GetRequiredService<MainWindowViewModel>();
        _pageRegistry = DIContainer.Services.GetRequiredService<PageNavigationRegistry>();

        // In WinUI 3, we need to set DataContext on a root element, not on the Window itself
        // Set the root Grid's DataContext for data binding
        if (Content is FrameworkElement rootElement)
        {
            rootElement.DataContext = _viewModel;
        }

        // Subscribe to events
        AppDataHelper.SaveFileLoaded += OnSaveFileLoaded;
        ContentFrameControl.Navigated += OnFrameNavigated;

        // Navigate to HomePage on startup
        _logger.LogInformation("Application started. Navigating to HomePage.");
        try
        {
            NavigateToPage(NavigationKeys.Home);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to HomePage on startup");
        }
    }

    /// <summary>
    /// Initializes the navigation service with the content frame.
    /// This is called after the window is fully initialized.
    /// </summary>
    private void InitializeNavigationService()
    {
        FrameProvider frameProvider = DIContainer.Services.GetRequiredService<FrameProvider>();
        frameProvider.Initialize(ContentFrameControl);
        _logger.LogInformation("Navigation service initialized with content frame");
    }

    /// <summary>
    /// Handles the SaveFileLoaded event by updating the view model's navigation state.
    /// This enables menu items in the navigation pane once a save file is loaded.
    /// </summary>
    private void OnSaveFileLoaded(object? sender, EventArgs e)
    {
        _logger.LogInformation("Save file loaded. Enabling navigation options.");
        _ = DispatcherQueue.TryEnqueue(() => _viewModel.UpdateNavigationState(true));
    }

    /// <summary>
    /// Handles NavigationView selection changes and navigates to the selected page.
    /// </summary>
    private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem selectedItem &&
            selectedItem.Tag is string navigationKey)
        {
            _logger.LogDebug("Navigation selection changed to {NavigationKey}", navigationKey);
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
            _logger.LogDebug("Back navigation requested");
            try
            {
                ContentFrameControl.GoBack();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during back navigation");
            }
        }
    }

    /// <summary>
    /// Handles the frame's Navigated event to update UI state after navigation.
    /// </summary>
    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
        // Update the Back button state
        NavigationViewControl.IsBackEnabled = ContentFrameControl.CanGoBack;

        // Update the navigation menu selection based on the current page
        if (e.SourcePageType != null)
        {
            string navigationKey = _pageRegistry.GetKeyByPageType(e.SourcePageType);
            _logger.LogDebug("Frame navigated to {PageType}, key: {NavigationKey}",
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
        try
        {
            Type pageType = _pageRegistry.GetPageTypeByKey(navigationKey);

            if (ContentFrameControl.CurrentSourcePageType != pageType)
            {
                _logger.LogDebug("Navigating to {PageType} with key {NavigationKey}",
                    pageType.Name, navigationKey);
                _ = ContentFrameControl.Navigate(pageType);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to page with key {NavigationKey}", navigationKey);
            DisplayNavigationError(ex, navigationKey);
        }
    }

    /// <summary>
    /// Displays an error dialog when navigation fails.
    /// </summary>
    private async void DisplayNavigationError(Exception ex, string navigationKey)
    {
        ContentDialog dialog = new()
        {
            Title = "Navigation Error",
            Content = $"Failed to navigate to {navigationKey}. Error: {ex.Message}",
            CloseButtonText = "OK",
            XamlRoot = Content.XamlRoot
        };

        _ = await dialog.ShowAsync();
    }

    /// <summary>
    /// Updates the NavigationView's selected item based on the navigation key.
    /// </summary>
    private void UpdateNavigationSelection(string navigationKey)
    {
        bool itemFound = false;

        // Search in main menu items
        foreach (object? item in NavigationViewControl.MenuItems)
        {
            if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == navigationKey)
            {
                NavigationViewControl.SelectedItem = navItem;
                itemFound = true;
                break;
            }
        }

        // If not found in main menu, search in footer menu
        if (!itemFound)
        {
            foreach (object? item in NavigationViewControl.FooterMenuItems)
            {
                if (item is NavigationViewItem navItem && navItem.Tag?.ToString() == navigationKey)
                {
                    NavigationViewControl.SelectedItem = navItem;
                    break;
                }
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
        if (!_disposed)
        {
            if (disposing)
            {
                // Unsubscribe from events
                AppDataHelper.SaveFileLoaded -= OnSaveFileLoaded;
                ContentFrameControl.Navigated -= OnFrameNavigated;
            }

            _disposed = true;
        }
    }
}