using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Features.Characters.ViewModels;

namespace Sheltered2SaveEditor.Features.Characters.Views;

public sealed partial class CharactersPage : Page
{
    public CharactersViewModel ViewModel { get; }

    public CharactersPage()
    {
        ViewModel = DIContainer.Services.GetRequiredService<CharactersViewModel>();
        InitializeComponent();
    }
}