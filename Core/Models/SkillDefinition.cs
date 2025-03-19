namespace Sheltered2SaveEditor.Core.Models;

/// <summary>
/// Represents an immutable definition for a skill, including its unique key, tier, display order, name, maximum level, and tooltip.
/// </summary>
/// <param name="SkillKey">A unique identifier for the skill.</param>
/// <param name="Tier">The tier level of the skill (from 1 to 3).</param>
/// <param name="DisplayOrder">The order in which the skill appears within its tier.</param>
/// <param name="Name">The display name of the skill.</param>
/// <param name="MaxLevel">The maximum level attainable for the skill.</param>
/// <param name="ToolTip">A description or tooltip for the skill.</param>
internal record SkillDefinition(int SkillKey, int Tier, int DisplayOrder, string Name, int MaxLevel, string ToolTip);