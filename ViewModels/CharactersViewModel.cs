using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Sheltered2SaveEditor.Helpers;
using Sheltered2SaveEditor.Pages.Skills;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Sheltered2SaveEditor.ViewModels;

/// <summary>
/// ViewModel for managing and presenting the list of characters and related operations.
/// Implements functionality for selecting a character and maximizing its stats.
/// </summary>
/// <remarks>
/// This ViewModel uses CommunityToolkit.Mvvm to simplify property change notifications and command implementations.
/// </remarks>
public partial class CharactersViewModel : ObservableObject
{
    private ObservableCollection<Character> _characters = [.. AppDataHelper.Characters];

    /// <summary>
    /// Gets or sets the collection of characters displayed in the UI.
    /// </summary>
    /// <value>
    /// An observable collection of <see cref="Character"/> objects.
    /// </value>
    public ObservableCollection<Character> Characters
    {
        get => _characters;
        set => SetProperty(ref _characters, value);
    }

    private Character? _selectedCharacter;

    /// <summary>
    /// Gets or sets the currently selected character.
    /// </summary>
    /// <value>
    /// The selected <see cref="Character"/>. If no character is selected, the value is <c>null</c>.
    /// </value>
    /// <remarks>
    /// When this property is set, it updates the global <see cref="AppDataHelper.SelectedCharacter"/>,
    /// notifies dependent properties, and triggers the re-evaluation of the <see cref="MaximizeStatsCommand"/>.
    /// </remarks>
    public Character? SelectedCharacter
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
    /// <value>
    /// <c>true</c> if a character is selected; otherwise, <c>false</c>.
    /// </value>
    public bool IsCharacterSelected => SelectedCharacter != null;

    private string _feedback = string.Empty;

    /// <summary>
    /// Gets or sets the feedback message to display in the UI.
    /// </summary>
    /// <value>
    /// A string containing user feedback.
    /// </value>
    public string Feedback
    {
        get => _feedback;
        set => SetProperty(ref _feedback, value);
    }

    /// <summary>
    /// Gets the command that maximizes the stats of the selected character.
    /// </summary>
    /// <value>
    /// A <see cref="RelayCommand"/> that, when executed, sets all stats of the selected character to the maximum level.
    /// The command is enabled only when a character is selected.
    /// </value>
    public RelayCommand MaximizeStatsCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CharactersViewModel"/> class.
    /// Sets up the commands and default values.
    /// </summary>
    public CharactersViewModel() => MaximizeStatsCommand = new RelayCommand(MaximizeStats, () => SelectedCharacter != null);

    /// <summary>
    /// Maximizes all stats of the selected character by setting each stat's level to 20.
    /// </summary>
    /// <remarks>
    /// This method iterates through the character's stats. If any stat is below 20, it is set to 20.
    /// The <see cref="Feedback"/> message is updated to indicate whether any changes were made.
    /// </remarks>
    public void MaximizeStats()
    {
        bool allMaxed = true;

        List<Stat> stats =
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