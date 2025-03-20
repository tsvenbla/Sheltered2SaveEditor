using System.Collections.Immutable;

namespace Sheltered2SaveEditor.Pages.Skills.Models;

/// <summary>
/// Represents an immutable definition for a skill tree, including its type and the associated skills.
/// </summary>
/// <param name="Type">The type of the skill tree.</param>
/// <param name="Skills">
/// An <see cref="ImmutableArray{T}"/> of <see cref="SkillDefinition"/> items representing the skills in this tree.
/// </param>
internal sealed record SkillTreeDefinition(SkillTreeType Type, ImmutableArray<SkillDefinition> Skills);