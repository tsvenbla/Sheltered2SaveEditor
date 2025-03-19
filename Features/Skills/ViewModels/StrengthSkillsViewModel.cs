using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sheltered2SaveEditor.Core.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sheltered2SaveEditor.Features.Skills.ViewModels;

/// <summary>
/// ViewModel for managing the Strength skill tree.
/// </summary>
internal partial class StrengthSkillsViewModel : ObservableObject
{
    /// <summary>
    /// Gets the collection of skill instances for the Strength skill tree.
    /// </summary>
    internal ObservableCollection<SkillInstanceViewModel> Skills { get; } = [];

    /// <summary>
    /// Gets the skills grouped by their tier (1 to 3) in ascending order.
    /// </summary>
    internal IEnumerable<IGrouping<int, SkillInstanceViewModel>> GroupedSkills =>
        (IEnumerable<IGrouping<int, SkillInstanceViewModel>>)Skills.GroupBy(skill => skill.Tier)
              .OrderBy(g => g.Key)
              .Select(g => g.OrderBy(skill => skill.DisplayOrder));

    /// <summary>
    /// Gets the command that maximizes all skills in the Strength skill tree.
    /// </summary>
    internal RelayCommand MaximizeSkillsCommand { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="StrengthSkillsViewModel"/> class.
    /// Loads the strength skill definitions from the lookup table.
    /// </summary>
    internal StrengthSkillsViewModel()
    {
        ImmutableArray<SkillDefinition> definitions = CharacterSkillDefinitions.StrengthSkillDefinitions;
        foreach (SkillDefinition def in definitions)
        {
            // Initialize each skill with a current level of 0.
            Skills.Add(new SkillInstanceViewModel(def, "Strength", 0));
        }
        MaximizeSkillsCommand = new RelayCommand(MaximizeSkills);
    }

    /// <summary>
    /// Maximizes all skills in the Strength skill tree by setting each skill's current level to its maximum.
    /// </summary>
    internal void MaximizeSkills()
    {
        foreach (SkillInstanceViewModel skill in Skills)
        {
            skill.CurrentLevel = skill.MaxLevel;
        }
    }
}