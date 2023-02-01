using System;
using System.Collections.Generic;

namespace SecureFolderFS.Sdk.AppModels
{
    public sealed record LicenseModel(string PackageName, IEnumerable<Uri> LicenseUris, string LicenseName, Uri? ProjectWebsiteUri, string License);
}