using Microsoft.UI.Xaml.Controls;
using System;

namespace Sheltered2SaveEditor.Infrastructure.Navigation;

/// <summary>
/// Defines a service that manages navigation between pages in the application.
/// </summary>
internal interface INavigationService
{
    /// <summary>
    /// Gets a value indicating whether navigation can go back.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Initializes the navigation service with the specified frame.
    /// </summary>
    /// <param name="frame">The frame to use for navigation.</param>
    void Initialize(Frame frame);

    /// <summary>
    /// Navigates to the specified page type with an optional parameter.
    /// </summary>
    /// <param name="pageType">The type of page to navigate to.</param>
    /// <param name="parameter">Optional parameter to pass to the page.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool Navigate(Type pageType, object? parameter = null);

    /// <summary>
    /// Navigates to the specified page key with an optional parameter.
    /// </summary>
    /// <param name="pageKey">The key of the page to navigate to.</param>
    /// <param name="parameter">Optional parameter to pass to the page.</param>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool NavigateToKey(string pageKey, object? parameter = null);

    /// <summary>
    /// Navigates back to the previous page if possible.
    /// </summary>
    /// <returns>True if navigation was successful, false otherwise.</returns>
    bool GoBack();
}