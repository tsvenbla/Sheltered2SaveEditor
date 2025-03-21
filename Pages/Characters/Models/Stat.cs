﻿using CommunityToolkit.Mvvm.ComponentModel;

namespace Sheltered2SaveEditor.Pages.Characters.Models;

/// <summary>
/// Represents a character stat with its level and cap.
/// </summary>
internal sealed partial class Stat : ObservableObject
{
    private int _level;

    /// <summary>
    /// Gets or sets the level of the stat.
    /// </summary>
    /// <remarks>
    /// The level is clamped between 1 and 20.
    /// When the level changes, the cap is recalculated.
    /// </remarks>
    internal int Level
    {
        get => _level;
        set
        {
            if (_level != value)
            {
                _level = Math.Clamp(value, 1, 20);
                OnPropertyChanged(nameof(Level));
                OnPropertyChanged(nameof(Cap));
            }
        }
    }

    /// <summary>
    /// Gets the cap (maximum) for this stat based on its level.
    /// </summary>
    /// <remarks>
    /// For levels 1-5, the cap is 10 + level * 2.
    /// For levels 6-20, the cap is 20.
    /// </remarks>
    internal int Cap => _level is >= 1 and <= 5 ? 10 + _level * 2 : _level is > 5 and <= 20 ? 20 : 10;
}