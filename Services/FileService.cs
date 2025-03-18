using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Helpers;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Sheltered2SaveEditor.Services;

/// <summary>
/// Service for handling file operations.
/// </summary>
public class FileService : IFileService
{
    private readonly IFilePickerService _filePickerService;
    private readonly FileValidator _fileValidator;
    private readonly IXorCipherService _cipherService;
    private readonly ILogger<FileService> _logger;

    public FileService(
        IFilePickerService filePickerService,
        FileValidator fileValidator,
        IXorCipherService cipherService,
        ILogger<FileService> logger)
    {
        _filePickerService = filePickerService;
        _fileValidator = fileValidator;
        _cipherService = cipherService;
        _logger = logger;
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