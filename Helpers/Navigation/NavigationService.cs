using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Sheltered2SaveEditor.Infrastructure.Navigation;

/// <summary>
/// Service that manages navigation between pages using a Frame.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="NavigationService"/> class.
/// </remarks>
/// <param name="logger">The logger used to log navigation operations.</param>
/// <param name="pageRegistry">The registry containing page type mappings.</param>
/// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
internal sealed class NavigationService(ILogger<NavigationService> logger, IPageNavigationRegistry pageRegistry) : INavigationService
{
    private readonly ILogger<NavigationService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IPageNavigationRegistry _pageRegistry = pageRegistry ?? throw new ArgumentNullException(nameof(pageRegistry));
    private Frame? _frame;

    /// <inheritdoc/>
    public bool CanGoBack => _frame?.CanGoBack ?? false;

    /// <inheritdoc/>
    public void Initialize(Frame frame)
    {
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        _logger.LogInformation("NavigationService initialized with frame");
    }

    /// <inheritdoc/>
    public bool Navigate(Type pageType, object? parameter = null)
    {
        if (_frame == null)
        {
            _logger.LogError("Cannot navigate: Frame is null");
            throw new InvalidOperationException("Navigation service not initialized with a Frame. Call Initialize first.");
        }

        ArgumentNullException.ThrowIfNull(pageType);

        try
        {
            bool result = _frame.Navigate(pageType, parameter);

            if (!result)
            {
                _logger.LogWarning("Navigation to {PageType} failed", pageType.Name);
            }
            else
            {
                _logger.LogInformation("Successfully navigated to {PageType}", pageType.Name);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to {PageType}", pageType.Name);
            throw;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to page with key {PageKey}", pageKey);
            throw;
        }
    }

    /// <inheritdoc/>
    public bool GoBack()
    {
        if (_frame == null)
        {
            _logger.LogError("Cannot go back: Frame is null");
            throw new InvalidOperationException("Navigation service not initialized with a Frame. Call Initialize first.");
        }

        if (!CanGoBack)
        {
            _logger.LogWarning("Cannot go back: No pages in back stack");
            return false;
        }

        try
        {
            _frame.GoBack();
            _logger.LogInformation("Successfully navigated back");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back");
            throw;
        }
    }
}