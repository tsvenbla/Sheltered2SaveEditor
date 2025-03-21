using Sheltered2SaveEditor.Helpers.Cipher;
using System.Globalization;
using System.Text;
using Windows.Storage;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Provides methods for securely loading, saving, encrypting, and decrypting save files.
/// </summary>
internal sealed class FileService : IFileService
{
    private readonly IXorCipherService _cipherService;
    private readonly IFileValidator _fileValidator;
    private readonly FileServiceOptions _options = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="FileService"/> class.
    /// </summary>
    /// <param name="cipherService">The service used for encryption and decryption.</param>
    /// <param name="fileValidator">The service used for file validation.</param>
    /// <exception cref="ArgumentNullException">Thrown if any parameter is null.</exception>
    internal FileService(IXorCipherService cipherService, IFileValidator fileValidator)
    {
        _cipherService = cipherService ?? throw new ArgumentNullException(nameof(cipherService));
        _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
    }

    /// <inheritdoc/>
    public async Task<string> LoadAndDecryptSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        // Validate the file first
        if (!await _fileValidator.IsValidSaveFileAsync(file, cancellationToken).ConfigureAwait(false))
        {
            throw new InvalidDataException($"The file '{file.Name}' is not a valid save file.");
        }

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
            // Basic check to ensure at minimum we have an XML-like structure 
            string openTag = FileValidationOptions.DefaultExpectedHeader;
            string closeTag = FileValidationOptions.DefaultExpectedFooter;

            if (!content.Contains(openTag, StringComparison.Ordinal) ||
                !content.Contains(closeTag, StringComparison.Ordinal))
            {
                throw new InvalidDataException($"The content does not have the required XML root tags.");
            }

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
            string backupFileName = $"{Path.GetFileNameWithoutExtension(file.Name)}{_options.BackupFileSuffix}{DateTime.Now.ToString(_options.BackupDateFormat, CultureInfo.InvariantCulture)}{Path.GetExtension(file.Name)}";

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