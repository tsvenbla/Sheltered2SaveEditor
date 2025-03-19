using Microsoft.UI.Xaml.Controls;
using Sheltered2SaveEditor.Features.Donate.Models;
using System;
using System.Collections.ObjectModel;

namespace Sheltered2SaveEditor.Features.Donate.Views;

public sealed partial class DonatePage : Page
{
    public ObservableCollection<DonationItem> Donations { get; } = [];

    public DonatePage()
    {
        InitializeComponent();
        LoadDonations();
    }

    /// <summary>
    /// Populates the Donations collection with items.
    /// </summary>
    private void LoadDonations()
    {
        Donations.Add(new DonationItem
        {
            ItemTitle = "Buy Me a Coffee",
            ItemImagePath = "ms-appx:///Assets/BuyMeaCoffee-QR.svg",
            ItemImageAutomationName = "Buy Me a Coffee QR Code",
            ItemNavigateUri = new Uri("https://www.buymeacoffee.com/tsvenbla"),
            ItemButtonContent = "Buy Me a Coffee"
        });

        Donations.Add(new DonationItem
        {
            ItemTitle = "PayPal",
            ItemImagePath = "ms-appx:///Assets/PayPal-QR.svg",
            ItemImageAutomationName = "PayPal QR Code",
            ItemNavigateUri = new Uri("https://www.paypal.com/donate/?hosted_button_id=ZS7ZV6GFU7ZA8"),
            ItemButtonContent = "PayPal"
        });

        Donations.Add(new DonationItem
        {
            ItemTitle = "Bitcoin",
            ItemImagePath = "ms-appx:///Assets/Bitcoin-QR.svg",
            ItemImageAutomationName = "Bitcoin QR Code",
            ItemNavigateUri = new Uri("bitcoin:bc1qwfz7zte3v032v8n0pfdzc9xlj5gug72vea4qwc"),
            ItemButtonContent = "Bitcoin",
            ItemAddress = "bc1qwfz7zte3v032v8n0pfdzc9xlj5gug72vea4qwc"
        });

        Donations.Add(new DonationItem
        {
            ItemTitle = "Ethereum",
            ItemImagePath = "ms-appx:///Assets/Ethereum-QR.svg",
            ItemImageAutomationName = "Ethereum QR Code",
            ItemNavigateUri = new Uri("eth:0xb4edC363bA5D59F00207fa78eA9149cF87382AD2"),
            ItemButtonContent = "Ethereum",
            ItemAddress = "0xb4edC363bA5D59F00207fa78eA9149cF87382AD2"
        });
    }
}