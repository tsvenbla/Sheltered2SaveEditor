using Windows.Storage;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Defines a service for picking files from the file system.
/// </summary>
internal interface IFilePickerService
{
    /// <summary>
    /// Allows the user to pick a single file.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The picked file, or null if the operation was canceled.</returns>
    Task<StorageFile?> PickFileAsync(CancellationToken cancellationToken = default);
}