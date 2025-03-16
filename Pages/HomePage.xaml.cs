using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.ViewModels;

namespace Sheltered2SaveEditor.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        DataContext = DIContainer.Services.GetRequiredService<HomePageViewModel>();
    }
}