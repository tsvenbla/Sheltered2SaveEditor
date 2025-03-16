using Microsoft.Extensions.Logging;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Provides access to the application's navigation Frame when it becomes available.
/// This resolves timing issues when the Frame isn't yet available during service registration.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FrameProvider"/> class.
/// </remarks>
/// <param name="logger">The logger used to log frame access operations.</param>
public class FrameProvider(ILogger<FrameProvider> logger)
{
    private readonly ILogger<FrameProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private Frame? _frame;

    /// <summary>
    /// Sets the Frame instance that will be used for navigation.
    /// This should be called when the Frame is available (typically after MainWindow initialization).
    /// </summary>
    /// <param name="frame">The Frame to use for navigation.</param>
    public void Initialize(Frame frame)
    {
        _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        _logger.LogInformation("FrameProvider initialized with frame");
    }

    /// <summary>
    /// Gets the Frame instance for navigation.
    /// </summary>
    /// <returns>The Frame instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the Frame has not been initialized.</exception>
    public Frame GetFrame()
    {
        if (_frame == null)
        {
            _logger.LogError("Attempted to get Frame before it was initialized");
            throw new InvalidOperationException("Frame has not been initialized. Make sure to call Initialize first.");
        }

        return _frame;
    }
}