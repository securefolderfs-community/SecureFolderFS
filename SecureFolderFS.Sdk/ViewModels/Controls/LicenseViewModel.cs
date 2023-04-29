using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed class LicenseViewModel : ObservableObject
    {
        public required string PackageName { get; init; }

        public required string LicenseName { get; init; }

        public required string LicenseContent { get; init; }

        public required Uri? ProjectWebsiteUri { get; init; }

        public required IEnumerable<Uri> LicenseUris { get; init; }
    }
}