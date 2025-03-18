using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Helpers;
using Sheltered2SaveEditor.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace Sheltered2SaveEditor.ViewModels;

/// <summary>
/// ViewModel for the Home page responsible for loading, validating, decrypting,
/// and processing the game save file.
/// </summary>
public partial class HomePageViewModel : ObservableObject, IDisposable
{
    #region Fields
    private readonly IFilePickerService _filePickerService;
    private readonly IFileService _fileService;
    private readonly FileValidator _fileValidator;
    private readonly IDialogService _dialogService;
    private readonly ILogger<HomePageViewModel> _logger;
    private bool _disposed;
    #endregion

    #region Observable Properties
    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _feedback = string.Empty;

    [ObservableProperty]
    private bool _isSaveButtonEnabled;

    [ObservableProperty]
    private StorageFile? _selectedFile;

    [ObservableProperty]
    private bool _hasUnsavedChanges;
    #endregion

    #region Commands
    /// <summary>
    /// Command to load a save file.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLoadFile))]
    private async Task LoadFileAsync()
    {
        if (HasUnsavedChanges)
        {
            bool shouldProceed = await _dialogService.ShowConfirmationDialogAsync(
                "Unsaved Changes",
                "You have unsaved changes. Are you sure you want to load a new file?",
                "Yes", "No");

            if (!shouldProceed)
            {
                return;
            }
        }

        IsLoading = true;
        Feedback = string.Empty;
        
        try
        {
            // Clear previous data
            AppDataHelper.Clear();
            
            // Pick a file
            StorageFile? file = await _filePickerService.PickFileAsync();
            if (file == null)
            {
                Feedback = "Operation cancelled.";
                return;
            }

            // Validate the file type
            if (!FileValidator.IsValidDatFile(file))
            {
                Feedback = "Invalid file type. Please select a DAT file.";
                return;
            }

            // Validate the file contents
            bool isValid = await _fileValidator.IsValidSaveFileAsync(file);
            if (!isValid)
            {
                Feedback = "Invalid file format. Not a valid Sheltered 2 save file.";
                return;
            }

            // Process valid file
            await ProcessValidFileAsync(file);
        }
        catch (Exception ex)
        {
            HandleFileLoadError(ex);
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Command to save changes to the save file.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSaveFile))]
    private async Task SaveFileAsync()
    {
        if (SelectedFile == null || AppDataHelper.SaveDocument == null)
        {
            await _dialogService.ShowErrorDialogAsync(
                "Save Error", "No file is currently loaded.");
            return;
        }

        IsLoading = true;
        Feedback = "Saving changes...";

        try
        {
            // Create backup of original file
            StorageFile? backupFile = await _fileService.CreateBackupAsync(SelectedFile);
            if (backupFile == null)
            {
                bool proceed = await _dialogService.ShowConfirmationDialogAsync(
                    "Backup Failed",
                    "Failed to create a backup of the original file. Do you want to proceed with saving anyway?",
                    "Yes", "No");

                if (!proceed)
                {
                    Feedback = "Save operation cancelled.";
                    return;
                }
            }

            // Save changes
            string updatedXml = AppDataHelper.SaveDocument.ToString();
            await _fileService.EncryptAndSaveSaveFileAsync(SelectedFile, updatedXml);

            // Update state
            AppDataHelper.MarkAsModified(false);
            HasUnsavedChanges = false;
            Feedback = $"Changes saved successfully to {SelectedFile.Name}";

            if (backupFile != null)
            {
                Feedback += $". Backup created at {backupFile.Name}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file");
            await _dialogService.ShowErrorDialogAsync(
                "Save Error", $"Failed to save changes: {ex.Message}");
            Feedback = $"Error saving file: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
    #endregion

    #region Constructor and Initialization
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
    /// </summary>
    public HomePageViewModel(
        IFilePickerService filePickerService,
        IFileService fileService,
        FileValidator fileValidator,
        IDialogService dialogService,
        ILogger<HomePageViewModel> logger)
    {
        _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to events
        AppDataHelper.SaveFileLoaded += OnSaveFileLoaded;
        AppDataHelper.SaveFileModified += OnSaveFileModified;

        _logger.LogInformation("HomePageViewModel initialized");
    }
    #endregion

    #region Event Handlers
    private void OnSaveFileLoaded(object? sender, SaveFileLoadedEventArgs e)
    {
        IsSaveButtonEnabled = e.IsLoaded;
        SelectedFile = e.SaveFile;
        OnPropertyChanged(nameof(IsFileLoaded));
    }

    private void OnSaveFileModified(object? sender, SaveFileModifiedEventArgs e)
    {
        HasUnsavedChanges = e.IsModified;
        SaveFileCommand.NotifyCanExecuteChanged();
    }
    #endregion

    #region Helper Methods
    /// <summary>
    /// Processes a valid save file by decrypting its content and parsing character data.
    /// </summary>
    private async Task ProcessValidFileAsync(StorageFile file)
    {
        try
        {
            // Decrypt the file
            string decryptedContent = await _fileService.LoadAndDecryptSaveFileAsync(file);
            
            // Parse the XML
            XDocument document = XDocument.Parse(decryptedContent);
            AppDataHelper.SaveDocument = document;
            
            // Extract character data
            List<Character> characters = CharacterParser.ParseCharacters(decryptedContent);
            AppDataHelper.UpdateCharacters(characters);
            
            // Update state
            AppDataHelper.CurrentSaveFile = file;
            AppDataHelper.MarkAsModified(false);
            SelectedFile = file;
            
            // Update UI
            Feedback = $"Selected file name: {file.Name}\nDecrypted successfully.";
            _logger.LogInformation("Successfully loaded save file: {FileName}", file.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file");
            Feedback = $"Error processing file: {ex.Message}";
            throw;
        }
    }

    /// <summary>
    /// Handles errors that occur during file loading.
    /// </summary>
    private void HandleFileLoadError(Exception ex)
    {
        string errorMessage = ex switch
        {
            System.IO.FileNotFoundException fileEx => $"File not found: {fileEx.FileName}",
            UnauthorizedAccessException => "Access denied. Please check file permissions.",
            System.IO.IOException => $"I/O error: {ex.Message}",
            _ => $"Unexpected error: {ex.Message}"
        };

        _logger.LogError(ex, "Error loading file: {ErrorMessage}", errorMessage);
        Feedback = errorMessage;
    }

    /// <summary>
    /// Determines whether the LoadFile command can execute.
    /// </summary>
    private bool CanLoadFile() => !IsLoading;

    /// <summary>
    /// Determines whether the SaveFile command can execute.
    /// </summary>
    private bool CanSaveFile() => 
        !IsLoading && 
        IsFileLoaded && 
        HasUnsavedChanges;

    /// <summary>
    /// Gets a value indicating whether a save file has been loaded.
    /// </summary>
    public bool IsFileLoaded => AppDataHelper.IsSaveFileLoaded;
    #endregion

    #region IDisposable Implementation
    /// <summary>
    /// Disposes resources used by the view model.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed and unmanaged resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Unsubscribe from events
            AppDataHelper.SaveFileLoaded -= OnSaveFileLoaded;
            AppDataHelper.SaveFileModified -= OnSaveFileModified;
        }

        _disposed = true;
    }
    #endregion
}