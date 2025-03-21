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
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is null.</exception>
    Task<bool> IsValidSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the save file by checking its size and signature.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a validation result indicating the outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is null.</exception>
    Task<ValidationResult> ValidateSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates the save file with enhanced reporting and progress monitoring.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <param name="progressMonitor">The monitor to report validation progress.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains detailed validation result info.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is null.</exception>
    Task<ValidationResultInfo> ValidateSaveFileWithDetailsAsync(
        StorageFile file,
        IValidationProgressMonitor progressMonitor,
        CancellationToken cancellationToken = default);
}