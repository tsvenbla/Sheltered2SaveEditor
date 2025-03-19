using System;

namespace Sheltered2SaveEditor.Features.Donate.Models;

/// <summary>
/// Represents a donation item with its associated properties.
/// </summary>
internal class DonationItem
{
    internal string? ItemTitle { get; set; }
    internal string? ItemImagePath { get; set; }
    internal string? ItemImageAutomationName { get; set; }
    internal Uri? ItemNavigateUri { get; set; }
    internal string? ItemButtonContent { get; set; }
    internal string? ItemAddress { get; set; }
}