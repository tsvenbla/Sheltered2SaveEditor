using System;
using System.Collections.Generic;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Holds shared application data.
/// </summary>
public static class AppDataHelper
{
    public static event EventHandler? SelectedCharacterChanged;

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
                SelectedCharacterChanged?.Invoke(null, EventArgs.Empty);
            }
        }
    }
    /// <summary>
    /// Gets or sets the list of characters parsed from the save file.
    /// </summary>
    public static List<Character> Characters { get; set; } = [];
}