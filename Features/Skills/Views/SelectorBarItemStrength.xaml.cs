using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Features.Skills.ViewModels;

namespace Sheltered2SaveEditor.Features.Skills.Views;

internal sealed partial class SelectorBarItemStrength : Page
{
    internal StrengthSkillsViewModel ViewModel { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectorBarItemStrength"/> class.
    /// </summary>
    internal SelectorBarItemStrength()
    {
        ViewModel = new StrengthSkillsViewModel();
        InitializeComponent();
        DataContext = ViewModel;
    }
}