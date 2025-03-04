using System.Collections.Immutable;

namespace Sheltered2SaveEditor.Helpers;

/// <summary>
/// Represents an immutable definition for a skill, including its key, name, and maximum level.
/// These values are hard-coded and never change.
/// </summary>
/// <param name="SkillKey">A unique identifier for the skill.</param>
/// <param name="Name">The display name of the skill.</param>
/// <param name="MaxLevel">The maximum level attainable for the skill.</param>
public record SkillDefinition(int SkillKey, string Name, int MaxLevel);

/// <summary>
/// Specifies the different types of skill trees available.
/// </summary>
public enum SkillTreeType
{
    /// <summary>
    /// The Strength skill tree.
    /// </summary>
    Strength,
    /// <summary>
    /// The Dexterity skill tree.
    /// </summary>
    Dexterity,
    /// <summary>
    /// The Intelligence skill tree.
    /// </summary>
    Intelligence,
    /// <summary>
    /// The Charisma skill tree.
    /// </summary>
    Charisma,
    /// <summary>
    /// The Perception skill tree.
    /// </summary>
    Perception,
    /// <summary>
    /// The Fortitude skill tree.
    /// </summary>
    Fortitude
}

/// <summary>
/// Represents an immutable definition for a skill tree, including its type and the associated skills.
/// </summary>
/// <param name="Type">The type of the skill tree.</param>
/// <param name="Skills">
/// An <see cref="ImmutableArray{T}"/> of <see cref="SkillDefinition"/> items representing the skills in this tree.
/// </param>
public record SkillTreeDefinition(SkillTreeType Type, ImmutableArray<SkillDefinition> Skills);

/// <summary>
/// Provides all immutable skill definitions (lookup tables) and lookup helpers.
/// </summary>
public static class CharacterSkillDefinitions
{
    /// <summary>
    /// Gets an immutable array of strength skill definitions.
    /// </summary>
    public static ImmutableArray<SkillDefinition> StrengthSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(0, "Kick", 3),
            new SkillDefinition(4, "Headbutt", 3),
            new SkillDefinition(8, "Shoulder Barge", 3),
            new SkillDefinition(12, "Backpack Weight Training", 3),
            new SkillDefinition(15, "Crush Windpipe", 3),
            new SkillDefinition(19, "Poison Punch", 3),
            new SkillDefinition(24, "Utility Specialist", 2),
            new SkillDefinition(25, "Imposing Physique", 1),
            new SkillDefinition(28, "Blunt Force Specialisation", 3),
            new SkillDefinition(30, "Inherent Strength", 3),
            new SkillDefinition(31, "Exploding Heart Attack", 3),
            new SkillDefinition(37, "Thunderous Uppercut", 3),
            new SkillDefinition(41, "Pump Up", 1),
            new SkillDefinition(42, "Set Bone", 3)
        );

    /// <summary>
    /// Gets an immutable array of dexterity skill definitions.
    /// </summary>
    public static ImmutableArray<SkillDefinition> DexteritySkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(100, "Aimed Gunshot", 3),
            new SkillDefinition(102, "Spray Gunshot", 3),
            new SkillDefinition(104, "CQC Training", 1),
            new SkillDefinition(105, "Ranged Weapon Training", 3),
            new SkillDefinition(106, "Disarm", 1),
            new SkillDefinition(110, "Flick Sand Attack", 3),
            new SkillDefinition(115, "Knick Artery", 3),
            new SkillDefinition(122, "Sleight Of Hand Attack", 3),
            new SkillDefinition(127, "Backstab", 3),
            new SkillDefinition(131, "Fast Reflexes", 3),
            new SkillDefinition(139, "Retreat Attack", 3),
            new SkillDefinition(143, "Blade Specialisation", 3)
        );

    /// <summary>
    /// Gets an immutable array of intelligence skill definitions.
    /// </summary>
    public static ImmutableArray<SkillDefinition> IntelligenceSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(200, "Focused", 3),
            new SkillDefinition(201, "Advanced CPR Training", 1),
            new SkillDefinition(206, "Surgeon", 3),
            new SkillDefinition(208, "Medical Training", 3),
            new SkillDefinition(209, "Resourceful Healing", 1),
            new SkillDefinition(210, "Emergency Healing", 3),
            new SkillDefinition(213, "Knowledge Of Anatomy", 3),
            new SkillDefinition(214, "Emergency Tourniquet", 1),
            new SkillDefinition(224, "Improvised Explosive", 1),
            new SkillDefinition(229, "Tactician", 3),
            new SkillDefinition(230, "Distraction Tactics", 3),
            new SkillDefinition(234, "Thick Skinned", 3),
            new SkillDefinition(235, "Calculated One-Two", 3),
            new SkillDefinition(239, "Mental Fortifications", 3),
            new SkillDefinition(240, "Putting On A Brave Face", 1),
            new SkillDefinition(241, "Combat Analysis", 3),
            new SkillDefinition(243, "Experiment", 1)
        );

    /// <summary>
    /// Gets an immutable array of charisma skill definitions.
    /// </summary>
    public static ImmutableArray<SkillDefinition> CharismaSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(300, "Rallying", 3),
            new SkillDefinition(301, "Marching Songs", 3),
            new SkillDefinition(302, "Bedside Manner", 3),
            new SkillDefinition(304, "Motivator", 3),
            new SkillDefinition(306, "Silver Tongue", 1),
            new SkillDefinition(309, "Inspiring", 3),
            new SkillDefinition(316, "Welcoming", 1),
            new SkillDefinition(319, "Production Manager", 3),
            new SkillDefinition(320, "Convincing Voice", 3),
            new SkillDefinition(324, "Confuse Opponent", 1),
            new SkillDefinition(326, "Mission Of Mercy", 1),
            new SkillDefinition(327, "Soothing Words", 1),
            new SkillDefinition(429, "Placebo Effect", 3)
        );

    /// <summary>
    /// Gets an immutable array of perception skill definitions.
    /// </summary>
    public static ImmutableArray<SkillDefinition> PerceptionSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(402, "Taunt", 1),
            new SkillDefinition(403, "Therapist", 1),
            new SkillDefinition(405, "Assess Opponent", 1),
            new SkillDefinition(406, "Quick Study", 3),
            new SkillDefinition(410, "Demoralise", 1),
            new SkillDefinition(413, "Hunter", 3),
            new SkillDefinition(416, "Automatic Repairing", 1),
            new SkillDefinition(419, "Return To Sender", 1),
            new SkillDefinition(420, "Eidetic Memory", 3),
            new SkillDefinition(422, "Locate Weakpoint", 1),
            new SkillDefinition(424, "Expedited Healing", 1),
            new SkillDefinition(425, "Autopsy", 3),
            new SkillDefinition(431, "Study Movements", 3),
            new SkillDefinition(432, "Unshakeable", 3),
            new SkillDefinition(433, "Always Prepared", 3),
            new SkillDefinition(507, "Poison Resilience", 3),
            new SkillDefinition(517, "Relishes A Challenge", 1)
        );

    /// <summary>
    /// Gets an immutable array of fortitude skill definitions.
    /// </summary>
    public static ImmutableArray<SkillDefinition> FortitudeSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(500, "Pain Resistance Training", 3),
            new SkillDefinition(501, "Hardened Skin", 3),
            new SkillDefinition(502, "Shake It Off", 3),
            new SkillDefinition(503, "Final Counter Down", 1),
            new SkillDefinition(504, "Valiant", 1),
            new SkillDefinition(505, "Iron Stomach", 3),
            new SkillDefinition(506, "Blood Transfusion", 1),
            new SkillDefinition(509, "Determined To Win", 1),
            new SkillDefinition(510, "Hardy", 1),
            new SkillDefinition(512, "Warm Blooded", 3),
            new SkillDefinition(513, "Home Turf Advantage", 3),
            new SkillDefinition(515, "Unarmed Specialisation", 3),
            new SkillDefinition(516, "Patch Yourself Up", 3),
            new SkillDefinition(518, "Fast Healer", 3),
            new SkillDefinition(519, "Extract Poison", 1),
            new SkillDefinition(521, "Shared Healing", 1),
            new SkillDefinition(522, "Tireless Engineering", 3),
            new SkillDefinition(523, "Working Long Hours", 1),
            new SkillDefinition(524, "Strong Immune System", 3),
            new SkillDefinition(526, "Rage Attack", 3)
        );

    /// <summary>
    /// Gets an immutable array of all skill trees.
    /// </summary>
    /// <remarks>
    /// Each <see cref="SkillTreeDefinition"/> groups a specific skill tree type with its associated immutable collection of <see cref="SkillDefinition"/> items.
    /// </remarks>
    public static ImmutableArray<SkillTreeDefinition> AllSkillTrees { get; } =
        ImmutableArray.Create(
            new SkillTreeDefinition(SkillTreeType.Strength, StrengthSkillDefinitions),
            new SkillTreeDefinition(SkillTreeType.Dexterity, DexteritySkillDefinitions),
            new SkillTreeDefinition(SkillTreeType.Intelligence, IntelligenceSkillDefinitions),
            new SkillTreeDefinition(SkillTreeType.Charisma, CharismaSkillDefinitions),
            new SkillTreeDefinition(SkillTreeType.Perception, PerceptionSkillDefinitions),
            new SkillTreeDefinition(SkillTreeType.Fortitude, FortitudeSkillDefinitions)
        );

    /// <summary>
    /// Gets an immutable dictionary for quick lookup of skill trees by their <see cref="SkillTreeType"/>.
    /// </summary>
    /// <remarks>
    /// The dictionary is built from <see cref="AllSkillTrees"/> and maps each skill tree type to its immutable array of <see cref="SkillDefinition"/> items.
    /// </remarks>
    public static ImmutableDictionary<SkillTreeType, ImmutableArray<SkillDefinition>> SkillTreeDictionary { get; } =
        AllSkillTrees.ToImmutableDictionary(tree => tree.Type, tree => tree.Skills);
}