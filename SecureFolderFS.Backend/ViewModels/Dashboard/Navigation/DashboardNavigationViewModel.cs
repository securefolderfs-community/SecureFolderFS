using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Extensions;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dashboard.Navigation
{
    public sealed class DashboardNavigationViewModel : ObservableObject, IRecipient<DashboardNavigationFinishedMessage>
    {
        private readonly VaultModel _vaultModel;

        public ObservableCollection<DashboardNavigationItemViewModel> DashboardNavigationItems { get; }

        public DashboardNavigationViewModel(VaultModel vaultModel)
        {
            this._vaultModel = vaultModel;

            DashboardNavigationItems = new();

            WeakReferenceMessenger.Default.Register<DashboardNavigationFinishedMessage>(this);
        }

        public void SetNavigation(IDashboardNavigationItemSource dashboardNavigationItemSource)
        {
            ArgumentNullException.ThrowIfNull(dashboardNavigationItemSource);

            if (DashboardNavigationItems.IsEmpty())
            {
                DashboardNavigationItems.Add(new()
                {
                    Index = dashboardNavigationItemSource.Index,
                    NavigationAction = dashboardNavigationItemSource.NavigationAction,
                    SectionName = dashboardNavigationItemSource.SectionName,
                    IsLeading = true
                });
            }
            else
            {
                for (int i = DashboardNavigationItems.Count - 1; i >= 0; i--)
                {
                    DashboardNavigationItems[i].IsLeading = false;

                    if (DashboardNavigationItems[i].Index > dashboardNavigationItemSource.Index)
                    {
                        DashboardNavigationItems.RemoveAt(i);
                    }
                    else if (DashboardNavigationItems[i].Index == dashboardNavigationItemSource.Index)
                    {
                        continue;
                    }
                    else
                    {
                        DashboardNavigationItems.Add(new()
                        {
                            Index = dashboardNavigationItemSource.Index,
                            NavigationAction = dashboardNavigationItemSource.NavigationAction,
                            SectionName = dashboardNavigationItemSource.SectionName,
                        });
                        break;
                    }
                }

                DashboardNavigationItems.Last().IsLeading = true;
            }
        }

        public void Receive(DashboardNavigationFinishedMessage message)
        {
            if (message.Value.UnlockedVaultModel.VaultModel != _vaultModel)
            {
                return;
            }

            SetNavigation(message.Value);
        }
    }
}
