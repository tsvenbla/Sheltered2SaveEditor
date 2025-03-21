namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Represents the result of a file validation operation.
/// </summary>
internal enum ValidationResult
{
    /// <summary>
    /// The file is valid.
    /// </summary>
    Valid,

    /// <summary>
    /// The file has an invalid format.
    /// </summary>
    InvalidFormat,

    /// <summary>
    /// The file exceeds the maximum allowed size.
    /// </summary>
    FileTooLarge,

    /// <summary>
    /// The file processing exceeded the maximum allowed time.
    /// </summary>
    ProcessingTimeExceeded,

    /// <summary>
    /// The file could not be accessed or read.
    /// </summary>
    AccessError,

    /// <summary>
    /// The file has potentially unsafe content.
    /// </summary>
    UnsafeContent,

    /// <summary>
    /// The validation operation was cancelled.
    /// </summary>
    Cancelled,

    /// <summary>
    /// An unexpected error occurred during validation.
    /// </summary>
    UnknownError,

    /// <summary>
    /// The file has an invalid structure beyond basic format checking.
    /// </summary>
    InvalidStructure,

    /// <summary>
    /// The file couldn't be decrypted properly.
    /// </summary>
    DecryptionError,

    /// <summary>
    /// The file was temporary unavailable, usually due to being locked by another process.
    /// </summary>
    FileTemporarilyUnavailable,

    /// <summary>
    /// The file passed validation with warnings.
    /// </summary>
    ValidWithWarnings
}

/// <summary>
/// Represents the result of a file validation operation with detailed information.
/// </summary>
/// <param name="Result">The validation result.</param>
/// <param name="Message">A detailed message describing the result.</param>
/// <param name="Exception">The exception that occurred, if any.</param>
/// <param name="ElapsedMilliseconds">The time taken for validation.</param>
/// <param name="FileInfo">Information about the validated file.</param>
internal sealed record ValidationResultInfo(
    ValidationResult Result,
    string Message,
    Exception? Exception = null,
    long ElapsedMilliseconds = 0,
    FileInfo? FileInfo = null)
{
    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    /// <param name="elapsedMilliseconds">The time taken for validation.</param>
    /// <param name="fileInfo">Information about the validated file.</param>
    /// <returns>A successful validation result.</returns>
    internal static ValidationResultInfo Success(long elapsedMilliseconds, FileInfo? fileInfo = null) =>
        new(ValidationResult.Valid, "File validation passed successfully.", null, elapsedMilliseconds, fileInfo);

    /// <summary>
    /// Creates a failed validation result with the specified details.
    /// </summary>
    /// <param name="result">The specific failure result.</param>
    /// <param name="message">A detailed error message.</param>
    /// <param name="exception">The exception that caused the failure, if any.</param>
    /// <param name="elapsedMilliseconds">The time taken before failure.</param>
    /// <param name="fileInfo">Information about the file that failed validation.</param>
    /// <returns>A validation result describing the failure.</returns>
    internal static ValidationResultInfo Failure(
        ValidationResult result,
        string message,
        Exception? exception = null,
        long elapsedMilliseconds = 0,
        FileInfo? fileInfo = null) =>
        new(result, message, exception, elapsedMilliseconds, fileInfo);
}