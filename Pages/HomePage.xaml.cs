using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Helpers;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Sheltered2SaveEditor.Pages;

/// <summary>
/// Code-behind for HomePage.xaml that handles file picking and processing.
/// </summary>
public sealed partial class HomePage : Page
{
    private readonly FileValidator _fileValidator;

    public HomePage()
    {
        InitializeComponent();
        // Dependency injection of the XorCipherService into the FileValidator.
        _fileValidator = new FileValidator(new XorCipherService());
    }

    /// <summary>
    /// Handles the click event for the LoadFile button.
    /// </summary>
    private async void LoadFileButton_Click(object sender, RoutedEventArgs e)
    {
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(e);

        try
        {
            await PickAFileAsync().ConfigureAwait(true);
        }
        catch (OperationCanceledException)
        {
            ShowError("Operation cancelled.");
        }
        catch (Exception ex)
        {
            ShowError($"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Opens a file picker to select a file and validates it.
    /// </summary>
    private async Task PickAFileAsync()
    {
        LoadFileProgressRing.IsActive = true;
        LoadFileButton.IsEnabled = false;

        FileOpenPicker openPicker = new()
        {
            ViewMode = PickerViewMode.Thumbnail,
            SuggestedStartLocation = PickerLocationId.ComputerFolder
        };
        openPicker.FileTypeFilter.Add(".dat");

        nint hWnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(openPicker, hWnd);

        try
        {
            StorageFile file = await openPicker.PickSingleFileAsync();
            if (file is null)
            {
                ShowError("Operation cancelled.");
                return;
            }

            if (!FileValidator.IsValidDatFile(file))
            {
                ShowError("Invalid file type. Please select a DAT file.");
                return;
            }

            if (!await _fileValidator.IsValidSaveFileAsync(file))
            {
                ShowError("Invalid file format. Not a valid Shelter 2 save file.");
                return;
            }

            await ProcessValidFileAsync(file);
        }
        catch (FileNotFoundException ex)
        {
            ShowError($"File not found: {ex.FileName}");
        }
        catch (UnauthorizedAccessException)
        {
            ShowError("Access denied. Please check file permissions.");
        }
        catch (IOException ex)
        {
            ShowError($"I/O error: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes the valid save file by decrypting its contents.
    /// </summary>
    /// <param name="file">The valid save file.</param>
    private async Task ProcessValidFileAsync(StorageFile file)
    {
        try
        {
            // Use a fresh instance of the cipher service to load and decrypt the file.
            XorCipherService cipherService = new();
            byte[] decryptedData = await cipherService.LoadAndXorAsync(file.Path);
            string decryptedContent = Encoding.UTF8.GetString(decryptedData);

            // Parse characters and store them in the App's shared data
            System.Collections.Generic.List<Character> characters = CharacterParser.ParseCharacters(decryptedContent);
            AppDataHelper.Characters = characters;

            LoadFileTextBlock.Text = $"Selected file name: {file.Name}\nDecrypted successfully.";

            if (App.MainWindow is MainWindow mainWindow)
            {
                mainWindow.EnableNavigationItems();
            }
        }
        finally
        {
            LoadFileProgressRing.IsActive = false;
            LoadFileButton.IsEnabled = true;
        }
    }

    /// <summary>
    /// Displays error messages in the UI.
    /// </summary>
    /// <param name="message">The error message to display.</param>
    private void ShowError(string message)
    {
        LoadFileTextBlock.Text = message;
        LoadFileProgressRing.IsActive = false;
        LoadFileButton.IsEnabled = true;
    }
}