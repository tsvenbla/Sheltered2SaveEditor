using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Features.SaveFiles.ViewModels;

namespace Sheltered2SaveEditor.Features.SaveFiles.Views;

public sealed partial class HomePage : Page
{
    public HomePageViewModel ViewModel { get; }

    public HomePage()
    {
        InitializeComponent();
        ViewModel = DIContainer.Services.GetRequiredService<HomePageViewModel>();
    }
}