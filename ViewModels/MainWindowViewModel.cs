using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Helpers;
using Sheltered2SaveEditor.Navigation;
using Sheltered2SaveEditor.Services;
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

    // Observable properties for navigation item enabled states
    [ObservableProperty]
    private bool _isHomeEnabled = true;

    [ObservableProperty]
    private bool _isCharactersEnabled;

    [ObservableProperty]
    private bool _isPetsEnabled;

    [ObservableProperty]
    private bool _isInventoryEnabled;

    [ObservableProperty]
    private bool _isCraftingEnabled;

    [ObservableProperty]
    private bool _isFactionsEnabled;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="logger">The logger used to log view model operations.</param>
    /// <param name="navigationService">The service used for navigation between pages.</param>
    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, INavigationService navigationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));

        // Initialize navigation state based on app state
        UpdateNavigationState(AppDataHelper.IsSaveFileLoaded);

        _logger.LogInformation("MainWindowViewModel initialized");
    }

    /// <summary>
    /// Gets a dictionary of navigation tags for use in the UI.
    /// </summary>
    /// <remarks>
    /// This provides a way to access navigation constants in XAML without using x:Static,
    /// which is not supported in WinUI 3.
    /// </remarks>
    public ReadOnlyDictionary<string, string> NavigationTags { get; } = new ReadOnlyDictionary<string, string>(
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
}