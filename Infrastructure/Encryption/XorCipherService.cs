using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sheltered2SaveEditor.Infrastructure.Encryption;

/// <summary>
/// Provides methods for encrypting and decrypting data using a configurable XOR key.
/// </summary>
/// <remarks>
/// This XOR cipher is used only for obfuscation and is not suitable for secure encryption.
/// The same key is used for both encryption and decryption.
/// </remarks>
public sealed class XorCipherService : IXorCipherService
{
    /// <summary>
    /// The key used to XOR the Sheltered 2 game save files.
    /// </summary>
    /// <remarks>
    /// This key is used for simple obfuscation and must not be changed to maintain compatibility with the game.
    /// </remarks>
    private static readonly ImmutableArray<byte> XorKey = ImmutableArray.Create<byte>(
        0xAC, 0x73, 0xFE, 0xF2, 0xAA, 0xBA, 0x6D, 0xAB,
        0x30, 0x3A, 0x8B, 0xA7, 0xDE, 0x0D, 0x15, 0x21, 0x4A
    );

    /// <inheritdoc/>
    public XorCipherOptions Options { get; }

    private readonly ILogger<XorCipherService>? _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="XorCipherService"/> class with default options.
    /// </summary>
    /// <param name="logger">Optional logger for monitoring operations.</param>
    public XorCipherService(ILogger<XorCipherService>? logger = null)
        : this(new XorCipherOptions(), logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XorCipherService"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options for configuring the cipher service.</param>
    /// <param name="logger">Optional logger for monitoring operations.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <c>null</c>.</exception>
    public XorCipherService(XorCipherOptions options, ILogger<XorCipherService>? logger = null)
    {
        Options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        _logger?.LogDebug("XorCipherService initialized with buffer size: {BufferSize}", Options.BufferSize);
    }

    /// <inheritdoc/>
    public async Task<byte[]> LoadAndXorAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        // Check if file exists
        if (!File.Exists(filePath))
        {
            _logger?.LogError("File not found: {FilePath}", filePath);
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        FileInfo fileInfo = new(filePath);
        _logger?.LogDebug("Loading file: {FilePath}, Size: {FileSize} bytes", filePath, fileInfo.Length);

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            // Choose the appropriate method based on file size and options
            if (Options.UseBufferedIO && fileInfo.Length > Options.BufferedIOThreshold)
            {
                _logger?.LogDebug("Using buffered I/O for large file: {FilePath}", filePath);
                return await LoadAndXorLargeFileAsync(filePath, cancellationToken);
            }
            else
            {
                // For smaller files, read the whole file at once
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken)
                    .ConfigureAwait(false);

                // Transform (decrypt) the file bytes using the XOR cipher
                _logger?.LogDebug("Transforming file content, Size: {Size} bytes", fileBytes.Length);
                byte[] result = Transform(fileBytes, cancellationToken);

                stopwatch.Stop();
                _logger?.LogInformation("File loaded and transformed successfully: {FilePath}, Time: {ElapsedMs}ms",
                    filePath, stopwatch.ElapsedMilliseconds);

                return result;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Error loading or transforming file: {FilePath}, Time: {ElapsedMs}ms",
                filePath, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> LoadAndXorChunkAsync(string filePath, long offset, int count = -1, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (offset < 0)
        {
            _logger?.LogError("Invalid offset: {Offset}", offset);
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
        }

        if (count < -1)
        {
            _logger?.LogError("Invalid count: {Count}", count);
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be -1 or a non-negative value.");
        }

        // Check if file exists
        if (!File.Exists(filePath))
        {
            _logger?.LogError("File not found: {FilePath}", filePath);
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        _logger?.LogDebug("Loading file chunk: {FilePath}, Offset: {Offset}, Count: {Count}",
            filePath, offset, count == -1 ? "to end" : count.ToString());

        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            using FileStream fileStream = new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                Options.BufferSize,
                FileOptions.Asynchronous);

            // Set the position to the requested offset
            _ = fileStream.Seek(offset, SeekOrigin.Begin);

            // Determine actual count
            int actualCount = count;
            if (count == -1 || count > fileStream.Length - offset)
            {
                actualCount = (int)(fileStream.Length - offset);
                _logger?.LogDebug("Adjusted count to {ActualCount} bytes based on file size", actualCount);
            }

            byte[] buffer = new byte[actualCount];
            int bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, actualCount), cancellationToken);

            if (bytesRead < actualCount)
            {
                // Resize buffer if we read fewer bytes than expected
                _logger?.LogDebug("Resizing buffer: read {BytesRead} bytes instead of expected {ExpectedBytes}",
                    bytesRead, actualCount);
                Array.Resize(ref buffer, bytesRead);
            }

            byte[] result = Transform(buffer, cancellationToken);

            stopwatch.Stop();
            _logger?.LogInformation("File chunk loaded and transformed: {FilePath}, Size: {Size} bytes, Time: {ElapsedMs}ms",
                filePath, result.Length, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Error loading or transforming file chunk: {FilePath}, Time: {ElapsedMs}ms",
                filePath, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveAndXorAsync(string filePath, byte[] content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(content);

        _logger?.LogDebug("Saving to file: {FilePath}, Content size: {Size} bytes", filePath, content.Length);
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            // Ensure the directory exists
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                _logger?.LogDebug("Creating directory: {Directory}", directory);
                _ = Directory.CreateDirectory(directory);
            }

            // Transform (encrypt) the content
            _logger?.LogDebug("Transforming content for writing");
            byte[] encryptedBytes = Transform(content, cancellationToken);

            // Asynchronously write the encrypted bytes to the file
            _logger?.LogDebug("Writing {Bytes} bytes to file", encryptedBytes.Length);
            await File.WriteAllBytesAsync(filePath, encryptedBytes, cancellationToken)
                .ConfigureAwait(false);

            // Verify the operation if enabled
            if (Options.VerifyOperations)
            {
                _logger?.LogDebug("Verifying saved file");
                await VerifySavedFileAsync(filePath, content, cancellationToken);
                _logger?.LogDebug("Verification successful");
            }

            stopwatch.Stop();
            _logger?.LogInformation("File saved successfully: {FilePath}, Time: {ElapsedMs}ms",
                filePath, stopwatch.ElapsedMilliseconds);
        }
        catch (UnauthorizedAccessException ex)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Access denied to file: {FilePath}, Time: {ElapsedMs}ms",
                filePath, stopwatch.ElapsedMilliseconds);
            throw new UnauthorizedAccessException($"Access denied to file: {filePath}. Check file permissions.", ex);
        }
        catch (IOException ex)
        {
            stopwatch.Stop();

            // Enhance with more specific error
            if (ex.Message.Contains("being used by another process"))
            {
                _logger?.LogError(ex, "File in use by another process: {FilePath}, Time: {ElapsedMs}ms",
                    filePath, stopwatch.ElapsedMilliseconds);
                throw new IOException($"File '{filePath}' is in use by another process.", ex);
            }

            _logger?.LogError(ex, "I/O error while saving file: {FilePath}, Time: {ElapsedMs}ms",
                filePath, stopwatch.ElapsedMilliseconds);
            throw new IOException($"I/O error while saving file: {filePath}", ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "Unexpected error saving file: {FilePath}, Time: {ElapsedMs}ms",
                filePath, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <inheritdoc/>
    public byte[] Transform(byte[] input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        _logger?.LogDebug("Transforming data: {Size} bytes", input.Length);
        Stopwatch stopwatch = Stopwatch.StartNew();

        byte[] output = new byte[input.Length];

        // Obtain a read-only span of the XOR key for efficient access
        ReadOnlySpan<byte> keySpan = XorKey.AsSpan();

        // For performance, check the cancellation token every 16KB
        const int checkFrequency = 16 * 1024;

        // Use SIMD-compatible loops when possible for better performance
        int i = 0;

        // Process the data in chunks
        for (; i < input.Length; i++)
        {
            if (i % checkFrequency == 0 && cancellationToken.IsCancellationRequested)
                cancellationToken.ThrowIfCancellationRequested();

            output[i] = (byte)(input[i] ^ keySpan[i % keySpan.Length]);
        }

        stopwatch.Stop();
        _logger?.LogDebug("Data transformed successfully: {Size} bytes, Time: {ElapsedMs}ms",
            input.Length, stopwatch.ElapsedMilliseconds);

        return output;
    }

    /// <inheritdoc/>
    public IXorCipherService WithOptions(XorCipherOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _logger?.LogDebug("Creating new XorCipherService with updated options");
        return new XorCipherService(options, _logger);
    }

    /// <summary>
    /// Asynchronously loads and processes a large file in chunks to reduce memory usage.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The decrypted file contents.</returns>
    private async Task<byte[]> LoadAndXorLargeFileAsync(string filePath, CancellationToken cancellationToken)
    {
        FileInfo fileInfo = new(filePath);
        _logger?.LogDebug("Processing large file: {FilePath}, Size: {FileSize} bytes", filePath, fileInfo.Length);
        Stopwatch stopwatch = Stopwatch.StartNew();

        using FileStream fileStream = new(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            Options.BufferSize,
            FileOptions.Asynchronous);

        // Create a buffer for the entire file
        byte[] result = new byte[fileInfo.Length];
        int bytesRead = 0;
        int totalBytesRead = 0;

        // Rent a buffer from the array pool for the chunk
        byte[] buffer = ArrayPool<byte>.Shared.Rent(Options.BufferSize);

        try
        {
            // Read the file in chunks
            int chunkNumber = 0;
            while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, Options.BufferSize), cancellationToken)) > 0)
            {
                chunkNumber++;
                _logger?.LogTrace("Processing chunk {Chunk}: {BytesRead} bytes", chunkNumber, bytesRead);

                // Transform this chunk
                for (int i = 0; i < bytesRead; i++)
                    result[totalBytesRead + i] = (byte)(buffer[i] ^ XorKey[(totalBytesRead + i) % XorKey.Length]);

                totalBytesRead += bytesRead;

                if (cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
            }
        }
        finally
        {
            // Return the buffer to the pool
            ArrayPool<byte>.Shared.Return(buffer);
        }

        // If we didn't read the entire file (should not happen under normal circumstances)
        if (totalBytesRead < fileInfo.Length)
        {
            _logger?.LogWarning("Did not read entire file. Expected {ExpectedBytes}, got {ActualBytes}",
                fileInfo.Length, totalBytesRead);
            Array.Resize(ref result, totalBytesRead);
        }

        stopwatch.Stop();
        _logger?.LogInformation("Large file processed successfully: {FilePath}, Size: {Size} bytes, Time: {ElapsedMs}ms",
            filePath, totalBytesRead, stopwatch.ElapsedMilliseconds);

        return result;
    }

    /// <summary>
    /// Verifies that a saved file can be successfully loaded and decrypted back to the original content.
    /// </summary>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="originalContent">The original content before encryption.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task representing the verification operation.</returns>
    private async Task VerifySavedFileAsync(string filePath, byte[] originalContent, CancellationToken cancellationToken)
    {
        _logger?.LogDebug("Starting verification of saved file: {FilePath}", filePath);
        Stopwatch stopwatch = Stopwatch.StartNew();

        try
        {
            // Load the saved file
            byte[] loadedContent = await LoadAndXorAsync(filePath, cancellationToken);

            // Verify that content matches
            if (loadedContent.Length != originalContent.Length)
            {
                _logger?.LogError("Verification failed: File length mismatch. Expected {ExpectedLength}, got {ActualLength}",
                    originalContent.Length, loadedContent.Length);
                throw new InvalidDataException($"Verification failed: File length mismatch. Expected {originalContent.Length} bytes, got {loadedContent.Length} bytes.");
            }

            for (int i = 0; i < loadedContent.Length; i++)
            {
                if (loadedContent[i] != originalContent[i])
                {
                    _logger?.LogError("Verification failed: Content mismatch at position {Position}", i);
                    throw new InvalidDataException($"Verification failed: Content mismatch at position {i}.");
                }

                // Check cancellation periodically
                if (i % 16384 == 0 && cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
            }

            stopwatch.Stop();
            _logger?.LogInformation("File verification completed successfully: {FilePath}, Time: {ElapsedMs}ms",
                filePath, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger?.LogError(ex, "File verification failed: {FilePath}, Time: {ElapsedMs}ms",
                filePath, stopwatch.ElapsedMilliseconds);
            throw new InvalidOperationException("File verification failed after save operation.", ex);
        }
    }
}