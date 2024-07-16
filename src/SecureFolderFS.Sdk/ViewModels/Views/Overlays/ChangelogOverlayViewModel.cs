﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IApplicationService>, Inject<IChangelogService>]
    [Bindable(true)]
    public sealed partial class ChangelogOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        private readonly Version _changelogSince;

        [ObservableProperty] private string? _UpdateText;
        [ObservableProperty] private string? _ErrorText;

        public ChangelogOverlayViewModel(Version changelogSince)
        {
            ServiceProvider = DI.Default;
            _changelogSince = changelogSince;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var changelogBuilder = new StringBuilder();
            var appVersion = ApplicationService.AppVersion;
            var loadLatest = appVersion == _changelogSince;

            try
            {
                if (loadLatest)
                {
                    var changelog = await ChangelogService.GetChangelogAsync(appVersion, ApplicationService.Platform, cancellationToken);
                    if (changelog is null)
                    {
                        UpdateText = "No update info available";
                        return;
                    }

                    BuildChangelog(changelog, changelogBuilder);
                }
                else
                {
                    await foreach (var item in ChangelogService.GetChangelogSinceAsync(_changelogSince, ApplicationService.Platform, cancellationToken))
                    {
                        BuildChangelog(item, changelogBuilder);
                    }
                }

                UpdateText = changelogBuilder.ToString();
            }
            catch (Exception ex)
            {
                ErrorText = $"{ex.GetType().Name}: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task RetryAsync(CancellationToken cancellationToken)
        {
            UpdateText = null;
            ErrorText = null;

            await Task.Yield();
            await InitAsync(cancellationToken);
        }

        // TODO: Perhaps use an IFormatProvider somehow?
        private static void BuildChangelog(ChangelogDataModel changelog, StringBuilder changelogBuilder)
        {
            changelogBuilder.Append(changelog.Name);
            changelogBuilder.Append("\n---\n");
            changelogBuilder.Append(changelog.Description.Replace("\r\n", "\r\n\n"));
            changelogBuilder.Append("\n----\n");
        }
    }
}
