using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Helpers;
using System;

namespace Sheltered2SaveEditor.Pages;

public sealed partial class CharactersPage : Page
{
    private readonly int previousSelectedIndex = 0;

    public CharactersPage()
    {
        InitializeComponent();
        LoadCharacters();
    }

    private void LoadCharacters() =>
        CharacterComboBox.ItemsSource = AppDataHelper.Characters;

    private void CharacterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        MaxStatsButton.IsEnabled = CharacterComboBox.SelectedItem != null;

        if (sender is ComboBox { SelectedItem: Character selectedCharacter })
        {
            AppDataHelper.SelectedCharacter = selectedCharacter;
            DataContext = selectedCharacter;
        }
    }

    private void MaxStatsButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Character character = (Character)DataContext;
            bool allMaxed = true;

            // List of all stat properties
            Stat[] stats =
            [
                character.Strength,
                character.Dexterity,
                character.Intelligence,
                character.Charisma,
                character.Perception,
                character.Fortitude
            ];

            // Set all stats to max level
            foreach (Stat? stat in stats)
            {
                if (stat.Level < 20)
                {
                    stat.Level = 20;
                    allMaxed = false;
                }
            }

            // Update InfoBar based on the result
            MaxStatsButton_Flyout.Text = allMaxed ? "All stats are already at maximum level." : "All stats have been maximized.";
        }
        catch (Exception ex)
        {
            MaxStatsButton_Flyout.Text = $"An error occurred: {ex.Message}";
        }
    }
    /**
    private void SelectorBar_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        SelectorBarItem selectedItem = sender.SelectedItem;
        int currentSelectedIndex = sender.Items.IndexOf(selectedItem);
        Type pageType = currentSelectedIndex switch
        {
            0 => typeof(SelectorBarItemStrength),
            1 => typeof(SelectorBarItemDexterity),
            2 => typeof(SelectorBarItemIntelligence),
            3 => typeof(SelectorBarItemCharisma),
            4 => typeof(SelectorBarItemPerception),
            5 => typeof(SelectorBarItemFortitude),
            _ => typeof(SelectorBarItemStrength),
        };
        SlideNavigationTransitionEffect slideNavigationTransitionEffect = currentSelectedIndex - previousSelectedIndex > 0 ? SlideNavigationTransitionEffect.FromRight : SlideNavigationTransitionEffect.FromLeft;

        _ = SkillsContentFrame.Navigate(pageType, null, new SlideNavigationTransitionInfo() { Effect = slideNavigationTransitionEffect });

        previousSelectedIndex = currentSelectedIndex;
    }**/
}