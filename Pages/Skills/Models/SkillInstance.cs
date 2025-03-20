using CommunityToolkit.Mvvm.ComponentModel;
using Sheltered2SaveEditor.Pages.Skills.Models;

namespace Sheltered2SaveEditor.Core.Models;

/// <summary>
/// Represents a skill instance with its current level for a character.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SkillInstance"/> class.
/// </remarks>
/// <param name="definition">The skill definition.</param>
/// <param name="level">The initial level of the skill.</param>
internal sealed partial class SkillInstance(SkillDefinition definition, int level = 0) : ObservableObject
{
    private readonly SkillDefinition _definition = definition ?? throw new ArgumentNullException(nameof(definition));
    private int _level = Math.Clamp(level, 0, definition.MaxLevel);

    /// <summary>
    /// Gets the unique identifier for this skill.
    /// </summary>
    internal int SkillKey => _definition.SkillKey;

    /// <summary>
    /// Gets the name of the skill.
    /// </summary>
    internal string Name => _definition.Name;

    /// <summary>
    /// Gets the tooltip or description of the skill.
    /// </summary>
    internal string ToolTip => _definition.ToolTip;

    /// <summary>
    /// Gets the maximum level this skill can reach.
    /// </summary>
    internal int MaxLevel => _definition.MaxLevel;

    /// <summary>
    /// Gets the tier of this skill (1-3).
    /// </summary>
    internal int Tier => _definition.Tier;

    /// <summary>
    /// Gets the display order within its tier.
    /// </summary>
    internal int DisplayOrder => _definition.DisplayOrder;

    /// <summary>
    /// Gets or sets the current level of this skill.
    /// </summary>
    /// <remarks>
    /// The value is clamped between 0 and the skill's maximum level.
    /// </remarks>
    internal int Level
    {
        get => _level;
        set => SetProperty(ref _level, Math.Clamp(value, 0, MaxLevel));
    }
}