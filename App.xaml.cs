using Microsoft.UI.Xaml;

namespace Sheltered2SaveEditor;

/// <summary>
/// Provides application-specific behavior to supplement the default Application class.
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// Gets the main window of the application.
    /// </summary>
    public static MainWindow MainWindow { get; } = new()
    {
        ExtendsContentIntoTitleBar = true,
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="App"/> class.
    /// </summary>
    public App() => InitializeComponent();

    /// <summary>
    /// Invoked when the application is launched.
    /// </summary>
    /// <param name="args">Details about the launch request and process.</param>
    protected override void OnLaunched(LaunchActivatedEventArgs args) => MainWindow.Activate();
}