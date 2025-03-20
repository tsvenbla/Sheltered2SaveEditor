using WinRT.Interop;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Provides helper methods for working with window handles.
/// </summary>
internal static class WindowHandleHelper
{
    /// <summary>
    /// Gets the handle for the main application window.
    /// </summary>
    /// <returns>The window handle as an <see cref="nint"/>.</returns>
    internal static nint GetMainWindowHandle() => WindowNative.GetWindowHandle(App.MainWindow);
}