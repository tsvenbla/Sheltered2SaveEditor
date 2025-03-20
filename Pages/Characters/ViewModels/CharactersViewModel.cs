using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Data;
using Sheltered2SaveEditor.Core.Models;
using Sheltered2SaveEditor.Helpers;
using System.Collections.ObjectModel;

namespace Sheltered2SaveEditor.Features.Characters.ViewModels;

/// <summary>
/// ViewModel for managing and presenting the list of characters and related operations.
/// Implements functionality for selecting a character and maximizing its stats.
/// </summary>
[Bindable]
internal sealed partial class CharactersViewModel : ObservableObject
{
    /// <summary>
    /// Gets the collection of characters displayed in the UI.
    /// </summary>
    internal ObservableCollection<Character> Characters { get; } = [.. AppDataHelper.Characters];

    private Character? _selectedCharacter;

    /// <summary>
    /// Gets or sets the currently selected character.
    /// </summary>
    internal Character? SelectedCharacter
    {
        get => _selectedCharacter;
        set
        {
            if (SetProperty(ref _selectedCharacter, value))
            {
                // Update the global selected character.
                AppDataHelper.SelectedCharacter = value;
                // Notify that the IsCharacterSelected property has changed.
                OnPropertyChanged(nameof(IsCharacterSelected));
                // Notify that the command's executable state may have changed.
                MaximizeStatsCommand.NotifyCanExecuteChanged();
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether a character is currently selected.
    /// </summary>
    internal bool IsCharacterSelected => SelectedCharacter != null;

    private string _feedback = string.Empty;

    /// <summary>
    /// Gets or sets the feedback message to display in the UI.
    /// </summary>
    internal string Feedback
    {
        get => _feedback;
        set => SetProperty(ref _feedback, value);
    }

    /// <summary>
    /// Gets the command that maximizes the stats of the selected character.
    /// </summary>
    internal RelayCommand MaximizeStatsCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharactersViewModel"/> class.
    /// Sets up the commands and default values.
    /// </summary>
    internal CharactersViewModel() => MaximizeStatsCommand = new RelayCommand(MaximizeStats, () => SelectedCharacter != null);

    /// <summary>
    /// Maximizes all stats of the selected character by setting each stat's level to 20.
    /// </summary>
    internal void MaximizeStats()
    {
        bool allMaxed = true;

        Stat[] stats =
        [
            SelectedCharacter!.Strength,
            SelectedCharacter!.Dexterity,
            SelectedCharacter!.Intelligence,
            SelectedCharacter!.Charisma,
            SelectedCharacter!.Perception,
            SelectedCharacter!.Fortitude
        ];

        foreach (Stat stat in stats)
        {
            if (stat.Level < 20)
            {
                stat.Level = 20;
                allMaxed = false;
            }
        }

        Feedback = allMaxed ? "All stats are already at maximum level." : "All stats have been maximized.";
    }
}