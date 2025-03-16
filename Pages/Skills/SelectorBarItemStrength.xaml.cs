using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.ViewModels;

namespace Sheltered2SaveEditor.Pages.Skills;

public sealed partial class SelectorBarItemStrength : Page
{
    public StrengthSkillsViewModel ViewModel { get; } = new StrengthSkillsViewModel();

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectorBarItemStrength"/> class.
    /// </summary>
    public SelectorBarItemStrength()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}