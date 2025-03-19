using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Infrastructure.Encryption;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Sheltered2SaveEditor.Infrastructure.Files;

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
    public bool EnableParallelProcessing { get; init; } = false;

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
}

/// <summary>
/// Represents the result of a file validation operation with detailed information.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ValidationResultInfo"/> class.
/// </remarks>
/// <param name="result">The validation result.</param>
/// <param name="message">A detailed message describing the result.</param>
/// <param name="exception">The exception that occurred, if any.</param>
/// <param name="elapsedMilliseconds">The time taken for validation.</param>
/// <param name="fileInfo">Information about the validated file.</param>
public sealed class ValidationResultInfo(
    ValidationResult result,
    string message,
    Exception? exception = null,
    long elapsedMilliseconds = 0,
    FileInfo? fileInfo = null)
{
    /// <summary>
    /// Gets the overall validation result.
    /// </summary>
    public ValidationResult Result { get; } = result;

    /// <summary>
    /// Gets a detailed message describing the validation result.
    /// </summary>
    public string Message { get; } = message;

    /// <summary>
    /// Gets the exception that occurred during validation, if any.
    /// </summary>
    public Exception? Exception { get; } = exception;

    /// <summary>
    /// Gets the time taken for the validation operation in milliseconds.
    /// </summary>
    public long ElapsedMilliseconds { get; } = elapsedMilliseconds;

    /// <summary>
    /// Gets information about the file that was validated.
    /// </summary>
    public FileInfo? FileInfo { get; } = fileInfo;

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
    FileTemporarilyUnavailable
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
public sealed class FileValidator
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
    public FileValidator(IXorCipherService cipherService, ILogger? logger = null)
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
    public Task<ValidationResult> ValidateSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default) => ValidateSaveFileWithDetailsAsync(file, NullValidationProgressMonitor.Instance, cancellationToken)
            .ContinueWith(t => t.Result.Result, cancellationToken);

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
            await progressMonitor.StartValidationAsync(file.Name, fileInfo != null ? (ulong)fileInfo.Length : 0UL, combinedToken);

            // Start with retries for common I/O operations
            using IRandomAccessStreamWithContentType? stream = await RetryAsync(
                async () => await file.OpenReadAsync(),
                _options.RetryAttempts,
                _options.RetryDelayMilliseconds,
                combinedToken);

            if (stream == null)
            {
                stopwatch.Stop();
                ValidationResultInfo resultInfo = ValidationResultInfo.Failure(
                    ValidationResult.AccessError,
                    $"Failed to open file {file.Name} after {_options.RetryAttempts} attempts",
                    null,
                    stopwatch.ElapsedMilliseconds,
                    fileInfo);
                await progressMonitor.CompleteValidationAsync(resultInfo, combinedToken);
                return resultInfo;
            }

            await progressMonitor.ReportProgressAsync(10, "File opened, checking size constraints", combinedToken);

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
                await progressMonitor.CompleteValidationAsync(resultInfo, combinedToken);
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
                await progressMonitor.CompleteValidationAsync(resultInfo, combinedToken);
                return resultInfo;
            }

            await progressMonitor.ReportProgressAsync(20, "Size validation passed, beginning content validation", combinedToken);

            // For small files (under 1MB), we can read the entire file at once
            if (stream.Size < 1024 * 1024)
            {
                await progressMonitor.ReportProgressAsync(30, "Small file detected, using simplified validation", combinedToken);
                ValidationResultInfo result = await ValidateSmallFileWithDetailsAsync(stream, progressMonitor, combinedToken);
                stopwatch.Stop();
                result = new ValidationResultInfo(
                    result.Result,
                    result.Message,
                    result.Exception,
                    stopwatch.ElapsedMilliseconds,
                    fileInfo);
                await progressMonitor.CompleteValidationAsync(result, combinedToken);
                return result;
            }

            // For larger files, we'll use a more memory-efficient approach
            await progressMonitor.ReportProgressAsync(30, "Large file detected, using optimized validation", combinedToken);
            ValidationResultInfo largeFileResult = await ValidateLargeFileWithDetailsAsync(stream, progressMonitor, combinedToken);
            stopwatch.Stop();
            largeFileResult = new ValidationResultInfo(
                largeFileResult.Result,
                largeFileResult.Message,
                largeFileResult.Exception,
                stopwatch.ElapsedMilliseconds,
                fileInfo);
            await progressMonitor.CompleteValidationAsync(largeFileResult, combinedToken);
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
            await progressMonitor.CompleteValidationAsync(result, CancellationToken.None); // Use a non-cancelled token
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
            await progressMonitor.CompleteValidationAsync(result, CancellationToken.None);
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
            await progressMonitor.CompleteValidationAsync(result, CancellationToken.None);
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
        ValidationResult result = await ValidateSaveFileAsync(file, cancellationToken);
        return result == ValidationResult.Valid;
    }

    /// <summary>
    /// Validates a small file by reading it entirely into memory.
    /// </summary>
    private async Task<ValidationResultInfo> ValidateSmallFileWithDetailsAsync(
        IRandomAccessStreamWithContentType stream,
        IValidationProgressMonitor progressMonitor,
        CancellationToken cancellationToken)
    {
        await progressMonitor.ReportProgressAsync(40, "Reading small file content", cancellationToken);

        byte[] encryptedData = new byte[stream.Size];
        await stream.AsStream().ReadExactlyAsync(encryptedData.AsMemory(0, (int)stream.Size), cancellationToken);

        await progressMonitor.ReportProgressAsync(60, "Decrypting file content", cancellationToken);

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

        await progressMonitor.ReportProgressAsync(80, "Validating file structure", cancellationToken);

        bool isValidSignature = HasValidSignature(decryptedData);
        if (!isValidSignature)
            return ValidationResultInfo.Failure(
                ValidationResult.InvalidFormat,
                "The file does not have the expected XML structure (<root> and </root> tags)");

        // If structure validation is enabled, do a more thorough check
        if (_options.ValidateStructure)
        {
            await progressMonitor.ReportProgressAsync(90, "Performing advanced structure validation", cancellationToken);

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
                    return ValidationResultInfo.Failure(
                        ValidationResult.InvalidStructure,
                        "The file contains unbalanced root XML tags");
            }
            catch (Exception ex)
            {
                return ValidationResultInfo.Failure(
                    ValidationResult.InvalidStructure,
                    $"Error validating file structure: {ex.Message}",
                    ex);
            }
        }

        await progressMonitor.ReportProgressAsync(100, "Validation completed successfully", cancellationToken);
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
        // We only need to check header and footer for basic validation
        const int bufferSize = 4096; // 4KB buffer is a good compromise

        // Check if the file is at least big enough to contain our header and footer with some content
        if (stream.Size < bufferSize * 2)
        {
            await progressMonitor.ReportProgressAsync(40, "File size is at boundary, using small file validation", cancellationToken);
            return await ValidateSmallFileWithDetailsAsync(stream, progressMonitor, cancellationToken);
        }

        // Read and validate header
        await progressMonitor.ReportProgressAsync(40, "Reading file header", cancellationToken);
        byte[] headerBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            stream.Seek(0);
            _ = await stream.AsStream().ReadAsync(headerBuffer.AsMemory(0, bufferSize), cancellationToken);

            await progressMonitor.ReportProgressAsync(50, "Decrypting file header", cancellationToken);
            byte[] decryptedHeader;
            try
            {
                decryptedHeader = _cipherService.Transform(headerBuffer.AsSpan(0, bufferSize).ToArray(), cancellationToken);
            }
            catch (Exception ex)
            {
                return ValidationResultInfo.Failure(
                    ValidationResult.DecryptionError,
                    $"Failed to decrypt file header: {ex.Message}",
                    ex);
            }

            if (!StartsWithHeader(decryptedHeader))
                return ValidationResultInfo.Failure(
                    ValidationResult.InvalidFormat,
                    "The file header does not contain the expected XML root tag");

            // Read and validate footer
            await progressMonitor.ReportProgressAsync(70, "Reading file footer", cancellationToken);
            byte[] footerBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            try
            {
                long position = Math.Max(0, (long)stream.Size - bufferSize);
                stream.Seek((ulong)position);
                _ = await stream.AsStream().ReadAsync(footerBuffer.AsMemory(0, bufferSize), cancellationToken);

                await progressMonitor.ReportProgressAsync(80, "Decrypting file footer", cancellationToken);
                byte[] decryptedFooter;
                try
                {
                    decryptedFooter = _cipherService.Transform(footerBuffer.AsSpan(0, bufferSize).ToArray(), cancellationToken);
                }
                catch (Exception ex)
                {
                    return ValidationResultInfo.Failure(
                        ValidationResult.DecryptionError,
                        $"Failed to decrypt file footer: {ex.Message}",
                        ex);
                }

                if (!EndsWithFooter(decryptedFooter))
                    return ValidationResultInfo.Failure(
                        ValidationResult.InvalidFormat,
                        "The file footer does not contain the expected XML closing tag");

                await progressMonitor.ReportProgressAsync(100, "Validation completed successfully", cancellationToken);
                return ValidationResultInfo.Success(0);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(footerBuffer);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(headerBuffer);
        }
    }

    /// <summary>
    /// Checks if the given decrypted data contains a valid XML signature.
    /// </summary>
    private bool HasValidSignature(byte[] decryptedData) => HasValidSignature(Encoding.UTF8.GetString(decryptedData));

    /// <summary>
    /// Checks if the decrypted text has the expected header and footer.
    /// </summary>
    private bool HasValidSignature(string decryptedText) =>
        decryptedText.StartsWith(_options.ExpectedHeader) &&
        decryptedText.EndsWith(_options.ExpectedFooter);

    /// <summary>
    /// Checks if the data starts with the expected header.
    /// </summary>
    private bool StartsWithHeader(byte[] data)
    {
        if (data.Length < _expectedHeaderBytes.Length)
            return false;

        for (int i = 0; i < _expectedHeaderBytes.Length; i++)
            if (data[i] != _expectedHeaderBytes[i])
                return false;

        return true;
    }

    /// <summary>
    /// Checks if the data ends with the expected footer.
    /// </summary>
    private bool EndsWithFooter(byte[] data)
    {
        if (data.Length < _expectedFooterBytes.Length)
            return false;

        // Search for the last occurrence of the footer bytes within the data
        for (int i = data.Length - _expectedFooterBytes.Length; i >= 0; i--)
        {
            bool found = true;
            for (int j = 0; j < _expectedFooterBytes.Length; j++)
                if (data[i + j] != _expectedFooterBytes[j])
                {
                    found = false;
                    break;
                }

            if (found)
                return true;
        }

        return false;
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
            try
            {
                return await operation();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Operation failed with no retries permitted");
                return default;
            }

        for (int i = 0; i <= maxRetries; i++)
            try
            {
                return await operation();
            }
            catch (Exception ex) when (i < maxRetries &&
                                      (ex is IOException || ex is UnauthorizedAccessException) &&
                                      !cancellationToken.IsCancellationRequested)
            {
                _logger?.LogWarning(ex, "Operation failed (attempt {Attempt}/{MaxRetries}), retrying after {Delay}ms",
                    i + 1, maxRetries, delayMilliseconds);

                if (delayMilliseconds > 0)
                    await Task.Delay(delayMilliseconds, cancellationToken);

                // Increase delay for exponential backoff (up to 5 seconds)
                delayMilliseconds = Math.Min(delayMilliseconds * 2, 5000);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Operation failed permanently after {Attempt} attempt(s)", i + 1);
                return default;
            }

        return default;
    }
}