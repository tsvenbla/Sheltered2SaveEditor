using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Implementation of the file picker service using the Windows Storage Pickers.
/// </summary>
internal sealed class FilePickerService : IFilePickerService
{
    private readonly FilePickerOptions _options = new();

    /// <inheritdoc/>
    public async Task<StorageFile?> PickFileAsync(CancellationToken cancellationToken = default)
    {
        FileOpenPicker openPicker = new()
        {
            ViewMode = _options.ViewMode,
            SuggestedStartLocation = _options.StartLocation
        };

        // Add file type filters
        foreach (string extension in _options.FileTypeFilter)
        {
            openPicker.FileTypeFilter.Add(extension);
        }

        try
        {
            // Use the main window handle for the picker
            nint hWnd = WindowHandleHelper.GetMainWindowHandle();
            InitializeWithWindow.Initialize(openPicker, hWnd);

            // Create a task that completes when the picker returns
            Task<StorageFile> pickTask = openPicker.PickSingleFileAsync().AsTask(cancellationToken);
            return await pickTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (Exception)
        {
            // If initialization fails or any other error occurs, return null instead of crashing
            return null;
        }
    }
}