using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Features.SaveFiles.ViewModels;

namespace Sheltered2SaveEditor.Features.SaveFiles.Views;

internal sealed partial class HomePage : Page
{
    internal HomePageViewModel ViewModel { get; }

    internal HomePage()
    {
        ViewModel = DIContainer.Services.GetRequiredService<HomePageViewModel>();
        InitializeComponent();
    }
}