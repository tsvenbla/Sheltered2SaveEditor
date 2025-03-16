using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Sheltered2SaveEditor.Helpers;
using Sheltered2SaveEditor.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Sheltered2SaveEditor.ViewModels;

/// <summary>
/// ViewModel for the Home page responsible for loading, validating, decrypting,
/// and processing the game save file.
/// </summary>
/// <remarks>
/// This ViewModel uses dependency injection to obtain services for file picking,
/// file validation, and logging. It provides an asynchronous command for loading a save file,
/// handles errors via minimal try/catch blocks, logs exceptions using Microsoft.Extensions.Logging,
/// and updates the UI through bindable properties. It also signals when a save file has been loaded,
/// allowing other parts of the application to update their state.
/// </remarks>
public partial class HomePageViewModel : ObservableObject
{
    private bool _isLoading;
    /// <summary>
    /// Gets or sets a value indicating whether the file loading operation is in progress.
    /// </summary>
    /// <value>
    /// <c>true</c> if a file is being loaded; otherwise, <c>false</c>.
    /// </value>
    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    private string _feedback = string.Empty;
    /// <summary>
    /// Gets or sets the feedback message to display to the user.
    /// This includes status messages, error messages, and information about the selected file.
    /// </summary>
    /// <value>
    /// A string containing status, error, or informational messages.
    /// </value>
    public string Feedback
    {
        get => _feedback;
        set => SetProperty(ref _feedback, value);
    }

    /// <summary>
    /// Gets a value indicating whether a save file has been loaded.
    /// This property is used to enable controls (e.g. the Save file button) that depend on a file being loaded.
    /// </summary>
    /// <value>
    /// <c>true</c> if a save file has been loaded; otherwise, <c>false</c>.
    /// </value>
    public static bool IsFileLoaded => AppDataHelper.IsSaveFileLoaded;

    /// <summary>
    /// Gets the command that initiates the file loading process.
    /// </summary>
    /// <value>
    /// An <see cref="AsyncRelayCommand"/> that executes the file loading operation asynchronously.
    /// </value>
    public AsyncRelayCommand LoadFileCommand => new(LoadFileAsync);

    private readonly IFilePickerService _filePickerService;
    private readonly FileValidator _fileValidator;
    private readonly ILogger<HomePageViewModel> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomePageViewModel"/> class.
    /// </summary>
    /// <param name="filePickerService">The service used to open a file picker dialog.</param>
    /// <param name="fileValidator">The service used to validate and decrypt the selected file.</param>
    /// <param name="logger">The logger used for logging errors and diagnostic information.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown if any of the injected services are <c>null</c>.
    /// </exception>
    public HomePageViewModel(IFilePickerService filePickerService, FileValidator fileValidator, ILogger<HomePageViewModel> logger)
        : base()
    {
        _filePickerService = filePickerService ?? throw new ArgumentNullException(nameof(filePickerService));
        _fileValidator = fileValidator ?? throw new ArgumentNullException(nameof(fileValidator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to changes so that IsFileLoaded notifies the UI.
        AppDataHelper.SaveFileLoaded += (s, e) => OnPropertyChanged(nameof(ViewModels.HomePageViewModel.IsFileLoaded));
    }

    /// <summary>
    /// Asynchronously loads a save file, validates it, and processes it.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method updates the <see cref="IsLoading"/> and <see cref="Feedback"/> properties based on the outcome of the operation.
    /// It catches exceptions only for operations where recovery is possible, logs errors, and provides user feedback.
    /// </remarks>
    private async Task LoadFileAsync()
    {
        IsLoading = true;
        Feedback = string.Empty;
        try
        {
            StorageFile? file = await _filePickerService.PickFileAsync();
            if (file == null)
            {
                Feedback = "Operation cancelled.";
                return;
            }

            if (!FileValidator.IsValidDatFile(file))
            {
                Feedback = "Invalid file type. Please select a DAT file.";
                return;
            }

            bool isValid = await _fileValidator.IsValidSaveFileAsync(file);
            if (!isValid)
            {
                Feedback = "Invalid file format. Not a valid Sheltered 2 save file.";
                return;
            }

            await ProcessValidFileAsync(file);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "File not found.");
            Feedback = $"File not found: {ex.FileName}";
        }
        catch (UnauthorizedAccessException)
        {
            Feedback = "Access denied. Please check file permissions.";
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "I/O error.");
            Feedback = $"I/O error: {ex.Message}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during file loading.");
            Feedback = $"Unexpected error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Processes the valid save file by decrypting its content and parsing character data.
    /// </summary>
    /// <param name="file">The <see cref="StorageFile"/> representing the selected save file.</param>
    /// <returns>A task representing the asynchronous processing operation.</returns>
    /// <remarks>
    /// This method creates a new instance of the XOR cipher service to decrypt the file,
    /// parses the decrypted XML to extract character information, updates the global character list,
    /// and signals that a save file has been successfully loaded.
    /// The feedback message is updated to include the file name and decryption status.
    /// </remarks>
    private async Task ProcessValidFileAsync(StorageFile file)
    {
        try
        {
            // Create a new instance of the cipher service.
            XorCipherService cipherService = new();
            byte[] decryptedData = await cipherService.LoadAndXorAsync(file.Path);
            string decryptedContent = Encoding.UTF8.GetString(decryptedData);

            List<Character> characters = CharacterParser.ParseCharacters(decryptedContent);
            AppDataHelper.Characters = characters;
            Feedback = $"Selected file name: {file.Name}\nDecrypted successfully.";

            // Signal that the save file has been loaded.
            AppDataHelper.IsSaveFileLoaded = true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file.");
            Feedback = $"Error processing file: {ex.Message}";
        }
    }
}