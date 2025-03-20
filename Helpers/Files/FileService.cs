using Sheltered2SaveEditor.Helpers.Cipher;
using System.Text;
using Windows.Storage;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Service for handling file operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FileService"/> class.
/// </remarks>
/// <param name="cipherService">The service used for encryption and decryption.</param>
/// <param name="fileValidator">The service used for file validation.</param>
/// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
internal sealed class FileService(IXorCipherService cipherService, IFileValidator fileValidator) : IFileService
{
    private readonly IXorCipherService _cipherService = cipherService ?? throw new ArgumentNullException(nameof(cipherService));
    private readonly IFileValidator _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));

    /// <inheritdoc/>
    public async Task<string> LoadAndDecryptSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        // Validate the file first
        if (!await _fileValidator.IsValidSaveFileAsync(file, cancellationToken).ConfigureAwait(false))
            throw new InvalidDataException($"The file '{file.Name}' is not a valid save file.");

        try
        {
            byte[] decryptedData = await _cipherService.LoadAndXorAsync(file.Path, cancellationToken).ConfigureAwait(false);
            return Encoding.UTF8.GetString(decryptedData);
        }
        catch (OperationCanceledException)
        {
            throw; // Rethrow cancellation directly
        }
        catch (Exception ex)
        {
            throw new IOException($"Error loading or decrypting save file: {file.Path}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task EncryptAndSaveSaveFileAsync(StorageFile file, string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(content, nameof(content));

        try
        {
            // Ensure the content has the root XML tag
            if (!content.Contains("<root>", StringComparison.Ordinal) ||
                !content.Contains("</root>", StringComparison.Ordinal))
                throw new InvalidDataException("The content does not have the required XML structure.");

            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            await _cipherService.SaveAndXorAsync(file.Path, contentBytes, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw; // Rethrow cancellation directly
        }
        catch (Exception ex)
        {
            throw new IOException($"Error encrypting or saving content to file: {file.Path}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<StorageFile?> CreateBackupAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        try
        {
            StorageFolder folder = await file.GetParentAsync();
            string backupFileName = $"{Path.GetFileNameWithoutExtension(file.Name)}_backup_{DateTime.Now:yyyyMMdd_HHmmss}{Path.GetExtension(file.Name)}";

            return await file.CopyAsync(folder, backupFileName, NameCollisionOption.GenerateUniqueName)
                .AsTask(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Return null rather than throwing on backup failure, as this is a non-critical operation
            return null;
        }
    }
}