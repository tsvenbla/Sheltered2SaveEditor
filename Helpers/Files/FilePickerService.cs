using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Implementation of the file picker service using the Windows Storage Pickers.
/// </summary>
internal sealed class FilePickerService : IFilePickerService
{
    private readonly FilePickerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="FilePickerService"/> class.
    /// </summary>
    /// <param name="options">Options for configuring the file picker.</param>
    internal FilePickerService(FilePickerOptions? options = null) => _options = options ?? new FilePickerOptions();

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
            return await openPicker.PickSingleFileAsync().AsTask(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // User canceled the dialog
            return null;
        }
        catch (Exception ex) when (ex is System.Runtime.InteropServices.COMException or ArgumentException)
        {
            throw new InvalidOperationException("File picker initialization failed.", ex);
        }
    }
}