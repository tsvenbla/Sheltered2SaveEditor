using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Storage;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Provides centralized access to application data and state.
/// </summary>
public static class AppDataHelper
{
    #region Events
    /// <summary>
    /// Event raised when a character is selected.
    /// </summary>
    public static event EventHandler<CharacterSelectedEventArgs>? CharacterSelected;

    /// <summary>
    /// Event raised when a save file is loaded.
    /// </summary>
    public static event EventHandler<SaveFileLoadedEventArgs>? SaveFileLoaded;

    /// <summary>
    /// Event raised when a save file is modified.
    /// </summary>
    public static event EventHandler<SaveFileModifiedEventArgs>? SaveFileModified;
    #endregion

    #region Properties
    private static bool _isSaveFileLoaded;

    /// <summary>
    /// Gets or sets a value indicating whether a save file has been loaded.
    /// </summary>
    public static bool IsSaveFileLoaded
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
    public static StorageFile? CurrentSaveFile
    {
        get => _currentSaveFile;
        set
        {
            if (_currentSaveFile != value)
            {
                _currentSaveFile = value;
                IsSaveFileLoaded = value != null;
            }
        }
    }

    private static Character? _selectedCharacter;

    /// <summary>
    /// Gets or sets the currently selected character.
    /// </summary>
    public static Character? SelectedCharacter
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

    private static readonly List<Character> _characters = [];

    /// <summary>
    /// Gets a read-only view of the characters parsed from the save file.
    /// </summary>
    public static ReadOnlyCollection<Character> Characters => _characters.AsReadOnly();

    /// <summary>
    /// Gets or sets the decrypted XML document from the save file.
    /// </summary>
    public static XDocument? SaveDocument { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the save file has been modified.
    /// </summary>
    public static bool IsSaveFileModified { get; set; }
    #endregion

    #region Methods
    /// <summary>
    /// Clears all application data.
    /// </summary>
    public static void Clear()
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
    public static void UpdateCharacters(IEnumerable<Character> characters)
    {
        _characters.Clear();
        _characters.AddRange(characters);
        SelectedCharacter = _characters.Count > 0 ? _characters[0] : null;
        IsSaveFileModified = true;
        OnSaveFileModified(new SaveFileModifiedEventArgs { IsModified = true });
    }

    /// <summary>
    /// Marks the save file as modified.
    /// </summary>
    /// <param name="isModified">Whether the save file is modified.</param>
    /// <param name="callerMemberName">The name of the member that called this method.</param>
    public static void MarkAsModified(bool isModified = true, [CallerMemberName] string callerMemberName = "")
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
public class CharacterSelectedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets the selected character.
    /// </summary>
    public Character? Character { get; set; }
}

/// <summary>
/// Contains event data for the <see cref="AppDataHelper.SaveFileLoaded"/> event.
/// </summary>
public class SaveFileLoadedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets a value indicating whether a file is loaded.
    /// </summary>
    public bool IsLoaded { get; set; }

    /// <summary>
    /// Gets or sets the loaded save file.
    /// </summary>
    public StorageFile? SaveFile { get; set; }
}

/// <summary>
/// Contains event data for the <see cref="AppDataHelper.SaveFileModified"/> event.
/// </summary>
public class SaveFileModifiedEventArgs : EventArgs
{
    /// <summary>
    /// Gets or sets a value indicating whether the save file is modified.
    /// </summary>
    public bool IsModified { get; set; }

    /// <summary>
    /// Gets or sets the source of the modification.
    /// </summary>
    public string? Source { get; set; }
}