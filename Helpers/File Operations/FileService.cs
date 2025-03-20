using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Infrastructure.Encryption;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Sheltered2SaveEditor.Infrastructure.Files;

/// <summary>
/// Service for handling file operations.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FileService"/> class.
/// </remarks>
/// <param name="logger">The logger used to log file operations.</param>
/// <param name="cipherService">The service used for encryption and decryption.</param>
internal sealed class FileService(ILogger<FileService> logger, IXorCipherService cipherService) : IFileService
{
    private readonly ILogger<FileService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IXorCipherService _cipherService = cipherService ?? throw new ArgumentNullException(nameof(cipherService));

    /// <inheritdoc/>
    public async Task<string> LoadAndDecryptSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        _logger.LogInformation("Loading and decrypting save file: {FilePath}", file.Path);

        try
        {
            byte[] decryptedData = await _cipherService.LoadAndXorAsync(file.Path, cancellationToken);
            string decryptedContent = Encoding.UTF8.GetString(decryptedData);

            _logger.LogInformation("Successfully loaded and decrypted save file: {FilePath}", file.Path);
            return decryptedContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading or decrypting save file: {FilePath}", file.Path);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task EncryptAndSaveSaveFileAsync(StorageFile file, string content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentException.ThrowIfNullOrEmpty(content);

        _logger.LogInformation("Encrypting and saving content to file: {FilePath}", file.Path);

        try
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            await _cipherService.SaveAndXorAsync(file.Path, contentBytes, cancellationToken);

            _logger.LogInformation("Successfully encrypted and saved content to file: {FilePath}", file.Path);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting or saving content to file: {FilePath}", file.Path);
            throw;
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

            _logger.LogInformation("Creating backup of file {OriginalFile} as {BackupFile}", file.Path, backupFileName);

            StorageFile backupFile = await file.CopyAsync(folder, backupFileName, NameCollisionOption.GenerateUniqueName);

            _logger.LogInformation("Successfully created backup of file {OriginalFile} as {BackupFile}", file.Path, backupFile.Path);

            return backupFile;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating backup of file: {FilePath}", file.Path);
            return null;
        }
    }
}