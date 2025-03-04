using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Pages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Provides navigation helper methods for the application.
/// </summary>
public sealed class NavigationHelper
{
    private readonly Frame _contentFrame;
    private readonly NavigationView _navigationView;
    private readonly Dictionary<string, Type> _pageMap;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationHelper"/> class.
    /// </summary>
    /// <param name="contentFrame">The frame used for navigation.</param>
    /// <param name="navigationView">The navigation view control.</param>
    /// <exception cref="ArgumentNullException">Thrown when contentFrame or navigationView is null.</exception>
    public NavigationHelper(Frame contentFrame, NavigationView navigationView)
    {
        ArgumentNullException.ThrowIfNull(contentFrame);
        ArgumentNullException.ThrowIfNull(navigationView);

        _contentFrame = contentFrame;
        _navigationView = navigationView;

        _pageMap = new Dictionary<string, Type>
        {
            ["Home"] = typeof(HomePage),
            ["Characters"] = typeof(CharactersPage),
            ["Pets"] = typeof(PetsPage),
            ["Inventory"] = typeof(InventoryPage),
            ["Crafting"] = typeof(CraftingPage),
            ["Factions"] = typeof(FactionsPage),
            ["Donate"] = typeof(DonatePage)
        };

        _navigationView.BackRequested += OnBackRequested;
    }

    /// <summary>
    /// Navigates to the specified page type.
    /// </summary>
    /// <param name="pageType">The type of the page to navigate to.</param>
    /// <exception cref="ArgumentNullException">Thrown when pageType is null.</exception>
    public void Navigate(Type pageType)
    {
        ArgumentNullException.ThrowIfNull(pageType);

        if (_contentFrame.CurrentSourcePageType != pageType)
        {
            _ = _contentFrame.Navigate(pageType);
            UpdateNavigationViewSelection(pageType);
            UpdateBackButtonState();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the frame can navigate back.
    /// </summary>
    public bool CanGoBack => _contentFrame.CanGoBack;

    /// <summary>
    /// Navigates back to the previous page.
    /// </summary>
    public void GoBack()
    {
        if (CanGoBack)
        {
            _contentFrame.GoBack();
            UpdateNavigationViewSelection(_contentFrame.CurrentSourcePageType);
            UpdateBackButtonState();
        }
    }

    /// <summary>
    /// Handles the selection changed event of the navigation view.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event arguments.</param>
    public void OnNavigationViewSelectionChanged(object sender, NavigationViewSelectionChangedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(sender);
        if (args.SelectedItem is NavigationViewItem { Tag: string tag } &&
            _pageMap.TryGetValue(tag, out Type? pageType))
        {
            Navigate(pageType);
        }
    }

    /// <summary>
    /// Handles the back requested event of the navigation view.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="args">The event arguments.</param>
    private void OnBackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args) => GoBack();

    /// <summary>
    /// Updates the state of the back button.
    /// </summary>
    private void UpdateBackButtonState() => _navigationView.IsBackEnabled = CanGoBack;

    /// <summary>
    /// Updates the selection of the navigation view based on the current page type.
    /// </summary>
    /// <param name="currentPageType">The type of the current page.</param>
    private void UpdateNavigationViewSelection(Type currentPageType)
    {
        if (!_pageMap.ContainsValue(currentPageType))
        {
            return;
        }

        string? tag = _pageMap.FirstOrDefault(pair => pair.Value == currentPageType).Key;
        if (tag is not null)
        {
            NavigationViewItem? menuItem = FindMenuItemByTag(tag);
            if (menuItem is not null)
            {
                _navigationView.SelectedItem = menuItem;
            }
        }
    }

    /// <summary>
    /// Finds a navigation view item by its tag.
    /// </summary>
    /// <param name="tag">The tag of the navigation view item.</param>
    /// <returns>The navigation view item if found; otherwise, null.</returns>
    private NavigationViewItem? FindMenuItemByTag(string tag) =>
        _navigationView.MenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(item => item.Tag?.ToString() == tag)
        ?? _navigationView.FooterMenuItems
            .OfType<NavigationViewItem>()
            .FirstOrDefault(item => item.Tag?.ToString() == tag);

    /// <summary>
    /// Enables the specified navigation view items.
    /// </summary>
    /// <param name="navigationItems">The navigation view items to enable.</param>
    /// <exception cref="ArgumentNullException">Thrown when navigationItems is null.</exception>
    public static void EnableNavigationItems(params NavigationViewItem[] navigationItems)
    {
        ArgumentNullException.ThrowIfNull(navigationItems);

        foreach (NavigationViewItem item in navigationItems)
        {
            item.IsEnabled = true;
        }
    }
}
public enum NavigationPage
{
    Home,
    Characters,
    Pets,
    Inventory,
    Crafting,
    Factions,
    Donate
}