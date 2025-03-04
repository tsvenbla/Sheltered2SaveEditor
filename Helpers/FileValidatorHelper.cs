using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Provides methods to validate game save files.
/// </summary>
public sealed class FileValidator(IXorCipherService cipherService)
{
    // Expected decrypted header and footer for the save file.
    private static readonly byte[] ExpectedHeader = Encoding.UTF8.GetBytes("<root>");
    private static readonly byte[] ExpectedFooter = Encoding.UTF8.GetBytes("</root>");

    // Maximum allowed file size (25MB).
    private const ulong MaxFileSize = 25 * 1024 * 1024;

    private readonly IXorCipherService _cipherService = cipherService ?? throw new ArgumentNullException(nameof(cipherService));

    /// <summary>
    /// Determines if the provided file has a .dat extension.
    /// </summary>
    /// <param name="file">The file to check.</param>
    /// <returns>True if the file has a .dat extension; otherwise, false.</returns>
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
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the file is valid.</returns>
    public async Task<bool> IsValidSaveFileAsync(StorageFile file)
    {
        ArgumentNullException.ThrowIfNull(file);

        try
        {
            using IRandomAccessStreamWithContentType stream = await file.OpenReadAsync();

            if (stream.Size > MaxFileSize ||
                stream.Size < (ulong)(ExpectedHeader.Length + ExpectedFooter.Length))
            {
                return false;
            }

            byte[] encryptedData = new byte[stream.Size];
            int totalBytesRead = await stream.AsStream().ReadAsync(encryptedData.AsMemory(0, (int)stream.Size));
            if (totalBytesRead != (int)stream.Size)
            {
                return false;
            }

            byte[] decryptedData = _cipherService.Transform(encryptedData);
            string decryptedText = Encoding.UTF8.GetString(decryptedData);

            return HasValidSignature(decryptedText);
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the decrypted text has the expected header and footer.
    /// </summary>
    /// <param name="decryptedText">The decrypted text to check.</param>
    /// <returns>True if the text has the expected header and footer; otherwise, false.</returns>
    private static bool HasValidSignature(string decryptedText) =>
        decryptedText.StartsWith("<root>") && decryptedText.EndsWith("</root>");
}