using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Pages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Sheltered2SaveEditor.Navigation;

/// <summary>
/// Maintains a registry of navigation keys mapped to page types
/// for type-safe navigation throughout the application.
/// </summary>
public sealed class PageNavigationRegistry
{
    private readonly ILogger<PageNavigationRegistry> _logger;
    private readonly ImmutableDictionary<string, Type> _pageTypesByKey;
    private readonly ImmutableDictionary<Type, string> _keysByPageType;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageNavigationRegistry"/> class.
    /// </summary>
    /// <param name="logger">The logger used to log registration and lookup operations.</param>
    public PageNavigationRegistry(ILogger<PageNavigationRegistry> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        Dictionary<string, Type> pageRegistrations = new()
        {
            { NavigationKeys.Home, typeof(HomePage) },
            { NavigationKeys.Characters, typeof(CharactersPage) },
            { NavigationKeys.Pets, typeof(PetsPage) },
            { NavigationKeys.Inventory, typeof(InventoryPage) },
            { NavigationKeys.Crafting, typeof(CraftingPage) },
            { NavigationKeys.Factions, typeof(FactionsPage) },
            { NavigationKeys.Donate, typeof(DonatePage) }
        };

        _pageTypesByKey = pageRegistrations.ToImmutableDictionary();

        // Build the reverse lookup
        Dictionary<Type, string> keyRegistrations = [];
        foreach (KeyValuePair<string, Type> kvp in pageRegistrations)
            keyRegistrations[kvp.Value] = kvp.Key;

        _keysByPageType = keyRegistrations.ToImmutableDictionary();

        _logger.LogInformation("PageNavigationRegistry initialized with {Count} page registrations", _pageTypesByKey.Count);
    }

    /// <summary>
    /// Gets the page type associated with the specified navigation key.
    /// </summary>
    /// <param name="key">The navigation key to look up.</param>
    /// <returns>
    /// The page type associated with the key, or the HomePage type if the key is not found.
    /// </returns>
    public Type GetPageTypeByKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            _logger.LogWarning("Attempt to get page type with null or empty key");
            return typeof(HomePage);
        }

        if (_pageTypesByKey.TryGetValue(key, out Type? pageType))
            return pageType;

        _logger.LogWarning("Navigation key not found: {Key}, defaulting to HomePage", key);
        return typeof(HomePage);
    }

    /// <summary>
    /// Gets the navigation key associated with the specified page type.
    /// </summary>
    /// <param name="pageType">The page type to look up.</param>
    /// <returns>
    /// The navigation key associated with the page type, or the Home key if the page type is not found.
    /// </returns>
    public string GetKeyByPageType(Type pageType)
    {
        if (pageType == null)
        {
            _logger.LogWarning("Attempt to get navigation key with null page type");
            return NavigationKeys.Home;
        }

        if (_keysByPageType.TryGetValue(pageType, out string? key))
            return key;

        _logger.LogWarning("Page type not registered: {PageType}, defaulting to Home key", pageType.Name);
        return NavigationKeys.Home;
    }
}