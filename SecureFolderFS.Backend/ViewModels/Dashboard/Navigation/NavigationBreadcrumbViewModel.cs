using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Backend.Messages;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dashboard.Navigation
{
    public sealed class NavigationBreadcrumbViewModel : ObservableObject, IRecipient<DashboardNavigationFinishedMessage>
    {
        public ObservableCollection<NavigationItemViewModel> DashboardNavigationItems { get; }

        public NavigationBreadcrumbViewModel()
        {
            DashboardNavigationItems = new();
        }

        public void SetNavigation(NavigationItemViewModel itemViewModel)
        {
            ArgumentNullException.ThrowIfNull(itemViewModel);

            if (DashboardNavigationItems.IsEmpty())
            {
                DashboardNavigationItems.Add(new()
                {
                    Index = itemViewModel.Index,
                    NavigationAction = itemViewModel.NavigationAction,
                    SectionName = itemViewModel.SectionName,
                    IsLeading = true
                });
            }
            else
            {
                for (int i = DashboardNavigationItems.Count - 1; i >= 0; i--)
                {
                    DashboardNavigationItems[i].IsLeading = false;

                    if (DashboardNavigationItems[i].Index > itemViewModel.Index)
                    {
                        DashboardNavigationItems.RemoveAt(i);
                    }
                    else if (DashboardNavigationItems[i].Index == itemViewModel.Index)
                    {
                        DashboardNavigationItems.RemoveAt(i);
                        DashboardNavigationItems.Insert(i, itemViewModel);
                        break;
                    }
                    else
                    {
                        DashboardNavigationItems.Add(new()
                        {
                            Index = itemViewModel.Index,
                            NavigationAction = itemViewModel.NavigationAction,
                            SectionName = itemViewModel.SectionName,
                        });

                        break;
                    }
                }

                DashboardNavigationItems.Last().IsLeading = true;
            }
        }

        public void Receive(DashboardNavigationFinishedMessage message)
        {
            SetNavigation(message.Value.NavigationItemViewModel!);
        }
    }
}
