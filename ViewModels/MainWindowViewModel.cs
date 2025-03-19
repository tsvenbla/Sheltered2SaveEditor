using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Core;
using Sheltered2SaveEditor.Core.Constants;
using Sheltered2SaveEditor.Infrastructure.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sheltered2SaveEditor.ViewModels;

/// <summary>
/// ViewModel for the MainWindow, managing navigation states and UI-related properties.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly INavigationService _navigationService;
    private readonly IPageNavigationRegistry _pageRegistry;

    // Fields for navigation item enabled states
    private bool _isHomeEnabled = true;
    private bool _isCharactersEnabled;
    private bool _isPetsEnabled;
    private bool _isInventoryEnabled;
    private bool _isCraftingEnabled;
    private bool _isFactionsEnabled;
    private bool _isNavigateBackEnabled;

    /// <summary>
    /// Gets or sets a value indicating whether the Home navigation item is enabled.
    /// </summary>
    public bool IsHomeEnabled
    {
        get => _isHomeEnabled;
        set => SetProperty(ref _isHomeEnabled, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Characters navigation item is enabled.
    /// </summary>
    public bool IsCharactersEnabled
    {
        get => _isCharactersEnabled;
        set => SetProperty(ref _isCharactersEnabled, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Pets navigation item is enabled.
    /// </summary>
    public bool IsPetsEnabled
    {
        get => _isPetsEnabled;
        set => SetProperty(ref _isPetsEnabled, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Inventory navigation item is enabled.
    /// </summary>
    public bool IsInventoryEnabled
    {
        get => _isInventoryEnabled;
        set => SetProperty(ref _isInventoryEnabled, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Crafting navigation item is enabled.
    /// </summary>
    public bool IsCraftingEnabled
    {
        get => _isCraftingEnabled;
        set => SetProperty(ref _isCraftingEnabled, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the Factions navigation item is enabled.
    /// </summary>
    public bool IsFactionsEnabled
    {
        get => _isFactionsEnabled;
        set => SetProperty(ref _isFactionsEnabled, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the back navigation is enabled.
    /// </summary>
    public bool IsNavigateBackEnabled
    {
        get => _isNavigateBackEnabled;
        set => SetProperty(ref _isNavigateBackEnabled, value);
    }

    /// <summary>
    /// Gets a dictionary of navigation tags for use in the UI.
    /// </summary>
    public ReadOnlyDictionary<string, string> NavigationTags { get; }

    /// <summary>
    /// Gets a command to navigate to a specific page by its key.
    /// </summary>
    [RelayCommand]
    private void NavigateTo(string pageKey)
    {
        ArgumentException.ThrowIfNullOrEmpty(pageKey);

        try
        {
            _logger.LogInformation("Navigating to page: {PageKey}", pageKey);
            Type pageType = _pageRegistry.GetPageTypeByKey(pageKey);
            _ = _navigationService.Navigate(pageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to page: {PageKey}", pageKey);
        }
    }

    /// <summary>
    /// Gets a command to navigate back.
    /// </summary>
    [RelayCommand]
    private void NavigateBack()
    {
        try
        {
            _logger.LogInformation("Navigating back");
            _ = _navigationService.GoBack();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate back: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="logger">The logger used to log view model operations.</param>
    /// <param name="navigationService">The service used for navigation between pages.</param>
    /// <param name="pageRegistry">The registry of page navigation keys and types.</param>
    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        INavigationService navigationService,
        IPageNavigationRegistry pageRegistry)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _pageRegistry = pageRegistry ?? throw new ArgumentNullException(nameof(pageRegistry));

        // Initialize navigation tag dictionary for UI binding
        NavigationTags = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>
            {
                { "Home", NavigationKeys.Home },
                { "Characters", NavigationKeys.Characters },
                { "Pets", NavigationKeys.Pets },
                { "Inventory", NavigationKeys.Inventory },
                { "Crafting", NavigationKeys.Crafting },
                { "Factions", NavigationKeys.Factions },
                { "Donate", NavigationKeys.Donate }
            });

        // Initialize navigation state based on app state
        UpdateNavigationState(AppDataHelper.IsSaveFileLoaded);

        _logger.LogInformation("MainWindowViewModel initialized");
    }

    /// <summary>
    /// Updates the enabled state of navigation items based on whether a save file is loaded.
    /// </summary>
    /// <param name="saveFileLoaded">A value indicating whether a save file is loaded.</param>
    public void UpdateNavigationState(bool saveFileLoaded)
    {
        _logger.LogInformation("Updating navigation state. Save file loaded: {SaveFileLoaded}", saveFileLoaded);

        // Home is always enabled
        IsHomeEnabled = true;

        // Other pages require a save file to be loaded
        IsCharactersEnabled = saveFileLoaded;
        IsPetsEnabled = saveFileLoaded;
        IsInventoryEnabled = saveFileLoaded;
        IsCraftingEnabled = saveFileLoaded;
        IsFactionsEnabled = saveFileLoaded;
    }

    /// <summary>
    /// Updates the back navigation state.
    /// </summary>
    /// <param name="canGoBack">A value indicating whether navigation can go back.</param>
    public void UpdateBackNavigationState(bool canGoBack) => IsNavigateBackEnabled = canGoBack;
}