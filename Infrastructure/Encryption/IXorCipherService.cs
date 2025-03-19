using System.Threading;
using System.Threading.Tasks;

namespace Sheltered2SaveEditor.Infrastructure.Encryption;

/// <summary>
/// Provides methods for encrypting and decrypting data using an XOR cipher.
/// </summary>
public interface IXorCipherService
{
    /// <summary>
    /// Gets the options currently configured for this cipher service.
    /// </summary>
    XorCipherOptions Options { get; }

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
    /// Asynchronously loads and decrypts a chunk of the file at the specified path.
    /// </summary>
    /// <param name="filePath">The path to the file to be loaded and decrypted.</param>
    /// <param name="offset">The starting position within the file.</param>
    /// <param name="count">The number of bytes to read, or -1 to read to the end of the file.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains the decrypted file contents as a byte array.
    /// </returns>
    Task<byte[]> LoadAndXorChunkAsync(string filePath, long offset, int count = -1, CancellationToken cancellationToken = default);

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
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>
    /// The transformed data as a new byte array.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is <c>null</c>.</exception>
    byte[] Transform(byte[] input, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new instance of the service with the specified options.
    /// </summary>
    /// <param name="options">The options to use for the new instance.</param>
    /// <returns>A new instance of the cipher service with the specified options.</returns>
    IXorCipherService WithOptions(XorCipherOptions options);
}