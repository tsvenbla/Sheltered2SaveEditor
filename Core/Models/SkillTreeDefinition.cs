using Sheltered2SaveEditor.Core.Enums;
using System.Collections.Immutable;

namespace Sheltered2SaveEditor.Core.Models;

/// <summary>
/// Represents an immutable definition for a skill tree, including its type and the associated skills.
/// </summary>
/// <param name="Type">The type of the skill tree.</param>
/// <param name="Skills">
/// An <see cref="ImmutableArray{T}"/> of <see cref="SkillDefinition"/> items representing the skills in this tree.
/// </param>
public record SkillTreeDefinition(SkillTreeType Type, ImmutableArray<SkillDefinition> Skills);