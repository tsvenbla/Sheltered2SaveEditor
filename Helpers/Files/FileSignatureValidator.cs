using System.Text;

namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Validates file signatures (headers and footers) to ensure they match expected formats.
/// </summary>
internal sealed class FileSignatureValidator
{
    private readonly byte[] _expectedHeaderBytes;
    private readonly byte[] _expectedFooterBytes;
    private readonly string _expectedHeader;
    private readonly string _expectedFooter;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileSignatureValidator"/> class.
    /// </summary>
    /// <param name="expectedHeader">The expected header text.</param>
    /// <param name="expectedFooter">The expected footer text.</param>
    internal FileSignatureValidator(string expectedHeader, string expectedFooter)
    {
        ArgumentException.ThrowIfNullOrEmpty(expectedHeader, nameof(expectedHeader));
        ArgumentException.ThrowIfNullOrEmpty(expectedFooter, nameof(expectedFooter));

        _expectedHeader = expectedHeader;
        _expectedFooter = expectedFooter;
        _expectedHeaderBytes = Encoding.UTF8.GetBytes(expectedHeader);
        _expectedFooterBytes = Encoding.UTF8.GetBytes(expectedFooter);
    }

    /// <summary>
    /// Checks if the given decrypted data contains a valid header and footer.
    /// </summary>
    /// <param name="decryptedData">The decrypted data to check.</param>
    /// <returns>True if the data has valid signatures; otherwise, false.</returns>
    internal bool HasValidSignature(byte[] decryptedData)
    {
        ArgumentNullException.ThrowIfNull(decryptedData);

        if (decryptedData.Length == 0)
        {
            return false;
        }

        try
        {
            string text = Encoding.UTF8.GetString(decryptedData);
            return HasValidSignature(text);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the decrypted text has the expected header and footer.
    /// </summary>
    /// <param name="decryptedText">The decrypted text to check.</param>
    /// <returns>True if the text has valid signatures; otherwise, false.</returns>
    internal bool HasValidSignature(string decryptedText)
    {
        if (string.IsNullOrEmpty(decryptedText))
        {
            return false;
        }

        // Trim leading/trailing whitespace for more robust checking
        string trimmed = decryptedText.Trim();

        return trimmed.StartsWith(_expectedHeader, StringComparison.Ordinal) &&
               trimmed.EndsWith(_expectedFooter, StringComparison.Ordinal);
    }

    /// <summary>
    /// Checks if the data starts with the expected header.
    /// </summary>
    /// <param name="data">The data to check.</param>
    /// <returns>True if the data starts with the expected header; otherwise, false.</returns>
    internal bool StartsWithHeader(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length < _expectedHeaderBytes.Length)
        {
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
            if (trimmed.StartsWith(_expectedHeader, StringComparison.Ordinal))
            {
                return true;
            }

            // If it contains the header anywhere near the start, this could be valid
            return headerSample.Contains(_expectedHeader, StringComparison.Ordinal);
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if the data ends with the expected footer.
    /// </summary>
    /// <param name="data">The data to check.</param>
    /// <returns>True if the data ends with the expected footer; otherwise, false.</returns>
    internal bool EndsWithFooter(byte[] data)
    {
        ArgumentNullException.ThrowIfNull(data);

        if (data.Length < _expectedFooterBytes.Length)
        {
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
            if (trimmed.EndsWith(_expectedFooter, StringComparison.Ordinal))
            {
                return true;
            }

            // If it contains the footer anywhere near the end, this could be valid
            return footerSample.Contains(_expectedFooter, StringComparison.Ordinal);
        }
        catch (Exception)
        {
            return false;
        }
    }
}