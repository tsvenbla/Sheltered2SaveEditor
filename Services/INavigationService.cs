using System;

namespace Sheltered2SaveEditor.Services;

public interface INavigationService
{
    bool CanGoBack { get; }
    void Navigate(Type pageType, object? parameter = null);
    void GoBack();
}