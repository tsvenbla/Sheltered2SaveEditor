using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using System;

namespace Sheltered2SaveEditor.Utils.Converters;

/// <summary>
/// Converts a string value to a <see cref="Visibility"/> value.
/// Returns <see cref="Visibility.Collapsed"/> if the string is null or empty; otherwise, returns <see cref="Visibility.Visible"/>.
/// </summary>
public sealed partial class NullToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> value returned when the input is null or empty.
    /// </summary>
    public Visibility NullValue { get; set; } = Visibility.Collapsed;

    /// <summary>
    /// Gets or sets the <see cref="Visibility"/> value returned when the input is not null or empty.
    /// </summary>
    public Visibility NonNullValue { get; set; } = Visibility.Visible;

    /// <inheritdoc/>
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value switch
        {
            null => NullValue,
            string s when string.IsNullOrEmpty(s) => NullValue,
            _ => NonNullValue,
        };

    /// <inheritdoc/>
    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException("ConvertBack is not supported.");
}