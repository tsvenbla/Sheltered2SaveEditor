using Windows.Storage.Pickers;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Provides configuration options for the file picker service.
/// </summary>
internal sealed record FilePickerOptions
{
    /// <summary>
    /// Gets the view mode for the file picker.
    /// </summary>
    internal PickerViewMode ViewMode { get; init; } = PickerViewMode.Thumbnail;

    /// <summary>
    /// Gets the suggested start location for the file picker.
    /// </summary>
    internal PickerLocationId StartLocation { get; init; } = PickerLocationId.ComputerFolder;

    /// <summary>
    /// Gets the file type(s) that the file picker should filter by.
    /// </summary>
    /// <remarks>
    /// At time of writing, Sheltered 2 uses only the .dat file type.
    /// </remarks>
    internal IReadOnlyList<string> FileTypeFilter { get; init; } = [".dat"];
}