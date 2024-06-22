using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Bindable(true)]
    public sealed partial class LicenseViewModel : ObservableObject
    {
        [ObservableProperty] private string? _PackageName;
        [ObservableProperty] private string? _LicenseName;
        [ObservableProperty] private string? _LicenseContent;
        [ObservableProperty] private Uri? _ProjectWebsiteUri;
        [ObservableProperty] private IEnumerable<Uri>? _LicenseUris;
    }
}