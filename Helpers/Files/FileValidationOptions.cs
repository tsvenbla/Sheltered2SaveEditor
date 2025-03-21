namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Contains configuration settings for file validation.
/// </summary>
internal sealed record FileValidationOptions
{
    /// <summary>
    /// The default expected header for XML validation.
    /// </summary>
    internal const string DefaultExpectedHeader = "<root>";

    /// <summary>
    /// The default expected footer for XML validation.
    /// </summary>
    internal const string DefaultExpectedFooter = "</root>";

    /// <summary>
    /// Gets the maximum allowed file size in bytes.
    /// </summary>
    /// <remarks>
    /// Default is 25MB (25 * 1024 * 1024 bytes).
    /// </remarks>
    internal ulong MaxFileSize { get; init; } = 25 * 1024 * 1024;

    /// <summary>
    /// Gets the expected header of the decrypted content.
    /// </summary>
    internal string ExpectedHeader { get; init; } = DefaultExpectedHeader;

    /// <summary>
    /// Gets the expected footer of the decrypted content.
    /// </summary>
    internal string ExpectedFooter { get; init; } = DefaultExpectedFooter;

    /// <summary>
    /// Gets the maximum time allowed for processing a file, in seconds.
    /// </summary>
    /// <remarks>
    /// Default is 60 seconds (1 minute).
    /// </remarks>
    internal int MaxProcessingTimeSeconds { get; init; } = 60;

    /// <summary>
    /// Gets the number of retries for file operations before failing.
    /// </summary>
    /// <remarks>
    /// Default is 3 retries.
    /// </remarks>
    internal int RetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets the delay between retry attempts in milliseconds.
    /// </summary>
    /// <remarks>
    /// Default is 500 milliseconds.
    /// </remarks>
    internal int RetryDelayMilliseconds { get; init; } = 500;

    /// <summary>
    /// Gets a value indicating whether to enable enhanced validation reporting.
    /// </summary>
    /// <remarks>
    /// When enabled, provides detailed information about validation failures.
    /// </remarks>
    internal bool EnableEnhancedValidationReporting { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to optimize for memory usage at the expense of some CPU.
    /// </summary>
    /// <remarks>
    /// When enabled, uses more memory-efficient but potentially slower approaches for large files.
    /// </remarks>
    internal bool OptimizeForMemory { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to use parallel processing for large files when applicable.
    /// </summary>
    /// <remarks>
    /// This may improve performance for large files but could increase memory usage.
    /// Only applies to files larger than 10MB by default.
    /// </remarks>
    internal bool EnableParallelProcessing { get; init; } = true;

    /// <summary>
    /// Gets the size threshold in bytes above which parallel processing is considered if enabled.
    /// </summary>
    /// <remarks>
    /// Default is 10MB.
    /// </remarks>
    internal ulong ParallelProcessingThreshold { get; init; } = 10 * 1024 * 1024;

    /// <summary>
    /// Gets a value indicating whether to validate the file's structure beyond basic header/footer checks.
    /// </summary>
    /// <remarks>
    /// For Sheltered 2 save files, this only does basic XML structure checking.
    /// </remarks>
    internal bool ValidateStructure { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to bypass validation failures and attempt to load the file anyway.
    /// </summary>
    /// <remarks>
    /// When enabled, validation failures will be logged but won't prevent the file from being loaded.
    /// </remarks>
    internal bool BypassValidationFailures { get; init; } = true;

    /// <summary>
    /// Gets the size of the buffer used for reading files.
    /// </summary>
    /// <remarks>
    /// Default is 64KB. Should be a power of 2 for best performance.
    /// </remarks>
    internal int BufferSize { get; init; } = 64 * 1024;

    /// <summary>
    /// Gets the size of the overlap between buffers when reading large files in chunks.
    /// </summary>
    /// <remarks>
    /// Default is 1KB. This helps ensure we don't miss content that spans chunk boundaries.
    /// </remarks>
    internal int ChunkOverlapSize { get; init; } = 1024;
}