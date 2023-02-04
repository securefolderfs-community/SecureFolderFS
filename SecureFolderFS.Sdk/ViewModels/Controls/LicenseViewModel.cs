using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    public sealed record LicenseViewModel(string PackageName, IEnumerable<Uri> LicenseUris, string LicenseName, Uri? ProjectWebsiteUri, string License);
}