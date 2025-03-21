using System.Collections.Immutable;

namespace Sheltered2SaveEditor.Helpers.Navigation;

/// <summary>
/// Defines the contract for a navigation registry that maps navigation keys to page types.
/// </summary>
internal interface IPageNavigationRegistry
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
/// <remarks>
/// Initializes a new instance of the <see cref="PageNavigationRegistry"/> class with custom defaults.
/// </remarks>
/// <param name="defaultPageType">The default page type to return when a key is not found.</param>
/// <param name="defaultNavigationKey">The default navigation key to return when a page type is not found.</param>
internal sealed class PageNavigationRegistry(Type defaultPageType, string defaultNavigationKey) : IPageNavigationRegistry
{
    private readonly Type _defaultPageType = defaultPageType ?? throw new ArgumentNullException(nameof(defaultPageType));
    private readonly string _defaultNavigationKey = defaultNavigationKey ?? throw new ArgumentNullException(nameof(defaultNavigationKey));
    private ImmutableDictionary<string, Type>? _pageTypesByKey;
    private ImmutableDictionary<Type, string>? _keysByPageType;
    private readonly Dictionary<string, Type> _registrations = [];
    private bool _isBuilt;

    /// <summary>
    /// Initializes a new instance of the <see cref="PageNavigationRegistry"/> class.
    /// </summary>
    public PageNavigationRegistry()
        : this(typeof(Features.SaveFiles.Views.HomePage), NavigationKeys.Home)
    {
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

        _registrations[key] = pageType;

        return this;
    }

    /// <inheritdoc/>
    public Type GetPageTypeByKey(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return _defaultPageType;
        }

        EnsureBuilt();

        return _pageTypesByKey!.TryGetValue(key, out Type? pageType) ? pageType : _defaultPageType;
    }

    /// <inheritdoc/>
    public string GetKeyByPageType(Type pageType)
    {
        if (pageType == null)
        {
            return _defaultNavigationKey;
        }

        EnsureBuilt();

        return _keysByPageType!.TryGetValue(pageType, out string? key) ? key : _defaultNavigationKey;
    }

    /// <inheritdoc/>
    public IPageNavigationRegistry Build()
    {
        if (_isBuilt)
        {
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

        return this;
    }

    /// <summary>
    /// Ensures the registry has been built before use.
    /// </summary>
    private void EnsureBuilt()
    {
        if (!_isBuilt)
        {
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