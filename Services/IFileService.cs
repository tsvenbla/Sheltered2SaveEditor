using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Sheltered2SaveEditor.Services;

/// <summary>
/// Defines methods for interacting with files.
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Loads and decrypts a save file.
    /// </summary>
    /// <param name="file">The save file to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The decrypted content as a string.</returns>
    Task<string> LoadAndDecryptSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Encrypts and saves content to a file.
    /// </summary>
    /// <param name="file">The file to save to.</param>
    /// <param name="content">The content to encrypt and save.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EncryptAndSaveSaveFileAsync(StorageFile file, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a backup of a file.
    /// </summary>
    /// <param name="file">The file to back up.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The backup file, or null if the backup failed.</returns>
    Task<StorageFile?> CreateBackupAsync(StorageFile file, CancellationToken cancellationToken = default);
}