using Sheltered2SaveEditor.Core.Models;
using System.Xml.Linq;
using Windows.Storage;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Manages loading, saving, and tracking changes to save files.
/// </summary>
internal sealed class SaveFileManager : ISaveFileManager
{
    private readonly IFileService _fileService;
    private readonly IFilePickerService _filePickerService;
    private readonly IFileValidator _fileValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="SaveFileManager"/> class.
    /// </summary>
    /// <param name="fileService">The file service.</param>
    /// <param name="filePickerService">The file picker service.</param>
    /// <param name="fileValidator">The file validator.</param>
    internal SaveFileManager(
        IFileService fileService,
        IFilePickerService filePickerService,
        IFileValidator fileValidator)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
    }

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
        ArgumentNullException.ThrowIfNull(file);

        try
        {
            // Clear previous data
            AppDataHelper.Clear();

            // Validate the file type
            if (!_fileValidator.HasValidExtension(file))
            {
                return false;
            }

            // Validate the file contents
            bool isValid = await _fileValidator.IsValidSaveFileAsync(file, cancellationToken).ConfigureAwait(false);
            if (!isValid)
            {
                return false;
            }

            // Load and decrypt the file
            string decryptedContent = await _fileService.LoadAndDecryptSaveFileAsync(file, cancellationToken).ConfigureAwait(false);

            // Parse the XML
            XDocument document = XDocument.Parse(decryptedContent);
            AppDataHelper.SaveDocument = document;

            // Extract character data
            IReadOnlyList<Character> characters = CharacterParser.ParseCharacters(decryptedContent);
            AppDataHelper.UpdateCharacters(characters);

            // Update state
            AppDataHelper.CurrentSaveFile = file;

            // Mark as modified to enable saving right after load
            // This allows saving to a different location even if no changes were made
            AppDataHelper.MarkAsModified(true);

            return true;
        }
        catch (Exception)
        {
            AppDataHelper.Clear();
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> SaveChangesAsync(bool createBackup = true, CancellationToken cancellationToken = default)
    {
        if (CurrentFile == null || AppDataHelper.SaveDocument == null)
        {
            return false;
        }

        try
        {
            // Create backup if requested
            if (createBackup)
            {
                _ = await _fileService.CreateBackupAsync(CurrentFile, cancellationToken).ConfigureAwait(false);
            }

            // Save changes
            string updatedXml = AppDataHelper.SaveDocument.ToString();
            await _fileService.EncryptAndSaveSaveFileAsync(CurrentFile, updatedXml, cancellationToken).ConfigureAwait(false);

            // Update state
            AppDataHelper.MarkAsModified(false);

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<bool> PickAndLoadSaveFileAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Prompt the user to pick a file
            StorageFile? file = await _filePickerService.PickFileAsync(cancellationToken).ConfigureAwait(false);
            if (file == null)
            {
                return false;
            }

            // Load the picked file
            return await LoadSaveFileAsync(file, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            return false;
        }
    }
}