using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.UI.Dispatching;
using Sheltered2SaveEditor.Core;
using Sheltered2SaveEditor.Infrastructure.UI.Dialogs;
using Windows.Storage;

namespace Sheltered2SaveEditor.Features.SaveFiles.ViewModels;

/// <summary>
/// ViewModel for the Home page responsible for loading and managing save files.
/// </summary>
internal sealed partial class HomePageViewModel : ObservableObject, IDisposable
{
    #region Fields
    private readonly ISaveFileManager _saveFileManager;
    private readonly IDialogService _dialogService;
    private readonly ILogger<HomePageViewModel> _logger;
    private readonly CancellationTokenSource _cts = new();
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
                "Yes", "No").ConfigureAwait(true);

            if (!shouldProceed)
            {
                return;
            }
        }

        IsLoading = true;
        Feedback = string.Empty;

        try
        {
            bool result = await _saveFileManager.PickAndLoadSaveFileAsync(_cts.Token).ConfigureAwait(true);
            if (!result)
            {
                Feedback = "Failed to load save file. It may be invalid or corrupted.";
            }
            else
            {
                SelectedFile = _saveFileManager.CurrentFile;
                Feedback = $"Selected file name: {SelectedFile?.Name}\nDecrypted successfully.";
            }
        }
        catch (OperationCanceledException)
        {
            Feedback = "File loading operation was canceled.";
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
        if (!_saveFileManager.IsFileLoaded)
        {
            _ = await _dialogService.ShowErrorDialogAsync(
                "Save Error", "No file is currently loaded.").ConfigureAwait(true);
            return;
        }

        IsLoading = true;
        Feedback = "Saving changes...";

        try
        {
            // Get the dispatcher queue for UI updates
            DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Perform the save operation
            bool success = await _saveFileManager.SaveChangesAsync(createBackup: true, _cts.Token).ConfigureAwait(false);

            // Update UI on UI thread
            _ = await dispatcherQueue.EnqueueAsync(async () =>
            {
                if (success)
                {
                    Feedback = $"Changes saved successfully to {_saveFileManager.CurrentFile?.Name}";
                }
                else
                {
                    Feedback = "Failed to save changes.";
                    _ = await _dialogService.ShowErrorDialogAsync(
                        "Save Error", "Failed to save changes to the file.").ConfigureAwait(true);
                }
                IsLoading = false;
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            Feedback = "Save operation was canceled.";
            IsLoading = false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file");

            try
            {
                // Update UI on UI thread
                DispatcherQueue dispatcherQueue = DispatcherQueue.GetForCurrentThread();
                _ = await dispatcherQueue.EnqueueAsync(async () =>
                {
                    Feedback = $"Error saving file: {ex.Message}";
                    IsLoading = false;

                    _ = await _dialogService.ShowErrorDialogAsync(
                        "Save Error", $"Failed to save changes: {ex.Message}").ConfigureAwait(true);
                }).ConfigureAwait(false);
            }
            catch (Exception dispatcherEx)
            {
                // Last resort fallback if dispatcher fails
                _logger.LogError(dispatcherEx, "Failed to show error dialog via dispatcher");
                Feedback = $"Error saving file: {ex.Message}";
                IsLoading = false;
            }
        }
    }
    #endregion

    #region Constructor and Initialization
    /// <summary>
    /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
    /// </summary>
    /// <param name="saveFileManager">The save file manager.</param>
    /// <param name="dialogService">The dialog service.</param>
    /// <param name="logger">The logger.</param>
    public HomePageViewModel(
        ISaveFileManager saveFileManager,
        IDialogService dialogService,
        ILogger<HomePageViewModel> logger)
    {
        _saveFileManager = saveFileManager ?? throw new ArgumentNullException(nameof(saveFileManager));
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

        // Update HasUnsavedChanges based on _saveFileManager
        HasUnsavedChanges = _saveFileManager.HasUnsavedChanges;

        // Notify the command that its execute status might have changed
        SaveFileCommand.NotifyCanExecuteChanged();
    }

    private void OnSaveFileModified(object? sender, SaveFileModifiedEventArgs e)
    {
        HasUnsavedChanges = e.IsModified;
        SaveFileCommand.NotifyCanExecuteChanged();
    }
    #endregion

    #region Helper Methods
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
    internal bool IsFileLoaded => _saveFileManager.IsFileLoaded;
    #endregion

    #region IDisposable Implementation
    /// <summary>
    /// Disposes resources used by the view model.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    private void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            // Unsubscribe from events
            AppDataHelper.SaveFileLoaded -= OnSaveFileLoaded;
            AppDataHelper.SaveFileModified -= OnSaveFileModified;

            // Cancel any pending operations
            try
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing cancellation token source");
            }
        }

        _disposed = true;
    }
    #endregion
}