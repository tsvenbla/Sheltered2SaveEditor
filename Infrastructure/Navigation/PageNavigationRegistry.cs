using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Core.Constants;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Sheltered2SaveEditor.Infrastructure.Navigation;

/// <summary>
/// Defines the contract for a navigation registry that maps navigation keys to page types.
/// </summary>
public interface IPageNavigationRegistry
{
    /// <summary>
    /// Registers a page type with the specified navigation key.
    /// </summary>
    /// <param name="key">The navigation key.</param>
    /// <param name="pageType">The page type to associate with the key.</param>
    /// <returns>The current instance for method chaining.</returns>
    IPageNavigationRegistry Register(string key, Type pageType);

    /// <summary>
    /// Gets the page type associated with the specified navigation key.
    /// </summary>
    /// <param name="key">The navigation key to look up.</param>
    /// <returns>
    /// The page type associated with the key, or a default page type if the key is not found.
    /// </returns>
    Type GetPageTypeByKey(string key);

    /// <summary>
    /// Gets the navigation key associated with the specified page type.
    /// </summary>
    /// <param name="pageType">The page type to look up.</param>
    /// <returns>
    /// The navigation key associated with the page type, or a default key if the page type is not found.
    /// </returns>
    string GetKeyByPageType(Type pageType);

    /// <summary>
    /// Finalizes the registration process, making the registry immutable.
    /// </summary>
    /// <returns>The current instance with registrations locked.</returns>
    IPageNavigationRegistry Build();
}

/// <summary>
/// Maintains a registry of navigation keys mapped to page types
/// for type-safe navigation throughout the application.
/// </summary>
public sealed class PageNavigationRegistry : IPageNavigationRegistry
{
    private readonly ILogger<PageNavigationRegistry> _logger;
    private readonly Type _defaultPageType;
    private readonly string _defaultNavigationKey;
    private ImmutableDictionary<string, Type>? _pageTypesByKey;
    private ImmutableDictionary<Type, string>? _keysByPageType;
    private readonly Dictionary<string, Type> _registrations = [];
    private bool _isBuilt;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageNavigationRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger used to log registration and lookup operations.</param>
    public PageNavigationRegistry(ILogger<PageNavigationRegistry> logger)
        : this(logger, typeof(Features.SaveFiles.Views.HomePage), NavigationKeys.Home)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PageNavigationRegistry"/> class with custom defaults.
    /// </summary>
    /// <param name="logger">The logger used to log registration and lookup operations.</param>
    /// <param name="defaultPageType">The default page type to return when a key is not found.</param>
    /// <param name="defaultNavigationKey">The default navigation key to return when a page type is not found.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    public PageNavigationRegistry(
        ILogger<PageNavigationRegistry> logger,
        Type defaultPageType,
        string defaultNavigationKey)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _defaultPageType = defaultPageType ?? throw new ArgumentNullException(nameof(defaultPageType));
        _defaultNavigationKey = defaultNavigationKey ?? throw new ArgumentNullException(nameof(defaultNavigationKey));

        _logger.LogInformation("PageNavigationRegistry initialized with default page type {DefaultPageType} and key {DefaultKey}",
            _defaultPageType.Name, _defaultNavigationKey);
    }

    /// <inheritdoc/>
    public IPageNavigationRegistry Register(string key, Type pageType)
    {
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));
        ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

        ThrowIfBuilt();

        if (!typeof(Microsoft.UI.Xaml.Controls.Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException($"Type {pageType.Name} is not a Page type", nameof(pageType));
        }

        if (_registrations.TryGetValue(key, out Type? value))
        {
            _logger.LogWarning("Overwriting existing registration for key {Key} from {OldPageType} to {NewPageType}",
                key, value.Name, pageType.Name);
        }

        _registrations[key] = pageType;
        _logger.LogDebug("Registered {PageType} with key {Key}", pageType.Name, key);

        return this;
    }

    /// <inheritdoc/>
    public Type GetPageTypeByKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Attempt to get page type with null or empty key, returning default page {DefaultPage}",
                _defaultPageType.Name);
            return _defaultPageType;
        }

        EnsureBuilt();

        if (_pageTypesByKey!.TryGetValue(key, out Type? pageType))
        {
            return pageType;
        }

        _logger.LogWarning("Navigation key not found: {Key}, returning default page {DefaultPage}",
            key, _defaultPageType.Name);
        return _defaultPageType;
    }

    /// <inheritdoc/>
    public string GetKeyByPageType(Type pageType)
    {
        if (pageType == null)
        {
            _logger.LogWarning("Attempt to get navigation key with null page type, returning default key {DefaultKey}",
                _defaultNavigationKey);
            return _defaultNavigationKey;
        }

        EnsureBuilt();

        if (_keysByPageType!.TryGetValue(pageType, out string? key))
        {
            return key;
        }

        _logger.LogWarning("Page type not registered: {PageType}, returning default key {DefaultKey}",
            pageType.Name, _defaultNavigationKey);
        return _defaultNavigationKey;
    }

    /// <inheritdoc/>
    public IPageNavigationRegistry Build()
    {
        if (_isBuilt)
        {
            _logger.LogWarning("PageNavigationRegistry already built, ignoring build request");
            return this;
        }

        // Build the immutable dictionaries
        _pageTypesByKey = _registrations.ToImmutableDictionary();

        // Build the reverse lookup
        Dictionary<Type, string> keysByType = [];
        foreach (KeyValuePair<string, Type> kvp in _registrations)
        {
            keysByType[kvp.Value] = kvp.Key;
        }
        _keysByPageType = keysByType.ToImmutableDictionary();

        _isBuilt = true;
        _logger.LogInformation("PageNavigationRegistry built with {Count} page registrations", _pageTypesByKey.Count);

        return this;
    }

    /// <summary>
    /// Registers the default Sheltered2SaveEditor pages.
    /// </summary>
    /// <returns>The current instance for method chaining.</returns>
    public IPageNavigationRegistry RegisterDefaultPages()
    {
        ThrowIfBuilt();

        _ = Register(NavigationKeys.Home, typeof(Features.SaveFiles.Views.HomePage));
        _ = Register(NavigationKeys.Characters, typeof(Features.Characters.Views.CharactersPage));
        _ = Register(NavigationKeys.Pets, typeof(Features.Pets.Views.PetsPage));
        _ = Register(NavigationKeys.Inventory, typeof(Features.Inventory.Views.InventoryPage));
        _ = Register(NavigationKeys.Crafting, typeof(Features.Crafting.Views.CraftingPage));
        _ = Register(NavigationKeys.Factions, typeof(Features.Factions.Views.FactionsPage));
        _ = Register(NavigationKeys.Donate, typeof(Features.Donate.Views.DonatePage));

        _logger.LogInformation("Registered default pages");
        return this;
    }

    /// <summary>
    /// Ensures the registry has been built before use.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the registry has not been built.</exception>
    private void EnsureBuilt()
    {
        if (!_isBuilt)
        {
            _logger.LogWarning("PageNavigationRegistry not built before use, automatically building");
            _ = Build();
        }
    }

    /// <summary>
    /// Throws an exception if the registry has already been built.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the registry has already been built.</exception>
    private void ThrowIfBuilt()
    {
        if (_isBuilt)
        {
            throw new InvalidOperationException("PageNavigationRegistry has already been built and cannot be modified");
        }
    }
}