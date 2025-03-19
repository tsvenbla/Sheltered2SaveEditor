using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Features.Skills.ViewModels;

namespace Sheltered2SaveEditor.Features.Skills.Views;

public sealed partial class SelectorBarItemStrength : Page
{
    public StrengthSkillsViewModel ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectorBarItemStrength"/> class.
    /// </summary>
    public SelectorBarItemStrength()
    {
        ViewModel = new StrengthSkillsViewModel();
        InitializeComponent();
        DataContext = ViewModel;
    }
}