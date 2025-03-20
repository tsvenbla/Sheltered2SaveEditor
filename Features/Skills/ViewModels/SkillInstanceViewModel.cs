using CommunityToolkit.Mvvm.ComponentModel;
using Sheltered2SaveEditor.Core.Models;
using System;

namespace Sheltered2SaveEditor.Features.Skills.ViewModels;

/// <summary>
/// ViewModel representing a single skill instance, including its definition and current assigned level.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SkillInstanceViewModel"/> class.
/// </remarks>
/// <param name="skillDefinition">The skill definition.</param>
/// <param name="skillTree">The skill tree this skill belongs to.</param>
/// <param name="currentLevel">The current level of the skill.</param>
internal sealed partial class SkillInstanceViewModel(SkillDefinition skillDefinition, string skillTree, int currentLevel) : ObservableObject
{
    private readonly SkillDefinition _skillDefinition = skillDefinition ?? throw new ArgumentNullException(nameof(skillDefinition));
    private int _currentLevel = Math.Clamp(currentLevel, 0, skillDefinition.MaxLevel);

    /// <summary>
    /// Gets the unique identifier for this skill.
    /// </summary>
    internal int SkillKey => _skillDefinition.SkillKey;

    /// <summary>
    /// Gets the name of the skill.
    /// </summary>
    internal string Name => _skillDefinition.Name;

    /// <summary>
    /// Gets the maximum level this skill can reach.
    /// </summary>
    internal int MaxLevel => _skillDefinition.MaxLevel;

    /// <summary>
    /// Gets the tier of this skill (1-3).
    /// </summary>
    internal int Tier => _skillDefinition.Tier;

    /// <summary>
    /// Gets the display order within its tier.
    /// </summary>
    internal int DisplayOrder => _skillDefinition.DisplayOrder;

    /// <summary>
    /// Gets or sets the current level of this skill.
    /// </summary>
    internal int CurrentLevel
    {
        get => _currentLevel;
        set => SetProperty(ref _currentLevel, Math.Clamp(value, 0, MaxLevel));
    }

    /// <summary>
    /// Gets the path to the image for this skill.
    /// </summary>
    internal string ImageSource => $"ms-appx:///Assets/Skills/Skill{_skillTree}{Name.Replace(" ", string.Empty)}.png";

    private readonly string _skillTree = skillTree ?? throw new ArgumentNullException(nameof(skillTree));
}