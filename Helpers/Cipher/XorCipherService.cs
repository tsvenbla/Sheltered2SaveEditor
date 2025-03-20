using System.Buffers;
using System.Collections.Immutable;
using System.Numerics;

namespace Sheltered2SaveEditor.Helpers.Cipher;

/// <summary>
/// Provides methods for encrypting and decrypting data using a configurable XOR key.
/// </summary>
/// <remarks>
/// This XOR cipher is used only for obfuscation and is not suitable for secure encryption.
/// The same key is used for both encryption and decryption.
/// </remarks>
internal sealed class XorCipherService : IXorCipherService
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
    internal XorCipherOptions Options { get; }

    /// <summary>
    /// Explicit interface implementation for Options property.
    /// </summary>
    XorCipherOptions IXorCipherService.Options => Options;

    /// <summary>
    /// Initializes a new instance of the <see cref="XorCipherService"/> class with default options.
    /// </summary>
    internal XorCipherService()
        : this(new XorCipherOptions())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XorCipherService"/> class with the specified options.
    /// </summary>
    /// <param name="options">The options for configuring the cipher service.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="options"/> is <c>null</c>.</exception>
    internal XorCipherService(XorCipherOptions options) => Options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc/>
    public async Task<byte[]> LoadAndXorAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        // Check if file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

        FileInfo fileInfo = new(filePath);

        try
        {
            // Choose the appropriate method based on file size and options
            if (Options.UseBufferedIO && fileInfo.Length > Options.BufferedIOThreshold)
            {
                return await LoadAndXorLargeFileAsync(filePath, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // For smaller files, read the whole file at once
                byte[] fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken)
                    .ConfigureAwait(false);

                // Transform (decrypt) the file bytes using the XOR cipher
                byte[] result = Transform(fileBytes, cancellationToken);

                return result;
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<byte[]> LoadAndXorChunkAsync(string filePath, long offset, int count = -1, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(offset), "Offset cannot be negative.");
        }

        if (count < -1)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be -1 or a non-negative value.");
        }

        // Check if file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("The specified file does not exist.", filePath);
        }

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
            }

            byte[] buffer = new byte[actualCount];
            int bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, actualCount), cancellationToken).ConfigureAwait(false);

            if (bytesRead < actualCount)
            {
                // Resize buffer if we read fewer bytes than expected
                Array.Resize(ref buffer, bytesRead);
            }

            byte[] result = Transform(buffer, cancellationToken);
            return result;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SaveAndXorAsync(string filePath, byte[] content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(content);

        try
        {
            // Ensure the directory exists
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                _ = Directory.CreateDirectory(directory);
            }

            // Transform (encrypt) the content
            byte[] encryptedBytes = Transform(content, cancellationToken);

            // Asynchronously write the encrypted bytes to the file
            await File.WriteAllBytesAsync(filePath, encryptedBytes, cancellationToken)
                .ConfigureAwait(false);

            // Verify the operation if enabled
            if (Options.VerifyOperations)
            {
                await VerifySavedFileAsync(filePath, content, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new UnauthorizedAccessException($"Access denied to file: {filePath}. Check file permissions.", ex);
        }
        catch (IOException ex)
        {
            // Enhance with more specific error
            if (ex.Message.Contains("being used by another process", StringComparison.Ordinal))
            {
                throw new IOException($"File '{filePath}' is in use by another process.", ex);
            }

            throw new IOException($"I/O error while saving file: {filePath}", ex);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw;
        }
    }

    /// <inheritdoc/>
    public byte[] Transform(byte[] input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        // For empty inputs, return an empty array
        if (input.Length == 0)
        {
            return [];
        }

        // Determine if we should use pooling based on input size
        bool usePooling = input.Length > 1024 * 1024; // 1MB threshold
        byte[]? rentedOutput = null;
        byte[] output;

        try
        {
            // Either rent from pool or allocate new array
            if (usePooling)
            {
                rentedOutput = ArrayPool<byte>.Shared.Rent(input.Length);
                output = rentedOutput;
            }
            else
            {
                output = new byte[input.Length];
            }

            // Obtain a read-only span of the XOR key for efficient access
            ReadOnlySpan<byte> keySpan = XorKey.AsSpan();

            // For performance, check the cancellation token every 16KB
            const int checkFrequency = 16 * 1024;

            // Use SIMD if enabled and available
            if (Options.UseSIMD && Vector.IsHardwareAccelerated && input.Length >= Vector<byte>.Count)
            {
                int vectorSize = Vector<byte>.Count;
                int vectorizedLength = input.Length - input.Length % vectorSize;

                // Process in vectorized chunks
                for (int i = 0; i < vectorizedLength; i += vectorSize)
                {
                    if (i % checkFrequency == 0 && cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();

                    // Create key vector (repeating pattern based on key length)
                    byte[] keyChunk = new byte[vectorSize];
                    for (int j = 0; j < vectorSize; j++)
                    {
                        keyChunk[j] = keySpan[(i + j) % keySpan.Length];
                    }

                    // Get vector from input
                    Vector<byte> inputVector = new(input, i);
                    Vector<byte> keyVector = new(keyChunk);

                    // XOR the vectors
                    Vector<byte> resultVector = Vector.Xor(inputVector, keyVector);

                    // Store result
                    resultVector.CopyTo(output, i);
                }

                // Process remaining elements
                for (int i = vectorizedLength; i < input.Length; i++)
                {
                    if (i % checkFrequency == 0 && cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();

                    output[i] = (byte)(input[i] ^ keySpan[i % keySpan.Length]);
                }
            }
            else
            {
                // Standard non-SIMD processing
                for (int i = 0; i < input.Length; i++)
                {
                    if (i % checkFrequency == 0 && cancellationToken.IsCancellationRequested)
                        cancellationToken.ThrowIfCancellationRequested();

                    output[i] = (byte)(input[i] ^ keySpan[i % keySpan.Length]);
                }
            }

            // If we used a rented buffer, copy to a right-sized array before returning
            if (usePooling)
            {
                byte[] result = new byte[input.Length];
                Array.Copy(output, result, input.Length);
                return result;
            }
            else
            {
                return output;
            }
        }
        finally
        {
            // Return the buffer to the pool if we rented one
            if (rentedOutput != null)
            {
                ArrayPool<byte>.Shared.Return(rentedOutput);
            }
        }
    }

    /// <inheritdoc/>
    public IXorCipherService WithOptions(XorCipherOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new XorCipherService(options);
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
            while ((bytesRead = await fileStream.ReadAsync(buffer.AsMemory(0, Options.BufferSize), cancellationToken).ConfigureAwait(false)) > 0)
            {
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
            Array.Resize(ref result, totalBytesRead);
        }

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
        try
        {
            // Load the saved file
            byte[] loadedContent = await LoadAndXorAsync(filePath, cancellationToken).ConfigureAwait(false);

            // Verify that content matches
            if (loadedContent.Length != originalContent.Length)
            {
                throw new InvalidDataException($"Verification failed: File length mismatch. Expected {originalContent.Length} bytes, got {loadedContent.Length} bytes.");
            }

            for (int i = 0; i < loadedContent.Length; i++)
            {
                if (loadedContent[i] != originalContent[i])
                {
                    throw new InvalidDataException($"Verification failed: Content mismatch at position {i}.");
                }

                // Check cancellation periodically
                if (i % 16384 == 0 && cancellationToken.IsCancellationRequested)
                    cancellationToken.ThrowIfCancellationRequested();
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("File verification failed after save operation.", ex);
        }
    }
}