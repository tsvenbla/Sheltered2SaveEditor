namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// Interface for monitoring the progress of file validation operations.
/// </summary>
internal interface IValidationProgressMonitor
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