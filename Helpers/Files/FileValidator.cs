using Sheltered2SaveEditor.Helpers.Cipher;
using System.Buffers;
using System.Diagnostics;
using System.Text;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Provides methods to validate game save files.
/// </summary>
internal sealed class FileValidator : IFileValidator
{
    private readonly IXorCipherService _cipherService;
    private readonly FileValidationOptions _options;
    private readonly FileSignatureValidator _signatureValidator;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileValidator"/> class with default options.
    /// </summary>
    /// <param name="cipherService">The service used for encryption and decryption.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="cipherService"/> is <c>null</c>.</exception>
    internal FileValidator(IXorCipherService cipherService)
    {
        _cipherService = cipherService ?? throw new ArgumentNullException(nameof(cipherService));
        _options = new FileValidationOptions();
        _signatureValidator = new FileSignatureValidator(_options.ExpectedHeader, _options.ExpectedFooter);
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

    /// <inheritdoc/>
    public bool HasValidExtension(StorageFile file) => IsValidDatFile(file);

    /// <inheritdoc/>
    public async Task<ValidationResult> ValidateSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        try
        {
            ValidationResultInfo result = await ValidateSaveFileWithDetailsAsync(file, NullValidationProgressMonitor.Instance, cancellationToken).ConfigureAwait(false);

            // If we're bypassing validation failures and we have an invalid format or structure,
            // return ValidWithWarnings instead of the actual error
            return _options.BypassValidationFailures &&
                (result.Result == ValidationResult.InvalidFormat ||
                 result.Result == ValidationResult.InvalidStructure ||
                 result.Result == ValidationResult.DecryptionError)
                ? ValidationResult.ValidWithWarnings
                : result.Result;
        }
        catch (Exception)
        {
            // Return Valid or ValidWithWarnings if bypassing failures, otherwise throw
            if (_options.BypassValidationFailures)
            {
                return ValidationResult.ValidWithWarnings;
            }

            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<ValidationResultInfo> ValidateSaveFileWithDetailsAsync(
        StorageFile file,
        IValidationProgressMonitor progressMonitor,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        ArgumentNullException.ThrowIfNull(progressMonitor);

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
            await timeoutCts.CancelAsync().ConfigureAwait(false); // Ensure the timeout is cancelled
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsValidSaveFileAsync(StorageFile file, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        try
        {
            ValidationResult result = await ValidateSaveFileAsync(file, cancellationToken).ConfigureAwait(false);

            // Consider both Valid and ValidWithWarnings as successfully validated
            return result is ValidationResult.Valid or ValidationResult.ValidWithWarnings;
        }
        catch (Exception)
        {
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

        bool isValidSignature = _signatureValidator.HasValidSignature(decryptedData);
        if (!isValidSignature)
        {
            if (_options.BypassValidationFailures)
            {
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

        // Check if the file is at least big enough to contain our header and footer
        if (stream.Size < (ulong)(bufferSize + Encoding.UTF8.GetByteCount(_options.ExpectedHeader) + Encoding.UTF8.GetByteCount(_options.ExpectedFooter)))
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
                if (!_signatureValidator.StartsWithHeader(decryptedHeader))
                {
                    if (_options.BypassValidationFailures)
                    {
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
                        catch
                        {
                            continue;
                        }

                        // Check if this chunk contains the footer tag
                        if (_signatureValidator.EndsWithFooter(decryptedFooter))
                        {
                            footerFound = true;
                            break;
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        // Continue to the next attempt
                    }
                }

                if (!footerFound)
                {
                    if (_options.BypassValidationFailures)
                    {
                        await progressMonitor.ReportProgressAsync(100, "Footer validation bypassed", cancellationToken).ConfigureAwait(false);
                        return ValidationResultInfo.Success(0);
                    }

                    return ValidationResultInfo.Failure(
                        ValidationResult.InvalidFormat,
                        "The file footer does not contain the expected XML closing tag");
                }

                await progressMonitor.ReportProgressAsync(100, "Validation completed successfully", cancellationToken).ConfigureAwait(false);
                return ValidationResultInfo.Success(0);
            }
            catch (EndOfStreamException ex)
            {
                return _options.BypassValidationFailures
                    ? ValidationResultInfo.Success(0)
                    : ValidationResultInfo.Failure(
                    ValidationResult.AccessError,
                    $"Unable to read beyond the end of the stream: {ex.Message}",
                    ex);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return _options.BypassValidationFailures
                    ? ValidationResultInfo.Success(0)
                    : ValidationResultInfo.Failure(
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
    /// Retries an asynchronous operation several times before giving up.
    /// </summary>
    /// <typeparam name="T">The type of result returned by the operation.</typeparam>
    /// <param name="operation">The asynchronous operation to retry.</param>
    /// <param name="maxRetries">The maximum number of retry attempts.</param>
    /// <param name="delayMilliseconds">The delay between retries in milliseconds.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>The result of the operation if successful, or default(T) if all retries failed.</returns>
    private static async Task<T?> RetryAsync<T>(
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
                return await operation().ConfigureAwait(false);
            }
            catch
            {
                return default;
            }
        }

        Exception? lastException = null;
        // Start with attempt = 0 (first try), and go up to maxRetries (inclusive)
        for (int attempt = 0; attempt <= maxRetries; attempt++)
        {
            try
            {
                return await operation().ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // Immediately propagate cancellation
                throw;
            }
            catch (Exception ex) when (attempt < maxRetries)
            {
                lastException = ex;

                // Only retry for certain types of exceptions
                bool shouldRetry = ex is IOException or UnauthorizedAccessException or TimeoutException;

                if (!shouldRetry)
                {
                    throw; // Rethrow exceptions we don't want to retry
                }

                try
                {
                    if (delayMilliseconds > 0)
                    {
                        // Use exponential backoff for the delay
                        int currentDelay = delayMilliseconds * (int)Math.Pow(2, attempt);
                        using CancellationTokenSource timeoutCts = new(currentDelay * 2);
                        using CancellationTokenSource linkedCts =
                            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                        await Task.Delay(currentDelay, linkedCts.Token).ConfigureAwait(false);
                    }
                }
                catch (OperationCanceledException)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw;
                    }
                }
            }
            catch
            {
                // This is either the last retry attempt or an exception we don't want to retry
                throw; // Propagate the exception
            }
        }

        // If we get here, we've exhausted all retries
        return lastException != null ? throw new IOException($"Operation failed after {maxRetries} retry attempts", lastException) : default;
    }
}