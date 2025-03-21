namespace Sheltered2SaveEditor.Helpers.Files;

/// <summary>
/// A default implementation of <see cref="IValidationProgressMonitor"/> that does nothing.
/// </summary>
/// <remarks>
/// This implements the Null Object pattern and is used when no progress reporting is needed.
/// </remarks>
internal sealed class NullValidationProgressMonitor : IValidationProgressMonitor
{
    /// <summary>
    /// Gets the singleton instance of the <see cref="NullValidationProgressMonitor"/>.
    /// </summary>
    internal static IValidationProgressMonitor Instance { get; } = new NullValidationProgressMonitor();

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