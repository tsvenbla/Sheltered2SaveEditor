using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.ViewModels;

namespace Sheltered2SaveEditor.Pages;

public sealed partial class CharactersPage : Page
{
    public CharactersPage()
    {
        InitializeComponent();
        DataContext = DIContainer.Services.GetRequiredService<CharactersViewModel>();
    }
}