using Sheltered2SaveEditor.Core.Models;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Windows.Storage;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Provides centralized access to application data and state.
/// </summary>
internal static class AppDataHelper
{
    #region Events
    /// <summary>
    /// Event raised when a character is selected.
    /// </summary>
    internal static event EventHandler<CharacterSelectedEventArgs>? CharacterSelected;

    /// <summary>
    /// Event raised when a save file is loaded.
    /// </summary>
    internal static event EventHandler<SaveFileLoadedEventArgs>? SaveFileLoaded;

    /// <summary>
    /// Event raised when a save file is modified.
    /// </summary>
    internal static event EventHandler<SaveFileModifiedEventArgs>? SaveFileModified;
    #endregion

    #region Properties
    private static bool _isSaveFileLoaded;

    /// <summary>
    /// Gets or sets a value indicating whether a save file has been loaded.
    /// </summary>
    internal static bool IsSaveFileLoaded
    {
        get => _isSaveFileLoaded;
        set
        {
            if (_isSaveFileLoaded != value)
            {
                _isSaveFileLoaded = value;
                OnSaveFileLoaded(new SaveFileLoadedEventArgs { IsLoaded = value });
            }
        }
    }

    private static StorageFile? _currentSaveFile;

    /// <summary>
    /// Gets or sets the currently loaded save file.
    /// </summary>
    internal static StorageFile? CurrentSaveFile
    {
        get => _currentSaveFile;
        set
        {
            if (_currentSaveFile != value)
            {
                _currentSaveFile = value;
                IsSaveFileLoaded = value != null;

                // When loading a new file, mark it as modified to enable Save button
                if (value != null)
                    MarkAsModified(true);

                // Include the file in the event args
                OnSaveFileLoaded(new SaveFileLoadedEventArgs { IsLoaded = value != null, SaveFile = value });
            }
        }
    }

    private static Character? _selectedCharacter;

    /// <summary>
    /// Gets or sets the currently selected character.
    /// </summary>
    internal static Character? SelectedCharacter
    {
        get => _selectedCharacter;
        set
        {
            if (_selectedCharacter != value)
            {
                _selectedCharacter = value;
                OnCharacterSelected(new CharacterSelectedEventArgs { Character = value });
            }
        }
    }

    private static readonly ObservableCollection<Character> _characters = [];

    /// <summary>
    /// Gets a read-only view of the characters parsed from the save file.
    /// </summary>
    internal static ReadOnlyObservableCollection<Character> Characters { get; } =
        new ReadOnlyObservableCollection<Character>(_characters);

    /// <summary>
    /// Gets or sets the decrypted XML document from the save file.
    /// </summary>
    internal static XDocument? SaveDocument { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the save file has been modified.
    /// </summary>
    internal static bool IsSaveFileModified { get; set; }
    #endregion

    #region Methods
    /// <summary>
    /// Clears all application data.
    /// </summary>
    internal static void Clear()
    {
        _characters.Clear();
        SelectedCharacter = null;
        CurrentSaveFile = null;
        SaveDocument = null;
        IsSaveFileLoaded = false;
        IsSaveFileModified = false;
    }

    /// <summary>
    /// Updates the character collection with a new list of characters.
    /// </summary>
    /// <param name="characters">The new characters to set.</param>
    internal static void UpdateCharacters(IEnumerable<Character> characters)
    {
        _characters.Clear();
        foreach (Character character in characters)
            _characters.Add(character);

        SelectedCharacter = _characters.Count > 0 ? _characters[0] : null;
        IsSaveFileModified = true;
        OnSaveFileModified(new SaveFileModifiedEventArgs { IsModified = true });
    }

    /// <summary>
    /// Marks the save file as modified.
    /// </summary>
    /// <param name="isModified">Whether the save file is modified.</param>
    /// <param name="callerMemberName">The name of the member that called this method.</param>
    internal static void MarkAsModified(bool isModified = true, [CallerMemberName] string callerMemberName = "")
    {
        IsSaveFileModified = isModified;
        OnSaveFileModified(new SaveFileModifiedEventArgs { IsModified = isModified, Source = callerMemberName });
    }

    private static void OnCharacterSelected(CharacterSelectedEventArgs e) => CharacterSelected?.Invoke(null, e);

    private static void OnSaveFileLoaded(SaveFileLoadedEventArgs e) => SaveFileLoaded?.Invoke(null, e);

    private static void OnSaveFileModified(SaveFileModifiedEventArgs e) => SaveFileModified?.Invoke(null, e);
    #endregion
}

/// <summary>
/// Contains event data for the <see cref="AppDataHelper.CharacterSelected"/> event.
/// </summary>
internal sealed class CharacterSelectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the selected character.
    /// </summary>
    internal Character? Character { get; set; }
}

/// <summary>
/// Contains event data for the <see cref="AppDataHelper.SaveFileLoaded"/> event.
/// </summary>
internal sealed class SaveFileLoadedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets a value indicating whether a file is loaded.
    /// </summary>
    internal bool IsLoaded { get; set; }

    /// <summary>
    /// Gets or sets the loaded save file.
    /// </summary>
    internal StorageFile? SaveFile { get; set; }
}

/// <summary>
/// Contains event data for the <see cref="AppDataHelper.SaveFileModified"/> event.
/// </summary>
internal sealed class SaveFileModifiedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets a value indicating whether the save file is modified.
    /// </summary>
    internal bool IsModified { get; set; }

    /// <summary>
    /// Gets or sets the source of the modification.
    /// </summary>
    internal string? Source { get; set; }
}