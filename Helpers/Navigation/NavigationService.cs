using Microsoft.UI.Xaml.Controls;

namespace Sheltered2SaveEditor.Helpers.Navigation;

/// <summary>
/// Service that manages navigation between pages using a Frame.
/// </summary>
internal sealed class NavigationService : INavigationService
{
    private readonly IPageNavigationRegistry _pageRegistry;
    private Frame? _frame;

    /// <inheritdoc/>
    public bool CanGoBack => _frame?.CanGoBack ?? false;

    /// <summary>
    /// Initializes a new instance of the <see cref="NavigationService"/> class.
    /// </summary>
    /// <param name="pageRegistry">The registry containing page type mappings.</param>
    internal NavigationService(IPageNavigationRegistry pageRegistry) => _pageRegistry = pageRegistry ?? throw new ArgumentNullException(nameof(pageRegistry));

    /// <inheritdoc/>
    public void Initialize(Frame frame) => _frame = frame ?? throw new ArgumentNullException(nameof(frame));

    /// <inheritdoc/>
    public bool Navigate(Type pageType, object? parameter = null)
    {
        if (_frame == null)
        {
            throw new InvalidOperationException("Navigation service not initialized with a Frame. Call Initialize first.");
        }

        ArgumentNullException.ThrowIfNull(pageType);

        try
        {
            return _frame.Navigate(pageType, parameter);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool NavigateToKey(string pageKey, object? parameter = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(pageKey);

        try
        {
            Type pageType = _pageRegistry.GetPageTypeByKey(pageKey);
            return Navigate(pageType, parameter);
        }
        catch
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public bool GoBack()
    {
        if (_frame == null)
        {
            throw new InvalidOperationException("Navigation service not initialized with a Frame. Call Initialize first.");
        }

        if (!CanGoBack)
        {
            return false;
        }

        try
        {
            _frame.GoBack();
            return true;
        }
        catch
        {
            return false;
        }
    }
}