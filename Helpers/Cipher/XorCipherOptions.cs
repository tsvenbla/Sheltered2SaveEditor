namespace Sheltered2SaveEditor.Helpers.Cipher;

/// <summary>
/// Defines the available options for XOR cipher operations.
/// </summary>
internal sealed record XorCipherOptions
{
    /// <summary>
    /// Gets the buffer size for file operations.
    /// </summary>
    /// <remarks>
    /// Default is 64KB. Larger values may improve performance for large files,
    /// while smaller values reduce memory usage.
    /// </remarks>
    internal int BufferSize { get; init; } = 64 * 1024;

    /// <summary>
    /// Gets a value indicating whether to use buffered I/O for large files.
    /// </summary>
    /// <remarks>
    /// Default is true. When enabled, larger files are processed in chunks
    /// instead of loading the entire file into memory.
    /// </remarks>
    internal bool UseBufferedIO { get; init; } = true;

    /// <summary>
    /// Gets the file size threshold (in bytes) above which buffered I/O is used.
    /// </summary>
    /// <remarks>
    /// Default is 4MB. Files larger than this will be processed in chunks if
    /// <see cref="UseBufferedIO"/> is enabled.
    /// </remarks>
    internal long BufferedIOThreshold { get; init; } = 4 * 1024 * 1024;

    /// <summary>
    /// Gets a value indicating whether to verify file operations after completion.
    /// </summary>
    /// <remarks>
    /// Default is true. When enabled, performs additional validation after file operations.
    /// This may impact performance but provides additional reliability.
    /// </remarks>
    internal bool VerifyOperations { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to use SIMD operations when available.
    /// </summary>
    /// <remarks>
    /// Default is true. When enabled, uses CPU vectorization for XOR operations.
    /// </remarks>
    internal bool UseSIMD { get; init; } = true;
}