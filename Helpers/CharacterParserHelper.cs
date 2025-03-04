using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace Sheltered2SaveEditor.Helpers;

public static class CharacterParser
{
    /// <summary>
    /// Parses the decrypted XML content and extracts character data.
    /// </summary>
    /// <param name="decryptedContent">The decrypted XML content of the save file.</param>
    /// <returns>A list of Character objects parsed from the content.</returns>
    public static List<Character> ParseCharacters(string decryptedContent)
    {
        List<Character> characters = [];

        try
        {
            XDocument doc = XDocument.Parse(decryptedContent);
            XElement? familyMembers = doc.Root?.Element("FamilyMembers");
            if (familyMembers is not null)
            {
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
                            {
                                foreach (XElement skillElement in strengthSkillsElement.Elements())
                                {
                                    XElement? skillKeyElement = skillElement.Element("skillKey");
                                    XElement? skillLevelElement = skillElement.Element("skillLevel");
                                    if (skillKeyElement is not null && skillLevelElement is not null)
                                    {
                                        int skillKey = int.TryParse(skillKeyElement.Value, out int sKey) ? sKey : 0;
                                        int skillLevel = int.TryParse(skillLevelElement.Value, out int sLevel) ? sLevel : 0;

                                        // Use the immutable lookup to get the definition.
                                        /**
                                        SkillDefinition? skillDef = CharacterSkillDefinitions.GetStrengthSkillDefinitionByKey(skillKey);
                                        if (skillDef is not null)
                                        {
                                            // Create a mutable SkillInstance with the current level.
                                            character.StrengthSkills.Add(new SkillInstance(skillDef, skillLevel));
                                        }**/
                                    }
                                }
                            }
                        }
                    }

                    characters.Add(character);
                }
            }
        }
        catch (XmlException ex)
        {
            throw new InvalidDataException("Failed to parse the decrypted content into valid XML.", ex);
        }

        return characters;
    }
}

// Updated Character, Stat, and (removed) Skill classes

public partial class Character : INotifyPropertyChanged
{
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private int _currentHealth;
    private int _maxHealth;
    private bool _interacting;
    private bool _interactingWithObj;
    private bool _hasBeenDefibbed;
    private bool _passedout;
    private bool _unconscious;

    public Stat Strength { get; set; } = new Stat();
    public Stat Dexterity { get; set; } = new Stat();
    public Stat Intelligence { get; set; } = new Stat();
    public Stat Charisma { get; set; } = new Stat();
    public Stat Perception { get; set; } = new Stat();
    public Stat Fortitude { get; set; } = new Stat();
    //public ObservableCollection<SkillInstance> StrengthSkills { get; set; } = [];

    public string FirstName
    {
        get => _firstName;
        set
        {
            if (_firstName != value)
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string LastName
    {
        get => _lastName;
        set
        {
            if (_lastName != value)
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                OnPropertyChanged(nameof(FullName));
            }
        }
    }

    public string FullName => $"{FirstName} {LastName}";

    public int CurrentHealth
    {
        get => _currentHealth;
        set
        {
            if (_currentHealth != value)
            {
                _currentHealth = value;
                OnPropertyChanged(nameof(CurrentHealth));
            }
        }
    }

    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            if (_maxHealth != value)
            {
                _maxHealth = value;
                OnPropertyChanged(nameof(MaxHealth));
            }
        }
    }

    public bool Interacting
    {
        get => _interacting;
        set
        {
            if (_interacting != value)
            {
                _interacting = value;
                OnPropertyChanged(nameof(Interacting));
            }
        }
    }

    public bool InteractingWithObj
    {
        get => _interactingWithObj;
        set
        {
            if (_interactingWithObj != value)
            {
                _interactingWithObj = value;
                OnPropertyChanged(nameof(InteractingWithObj));
            }
        }
    }

    public bool HasBeenDefibbed
    {
        get => _hasBeenDefibbed;
        set
        {
            if (_hasBeenDefibbed != value)
            {
                _hasBeenDefibbed = value;
                OnPropertyChanged(nameof(HasBeenDefibbed));
            }
        }
    }

    public bool PassedOut
    {
        get => _passedout;
        set
        {
            if (_passedout != value)
            {
                _passedout = value;
                OnPropertyChanged(nameof(PassedOut));
            }
        }
    }

    public bool IsUnconscious
    {
        get => _unconscious;
        set
        {
            if (_unconscious != value)
            {
                _unconscious = value;
                OnPropertyChanged(nameof(IsUnconscious));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public partial class Stat : INotifyPropertyChanged
{
    private int _level;

    public int Level
    {
        get => _level;
        set
        {
            if (_level != value)
            {
                _level = Math.Clamp(value, 1, 20);
                OnPropertyChanged(nameof(Level));
                OnPropertyChanged(nameof(Cap));
            }
        }
    }

    public int Cap => _level is >= 1 and <= 5 ? 10 + _level * 2 : _level is > 5 and <= 20 ? 20 : 10;

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}