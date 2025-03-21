using Sheltered2SaveEditor.Pages.Characters.Models;
using System.Xml.Linq;

namespace Sheltered2SaveEditor.Pages.Characters;

/// <summary>
/// Provides methods for parsing character data from save files.
/// </summary>
internal static class CharacterParser
{
    /// <summary>
    /// Parses the decrypted XML content and extracts character data.
    /// </summary>
    /// <param name="decryptedContent">The decrypted XML content of the save file.</param>
    /// <returns>A collection of Character objects parsed from the content.</returns>
    internal static IReadOnlyList<Character> ParseCharacters(string decryptedContent)
    {
        List<Character> characters = [];

        try
        {
            XDocument doc = XDocument.Parse(decryptedContent);
            XElement? familyMembers = doc.Root?.Element("FamilyMembers");
            if (familyMembers is not null)
                foreach (XElement memberElement in familyMembers.Elements())
                {
                    Character character = new()
                    {
                        FirstName = memberElement.Element("firstName")?.Value ?? string.Empty,
                        LastName = memberElement.Element("lastName")?.Value ?? string.Empty,
                        CurrentHealth = int.TryParse(memberElement.Element("health")?.Value, out int health) ? health : 0,
                        MaxHealth = int.TryParse(memberElement.Element("maxHealth")?.Value, out int maxHealth) ? maxHealth : 0,
                        Interacting = bool.TryParse(memberElement.Element("interacting")?.Value, out bool interacting) && interacting,
                        InteractingWithObj = bool.TryParse(memberElement.Element("interactingWithObj")?.Value, out bool interactingWithObj) && interactingWithObj,
                        HasBeenDefibbed = bool.TryParse(memberElement.Element("hasBeenDefibbed")?.Value, out bool defibbed) && defibbed,
                        PassedOut = bool.TryParse(memberElement.Element("PassedOut")?.Value, out bool passedout) && passedout,
                        IsUnconscious = bool.TryParse(memberElement.Element("isUnconscious")?.Value, out bool unconscious) && unconscious
                    };

                    // Parse Stats
                    XElement? baseStatsElement = memberElement.Element("BaseStats");
                    if (baseStatsElement is not null)
                    {
                        string[] statNames = ["Strength", "Dexterity", "Intelligence", "Charisma", "Perception", "Fortitude"];

                        foreach (string statName in statNames)
                        {
                            XElement? statElement = baseStatsElement.Element(statName);
                            if (statElement is not null)
                            {
                                int level = int.TryParse(statElement.Element("level")?.Value, out int lvl) ? lvl : 1;

                                switch (statName)
                                {
                                    case "Strength":
                                        character.Strength.Level = level;
                                        break;
                                    case "Dexterity":
                                        character.Dexterity.Level = level;
                                        break;
                                    case "Intelligence":
                                        character.Intelligence.Level = level;
                                        break;
                                    case "Charisma":
                                        character.Charisma.Level = level;
                                        break;
                                    case "Perception":
                                        character.Perception.Level = level;
                                        break;
                                    case "Fortitude":
                                        character.Fortitude.Level = level;
                                        break;
                                }
                            }
                        }
                    }

                    // Parse Skills for Strength (apply similar logic for other skill trees if needed)
                    XElement? professionElement = memberElement.Element("Profession");
                    if (professionElement is not null)
                    {
                        XElement? strengthSkillsElement = professionElement.Element("StrengthSkills")?.Element("strengthSkills");
                        if (strengthSkillsElement is not null)
                        {
                            int size = int.TryParse(strengthSkillsElement.Attribute("size")?.Value, out int sSize) ? sSize : 0;
                            if (size > 0)
                                foreach (XElement skillElement in strengthSkillsElement.Elements())
                                {
                                    XElement? skillKeyElement = skillElement.Element("skillKey");
                                    XElement? skillLevelElement = skillElement.Element("skillLevel");
                                    if (skillKeyElement is not null && skillLevelElement is not null)
                                    {
                                        // Currently commented out in original code - keeping structure
                                        /*
                                        int skillKey = int.TryParse(skillKeyElement.Value, out int sKey) ? sKey : 0;
                                        int skillLevel = int.TryParse(skillLevelElement.Value, out int sLevel) ? sLevel : 0;

                                        // Use the immutable lookup to get the definition.
                                        SkillDefinition? skillDef = CharacterSkillDefinitions.GetStrengthSkillDefinitionByKey(skillKey);
                                        if (skillDef is not null)
                                        {
                                            // Create a mutable SkillInstance with the current level.
                                            character.StrengthSkills.Add(new SkillInstance(skillDef, skillLevel));
                                        }
                                        */
                                    }
                                }
                        }
                    }

                    characters.Add(character);
                }
        }
        catch (System.Xml.XmlException ex)
        {
            throw new InvalidDataException("Failed to parse the decrypted content into valid XML.", ex);
        }

        return characters.AsReadOnly();
    }
}