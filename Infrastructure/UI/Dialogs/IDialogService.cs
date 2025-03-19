using Microsoft.UI.Xaml;
using System.Threading.Tasks;

namespace Sheltered2SaveEditor.Infrastructure.UI.Dialogs;

/// <summary>
/// Defines a service that handles showing modal dialogs to the user.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Shows an error dialog to the user.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The error message to display.</param>
    /// <param name="includeGitHubOption">Whether to include an option to create a GitHub issue.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task<bool> ShowErrorDialogAsync(string title, string message, bool includeGitHubOption = false);

    /// <summary>
    /// Shows a confirmation dialog to the user.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display.</param>
    /// <param name="acceptButtonText">The text for the accept button.</param>
    /// <param name="cancelButtonText">The text for the cancel button.</param>
    /// <returns>True if the user accepted, false otherwise.</returns>
    Task<bool> ShowConfirmationDialogAsync(string title, string message, string acceptButtonText = "OK", string cancelButtonText = "Cancel");

    /// <summary>
    /// Shows an information dialog to the user.
    /// </summary>
    /// <param name="title">The title of the dialog.</param>
    /// <param name="message">The message to display.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ShowInformationDialogAsync(string title, string message);

    /// <summary>
    /// Updates the XamlRoot for the dialog service.
    /// </summary>
    /// <param name="root">The XamlRoot to use for dialogs.</param>
    void SetXamlRoot(XamlRoot root);
}