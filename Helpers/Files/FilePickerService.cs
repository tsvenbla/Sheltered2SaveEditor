using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Implementation of the file picker service using the Windows Storage Pickers.
/// </summary>
internal sealed class FilePickerService : IFilePickerService
{
    /// <inheritdoc/>
    public async Task<StorageFile?> PickFileAsync()
    {
        FileOpenPicker openPicker = new()
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.ComputerFolder
        };
        openPicker.FileTypeFilter.Add(".dat");

        // Use MainWindow's handle for the picker.
        nint hWnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(openPicker, hWnd);

        return await openPicker.PickSingleFileAsync();
    }
}