using CommunityToolkit.Mvvm.ComponentModel;

namespace Sheltered2SaveEditor.Core.Models;

/// <summary>
/// Represents a character in the game with their attributes and stats.
/// </summary>
public partial class Character : ObservableObject
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

    /// <summary>
    /// Gets or sets the character's strength stat.
    /// </summary>
    public Stat Strength { get; set; } = new Stat();

    /// <summary>
    /// Gets or sets the character's dexterity stat.
    /// </summary>
    public Stat Dexterity { get; set; } = new Stat();

    /// <summary>
    /// Gets or sets the character's intelligence stat.
    /// </summary>
    public Stat Intelligence { get; set; } = new Stat();

    /// <summary>
    /// Gets or sets the character's charisma stat.
    /// </summary>
    public Stat Charisma { get; set; } = new Stat();

    /// <summary>
    /// Gets or sets the character's perception stat.
    /// </summary>
    public Stat Perception { get; set; } = new Stat();

    /// <summary>
    /// Gets or sets the character's fortitude stat.
    /// </summary>
    public Stat Fortitude { get; set; } = new Stat();

    /// <summary>
    /// Gets the collection of strength skills for this character.
    /// </summary>
    // public ObservableCollection<SkillInstance> StrengthSkills { get; } = [];

    /// <summary>
    /// Gets or sets the character's first name.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the character's last name.
    /// </summary>
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

    /// <summary>
    /// Gets the character's full name (first name + last name).
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Gets or sets the character's current health.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the character's maximum health.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the character is interacting.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the character is interacting with an object.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the character has been defibrillated.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the character is passed out.
    /// </summary>
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

    /// <summary>
    /// Gets or sets a value indicating whether the character is unconscious.
    /// </summary>
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
}