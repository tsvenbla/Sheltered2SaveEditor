using CommunityToolkit.Mvvm.ComponentModel;
using Sheltered2SaveEditor.Helpers;

namespace Sheltered2SaveEditor.ViewModels;

/// <summary>
/// ViewModel representing a single skill instance, including its definition and current assigned level.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="SkillInstanceViewModel"/> class.
/// </remarks>
/// <param name="skillDefinition">The immutable definition for the skill.</param>
/// <param name="skillTree">
/// The name of the skill tree to which the skill belongs (e.g., "Strength", "Intelligence", "Charisma", "Fortitude", "Perception", or "Dexterity").
/// </param>
/// <param name="currentLevel">The current level assigned to the skill.</param>
public partial class SkillInstanceViewModel(SkillDefinition skillDefinition, string skillTree, int currentLevel) : ObservableObject
{
    public int SkillKey => skillDefinition.SkillKey;
    public string Name => skillDefinition.Name;
    public int MaxLevel => skillDefinition.MaxLevel;
    public int Tier => skillDefinition.Tier;
    public int DisplayOrder => skillDefinition.DisplayOrder;

    public int CurrentLevel
    {
        get => currentLevel;
        set => SetProperty(ref currentLevel, value);
    }

    public string ImageSource => $"ms-appx:///Assets/Skills/Skill{skillTree}{Name.Replace(" ", string.Empty)}.png";
}