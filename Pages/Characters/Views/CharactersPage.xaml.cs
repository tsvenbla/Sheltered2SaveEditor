using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Pages.Characters.ViewModels;

namespace Sheltered2SaveEditor.Pages.Characters.Views;

internal sealed partial class CharactersPage : Page
{
    internal CharactersViewModel ViewModel { get; }

    internal CharactersPage()
    {
        ViewModel = DIContainer.Services.GetRequiredService<CharactersViewModel>();
        InitializeComponent();
    }
}