using System.Threading.Tasks;
using Windows.Storage;

namespace Sheltered2SaveEditor.Infrastructure.Files;

/// <summary>
/// Defines a service for picking files from the file system.
/// </summary>
internal interface IFilePickerService
{
    /// <summary>
    /// Allows the user to pick a single file.
    /// </summary>
    /// <returns>The picked file, or null if the operation was canceled.</returns>
    Task<StorageFile?> PickFileAsync();
}