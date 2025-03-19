using Sheltered2SaveEditor.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Sheltered2SaveEditor.Features.SaveFiles;

/// <summary>
/// Defines methods for managing save files.
/// </summary>
internal interface ISaveFileManager
{
    /// <summary>
    /// Gets a value indicating whether a save file is currently loaded.
    /// </summary>
    bool IsFileLoaded { get; }

    /// <summary>
    /// Gets a value indicating whether the currently loaded save file has unsaved changes.
    /// </summary>
    bool HasUnsavedChanges { get; }

    /// <summary>
    /// Gets the currently loaded save file, if any.
    /// </summary>
    StorageFile? CurrentFile { get; }

    /// <summary>
    /// Event raised when a save file is loaded.
    /// </summary>
    event EventHandler<SaveFileLoadedEventArgs> SaveFileLoaded;

    /// <summary>
    /// Event raised when a save file is modified.
    /// </summary>
    event EventHandler<SaveFileModifiedEventArgs> SaveFileModified;

    /// <summary>
    /// Loads and validates a save file.
    /// </summary>
    /// <param name="file">The save file to load.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the file was loaded successfully.</returns>
    Task<bool> LoadSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the currently loaded save file.
    /// </summary>
    /// <param name="createBackup">Whether to create a backup of the original file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if the file was saved successfully.</returns>
    Task<bool> SaveChangesAsync(bool createBackup = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prompts the user to pick a save file and loads it.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the asynchronous operation. The task result is true if a file was picked and loaded successfully.</returns>
    Task<bool> PickAndLoadSaveFileAsync(CancellationToken cancellationToken = default);
}