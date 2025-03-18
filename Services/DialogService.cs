using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using Windows.System;

namespace Sheltered2SaveEditor.Services;

/// <summary>
/// Implements the dialog service for showing UI dialogs to the user.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DialogService"/> class.
/// </remarks>
/// <param name="logger">The logger used to log dialog operations.</param>
public class DialogService(ILogger<DialogService> logger) : IDialogService
{
    private readonly ILogger<DialogService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private XamlRoot? _xamlRoot;

    /// <inheritdoc/>
    public void SetXamlRoot(XamlRoot root)
    {
        _xamlRoot = root ?? throw new ArgumentNullException(nameof(root));
        _logger.LogInformation("XamlRoot set for dialog service");
    }

    /// <inheritdoc/>
    public async Task<bool> ShowErrorDialogAsync(string title, string message, bool includeGitHubOption = false)
    {
        try
        {
            EnsureXamlRoot();

            ContentDialog dialog = new()
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot,
                DefaultButton = ContentDialogButton.Close
            };

            if (includeGitHubOption)
            {
                dialog.PrimaryButtonText = "Report issue";
                dialog.DefaultButton = ContentDialogButton.Primary;
            }

            ContentDialogResult result = await dialog.ShowAsync();

            // If the user clicks "Report issue", launch the GitHub issues page
            if (result == ContentDialogResult.Primary && includeGitHubOption)
            {
                Uri uri = new("https://github.com/tsvenbla/Sheltered2SaveEditor/issues/new");
                _ = await Launcher.LaunchUriAsync(uri);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show error dialog: {Title} - {Message}", title, message);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ShowConfirmationDialogAsync(string title, string message, string acceptButtonText = "OK", string cancelButtonText = "Cancel")
    {
        try
        {
            EnsureXamlRoot();

            ContentDialog dialog = new()
            {
                Title = title,
                Content = message,
                PrimaryButtonText = acceptButtonText,
                CloseButtonText = cancelButtonText,
                XamlRoot = _xamlRoot,
                DefaultButton = ContentDialogButton.Primary
            };

            ContentDialogResult result = await dialog.ShowAsync();
            return result == ContentDialogResult.Primary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show confirmation dialog: {Title} - {Message}", title, message);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task ShowInformationDialogAsync(string title, string message)
    {
        try
        {
            EnsureXamlRoot();

            ContentDialog dialog = new()
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = _xamlRoot,
                DefaultButton = ContentDialogButton.Close
            };

            _ = await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to show information dialog: {Title} - {Message}", title, message);
        }
    }

    /// <summary>
    /// Ensures that the XamlRoot has been set before showing dialogs.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when XamlRoot is null.</exception>
    private void EnsureXamlRoot()
    {
        if (_xamlRoot == null)
        {
            _logger.LogError("XamlRoot not set for dialog service");
            throw new InvalidOperationException("XamlRoot must be set before showing dialogs");
        }
    }
}