using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sheltered2SaveEditor.Infrastructure.Navigation;

/// <summary>
/// Defines the contract for a service that provides access to the application's navigation Frame.
/// </summary>
internal interface IFrameProvider
{
    /// <summary>
    /// Gets a value indicating whether the Frame has been initialized.
    /// </summary>
    bool IsInitialized { get; }

    /// <summary>
    /// Gets a value indicating whether navigation can go back.
    /// </summary>
    bool CanGoBack { get; }

    /// <summary>
    /// Event that is raised when the Frame becomes available.
    /// </summary>
    event EventHandler<Frame> FrameInitialized;

    /// <summary>
    /// Event that is raised when navigation occurs.
    /// </summary>
    event NavigatedEventHandler FrameNavigated;

    /// <summary>
    /// Sets the Frame instance that will be used for navigation.
    /// This should be called when the Frame is available (typically after MainWindow initialization).
    /// </summary>
    /// <param name="frame">The Frame to use for navigation.</param>
    void Initialize(Frame frame);

    /// <summary>
    /// Gets the Frame instance for navigation.
    /// </summary>
    /// <returns>The Frame instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Frame has not been initialized.</exception>
    Frame GetFrame();

    /// <summary>
    /// Asynchronously waits for the Frame to be initialized and returns it.
    /// </summary>
    /// <param name="timeout">Optional timeout in milliseconds. Default is 30 seconds.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the Frame instance.</returns>
    /// <exception cref="TimeoutException">Thrown if the wait operation times out.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the operation is canceled.</exception>
    Task<Frame> GetFrameAsync(int timeout = 30000, CancellationToken cancellationToken = default);

    /// <summary>
    /// Navigates to the specified page type with optional parameter.
    /// </summary>
    /// <param name="sourcePageType">The type of the page to navigate to.</param>
    /// <param name="parameter">Optional parameter to pass to the page.</param>
    /// <returns>True if navigation was successful; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Frame has not been initialized.</exception>
    bool Navigate(Type sourcePageType, object? parameter = null);

    /// <summary>
    /// Navigates back in the navigation stack.
    /// </summary>
    /// <returns>True if navigation was successful; otherwise, false.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Frame has not been initialized.</exception>
    bool GoBack();
}

/// <summary>
/// Provides access to the application's navigation Frame when it becomes available.
/// This resolves timing issues when the Frame isn't yet available during service registration.
/// </summary>
internal partial class FrameProvider : IFrameProvider, IDisposable
{
    private readonly ILogger<FrameProvider> _logger;
    private Frame? _frame;
    private readonly SemaphoreSlim _frameSemaphore = new(1, 1);
    private readonly TaskCompletionSource<Frame> _frameInitTcs = new();
    private bool _disposed;

    /// <inheritdoc/>
    public bool IsInitialized => _frame != null;

    /// <inheritdoc/>
    public bool CanGoBack => _frame?.CanGoBack ?? false;

    /// <inheritdoc/>
    public event EventHandler<Frame>? FrameInitialized;

    /// <inheritdoc/>
    public event NavigatedEventHandler? FrameNavigated;

    /// <summary>
    /// Initializes a new instance of the <see cref="FrameProvider"/> class.
    /// </summary>
    /// <param name="logger">The logger used to log frame access operations.</param>
    public FrameProvider(ILogger<FrameProvider> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.LogInformation("FrameProvider instance created");
    }

    /// <inheritdoc/>
    public void Initialize(Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        _frameSemaphore.Wait();
        try
        {
            if (_frame != null)
            {
                _logger.LogWarning("FrameProvider is already initialized. Replacing existing frame.");

                // Unsubscribe from the old frame's events
                _frame.Navigated -= OnFrameNavigated;
            }

            _frame = frame;
            _frame.Navigated += OnFrameNavigated;

            _logger.LogInformation("FrameProvider initialized with frame {FrameHashCode}", frame.GetHashCode());

            // Set the result for any waiting async operations
            if (!_frameInitTcs.Task.IsCompleted)
            {
                _frameInitTcs.SetResult(frame);
            }

            // Raise event
            FrameInitialized?.Invoke(this, frame);
        }
        finally
        {
            _ = _frameSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public Frame GetFrame()
    {
        _frameSemaphore.Wait();
        try
        {
            if (_frame == null)
            {
                _logger.LogError("Attempted to get Frame before it was initialized");
                throw new InvalidOperationException("Frame has not been initialized. Make sure to call Initialize first.");
            }

            return _frame;
        }
        finally
        {
            _ = _frameSemaphore.Release();
        }
    }

    /// <inheritdoc/>
    public async Task<Frame> GetFrameAsync(int timeout = 30000, CancellationToken cancellationToken = default)
    {
        if (_frame != null)
        {
            return _frame;
        }

        if (timeout <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), "Timeout must be a positive value.");
        }

        _logger.LogInformation("Waiting for Frame to be initialized (timeout: {Timeout}ms)", timeout);

        using CancellationTokenSource timeoutCts = new(timeout);
        using CancellationTokenSource linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            return await _frameInitTcs.Task.WaitAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            _logger.LogError("Timed out waiting for Frame initialization");
            throw new TimeoutException($"Timed out waiting for Frame initialization after {timeout}ms.");
        }
    }

    /// <inheritdoc/>
    public bool Navigate(Type sourcePageType, object? parameter = null)
    {
        ArgumentNullException.ThrowIfNull(sourcePageType);

        Frame frame = GetFrame();
        bool result = false;

        try
        {
            result = frame.Navigate(sourcePageType, parameter);

            if (!result)
            {
                _logger.LogWarning("Navigation to {PageType} failed", sourcePageType.Name);
            }
            else
            {
                _logger.LogInformation("Successfully navigated to {PageType}", sourcePageType.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to {PageType}", sourcePageType.Name);
        }

        return result;
    }

    /// <inheritdoc/>
    public bool GoBack()
    {
        Frame frame = GetFrame();

        if (!frame.CanGoBack)
        {
            _logger.LogWarning("Cannot navigate back: no entries in back stack");
            return false;
        }

        try
        {
            frame.GoBack();
            _logger.LogInformation("Successfully navigated back");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating back");
            return false;
        }
    }

    /// <summary>
    /// Handles the Navigated event of the Frame.
    /// </summary>
    private void OnFrameNavigated(object sender, NavigationEventArgs e)
    {
        _logger.LogDebug("Frame navigated to {PageType}", e.SourcePageType.Name);
        FrameNavigated?.Invoke(sender, e);
    }

    /// <summary>
    /// Disposes resources used by the FrameProvider.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the unmanaged resources used by the FrameProvider and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _frameSemaphore.Dispose();

            if (_frame != null)
            {
                _frame.Navigated -= OnFrameNavigated;
            }
        }

        _disposed = true;
    }
}