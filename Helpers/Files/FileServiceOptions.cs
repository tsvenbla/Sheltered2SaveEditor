namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Provides configuration options for file service operations.
/// </summary>
internal sealed record FileServiceOptions
{
    /// <summary>
    /// Gets the suffix to append to backup file names.
    /// </summary>
    /// <remarks>
    /// Default is "_backup_". This is inserted between the filename and the date portion.
    /// </remarks>
    internal string BackupFileSuffix { get; init; } = "_backup_";

    /// <summary>
    /// Gets the date format string for backup file names.
    /// </summary>
    /// <remarks>
    /// Default is "yyyyMMdd_HHmmss". This format is used to create unique backup files.
    /// </remarks>
    internal string BackupDateFormat { get; init; } = "yyyyMMdd_HHmmss";

    /// <summary>
    /// Gets a value indicating whether to validate the XML structure of save files.
    /// </summary>
    /// <remarks>
    /// Default is true. When enabled, the service checks that the content has proper XML structure.
    /// </remarks>
    internal bool ValidateXmlStructure { get; init; } = true;
}