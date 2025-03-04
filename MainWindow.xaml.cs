using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Helpers;
using Sheltered2SaveEditor.Pages;

namespace Sheltered2SaveEditor;
public sealed partial class MainWindow : Window
{
    private readonly NavigationHelper _navigationHelper;

    public MainWindow()
    {
        InitializeComponent();
        _navigationHelper = new(ContentFrame, NavigationViewControl);

        // Set the initial page.
        _navigationHelper.Navigate(typeof(HomePage));
        NavigationViewControl.SelectedItem = HomePageView;
    }

    private void OnNavigationViewSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        => _navigationHelper.OnNavigationViewSelectionChanged(sender, args);

    public void EnableNavigationItems() =>
        NavigationHelper.EnableNavigationItems(CharactersPageView, PetsPageView, InventoryPageView, CraftingPageView, FactionsPageView);
}