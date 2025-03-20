namespace Sheltered2SaveEditor.Infrastructure.Encryption;

/// <summary>
/// Defines the available options for XOR cipher operations.
/// </summary>
internal sealed record XorCipherOptions
{
    /// <summary>
    /// Gets or sets the buffer size for file operations.
    /// </summary>
    /// <remarks>
    /// Default is 64KB. Larger values may improve performance for large files,
    /// while smaller values reduce memory usage.
    /// </remarks>
    internal int BufferSize { get; init; } = 64 * 1024;

    /// <summary>
    /// Gets or sets a value indicating whether to use buffered I/O for large files.
    /// </summary>
    /// <remarks>
    /// Default is true. When enabled, larger files are processed in chunks
    /// instead of loading the entire file into memory.
    /// </remarks>
    internal bool UseBufferedIO { get; init; } = true;

    /// <summary>
    /// Gets or sets the file size threshold (in bytes) above which buffered I/O is used.
    /// </summary>
    /// <remarks>
    /// Default is 4MB. Files larger than this will be processed in chunks if
    /// <see cref="UseBufferedIO"/> is enabled.
    /// </remarks>
    internal long BufferedIOThreshold { get; init; } = 4 * 1024 * 1024;

    /// <summary>
    /// Gets or sets a value indicating whether to verify file operations after completion.
    /// </summary>
    /// <remarks>
    /// Default is false. When enabled, performs additional validation after file operations.
    /// This may impact performance but provides additional reliability.
    /// </remarks>
    internal bool VerifyOperations { get; init; } = true;
}