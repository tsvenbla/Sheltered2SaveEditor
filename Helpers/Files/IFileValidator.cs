using Windows.Storage;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Defines methods for validating game save files.
/// </summary>
internal interface IFileValidator
{
    /// <summary>
    /// Checks if a file has the correct extension for a save file.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <returns>True if the file has a valid extension; otherwise, false.</returns>
    bool HasValidExtension(StorageFile file);

    /// <summary>
    /// Validates the content of a save file.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the file is valid.</returns>
    Task<bool> IsValidSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default);
}