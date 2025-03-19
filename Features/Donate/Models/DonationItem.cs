using System;

namespace Sheltered2SaveEditor.Features.Donate.Models;

/// <summary>
/// Represents a donation item with its associated properties.
/// </summary>
public class DonationItem
{
    public string? ItemTitle { get; set; }
    public string? ItemImagePath { get; set; }
    public string? ItemImageAutomationName { get; set; }
    public Uri? ItemNavigateUri { get; set; }
    public string? ItemButtonContent { get; set; }
    public string? ItemAddress { get; set; }
}