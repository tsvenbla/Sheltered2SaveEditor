using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Core;
using Sheltered2SaveEditor.Core.Models;
using Sheltered2SaveEditor.Infrastructure.Files;
using Sheltered2SaveEditor.Utils.Helpers;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace Sheltered2SaveEditor.Features.SaveFiles;

/// <summary>
/// Manages loading, saving, and tracking changes to save files.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SaveFileManager"/> class.
/// </remarks>
/// <param name="fileService">The file service.</param>
/// <param name="filePickerService">The file picker service.</param>
/// <param name="fileValidator">The file validator.</param>
/// <param name="logger">The logger.</param>
public class SaveFileManager(
    IFileService fileService,
    IFilePickerService filePickerService,
    FileValidator fileValidator,
    ILogger<SaveFileManager> logger) : ISaveFileManager
{
    private readonly IFileService _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    private readonly IFilePickerService _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
    private readonly FileValidator _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
    private readonly ILogger<SaveFileManager> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    /// <inheritdoc/>
    public bool IsFileLoaded => AppDataHelper.IsSaveFileLoaded;

    /// <inheritdoc/>
    public bool HasUnsavedChanges => AppDataHelper.IsSaveFileModified;

    /// <inheritdoc/>
    public StorageFile? CurrentFile => AppDataHelper.CurrentSaveFile;

    /// <inheritdoc/>
    public event EventHandler<SaveFileLoadedEventArgs> SaveFileLoaded
    {
        add => AppDataHelper.SaveFileLoaded += value;
        remove => AppDataHelper.SaveFileLoaded -= value;
    }

    /// <inheritdoc/>
    public event EventHandler<SaveFileModifiedEventArgs> SaveFileModified
    {
        add => AppDataHelper.SaveFileModified += value;
        remove => AppDataHelper.SaveFileModified -= value;
    }

    /// <inheritdoc/>
    public async Task<bool> LoadSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            // Clear previous data
            AppDataHelper.Clear();

            // Validate the file type
            if (!FileValidator.IsValidDatFile(file))
            {
                _logger.LogWarning("Invalid file type: {FilePath}", file.Path);
                return false;
            }

            // Validate the file contents
            bool isValid = await _fileValidator.IsValidSaveFileAsync(file, cancellationToken);
            if (!isValid)
            {
                _logger.LogWarning("Invalid file format: {FilePath}", file.Path);
                return false;
            }

            // Load and decrypt the file
            string decryptedContent = await _fileService.LoadAndDecryptSaveFileAsync(file, cancellationToken);

            // Parse the XML
            XDocument document = XDocument.Parse(decryptedContent);
            AppDataHelper.SaveDocument = document;

            // Extract character data
            List<Character> characters = CharacterParser.ParseCharacters(decryptedContent);
            AppDataHelper.UpdateCharacters(characters);

            // Update state
            AppDataHelper.CurrentSaveFile = file;
            AppDataHelper.MarkAsModified(false);

            _logger.LogInformation("Successfully loaded save file: {FilePath}", file.Path);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading save file: {FilePath}", file?.Path);
            AppDataHelper.Clear();
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveChangesAsync(bool createBackup = true, CancellationToken cancellationToken = default)
    {
        if (CurrentFile == null || AppDataHelper.SaveDocument == null)
        {
            _logger.LogWarning("Cannot save changes: No file is currently loaded");
            return false;
        }

        try
        {
            // Create backup if requested
            if (createBackup)
            {
                StorageFile? backupFile = await _fileService.CreateBackupAsync(CurrentFile, cancellationToken);
                if (backupFile == null)
                {
                    _logger.LogWarning("Failed to create backup of file: {FilePath}", CurrentFile.Path);
                    // Continue without backup - the caller should decide whether to proceed
                }
                else
                {
                    _logger.LogInformation("Created backup of file: {OriginalPath} as {BackupPath}",
                        CurrentFile.Path, backupFile.Path);
                }
            }

            // Save changes
            string updatedXml = AppDataHelper.SaveDocument.ToString();
            await _fileService.EncryptAndSaveSaveFileAsync(CurrentFile, updatedXml, cancellationToken);

            // Update state
            AppDataHelper.MarkAsModified(false);

            _logger.LogInformation("Successfully saved changes to file: {FilePath}", CurrentFile.Path);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving changes to file: {FilePath}", CurrentFile?.Path);
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> PickAndLoadSaveFileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Prompt the user to pick a file
            StorageFile? file = await _filePickerService.PickFileAsync();
            if (file == null)
            {
                _logger.LogInformation("File picking canceled by user");
                return false;
            }

            // Load the picked file
            return await LoadSaveFileAsync(file, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error picking and loading save file");
            return false;
        }
    }
}