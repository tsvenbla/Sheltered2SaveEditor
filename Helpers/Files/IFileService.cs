using Windows.Storage;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Defines methods for secure file operations specific to save files, including encryption, decryption, and backup capabilities.
/// </summary>
internal interface IFileService
{
    /// <summary>
    /// Loads and decrypts a save file using the game's encryption algorithm.
    /// </summary>
    /// <param name="file">The save file to load.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the decrypted XML content as a string if successful.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is null.</exception>
    /// <exception cref="InvalidDataException">Thrown if the file is not a valid save file.</exception>
    /// <exception cref="IOException">Thrown if there is an error reading or decrypting the file.</exception>
    Task<string> LoadAndDecryptSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Encrypts and saves content to a file using the game's encryption algorithm.
    /// </summary>
    /// <param name="file">The file to save to.</param>
    /// <param name="content">The XML content to encrypt and save.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="content"/> is null or empty, or does not have valid XML structure.</exception>
    /// <exception cref="IOException">Thrown if there is an error encrypting or saving the file.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown if the application does not have permission to write to the file.</exception>
    Task EncryptAndSaveSaveFileAsync(StorageFile file, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a timestamped backup of a file in the same directory.
    /// </summary>
    /// <param name="file">The file to back up.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>
    /// A task representing the asynchronous operation.
    /// The task result contains the backup file if successful, or null if the backup operation failed.
    /// </returns>
    /// <remarks>
    /// This method does not throw exceptions if the backup fails, but returns null instead.
    /// This allows the caller to decide whether to proceed with the main operation if the backup fails.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is null.</exception>
    Task<StorageFile?> CreateBackupAsync(StorageFile file, CancellationToken cancellationToken = default);
}