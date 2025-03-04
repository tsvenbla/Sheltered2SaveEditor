using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Provides methods for encrypting and decrypting data using an XOR cipher.
/// </summary>
public interface IXorCipherService
{
    /// <summary>
    /// Asynchronously loads and decrypts the contents of the file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file to be loaded and decrypted.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the decrypted file contents as a byte array.
    /// </returns>
    Task<byte[]> LoadAndXorAsync(string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously encrypts and saves the provided content to a file.
    /// </summary>
    /// <param name="filePath">The path to the file where the content will be saved.</param>
    /// <param name="content">The content to be encrypted and saved.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SaveAndXorAsync(string filePath, byte[] content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs an XOR transformation on the specified input data.
    /// </summary>
    /// <param name="input">The input data to be transformed.</param>
    /// <returns>
    /// The transformed data as a new byte array.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is <c>null</c>.</exception>
    byte[] Transform(byte[] input);
}

/// <summary>
/// Provides methods for encrypting and decrypting data using a fixed XOR key.
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
    /// The key is stored as an immutable array and its elements are used cyclically.
    /// </remarks>
    private static readonly ImmutableArray<byte> XorKey = ImmutableArray.Create<byte>(
        0xAC, 0x73, 0xFE, 0xF2, 0xAA, 0xBA, 0x6D, 0xAB,
        0x30, 0x3A, 0x8B, 0xA7, 0xDE, 0x0D, 0x15, 0x21, 0x4A
    );

    /// <inheritdoc/>
    public async Task<byte[]> LoadAndXorAsync(string filePath, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);

        // Asynchronously read all bytes from the file.
        byte[] fileBytes = await File.ReadAllBytesAsync(filePath, cancellationToken)
            .ConfigureAwait(false);

        // Transform (decrypt) the file bytes using the XOR cipher.
        return Transform(fileBytes, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task SaveAndXorAsync(string filePath, byte[] content, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(filePath);
        ArgumentNullException.ThrowIfNull(content);

        // Transform (encrypt) the content.
        byte[] encryptedBytes = Transform(content, cancellationToken);

        // Asynchronously write the encrypted bytes to the file.
        await File.WriteAllBytesAsync(filePath, encryptedBytes, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Checks for <paramref name="input"/> being <c>null</c> and throws an <see cref="ArgumentNullException"/> if so.
    /// Also periodically checks the <see cref="CancellationToken"/> (if provided) during processing.
    /// </remarks>
    public static byte[] Transform(byte[] input, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);

        byte[] output = new byte[input.Length];

        // Obtain a read-only span of the XOR key for efficient access.
        ReadOnlySpan<byte> keySpan = XorKey.AsSpan();

        // For performance, check the cancellation token every 1024 iterations.
        const int checkFrequency = 1024;

        for (int i = 0; i < input.Length; i++)
        {
            if (i % checkFrequency == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }
            output[i] = (byte)(input[i] ^ keySpan[i % keySpan.Length]);
        }

        return output;
    }

    /// <inheritdoc/>
    byte[] IXorCipherService.Transform(byte[] input) => Transform(input);
}