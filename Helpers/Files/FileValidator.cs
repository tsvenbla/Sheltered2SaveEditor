using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Helpers.Cipher;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Sheltered2SaveEditor.Helpers.Files;
/// <summary>
/// Contains configuration settings for file validation.
/// </summary>
public sealed record FileValidationOptions
{
    /// <summary>
    /// Gets the maximum allowed file size in bytes.
    /// </summary>
    /// <remarks>
    /// Default is 25MB (25 * 1024 * 1024 bytes).
    /// </remarks>
    public ulong MaxFileSize { get; init; } = 25 * 1024 * 1024;

    /// <summary>
    /// Gets the expected header of the decrypted content.
    /// </summary>
    public string ExpectedHeader { get; init; } = "<root>";

    /// <summary>
    /// Gets the expected footer of the decrypted content.
    /// </summary>
    public string ExpectedFooter { get; init; } = "</root>";

    /// <summary>
    /// Gets the maximum time allowed for processing a file, in seconds.
    /// </summary>
    /// <remarks>
    /// Default is 60 seconds (1 minute).
    /// </remarks>
    public int MaxProcessingTimeSeconds { get; init; } = 60;

    /// <summary>
    /// Gets the number of retries for file operations before failing.
    /// </summary>
    /// <remarks>
    /// Default is 3 retries.
    /// </remarks>
    public int RetryAttempts { get; init; } = 3;

    /// <summary>
    /// Gets the delay between retry attempts in milliseconds.
    /// </summary>
    /// <remarks>
    /// Default is 500 milliseconds.
    /// </remarks>
    public int RetryDelayMilliseconds { get; init; } = 500;

    /// <summary>
    /// Gets a value indicating whether to enable enhanced validation reporting.
    /// </summary>
    /// <remarks>
    /// When enabled, provides detailed information about validation failures.
    /// </remarks>
    public bool EnableEnhancedValidationReporting { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to optimize for memory usage at the expense of some CPU.
    /// </summary>
    /// <remarks>
    /// When enabled, uses more memory-efficient but potentially slower approaches for large files.
    /// </remarks>
    public bool OptimizeForMemory { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to use parallel processing for large files when applicable.
    /// </summary>
    /// <remarks>
    /// This may improve performance for large files but could increase memory usage.
    /// Only applies to files larger than 10MB by default.
    /// </remarks>
    public bool EnableParallelProcessing { get; init; } = true;

    /// <summary>
    /// Gets the size threshold in bytes above which parallel processing is considered if enabled.
    /// </summary>
    /// <remarks>
    /// Default is 10MB.
    /// </remarks>
    public ulong ParallelProcessingThreshold { get; init; } = 10 * 1024 * 1024;

    /// <summary>
    /// Gets a value indicating whether to validate the file's structure beyond basic header/footer checks.
    /// </summary>
    /// <remarks>
    /// For Sheltered 2 save files, this only does basic XML structure checking.
    /// </remarks>
    public bool ValidateStructure { get; init; } = true;

    /// <summary>
    /// Gets a value indicating whether to bypass validation failures and attempt to load the file anyway.
    /// </summary>
    /// <remarks>
    /// When enabled, validation failures will be logged but won't prevent the file from being loaded.
    /// </remarks>
    public bool BypassValidationFailures { get; init; } = true;

    /// <summary>
    /// Gets the size of the buffer used for reading files.
    /// </summary>
    /// <remarks>
    /// Default is 64KB. Should be a power of 2 for best performance.
    /// </remarks>
    public int BufferSize { get; init; } = 64 * 1024;

    /// <summary>
    /// Gets the size of the overlap between buffers when reading large files in chunks.
    /// </summary>
    /// <remarks>
    /// Default is 1KB. This helps ensure we don't miss content that spans chunk boundaries.
    /// </remarks>
    public int ChunkOverlapSize { get; init; } = 1024;
}

/// <summary>
/// Represents the result of a file validation operation with detailed information.
/// </summary>
/// <param name="Result">The validation result.</param>
/// <param name="Message">A detailed message describing the result.</param>
/// <param name="Exception">The exception that occurred, if any.</param>
/// <param name="ElapsedMilliseconds">The time taken for validation.</param>
/// <param name="FileInfo">Information about the validated file.</param>
public sealed record ValidationResultInfo(
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
    public static ValidationResultInfo Success(long elapsedMilliseconds, FileInfo? fileInfo = null) =>
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
    public static ValidationResultInfo Failure(
        ValidationResult result,
        string message,
        Exception? exception = null,
        long elapsedMilliseconds = 0,
        FileInfo? fileInfo = null) =>
        new(result, message, exception, elapsedMilliseconds, fileInfo);
}

/// <summary>
/// Represents the result of a file validation operation.
/// </summary>
public enum ValidationResult
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
/// Interface for monitoring the progress of file validation operations.
/// </summary>
public interface IValidationProgressMonitor
{
    /// <summary>
    /// Reports progress of the validation operation.
    /// </summary>
    /// <param name="progressPercentage">The percentage of completion (0-100).</param>
    /// <param name="message">A message describing the current operation.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ReportProgressAsync(int progressPercentage, string message, CancellationToken cancellationToken);

    /// <summary>
    /// Reports that validation has started.
    /// </summary>
    /// <param name="fileName">The name of the file being validated.</param>
    /// <param name="fileSize">The size of the file in bytes.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StartValidationAsync(string fileName, ulong fileSize, CancellationToken cancellationToken);

    /// <summary>
    /// Reports that validation has completed.
    /// </summary>
    /// <param name="result">The validation result.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CompleteValidationAsync(ValidationResultInfo result, CancellationToken cancellationToken);
}

/// <summary>
/// A default implementation of <see cref="IValidationProgressMonitor"/> that does nothing.
/// </summary>
/// <remarks>
/// This implements the Null Object pattern and is used when no progress reporting is needed.
/// </remarks>
public sealed class NullValidationProgressMonitor : IValidationProgressMonitor
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullValidationProgressMonitor"/>.
    /// </summary>
    public static IValidationProgressMonitor Instance { get; } = new NullValidationProgressMonitor();

    /// <inheritdoc/>
    public Task CompleteValidationAsync(ValidationResultInfo result, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <inheritdoc/>
    public Task ReportProgressAsync(int progressPercentage, string message, CancellationToken cancellationToken) =>
        Task.CompletedTask;

    /// <inheritdoc/>
    public Task StartValidationAsync(string fileName, ulong fileSize, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}

/// <summary>
/// Provides methods to validate game save files.
/// </summary>
internal sealed class FileValidator
{
    private readonly IXorCipherService _cipherService;
    private readonly FileValidationOptions _options;
    private readonly ILogger? _logger;

    // Expected decrypted header and footer as byte arrays for efficient comparison
    private readonly byte[] _expectedHeaderBytes;
    private readonly byte[] _expectedFooterBytes;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileValidator"/> class with default options.
    /// </summary>
    /// <param name="cipherService">The service used for encryption and decryption.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cipherService"/> is <c>null</c>.</exception>
    internal FileValidator(IXorCipherService cipherService, ILogger? logger = null)
        : this(cipherService, new FileValidationOptions(), logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileValidator"/> class with custom options.
    /// </summary>
    /// <param name="cipherService">The service used for encryption and decryption.</param>
    /// <param name="options">The options for file validation.</param>
    /// <param name="logger">Optional logger for diagnostic information.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cipherService"/> or <paramref name="options"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown if validation options contain invalid values.</exception>
    public FileValidator(IXorCipherService cipherService, FileValidationOptions options, ILogger? logger = null)
    {
        _cipherService = cipherService ?? throw new ArgumentNullException(nameof(cipherService));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        if (string.IsNullOrEmpty(_options.ExpectedHeader))
            throw new ArgumentException("Expected header cannot be null or empty", nameof(options));

        if (string.IsNullOrEmpty(_options.ExpectedFooter))
            throw new ArgumentException("Expected footer cannot be null or empty", nameof(options));

        if (_options.MaxProcessingTimeSeconds <= 0)
            throw new ArgumentException("Max processing time must be greater than zero", nameof(options));

        if (_options.RetryAttempts < 0)
            throw new ArgumentException("Retry attempts cannot be negative", nameof(options));

        if (_options.RetryDelayMilliseconds < 0)
            throw new ArgumentException("Retry delay cannot be negative", nameof(options));

        _expectedHeaderBytes = Encoding.UTF8.GetBytes(_options.ExpectedHeader);
        _expectedFooterBytes = Encoding.UTF8.GetBytes(_options.ExpectedFooter);

        EnsureValidState();
    }

    /// <summary>
    /// Determines if the provided file has a .dat extension.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <returns>True if the file has a .dat extension; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is <c>null</c>.</exception>
    public static bool IsValidDatFile(StorageFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return Path.GetExtension(file.Name)
                  .Equals(".dat", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates the save file by checking its size and signature.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a validation result indicating the outcome.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is <c>null</c>.</exception>
    public async Task<ValidationResult> ValidateSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidationResultInfo result = await ValidateSaveFileWithDetailsAsync(file, NullValidationProgressMonitor.Instance, cancellationToken);

            // If we're bypassing validation failures and we have an invalid format or structure,
            // return ValidWithWarnings instead of the actual error
            if (_options.BypassValidationFailures &&
                (result.Result == ValidationResult.InvalidFormat ||
                 result.Result == ValidationResult.InvalidStructure ||
                 result.Result == ValidationResult.DecryptionError))
            {
                _logger?.LogWarning("Bypassing validation failure: {Result} - {Message}", result.Result, result.Message);
                return ValidationResult.ValidWithWarnings;
            }

            return result.Result;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in ValidateSaveFileAsync");

            // Return Valid or ValidWithWarnings if bypassing failures, otherwise throw
            if (_options.BypassValidationFailures)
            {
                _logger?.LogWarning("Bypassing validation exception: {Message}", ex.Message);
                return ValidationResult.ValidWithWarnings;
            }

            throw;
        }
    }

    /// <summary>
    /// Validates the save file with enhanced reporting and progress monitoring.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <param name="progressMonitor">The monitor to report validation progress.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains detailed validation result info.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is <c>null</c>.</exception>
    public async Task<ValidationResultInfo> ValidateSaveFileWithDetailsAsync(
        StorageFile file,
        IValidationProgressMonitor progressMonitor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(progressMonitor);
        EnsureValidState();

        Stopwatch stopwatch = Stopwatch.StartNew();

        // Create file info for additional tracking
        FileInfo? fileInfo = null;
        try
        {
            fileInfo = new FileInfo(file.Path);
        }
        catch
        {
            // File info creation is optional, so we continue even if it fails
            _logger?.LogWarning("Failed to create FileInfo for {FilePath}", file.Path);
        }

        // Create a linked cancellation token that includes both the provided token and a timeout
        using CancellationTokenSource timeoutCts = new(_options.MaxProcessingTimeSeconds * 1000);
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
        CancellationToken combinedToken = linkedCts.Token;

        try
        {
            await progressMonitor.StartValidationAsync(file.Name, fileInfo != null ? (ulong)fileInfo.Length : 0UL, combinedToken).ConfigureAwait(false);

            // Start with retries for common I/O operations
            using IRandomAccessStreamWithContentType? stream = await RetryAsync(
                async () => await file.OpenReadAsync(),
                _options.RetryAttempts,
                _options.RetryDelayMilliseconds,
                combinedToken).ConfigureAwait(false);

            if (stream == null)
            {
                stopwatch.Stop();
                ValidationResultInfo resultInfo = ValidationResultInfo.Failure(
                    ValidationResult.AccessError,
                    $"Failed to open file {file.Name} after {_options.RetryAttempts} attempts",
                    null,
                    stopwatch.ElapsedMilliseconds,
                    fileInfo);
                await progressMonitor.CompleteValidationAsync(resultInfo, combinedToken).ConfigureAwait(false);
                return resultInfo;
            }

            await progressMonitor.ReportProgressAsync(10, "File opened, checking size constraints", combinedToken).ConfigureAwait(false);

            // Check file size constraints
            if (stream.Size > _options.MaxFileSize)
            {
                stopwatch.Stop();
                ValidationResultInfo resultInfo = ValidationResultInfo.Failure(
                    ValidationResult.FileTooLarge,
                    $"File size ({stream.Size} bytes) exceeds the maximum allowed size ({_options.MaxFileSize} bytes)",
                    null,
                    stopwatch.ElapsedMilliseconds,
                    fileInfo);
                await progressMonitor.CompleteValidationAsync(resultInfo, combinedToken).ConfigureAwait(false);
                return resultInfo;
            }

            if (stream.Size < (ulong)(_expectedHeaderBytes.Length + _expectedFooterBytes.Length))
            {
                stopwatch.Stop();
                ValidationResultInfo resultInfo = ValidationResultInfo.Failure(
                    ValidationResult.InvalidFormat,
                    $"File is too small ({stream.Size} bytes) to contain valid header and footer",
                    null,
                    stopwatch.ElapsedMilliseconds,
                    fileInfo);
                await progressMonitor.CompleteValidationAsync(resultInfo, combinedToken).ConfigureAwait(false);
                return resultInfo;
            }

            await progressMonitor.ReportProgressAsync(20, "Size validation passed, beginning content validation", combinedToken).ConfigureAwait(false);

            // For small files (under 1MB), we can read the entire file at once
            if (stream.Size < 1024 * 1024)
            {
                await progressMonitor.ReportProgressAsync(30, "Small file detected, using simplified validation", combinedToken).ConfigureAwait(false);
                ValidationResultInfo result = await ValidateSmallFileWithDetailsAsync(stream, progressMonitor, combinedToken).ConfigureAwait(false);
                stopwatch.Stop();
                result = new ValidationResultInfo(
                    result.Result,
                    result.Message,
                    result.Exception,
                    stopwatch.ElapsedMilliseconds,
                    fileInfo);
                await progressMonitor.CompleteValidationAsync(result, combinedToken).ConfigureAwait(false);
                return result;
            }

            // For larger files, we'll use a more memory-efficient approach
            await progressMonitor.ReportProgressAsync(30, "Large file detected, using optimized validation", combinedToken).ConfigureAwait(false);
            ValidationResultInfo largeFileResult = await ValidateLargeFileWithDetailsAsync(stream, progressMonitor, combinedToken).ConfigureAwait(false);
            stopwatch.Stop();
            largeFileResult = new ValidationResultInfo(
                largeFileResult.Result,
                largeFileResult.Message,
                largeFileResult.Exception,
                stopwatch.ElapsedMilliseconds,
                fileInfo);
            await progressMonitor.CompleteValidationAsync(largeFileResult, combinedToken).ConfigureAwait(false);
            return largeFileResult;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            ValidationResultInfo result = timeoutCts.IsCancellationRequested
                ? ValidationResultInfo.Failure(
                    ValidationResult.ProcessingTimeExceeded,
                    $"Validation timed out after {_options.MaxProcessingTimeSeconds} seconds",
                    null,
                    stopwatch.ElapsedMilliseconds,
                    fileInfo)
                : ValidationResultInfo.Failure(
                    ValidationResult.Cancelled,
                    "Validation was cancelled by user request",
                    null,
                    stopwatch.ElapsedMilliseconds,
                    fileInfo);
            await progressMonitor.CompleteValidationAsync(result, CancellationToken.None).ConfigureAwait(false); // Use a non-cancelled token
            return result;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            stopwatch.Stop();
            ValidationResultInfo result = ValidationResultInfo.Failure(
                ValidationResult.AccessError,
                $"Error accessing file: {ex.Message}",
                ex,
                stopwatch.ElapsedMilliseconds,
                fileInfo);
            await progressMonitor.CompleteValidationAsync(result, CancellationToken.None).ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Unexpected error validating file {FilePath}", file.Path);
            ValidationResultInfo result = ValidationResultInfo.Failure(
                ValidationResult.UnknownError,
                $"Unexpected error: {ex.Message}",
                ex,
                stopwatch.ElapsedMilliseconds,
                fileInfo);
            await progressMonitor.CompleteValidationAsync(result, CancellationToken.None).ConfigureAwait(false);
            return result;
        }
        finally
        {
            timeoutCts.Cancel(); // Ensure the timeout is cancelled
        }
    }

    /// <summary>
    /// Validates the save file by checking its size and signature.
    /// </summary>
    /// <param name="file">The file to validate.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the file is valid.</returns>
    /// <remarks>This method is maintained for backward compatibility.</remarks>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="file"/> is <c>null</c>.</exception>
    public async Task<bool> IsValidSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        try
        {
            ValidationResult result = await ValidateSaveFileAsync(file, cancellationToken).ConfigureAwait(false);

            // Consider both Valid and ValidWithWarnings as successfully validated
            return result is ValidationResult.Valid or ValidationResult.ValidWithWarnings;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error in IsValidSaveFileAsync for {FilePath}", file.Path);

            // If we're bypassing validation failures, return true despite the error
            return _options.BypassValidationFailures;
        }
    }

    /// <summary>
    /// Validates a small file by reading it entirely into memory.
    /// </summary>
    private async Task<ValidationResultInfo> ValidateSmallFileWithDetailsAsync(
        IRandomAccessStreamWithContentType stream,
        IValidationProgressMonitor progressMonitor,
        CancellationToken cancellationToken)
    {
        await progressMonitor.ReportProgressAsync(40, "Reading small file content", cancellationToken).ConfigureAwait(false);

        byte[] encryptedData;
        try
        {
            // Create a buffer of the right size
            int streamSize = (int)Math.Min(stream.Size, int.MaxValue);
            encryptedData = new byte[streamSize];

            // Reset the stream position
            stream.Seek(0);

            // Read the data
            int bytesRead = await stream.AsStream().ReadAsync(encryptedData.AsMemory(0, streamSize), cancellationToken).ConfigureAwait(false);

            // If we didn't read the full size, resize the array
            if (bytesRead < streamSize)
            {
                Array.Resize(ref encryptedData, bytesRead);
                _logger?.LogWarning("Read {BytesRead} bytes from stream with size {StreamSize}",
                    bytesRead, streamSize);
            }

            if (bytesRead == 0)
            {
                return ValidationResultInfo.Failure(
                    ValidationResult.AccessError,
                    "No data read from file");
            }
        }
        catch (Exception ex)
        {
            return ValidationResultInfo.Failure(
                ValidationResult.AccessError,
                $"Error reading file content: {ex.Message}",
                ex);
        }

        await progressMonitor.ReportProgressAsync(60, "Decrypting file content", cancellationToken).ConfigureAwait(false);

        byte[] decryptedData;
        try
        {
            decryptedData = _cipherService.Transform(encryptedData, cancellationToken);
        }
        catch (Exception ex)
        {
            return ValidationResultInfo.Failure(
                ValidationResult.DecryptionError,
                $"Failed to decrypt file content: {ex.Message}",
                ex);
        }

        if (decryptedData.Length == 0)
        {
            return ValidationResultInfo.Failure(
                ValidationResult.DecryptionError,
                "Decryption produced empty content");
        }

        await progressMonitor.ReportProgressAsync(80, "Validating file structure", cancellationToken).ConfigureAwait(false);

        bool isValidSignature = HasValidSignature(decryptedData);
        if (!isValidSignature)
        {
            // If header/footer validation failed, log details but proceed if bypassing is enabled
            _logger?.LogWarning("File signature validation failed");

            // Log the first and last few bytes for debugging
            if (_logger != null)
            {
                int headerSampleSize = Math.Min(100, decryptedData.Length);
                int footerSampleSize = Math.Min(100, decryptedData.Length);

                try
                {
                    string headerSample = Encoding.UTF8.GetString(decryptedData, 0, headerSampleSize);
                    string footerSample = decryptedData.Length > footerSampleSize
                        ? Encoding.UTF8.GetString(decryptedData, decryptedData.Length - footerSampleSize, footerSampleSize)
                        : string.Empty;

                    _logger.LogDebug("File header sample: {HeaderSample}", headerSample);
                    _logger.LogDebug("File footer sample: {FooterSample}", footerSample);
                }
                catch
                {
                    // Ignore any encoding errors in the debug samples
                }
            }

            if (_options.BypassValidationFailures)
            {
                _logger?.LogWarning("Bypassing file signature validation failure");
                await progressMonitor.ReportProgressAsync(100, "Validation bypassed", cancellationToken).ConfigureAwait(false);
                return ValidationResultInfo.Success(0);
            }

            return ValidationResultInfo.Failure(
                ValidationResult.InvalidFormat,
                "The file does not have the expected XML structure (<root> and </root> tags)");
        }

        // If structure validation is enabled, do a more thorough check
        if (_options.ValidateStructure)
        {
            await progressMonitor.ReportProgressAsync(90, "Performing advanced structure validation", cancellationToken).ConfigureAwait(false);

            try
            {
                string decryptedContent = Encoding.UTF8.GetString(decryptedData);

                // For Sheltered 2 save files, we just do a basic XML well-formedness check
                // We don't parse the entire document as it's a heavy operation
                bool hasBalancedRootTags =
                    decryptedContent.StartsWith(_options.ExpectedHeader) &&
                    decryptedContent.EndsWith(_options.ExpectedFooter) &&
                    decryptedContent.IndexOf(_options.ExpectedHeader, 1, StringComparison.Ordinal) == -1 &&
                    decryptedContent.LastIndexOf(_options.ExpectedFooter, decryptedContent.Length - _options.ExpectedFooter.Length - 1, StringComparison.Ordinal) == -1;

                if (!hasBalancedRootTags)
                {
                    if (_options.BypassValidationFailures)
                    {
                        _logger?.LogWarning("File has unbalanced XML tags, but bypassing validation failure");
                        await progressMonitor.ReportProgressAsync(100, "Validation bypassed", cancellationToken).ConfigureAwait(false);
                        return ValidationResultInfo.Success(0);
                    }

                    return ValidationResultInfo.Failure(
                        ValidationResult.InvalidStructure,
                        "The file contains unbalanced root XML tags");
                }
            }
            catch (Exception ex)
            {
                if (_options.BypassValidationFailures)
                {
                    _logger?.LogWarning(ex, "Error during structure validation, but bypassing validation failure");
                    await progressMonitor.ReportProgressAsync(100, "Validation bypassed", cancellationToken).ConfigureAwait(false);
                    return ValidationResultInfo.Success(0);
                }

                return ValidationResultInfo.Failure(
                    ValidationResult.InvalidStructure,
                    $"Error validating file structure: {ex.Message}",
                    ex);
            }
        }

        await progressMonitor.ReportProgressAsync(100, "Validation completed successfully", cancellationToken).ConfigureAwait(false);
        return ValidationResultInfo.Success(0);
    }

    /// <summary>
    /// Validates a large file by reading only the header and footer parts.
    /// </summary>
    private async Task<ValidationResultInfo> ValidateLargeFileWithDetailsAsync(
        IRandomAccessStreamWithContentType stream,
        IValidationProgressMonitor progressMonitor,
        CancellationToken cancellationToken)
    {
        // Use a buffer size from options
        int bufferSize = _options.BufferSize;
        _ = _options.ChunkOverlapSize;

        // Check if the file is at least big enough to contain our header and footer
        if (stream.Size < (ulong)(bufferSize + _expectedHeaderBytes.Length + _expectedFooterBytes.Length))
        {
            await progressMonitor.ReportProgressAsync(40, "File size is at boundary, using small file validation", cancellationToken).ConfigureAwait(false);
            return await ValidateSmallFileWithDetailsAsync(stream, progressMonitor, cancellationToken).ConfigureAwait(false);
        }

        // Allocate buffers only once and return them after use
        byte[]? headerBuffer = null;
        byte[]? footerBuffer = null;

        try
        {
            // Read and validate header
            await progressMonitor.ReportProgressAsync(40, "Reading file header", cancellationToken).ConfigureAwait(false);

            // Get a buffer from pool 
            headerBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                // Reset stream position
                stream.Seek(0);

                // Read header chunk
                Stream headerStream = stream.AsStream();
                int bytesRead = await headerStream.ReadAsync(headerBuffer.AsMemory(0, bufferSize), cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    return ValidationResultInfo.Failure(
                        ValidationResult.AccessError,
                        "Unable to read file header: no bytes read");
                }

                await progressMonitor.ReportProgressAsync(50, "Decrypting file header", cancellationToken).ConfigureAwait(false);

                // Decrypt the header
                byte[] decryptedHeader;
                try
                {
                    // Only transform the bytes we actually read
                    decryptedHeader = _cipherService.Transform(headerBuffer.AsSpan(0, bytesRead).ToArray(), cancellationToken);
                }
                catch (Exception ex)
                {
                    return ValidationResultInfo.Failure(
                        ValidationResult.DecryptionError,
                        $"Failed to decrypt file header: {ex.Message}",
                        ex);
                }

                // Check if the header contains the expected XML opening tag
                if (!StartsWithHeader(decryptedHeader))
                {
                    _logger?.LogWarning("Header validation failed, expected: {Expected}",
                        _options.ExpectedHeader);

                    // Log what we actually found
                    if (decryptedHeader.Length > 0 && _logger != null)
                    {
                        try
                        {
                            string foundHeader = Encoding.UTF8.GetString(
                                decryptedHeader, 0, Math.Min(100, decryptedHeader.Length));
                            _logger.LogDebug("Found header start: {Actual}", foundHeader);
                        }
                        catch
                        {
                            // Ignore encoding errors
                        }
                    }

                    if (_options.BypassValidationFailures)
                    {
                        _logger?.LogWarning("Bypassing header validation failure");
                        await progressMonitor.ReportProgressAsync(100, "Header validation bypassed", cancellationToken).ConfigureAwait(false);
                        return ValidationResultInfo.Success(0);
                    }

                    return ValidationResultInfo.Failure(
                        ValidationResult.InvalidFormat,
                        "The file header does not contain the expected XML root tag");
                }

                // Read and validate footer in multiple attempts
                await progressMonitor.ReportProgressAsync(70, "Reading file footer", cancellationToken).ConfigureAwait(false);

                // Get a buffer from pool for footer
                footerBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);

                // Try reading from different positions near the end to find the footer
                // This improves robustness against potential formatting issues or padding
                bool footerFound = false;

                // Start from the end, and try increasingly earlier positions if needed
                for (int attempt = 0; attempt < 3 && !footerFound; attempt++)
                {
                    try
                    {
                        // Calculate position to start reading, moving backward each attempt
                        long position = Math.Max(0, (long)stream.Size - bufferSize - attempt * (bufferSize / 2));
                        stream.Seek((ulong)position);

                        Stream footerStream = stream.AsStream();
                        int bytesRead2 = await footerStream.ReadAsync(footerBuffer.AsMemory(0, bufferSize), cancellationToken).ConfigureAwait(false);

                        if (bytesRead2 == 0)
                        {
                            _logger?.LogWarning("No bytes read from footer chunk on attempt {Attempt}", attempt + 1);
                            continue;
                        }

                        await progressMonitor.ReportProgressAsync(80 + attempt * 5,
                            $"Decrypting file footer (attempt {attempt + 1}/3)", cancellationToken).ConfigureAwait(false);

                        byte[] decryptedFooter;
                        try
                        {
                            // Only transform the bytes we actually read
                            decryptedFooter = _cipherService.Transform(footerBuffer.AsSpan(0, bytesRead2).ToArray(), cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Error decrypting footer chunk on attempt {Attempt}", attempt + 1);
                            continue;
                        }

                        // Check if this chunk contains the footer tag
                        if (EndsWithFooter(decryptedFooter))
                        {
                            footerFound = true;
                            _logger?.LogDebug("Footer found in attempt {Attempt}", attempt + 1);
                            break;
                        }

                        _logger?.LogDebug("Footer not found in attempt {Attempt}, will try at earlier position",
                            attempt + 1);
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger?.LogWarning(ex, "Error processing footer chunk on attempt {Attempt}", attempt + 1);
                        // Continue to the next attempt
                    }
                }

                if (!footerFound)
                {
                    _logger?.LogWarning("Footer not found in any position attempts");

                    if (_options.BypassValidationFailures)
                    {
                        _logger?.LogWarning("Bypassing footer validation failure");
                        await progressMonitor.ReportProgressAsync(100, "Footer validation bypassed", cancellationToken).ConfigureAwait(false);
                        return ValidationResultInfo.Success(0);
                    }

                    return ValidationResultInfo.Failure(
                        ValidationResult.InvalidFormat,
                        "The file footer does not contain the expected XML closing tag");
                }

                await progressMonitor.ReportProgressAsync(100, "Validation completed successfully", cancellationToken);
                return ValidationResultInfo.Success(0);
            }
            catch (EndOfStreamException ex)
            {
                if (_options.BypassValidationFailures)
                {
                    _logger?.LogWarning(ex, "EndOfStreamException occurred, but bypassing validation failure");
                    return ValidationResultInfo.Success(0);
                }

                return ValidationResultInfo.Failure(
                    ValidationResult.AccessError,
                    $"Unable to read beyond the end of the stream: {ex.Message}",
                    ex);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                if (_options.BypassValidationFailures)
                {
                    _logger?.LogWarning(ex, "Error during large file validation, but bypassing validation failure");
                    return ValidationResultInfo.Success(0);
                }

                return ValidationResultInfo.Failure(
                    ValidationResult.AccessError,
                    $"Error accessing file: {ex.Message}",
                    ex);
            }
        }
        finally
        {
            // Always return buffers to the pool
            if (headerBuffer != null)
                ArrayPool<byte>.Shared.Return(headerBuffer);

            if (footerBuffer != null)
                ArrayPool<byte>.Shared.Return(footerBuffer);
        }
    }

    /// <summary>
    /// Ensures that the validator is in a valid state.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the validator is in an invalid state.</exception>
    private void EnsureValidState()
    {
        if (_expectedHeaderBytes == null || _expectedHeaderBytes.Length == 0)
            throw new InvalidOperationException("Expected header bytes are not properly initialized");

        if (_expectedFooterBytes == null || _expectedFooterBytes.Length == 0)
            throw new InvalidOperationException("Expected footer bytes are not properly initialized");

        if (_options.MaxProcessingTimeSeconds <= 0)
            throw new InvalidOperationException("Maximum processing time must be greater than zero");
    }

    /// <summary>
    /// Retries an asynchronous operation several times before giving up.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to retry.</param>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <param name="delayMilliseconds">The delay between retries in milliseconds.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the operation if successful, or default(T) if all retries failed.</returns>
    private async Task<T?> RetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries,
        int delayMilliseconds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);

        if (maxRetries <= 0)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Operation failed with no retries permitted");
                return default;
            }
        }

        Exception? lastException = null;
        // Start with attempt = 0 (first try), and go up to maxRetries (inclusive)
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                // If this isn't the first attempt, log that we're retrying
                if (attempt > 0)
                {
                    _logger?.LogDebug("Retry attempt {Attempt}/{MaxRetries}", attempt, maxRetries);
                }

                return await operation();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Immediately propagate cancellation
                _logger?.LogInformation("Operation cancelled during retry {Attempt}", attempt);
                throw;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                lastException = ex;

                // Only retry for certain types of exceptions
                bool shouldRetry = ex is IOException or UnauthorizedAccessException or TimeoutException;

                if (!shouldRetry)
                {
                    _logger?.LogWarning(ex, "Exception type {ExceptionType} not eligible for retry", ex.GetType().Name);
                    throw; // Rethrow exceptions we don't want to retry
                }

                _logger?.LogWarning(ex, "Operation failed (attempt {Attempt}/{MaxRetries}), retrying after {Delay}ms",
                    attempt + 1, maxRetries, delayMilliseconds);

                try
                {
                    if (delayMilliseconds > 0)
                    {
                        using CancellationTokenSource timeoutCts = new(delayMilliseconds * 2);
                        using CancellationTokenSource linkedCts =
                            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                        await Task.Delay(delayMilliseconds, linkedCts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger?.LogInformation("Operation cancelled during retry delay");
                        throw;
                    }

                    // Delay timed out but operation wasn't cancelled - continue with retry
                }

                // Increase delay for exponential backoff (up to 5 seconds)
                delayMilliseconds = Math.Min(delayMilliseconds * 2, 5000);
            }
            catch (Exception ex)
            {
                // This is either the last retry attempt or an exception we don't want to retry
                _logger?.LogError(ex, "Operation failed permanently after {Attempt} attempt(s)", attempt + 1);
                throw; // Propagate the exception
            }
        }

        // If we get here, we've exhausted all retries
        _logger?.LogError(lastException, "All {MaxRetries} retry attempts failed", maxRetries);

        return lastException != null ? throw new IOException($"Operation failed after {maxRetries} retry attempts", lastException) : default;
    }

    /// <summary>
    /// Checks if the given decrypted data contains a valid XML signature.
    /// </summary>
    private bool HasValidSignature(byte[] decryptedData)
    {
        try
        {
            string text = Encoding.UTF8.GetString(decryptedData);
            return HasValidSignature(text);
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error in HasValidSignature for byte array");
            return false;
        }
    }

    /// <summary>
    /// Checks if the decrypted text has the expected header and footer.
    /// </summary>
    private bool HasValidSignature(string decryptedText)
    {
        // Trim leading/trailing whitespace for more robust checking
        string trimmed = decryptedText.Trim();

        if (string.IsNullOrEmpty(trimmed))
        {
            _logger?.LogWarning("Empty content in HasValidSignature");
            return false;
        }

        try
        {
            bool startsWithHeader = trimmed.StartsWith(_options.ExpectedHeader, StringComparison.Ordinal);
            bool endsWithFooter = trimmed.EndsWith(_options.ExpectedFooter, StringComparison.Ordinal);

            if (!startsWithHeader)
                _logger?.LogWarning("Content does not start with expected header: {ExpectedHeader}",
                    _options.ExpectedHeader);

            if (!endsWithFooter)
                _logger?.LogWarning("Content does not end with expected footer: {ExpectedFooter}",
                    _options.ExpectedFooter);

            return startsWithHeader && endsWithFooter;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error in HasValidSignature string check");
            return false;
        }
    }

    /// <summary>
    /// Checks if the data starts with the expected header.
    /// </summary>
    private bool StartsWithHeader(byte[] data)
    {
        if (data == null)
        {
            _logger?.LogWarning("Null data in StartsWithHeader");
            return false;
        }

        if (data.Length < _expectedHeaderBytes.Length)
        {
            _logger?.LogWarning("Data too short in StartsWithHeader: {Length} bytes, need at least {HeaderLength} bytes",
                data.Length, _expectedHeaderBytes.Length);
            return false;
        }

        // First try exact matching
        bool exactMatch = true;
        for (int i = 0; i < _expectedHeaderBytes.Length; i++)
        {
            if (data[i] != _expectedHeaderBytes[i])
            {
                exactMatch = false;
                break;
            }
        }

        if (exactMatch)
            return true;

        // If exact match fails, try to find the header with whitespace tolerance
        try
        {
            // Convert a reasonable chunk to string for more flexible checking
            int sampleSize = Math.Min(256, data.Length);
            string headerSample = Encoding.UTF8.GetString(data, 0, sampleSize);

            // Trim and check if it starts with the expected header
            string trimmed = headerSample.TrimStart();
            if (trimmed.StartsWith(_options.ExpectedHeader, StringComparison.Ordinal))
            {
                _logger?.LogDebug("Found header with whitespace trimming");
                return true;
            }

            // If it contains the header anywhere near the start, this could be valid
            // but with some unexpected content before the XML starts
            if (headerSample.Contains(_options.ExpectedHeader))
            {
                _logger?.LogWarning("Header found but not at the start of file");
                // This is a compromise - consider it valid but log a warning
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error in string-based header detection");
        }

        return false;
    }

    /// <summary>
    /// Checks if the data ends with the expected footer.
    /// </summary>
    private bool EndsWithFooter(byte[] data)
    {
        if (data == null)
        {
            _logger?.LogWarning("Null data in EndsWithFooter");
            return false;
        }

        if (data.Length < _expectedFooterBytes.Length)
        {
            _logger?.LogWarning("Data too short in EndsWithFooter: {Length} bytes, need at least {FooterLength} bytes",
                data.Length, _expectedFooterBytes.Length);
            return false;
        }

        // First try matching at the very end of the data
        bool exactEndMatch = true;
        for (int i = 0; i < _expectedFooterBytes.Length; i++)
        {
            if (data[data.Length - _expectedFooterBytes.Length + i] != _expectedFooterBytes[i])
            {
                exactEndMatch = false;
                break;
            }
        }

        if (exactEndMatch)
            return true;

        // Search for the last occurrence of the footer bytes within the data
        // Start searching from the end and move backward
        for (int i = data.Length - _expectedFooterBytes.Length; i >= 0; i--)
        {
            bool found = true;
            for (int j = 0; j < _expectedFooterBytes.Length; j++)
            {
                if (i + j >= data.Length || data[i + j] != _expectedFooterBytes[j])
                {
                    found = false;
                    break;
                }
            }

            if (found)
            {
                // If we found the footer but it's not at the very end, log it
                if (i + _expectedFooterBytes.Length < data.Length)
                {
                    _logger?.LogDebug("Footer found at position {Position} (not at the very end)", i);
                }
                return true;
            }
        }

        // If the exact search failed, try a more flexible string-based search
        try
        {
            // Convert a reasonable chunk to string for more flexible checking
            int sampleSize = Math.Min(512, data.Length);
            int startIndex = Math.Max(0, data.Length - sampleSize);
            string footerSample = Encoding.UTF8.GetString(data, startIndex, data.Length - startIndex);

            // Trim and check if it ends with the expected footer
            string trimmed = footerSample.TrimEnd();
            if (trimmed.EndsWith(_options.ExpectedFooter, StringComparison.Ordinal))
            {
                _logger?.LogDebug("Found footer with whitespace trimming");
                return true;
            }

            // If it contains the footer anywhere near the end, this could be valid
            if (footerSample.Contains(_options.ExpectedFooter))
            {
                _logger?.LogWarning("Footer found but not at the end of file");
                // This is a compromise - consider it valid but log a warning
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error in string-based footer detection");
        }

        return false;
    }
}