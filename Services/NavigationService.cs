using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Helpers;
using System;
using System.Collections.Generic;

namespace Sheltered2SaveEditor.Services;

public class NavigationService : INavigationService
{
    private readonly FrameProvider _frameProvider;
    private readonly ILogger<NavigationService> _logger;

    public NavigationService(FrameProvider frameProvider)
    {
        _frameProvider = frameProvider ?? throw new ArgumentNullException(nameof(frameProvider));
        _logger = DIContainer.Services.GetRequiredService<ILogger<NavigationService>>();
        _logger.LogInformation("NavigationService created");
    }

    public bool CanGoBack => GetFrame().CanGoBack;

    public void Navigate(Type pageType, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(pageType);

        try
        {
            Frame frame = GetFrame();
            _logger.LogDebug("Navigating to {PageType}", pageType.Name);

            // Optionally use caching here. For now, we navigate directly.
            _ = frame.Navigate(pageType, parameter);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate to {PageType}", pageType.Name);
            throw;
        }
    }

    public void GoBack()
    {
        try
        {
            Frame frame = GetFrame();
            if (frame.CanGoBack)
            {
                _logger.LogDebug("Navigating back");
                frame.GoBack();
            }
            else
            {
                _logger.LogWarning("Attempted to go back when navigation stack is empty");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to navigate back");
            throw;
        }
    }

    private Frame GetFrame() => _frameProvider.GetFrame();
}