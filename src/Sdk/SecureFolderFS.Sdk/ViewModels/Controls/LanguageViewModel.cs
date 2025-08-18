﻿using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    /// <summary>
    /// Represents a language view model.
    /// </summary>
    [Bindable(true)]
    public sealed partial class LanguageViewModel : ObservableObject, IViewable
    {
        /// <summary>
        /// Gets the <see cref="CultureInfo"/> of the language.
        /// </summary>
        public CultureInfo CultureInfo { get; }

        /// <inheritdoc cref="IViewable.Title"/>
        [ObservableProperty] private string? _Title;

        public LanguageViewModel(CultureInfo cultureInfo)
            : this(cultureInfo, FormatName(cultureInfo))
        {
        }

        public LanguageViewModel(CultureInfo cultureInfo, string title)
        {
            CultureInfo = cultureInfo;
            Title = title;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Title ?? CultureInfo.DisplayName;
        }

        private static string FormatName(CultureInfo cultureInfo)
        {
            var name = cultureInfo.NativeName;

            // Convert the first letter to uppercase
            name = char.ToUpperInvariant(name[0]) + name.Substring(1);

            // Sometimes the name may not have the country
            if (name.Contains('('))
                return name;

            // Get the region to use for the country name
            try
            {
                var regionInfo = new RegionInfo(cultureInfo.LCID);
                return $"{name} ({regionInfo.DisplayName})";
            }
            catch (Exception ex)
            {
                _ = ex;
                Debugger.Break();

                return name;
            }
        }
    }
}
