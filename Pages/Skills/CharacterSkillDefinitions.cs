using Sheltered2SaveEditor.Pages.Skills.Models;
using System.Collections.Immutable;

namespace Sheltered2SaveEditor.Features.Skills;

/// <summary>
/// Provides all immutable skill definitions (lookup tables) and lookup helpers.
/// </summary>
internal static class CharacterSkillDefinitions
{
    /// <summary>
    /// Gets an immutable array of strength skill definitions.
    /// </summary>
    internal static ImmutableArray<SkillDefinition> StrengthSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(0, 2, 3, "Kick", 3, "An attack which targets either arms or legs, dealing 200% base damage. Has a 50% chance of breaking the targeted limb."),
            new SkillDefinition(4, 2, 1, "Headbutt", 3, "An attack which targets the head. 100% chance to daze the target and deals 200% base damage."),
            new SkillDefinition(8, 2, 4, "Shoulder Barge", 3, "Charge at the target and smash into them with your shoulder, dealing 100% base damage. If there is a character in the back row behind them, they take 50% base damage, otherwise the target is pushed into the back row."),
            new SkillDefinition(12, 1, 3, "Backpack Weight Training", 3, "Increase carrying capacity by 5lbs."),
            new SkillDefinition(15, 1, 1, "Crush Windpipe", 3, "A jab to the target's windpipe, causing the winded status effect. Deals 100% base damage."),
            new SkillDefinition(19, 1, 2, "Poison Punch", 3, "An attack with a concealed poison weapon. Deals 100% base damage and causes the poison status effect."),
            new SkillDefinition(24, 3, 1, "Utility Specialist", 2, "Can equip 1 extra piece of equipment."),
            new SkillDefinition(25, 1, 7, "Imposing Physique", 1, "Has a 10% chance to cause the Fear status effect on opposing targets at the beginning of combat."),
            new SkillDefinition(28, 1, 6, "Blunt Force Specialisation", 3, "Increase the damage dealt by blunt weapons by 10%."),
            new SkillDefinition(30, 1, 5, "Inherent Strength", 3, "Gain 5% extra experience from using strength related exercise objects."),
            new SkillDefinition(31, 3, 2, "Exploding Heart Attack", 3, "An unarmed attack targetting the opponent's chest, dealing 200% base damage and inflicting the winded and fear status effects."),
            new SkillDefinition(37, 3, 3, "Thunderous Uppercut", 3, "An attack in an upwards direction targeting the opponent's jaw. Causes 100% base damage and the Dazed and Impaired vision status effects."),
            new SkillDefinition(41, 1, 4, "Pump Up", 1, "This character gains +5 strength. The boosted strength will wear off after the next turn."),
            new SkillDefinition(42, 2, 2, "Set Bone", 3, "Can attempt to cure a broken limb. Has a 25% chance of success.")
        );

    /// <summary>
    /// Gets an immutable array of dexterity skill definitions.
    /// </summary>
    internal static ImmutableArray<SkillDefinition> DexteritySkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(100, 2, 1, "Aimed Gunshot", 3, "Can make a gunshot with 15% increased accuracy."),
            new SkillDefinition(102, 1, 1, "Spray Gunshot", 3, "Can shoot a gun at random multiple targets until the clip is empty. Accuracy reduced by 30%."),
            new SkillDefinition(104, 3, 3, "CQC Training", 1, "Increases the base attack combo by 1."),
            new SkillDefinition(105, 1, 2, "Ranged Weapon Training", 3, "Increases ranged accuracy by 5% for all ranged attacks."),
            new SkillDefinition(106, 3, 2, "Disarm", 1, "An attack which deals a small amount of damage whilst simultaneously disarming the opponent."),
            new SkillDefinition(110, 1, 5, "Flick Sand Attack", 3, "Flicks sand into the eyes of the target, and follows up with a punch to the head dealing 100% base damage. Causes the impaired vision status effect."),
            new SkillDefinition(115, 2, 2, "Knick Artery", 3, "A single attack dealing 100% base damage and guaranteed to cause bleeding. Must have a bladed weapon equipped. Can stack the bleeding status effect with this skill."),
            new SkillDefinition(122, 1, 6, "Sleight Of Hand Attack", 3, "Performs an attack which deals 50% base damage, whilst simultaneously stealing an item from the target."),
            new SkillDefinition(127, 3, 1, "Backstab", 3, "Perform a single attack on a character in the back row of the opposing team. Causes 200% of the equipped weapons damage. Can only be used when this character is in the back row."),
            new SkillDefinition(131, 1, 3, "Fast Reflexes", 3, "Increase the chance of performing a counter attack by 10%."),
            new SkillDefinition(139, 2, 3, "Retreat Attack", 3, "A single attack dealing 100% base damage. This character then switches position with the character on the back row behind them. Can only be performed when on the front row and if there is a character standing behind them."),
            new SkillDefinition(143, 1, 4, "Blade Specialisation", 3, "Increase the damage dealt by bladed weapons by 10%.")
        );

    /// <summary>
    /// Gets an immutable array of intelligence skill definitions.
    /// </summary>
    internal static ImmutableArray<SkillDefinition> IntelligenceSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(200, 1, 2, "Focused", 3, "Increase accuracy of all melee attacks by 10%."),
            new SkillDefinition(201, 2, 2, "Advanced CPR Training", 1, "Increases the chance of successfully reviving a character using CPR from 50% to 75%."),
            new SkillDefinition(206, 3, 2, "Surgeon", 3, "Can attempt to cure a character's physical ailment, with a 50% chance of success."),
            new SkillDefinition(208, 1, 3, "Medical Training", 3, "Increase first-aid kit effectiveness by 25%."),
            new SkillDefinition(209, 2, 5, "Resourceful Healing", 1, "When using a first-aid kit there's a 10% chance it won't be used up."),
            new SkillDefinition(210, 2, 1, "Emergency Healing", 3, "Can perform a heal action to heal 25% of the target character's max health, so long as their health is less than 25%."),
            new SkillDefinition(213, 2, 3, "Knowledge Of Anatomy", 3, "Your advanced knowledge of anatomy provides an increase to melee damage by 10%."),
            new SkillDefinition(214, 1, 1, "Emergency Tourniquet", 1, "Can attempt to cure the bleeding status effect without requiring a bandage. 50% chance of working."),
            new SkillDefinition(224, 3, 4, "Improvised Explosive", 1, "Craft and throw an improvised explosive on the spot."),
            new SkillDefinition(229, 1, 4, "Tactician", 3, "Whilst conscious, all party members deal 5% more damage."),
            new SkillDefinition(230, 1, 7, "Distraction Tactics", 3, "The party has a 10% increased chance to escape from combat."),
            new SkillDefinition(234, 1, 6, "Thick Skinned", 3, "Reduce the amount of time insult mood modifiers last for by 10%."),
            new SkillDefinition(235, 3, 1, "Calculated One-Two", 3, "A double blow where the first attack is designed to draw attention away from the impactful second attack. The first attack deals 100% base damage. The second attack deals 200% base damage and is guaranteed to land if the first attack was successful."),
            new SkillDefinition(239, 1, 8, "Mental Fortifications", 3, "2.5% less chance of having a mental breakdown."),
            new SkillDefinition(240, 1, 5, "Putting On A Brave Face", 1, "Immune to injury mood modifiers."),
            new SkillDefinition(241, 2, 4, "Combat Analysis", 3, "XP gained from combat is increased by 50%."),
            new SkillDefinition(243, 3, 3, "Experiment", 1, "Can attempt to increase the max level cap of a single stat on another character. Has a 25% chance of success. If successful, that character can no longer be experimented on.")
        );

    /// <summary>
    /// Gets an immutable array of charisma skill definitions.
    /// </summary>
    internal static ImmutableArray<SkillDefinition> CharismaSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(300, 3, 2, "Rallying", 3, "Provides a passive +1 strength and +1 fortitude boost to other party members."),
            new SkillDefinition(301, 2, 2, "Marching Songs", 3, "Increases the travel speed of the party by 5%, when travelling on foot."),
            new SkillDefinition(302, 1, 3, "Bedside Manner", 3, "Characters healing in bed heal 10% faster whilst this character is in the shelter."),
            new SkillDefinition(304, 1, 4, "Motivator", 3, "25% increase to the crafting and fixing speeds whilst this character is in the shelter."),
            new SkillDefinition(306, 3, 1, "Silver Tongue", 1, "Can attempt to 'Talk your way out' of a faction encounter if you have a bad reputation with that faction group. 10% chance of working."),
            new SkillDefinition(309, 1, 2, "Inspiring", 3, "Increase the mood (positive mood modifier timers are slowed) of characters in the same room as this character by 5%."),
            new SkillDefinition(316, 1, 5, "Welcoming", 1, "Will never insult new recruits when having a conversation with them."),
            new SkillDefinition(319, 1, 6, "Production Manager", 3, "Generate resources 10% faster when generating resources from a production building on the map."),
            new SkillDefinition(320, 1, 7, "Convincing Voice", 3, "Increase the chance of successfully attracting a recruit or trader by 5%, when requesting them through the radio."),
            new SkillDefinition(324, 2, 1, "Confuse Opponent", 1, "This character attempts to confuse an opponent. Has a 75% chance of working. If successful then that character becomes dazed."),
            new SkillDefinition(326, 2, 4, "Mission Of Mercy", 1, "If travelling alone, or with other characters who have this skill, will not be attacked by other factions."),
            new SkillDefinition(327, 1, 1, "Soothing Words", 1, "Attempt to cure a character of Fear. Has a 50% chance of success."),
            new SkillDefinition(429, 2, 3, "Placebo Effect", 3, "Whilst this character is in combat and not unconscious, all damage dealt to other party members is reduced by 10%.")
        );

    /// <summary>
    /// Gets an immutable array of perception skill definitions.
    /// </summary>
    internal static ImmutableArray<SkillDefinition> PerceptionSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(402, 2, 1, "Taunt", 1, "Causes the target of the taunt to miss their next turn, but on their following turn they will target this character (or the character standing in front of them if they're in the back row), and have +1 strength."),
            new SkillDefinition(403, 3, 1, "Therapist", 1, "Can attempt to cure a character's mental trait with a 50% chance of success. Target needs to be resting in a bed."),
            new SkillDefinition(405, 1, 1, "Assess Opponent", 1, "Reveals information about the target."),
            new SkillDefinition(406, 1, 5, "Quick Study", 3, "Increase the speed at which this character reads books by 10%."),
            new SkillDefinition(410, 3, 4, "Demoralise", 1, "Attempts to demoralise target opponent, giving them a -2 to their strength and dexterity. 75% chance of not working."),
            new SkillDefinition(413, 2, 5, "Hunter", 3, "Gather resources 10% faster when hunting on a tile."),
            new SkillDefinition(416, 3, 3, "Automatic Repairing", 1, "Will automatically repair objects in the shelter which have reached 0% integrity (providing they are not currently performing another job)."),
            new SkillDefinition(419, 3, 2, "Return To Sender", 1, "50% chance that this character will notice an incoming grenade and intercept it, then throw it back at the opponents."),
            new SkillDefinition(420, 2, 3, "Eidetic Memory", 3, "Increase the experience gained from reading stat books by 5%."),
            new SkillDefinition(422, 3, 5, "Locate Weakpoint", 1, "50% chance of locating a weak point on the target, causing them to take 25% more damage for the rest of combat."),
            new SkillDefinition(424, 1, 6, "Expedited Healing", 1, "Using healing items costs 50% less stamina."),
            new SkillDefinition(425, 2, 2, "Autopsy", 3, "This character can perform an autopsy on a corpse (inside the shelter), providing them with 50 EXP in each of their six stats. Also provides a small amount of desperate meat."),
            new SkillDefinition(431, 1, 4, "Study Movements", 3, "After each round of combat this character gains a 2% increase to their melee accuracy (stacks up to five times)."),
            new SkillDefinition(432, 1, 3, "Unshakeable", 3, "50% smaller chance of gaining the Fear status effect."),
            new SkillDefinition(433, 1, 2, "Always Prepared", 3, "On the first round of combat this character has a 30% increased chance to perform a counter attack."),
            new SkillDefinition(507, 1, 7, "Poison Resilience", 3, "Poison deals 10% less damage."),
            new SkillDefinition(517, 2, 4, "Relishes A Challenge", 1, "If outnumbered in combat, this character gains +2 dexterity and deals 50% more damage.")
        );

    /// <summary>
    /// Gets an immutable array of fortitude skill definitions.
    /// </summary>
    internal static ImmutableArray<SkillDefinition> FortitudeSkillDefinitions { get; } =
        ImmutableArray.Create(
            new SkillDefinition(500, 1, 2, "Pain Resistance Training", 3, "Reduce the effects of broken limbs by 25%."),
            new SkillDefinition(501, 2, 2, "Hardened Skin", 3, "Reduce the amount of damage taken from attacks by 10%."),
            new SkillDefinition(502, 1, 3, "Shake It Off", 3, "25% less likely to be dazed."),
            new SkillDefinition(503, 3, 2, "Final Counter Down", 1, "If this character is about to take a hit which would put them on 0 health they perform a counter attack before becoming unconscious."),
            new SkillDefinition(504, 2, 3, "Valiant", 1, "When this character's health is below 25% they deal 100% more damage."),
            new SkillDefinition(505, 1, 4, "Iron Stomach", 3, "10% smaller chance of contracting food poisoning."),
            new SkillDefinition(506, 2, 1, "Blood Transfusion", 1, "Give 25% of your health to another party member (as long as your health is greater than 25%)."),
            new SkillDefinition(509, 3, 3, "Determined To Win", 1, "This character can't be killed by the bleeding status effect. If it would reduce their health to 0 they lose the bleeding status effect and their health goes to 1."),
            new SkillDefinition(510, 3, 4, "Hardy", 1, "If this character would take a hit which would kill them it instead leaves them on 1 health (unless their health is already 1)."),
            new SkillDefinition(512, 2, 6, "Warm Blooded", 3, "Can resist the cold by 1 degree extra before taking damage."),
            new SkillDefinition(513, 1, 8, "Home Turf Advantage", 3, "If fighting inside the shelter this character deals 10% more damage."),
            new SkillDefinition(515, 1, 9, "Unarmed Specialisation", 3, "Unarmed attacks deal 20% more damage."),
            new SkillDefinition(516, 3, 5, "Patch Yourself Up", 3, "This character heals 5% of their maximum health at the end of combat."),
            new SkillDefinition(518, 1, 6, "Fast Healer", 3, "Heals 5% faster when healing in a bed."),
            new SkillDefinition(519, 1, 1, "Extract Poison", 1, "Attempt to cure a character who is poisoned. Has a 50% chance of success."),
            new SkillDefinition(521, 2, 4, "Shared Healing", 1, "When this character heals another character they also replenish 5% of their own max health."),
            new SkillDefinition(522, 1, 7, "Tireless Engineering", 3, "Reduce the rate at which Tiredness increases whilst repairing or crafting by 10%."),
            new SkillDefinition(523, 2, 5, "Working Long Hours", 1, "This character will not pass out if their tiredness stat reaches 100%."),
            new SkillDefinition(524, 1, 5, "Strong Immune System", 3, "Reduce the chance of developing an infection by 10%."),
            new SkillDefinition(526, 3, 1, "Rage Attack", 3, "A single rage fuelled attack which is guaranteed to land and deals 125% base damage. Also deals 25% base damage to this character.")
        );

    /// <summary>
    /// Gets an immutable array of all skill trees.
    /// </summary>
    /// <remarks>
    /// Each <see cref="SkillTreeDefinition"/> groups a specific skill tree type with its associated immutable collection of <see cref="SkillDefinition"/> items.
    /// </remarks>
    internal static ImmutableArray<SkillTreeDefinition> AllSkillTrees { get; } =
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
    internal static ImmutableDictionary<SkillTreeType, ImmutableArray<SkillDefinition>> SkillTreeDictionary { get; } =
        AllSkillTrees.ToImmutableDictionary(tree => tree.Type, tree => tree.Skills);

    /// <summary>
    /// Gets a skill definition from the strength skill tree by its key.
    /// </summary>
    /// <param name="skillKey">The unique identifier of the skill.</param>
    /// <returns>The skill definition if found; otherwise, null.</returns>
    internal static SkillDefinition? GetStrengthSkillDefinitionByKey(int skillKey)
    {
        foreach (SkillDefinition definition in StrengthSkillDefinitions)
        {
            if (definition.SkillKey == skillKey)
                return definition;
        }
        return null;
    }

    /// <summary>
    /// Gets a skill definition from the dexterity skill tree by its key.
    /// </summary>
    /// <param name="skillKey">The unique identifier of the skill.</param>
    /// <returns>The skill definition if found; otherwise, null.</returns>
    internal static SkillDefinition? GetDexteritySkillDefinitionByKey(int skillKey)
    {
        foreach (SkillDefinition definition in DexteritySkillDefinitions)
        {
            if (definition.SkillKey == skillKey)
                return definition;
        }
        return null;
    }

    /// <summary>
    /// Gets a skill definition from the intelligence skill tree by its key.
    /// </summary>
    /// <param name="skillKey">The unique identifier of the skill.</param>
    /// <returns>The skill definition if found; otherwise, null.</returns>
    internal static SkillDefinition? GetIntelligenceSkillDefinitionByKey(int skillKey)
    {
        foreach (SkillDefinition definition in IntelligenceSkillDefinitions)
        {
            if (definition.SkillKey == skillKey)
                return definition;
        }
        return null;
    }

    /// <summary>
    /// Gets a skill definition from the charisma skill tree by its key.
    /// </summary>
    /// <param name="skillKey">The unique identifier of the skill.</param>
    /// <returns>The skill definition if found; otherwise, null.</returns>
    internal static SkillDefinition? GetCharismaSkillDefinitionByKey(int skillKey)
    {
        foreach (SkillDefinition definition in CharismaSkillDefinitions)
        {
            if (definition.SkillKey == skillKey)
                return definition;
        }
        return null;
    }

    /// <summary>
    /// Gets a skill definition from the perception skill tree by its key.
    /// </summary>
    /// <param name="skillKey">The unique identifier of the skill.</param>
    /// <returns>The skill definition if found; otherwise, null.</returns>
    internal static SkillDefinition? GetPerceptionSkillDefinitionByKey(int skillKey)
    {
        foreach (SkillDefinition definition in PerceptionSkillDefinitions)
        {
            if (definition.SkillKey == skillKey)
                return definition;
        }
        return null;
    }

    /// <summary>
    /// Gets a skill definition from the fortitude skill tree by its key.
    /// </summary>
    /// <param name="skillKey">The unique identifier of the skill.</param>
    /// <returns>The skill definition if found; otherwise, null.</returns>
    internal static SkillDefinition? GetFortitudeSkillDefinitionByKey(int skillKey)
    {
        foreach (SkillDefinition definition in FortitudeSkillDefinitions)
        {
            if (definition.SkillKey == skillKey)
                return definition;
        }
        return null;
    }
}